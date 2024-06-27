using BExIS.App.Bootstrap.Attributes;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using BExIS.Modules.Dcm.UI.Hooks;
using BExIS.Modules.Dcm.UI.Models.Edit;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.UI.Hooks;
using BExIS.UI.Hooks.Caches;
using BExIS.UI.Hooks.Logs;
using BExIS.UI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Utils.Cfg;
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
                                Unit = variable.Unit.Name,
                                IsKeys = variable.IsKey
                            });
                        }

                        //if data structure is there,  check also if data is there
                        model.HasData = datasetManager.RowAny(model.Id);
                    }

                    // set enable edit rights for strtucture

                    model.EnableEdit = hasEditRights(this.HttpContext);
                }

                // get values from template
                using (var entityTemplateManager = new EntityTemplateManager())
                using (var datasetManager = new DatasetManager())
                {
                    var dataset = datasetManager.GetDataset(id);
                    if (dataset == null) throw new NullReferenceException(String.Format("Subject wih id {0} not exist", id));

                    var template = entityTemplateManager.Repo.Get(dataset.EntityTemplate.Id);
                    if (template == null) throw new NullReferenceException(String.Format("Template wih id {0} not exist", id));

                    model.IsStructured = template.HasDatastructure;
                    model.IsRestricted = template.DatastructureList.Any();
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
                    EditDatasetDetailsLog log = hookManager.LoadCache<EditDatasetDetailsLog>("dataset", "details", HookMode.edit, id);
                    var message = String.Format("the data structure {0} was successfully deleted", structureId);

                    var username = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);
                    log.Messages.Add(new LogMessage(DateTime.Now, message, username, "Data description", "delete"));

                    cache.UpdateLastModificarion(typeof(DataDescriptionHook));

                    hookManager.SaveCache<EditDatasetDetailsCache>(cache, "dataset", "details", HookMode.edit, id);
                }
                catch (Exception ex)
                {
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

        [JsonNetFilter]
        [HttpGet]
        public JsonResult AvailableStructues(long id)
        {
            if (id <= 0) throw new ArgumentException("must be greater then 0", "id");

            List<ListItem> tmp = new List<ListItem>();

            using (var entityTemplateManager = new EntityTemplateManager())
            using (var structureManager = new DataStructureManager())
            using (var datasetManager = new DatasetManager())
            {
                var dataset = datasetManager.GetDataset(id);
                if (dataset == null) return Json(new { success = false, message = "dataset not exit" });

                var template = entityTemplateManager.Repo.Get(dataset.EntityTemplate.Id);
                if (template == null) return Json(new { success = false, message = "template not exit" });

                var structures = structureManager.GetStructuredDataStructuresAsKVP();

                // get only subset of the structures restricted by the entity template.DatastructureList
                if (template.HasDatastructure == true && template.DatastructureList.Any())
                {
                    foreach (var dsId in template.DatastructureList)
                    {
                        var ds = structures.Where(d => d.Key == dsId).FirstOrDefault();
                        tmp.Add(new ListItem() { Id = ds.Key, Text = ds.Value, Group = "structure" });
                    }
                }
                else if (template.HasDatastructure == true) // get all structures
                {
                    foreach (var ds in structures)
                    {
                        tmp.Add(new ListItem() { Id = ds.Key, Text = ds.Value, Group = "structure" });
                    }
                }
            }

            return Json(tmp, JsonRequestBehavior.AllowGet);
        }

        [HttpPut]
        public JsonResult Set(long id, long structureId)
        {
            if (id <= 0) throw new ArgumentException("must be greater then 0", "id");
            if (structureId <= 0) throw new ArgumentException("must be greater then 0", "structureId");

            using (var structureManager = new DataStructureManager())
            using (var datasetManager = new DatasetManager())
            {
                var hookManager = new HookManager();
                EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);
                EditDatasetDetailsLog log = hookManager.LoadLog<EditDatasetDetailsLog>("dataset", "details", HookMode.edit, id);
                var username = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);

                var dataset = datasetManager.GetDataset(id);
                if (dataset == null) throw new ArgumentNullException("dataset");

                var structure = structureManager.StructuredDataStructureRepo.Get(structureId);
                if (structure == null) throw new ArgumentNullException("structure");

                dataset.DataStructure = structure;
                datasetManager.UpdateDataset(dataset);

                // update cache
                //Update primary key in update method
                if (cache.UpdateSetup == null) cache.UpdateSetup = new UpdateSetup();
                cache.UpdateSetup.PrimaryKeys = structure.Variables.Where(v => v.IsKey)?.Select(v =>v.Id).ToList();

                // update modifikation date
                cache.UpdateLastModificarion(typeof(DataDescriptionHook));

                // store in messages
                string message = String.Format("the structure {0} was successfully attached to the dataset {1}.", structure.Name, id);
                log.Messages.Add(new LogMessage(DateTime.Now, new List<string>() { message }, username, "Data description", "set"));

                // save cache
                hookManager.Save(cache, log, "dataset", "details", HookMode.edit, id);

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Remove(long id)
        {
            if (id <= 0) throw new ArgumentException("must be greater then 0", "id");

            using (var datasetManager = new DatasetManager())
            {
                var hookManager = new HookManager();
                EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);
                EditDatasetDetailsLog log = hookManager.LoadLog<EditDatasetDetailsLog>("dataset", "details", HookMode.edit, id);
                var username = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);

                var dataset = datasetManager.GetDataset(id);
                if (dataset == null) throw new ArgumentNullException("dataset");

                // if data exist do not reset the structure
                if (datasetManager.RowAny(dataset.Id))
                    throw new Exception("can not change the structure of this dataset, because data allready exist.");

                dataset.DataStructure = null;
                datasetManager.UpdateDataset(dataset);

                // update cache
                // update modifikation date
                cache.UpdateLastModificarion(typeof(DataDescriptionHook));

                // store in messages
                string message = String.Format("structure was successfully removed from the dataset {0}.", id);
                log.Messages.Add(new LogMessage(DateTime.Now, new List<string>() { message }, username, "Data description", "remove"));

                // save cache
                hookManager.Save(cache, log, "dataset", "details", HookMode.edit, id);

                return Json(true, JsonRequestBehavior.AllowGet);
            }
        }

        private bool hasEditRights(HttpContextBase context)
        {
            using (var featurePermissionManager = new FeaturePermissionManager())
            using (var operationManager = new OperationManager())
            {
                var operation = operationManager.Find("rpm", "datastructure", "*");
                var user = BExISAuthorizeHelper.GetUserFromAuthorizationAsync(context).Result;

                var feature = operation.Feature;

                //if opration has no feature and is public
                if (operation.Feature == null) return true;

                // if feature is public
                if (featurePermissionManager.Exists(null, feature.Id)) return true;

                // feature and user exist
                if (feature != null && !featurePermissionManager.Exists(null, feature.Id))
                    if (featurePermissionManager.HasAccess(user.Id, feature.Id))
                        return true;

                return false;
            }
        }
    }
}