using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Dcm.UI.Hooks;
using BExIS.UI.Hooks;
using BExIS.UI.Hooks.Caches;
using BExIS.UI.Models;
using BExIS.Utils.Data.Upload;
using BExIS.Utils.Upload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Modularity;
using Cache = BExIS.UI.Hooks.Caches;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class FileUploadController : Controller
    {
        /// <summary>
        /// entry for hook
        /// </summary>
        /// <returns></returns>
        public ActionResult Start(long id, int version)
        {
            return RedirectToAction("load", new { id, version });
        }

        // GET: FileUpload
        [JsonNetFilter]
        public JsonResult Load(long id, int version)
        {
            // check incoming variables
            if (id <= 0) throw new ArgumentException("id must be greater than 0");

            FileUploader model = new FileUploader();

            model.Multiple = true;

            # region settings

            var settings = ModuleManager.GetModuleSettings("dcm");

            // description
            var descrType = settings.GetEntryValue("fileuploaddescription").ToString();//
            model.DescriptionType = (DescriptionType)Enum.Parse(typeof(DescriptionType), descrType);

            // max size
            model.MaxSize = Session.GetTenant().MaximumUploadSize; // need to load from tenant
            //multifileupload
            model.Multiple = Boolean.Parse(settings.GetEntryValue("allowMultiFileupload").ToString());

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
                        model.ExistingFiles.Add(file); // if exist  add to model
                    else
                        cache.Files.RemoveAt(i); // if not remove from cache
                }
            }

            // get accepted file extentions
            using (var datasetManager = new DatasetManager())
            {
                var dataset = datasetManager.GetDataset(id);

                // get list of file types based on system settings, like 
                // if structure exist or not

                DataStructureType datastructureType = DataStructureType.None;

                // check if datastructure type exist
                if (dataset.DataStructure != null &&
                    dataset.DataStructure.Self.GetType().Equals(typeof(StructuredDataStructure)))
                    datastructureType = DataStructureType.Structured;

                // get default list of allowed file types
                model.Accept = UploadHelper.GetExtentionList(datastructureType, this.Session.GetTenant());

                // if entity template has some allowed filestypes
                if (dataset.EntityTemplate.AllowedFileTypes!=null && dataset.EntityTemplate.AllowedFileTypes.Any())
                {
                    // the system needs to compare them with the system list 
                    // only add filetypes dat are in both lists
                    model.Accept = model.Accept.Intersect(dataset.EntityTemplate.AllowedFileTypes).ToList();
                }

            }

            // set modification date
            model.LastModification = cache.GetLastModificarion(typeof(FileUploadHook));

            hookManager.SaveCache(cache, "dataset", "details", HookMode.edit, id);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Upload(long id)
        {
            HookManager hookManager = new HookManager();
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);
            List<string> filesNames = new List<string>();

            string folder = "Temp"; // folder name inside dataset - temp or attachments

            if (Request.Files.Count > 0)
            {
                try
                {
                    //  Get all files from Request object
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        var file = files[i];
                        string fname = getFileName(file);
                        //data/datasets/1/1/
                        var dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");
                        var storepath = Path.Combine(dataPath, "Datasets", id.ToString(), folder);

                        // if folder not exist
                        if (!Directory.Exists(storepath)) Directory.CreateDirectory(storepath);

                        var path = Path.Combine(storepath, fname);

                        file.SaveAs(path);

                        if (cache.Files == null) cache.Files = new List<Cache.FileInfo>();
                        cache.Files.Add(new Cache.FileInfo(fname, file.ContentType, file.ContentLength, ""));
                        filesNames.Add(fname);
                    }


                    // update last modification time
                    cache.UpdateLastModificarion(typeof(FileUploadHook));

                    // Returns message that successfully uploaded
                    return Json("File Uploaded Successfully!");
                }
                catch (Exception ex)
                {
                    return Json("Error occurred. Error details: " + ex.Message);
                }
                finally
                {
                    List<string> messages = new List<string> { "Files uploaded" };
                    messages.AddRange(filesNames);

                    cache.Messages.Add(new ResultMessage(DateTime.Now, messages));
                    hookManager.SaveCache(cache, "dataset", "details", HookMode.edit, id);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }

        [HttpPost]
        public JsonResult RemoveFile(long id, string file)
        {
            // remove file from server
            string path = Path.Combine(AppConfiguration.DataPath, "datasets", id.ToString(), "Temp", file);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

            // remove file from cache
            HookManager hookManager = new HookManager();
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);

            if (cache.Files.Any(f => f.Name == file))
            {
                var f = cache.Files.Where(x => x.Name == file).FirstOrDefault();
                if (f != null) cache.Files.Remove(f);
            }

            cache.Messages.Add(new ResultMessage(DateTime.Now, new List<string>() { file + " removed" }));
            // update last modification time
            cache.UpdateLastModificarion(typeof(FileUploadHook));
            hookManager.SaveCache(cache, "dataset", "details", HookMode.edit, id);

            return Json(true);
        }

        [HttpPost]
        public JsonResult SaveFileDescription(long id, string file, string description)
        {
            // remove file from cache
            HookManager hookManager = new HookManager();
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);

            if (cache.Files.Any(f => f.Name == file))
            {
                var f = cache.Files.Where(x => x.Name == file).FirstOrDefault();
                if (f != null) f.Description = description;
            }

            cache.Messages.Add(new ResultMessage(DateTime.Now, new List<string>() { file + " description updated" }));
            // update last modification time
            cache.UpdateLastModificarion(typeof(FileUploadHook));
            hookManager.SaveCache(cache, "dataset", "details", HookMode.edit, id);

            return Json(true);
        }

        private string getFileName(HttpPostedFileBase file)
        {
            // Checking for Internet Explorer
            if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
            {
                string[] testfiles = file.FileName.Split(new char[] { '\\' });
                return testfiles[testfiles.Length - 1];
            }
            else
            {
                return file.FileName;
            }
        }
    }
}