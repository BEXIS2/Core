using BExIS.App.Bootstrap.Attributes;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
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
                        if(isReadable(file))
                            model.ReadableFiles.Add(file); // if exist  add to model
                    }
                    else
                        cache.Files.RemoveAt(i); // if not remove from cache
                }
            }


             // set modification date
            model.LastModification = cache.GetLastModificarion(typeof(AttachmentEditHook));


            return Json(model, JsonRequestBehavior.AllowGet);
        }

        private bool isReadable(Cache.FileInfo file)
        {
            IOUtility iou = new IOUtility();
            return iou.IsSupportedAsciiFile(Path.GetExtension(file.Name));
        }
    }
}