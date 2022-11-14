﻿using BExIS.App.Bootstrap.Attributes;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using BExIS.IO.Transform.Input;
using BExIS.Modules.Dcm.UI.Hooks;
using BExIS.Modules.Dcm.UI.Models.Edit;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Utilities;
using BExIS.UI.Hooks;
using BExIS.UI.Hooks.Caches;
using BExIS.UI.Models;
using BExIS.Utils.Config;
using BExIS.Utils.Data.Upload;
using BExIS.Utils.Upload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Mvc;
using Vaiona.Entities.Common;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Modularity;
using Cache = BExIS.UI.Hooks.Caches;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class DataDescriptionController : Controller
    {
        /// <summary>
        /// entry for hook
        /// </summary>
        /// <returns></returns>
        public ActionResult Start(long id, int version)
        {
            //return View();
            return RedirectToAction("load", new { id, version });
        }

        // GET: FileUpload
        [JsonNetFilter]
        public JsonResult Load(long id, int version)
        {
            // check incoming variables
            if (id <= 0) throw new ArgumentException("id must be greater than 0");

            DataDescriptionModel model = new DataDescriptionModel();
            model.Id = id;

            # region settings

            var settings = ModuleManager.GetModuleSettings("dcm");

            #endregion

            if (id > 0)
            {
                // load data from database
                using (var datasetManager = new DatasetManager())
                using (var structureManager = new DataStructureManager())
                {
                    var dataset = datasetManager.GetDataset(id);
                    if (dataset.DataStructure != null)
                        model.StructureId = dataset.DataStructure.Id;

                    var structure = structureManager.StructuredDataStructureRepo.Get(model.StructureId);

                    if (structure != null && structure.Variables.Any())
                    {
                        model.Title = structure.Name;
                        model.Description = structure.Description;

                        foreach (var variable in structure.Variables)
                        {
                            model.Variables.Add(new VariableModel()
                            {
                                Id = variable.Id,
                                Name = variable.Label,
                                DataType = variable.DataType.Name,
                                Unit = variable.Unit.Name
                            });

                        }
                    }
                }
            }


            HookManager hookManager = new HookManager();
            // load cache to check existing files
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);

            // check if files in list also on server
            string path = Path.Combine(AppConfiguration.DataPath, "datasets", id.ToString(), "Temp");
            if (cache.Files != null)
            {
                for (int i = 0; i < cache.Files.Count; i++)
                {
                    var file = cache.Files[i];
                    //check if if exist on server or not
                    if (file != null && !string.IsNullOrEmpty(file.Name) && System.IO.File.Exists(Path.Combine(path, file.Name)))
                    {
                        if (isReadable(file))
                            model.ReadableFiles.Add(file); // if exist  add to model
                    }
                    else
                        cache.Files.RemoveAt(i); // if not remove from cache
                }

                model.AllFilesReadable = cache.Files.Count == model.ReadableFiles.Count;
            }


            // set modification date
            model.LastModification = cache.GetLastModificarion(typeof(DataDescriptionHook));


            return Json(model, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// this action delete a structure from database and remove the link the the entity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="structureId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        [HttpDelete]
        public JsonResult Delete(long id, long structureId)
        {
            // check incoming variables
            if (id <= 0) throw new ArgumentException("id must be greater than 0");
            if (structureId <= 0) throw new ArgumentException("structureId must be greater than 0");

            using (DatasetManager datasetManager = new DatasetManager())
            using (DataStructureManager dataStructureManager = new DataStructureManager())
            {
                var dataset = datasetManager.GetDataset(id);
                if (dataset == null)
                    return Json(new { success = false, message = "entity not exist" });

                var structure = dataStructureManager.StructuredDataStructureRepo.Get(structureId);

                if (structure == null)
                    return Json(new { success = false, message = "structure not exist" });

                try
                {
                    // remove link to dataset
                    dataset.DataStructure = null;
                    datasetManager.UpdateDataset(dataset); // store in db

                    // delete datastructure from database
                    dataStructureManager.DeleteStructuredDataStructure(structure);

                    // maybe delete also all variables and missng values


                    // update cache
                    HookManager hookManager = new HookManager();

                    EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);
                    var message = String.Format("the data structure {0} was successfully deleted", structureId);
                    cache.Messages.Add(new ResultMessage(DateTime.Now, message ));

                    cache.UpdateLastModificarion(typeof(DataDescriptionHook));

                    hookManager.SaveCache<EditDatasetDetailsCache>(cache, "dataset", "details", HookMode.edit, id);
                }
                catch(Exception ex) {

                    return Json(new { success = false, message = ex.Message });
                }


            }


            
           return Json(new { success = true, message = "the data structure was successfully deleted" });
        }

        private bool isReadable(Cache.FileInfo file)
        {
            IOUtility iou = new IOUtility();
            return iou.IsSupportedAsciiFile(Path.GetExtension(file.Name));
        }
    }
}