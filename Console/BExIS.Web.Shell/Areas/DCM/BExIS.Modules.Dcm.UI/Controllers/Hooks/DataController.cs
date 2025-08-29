using BExIS.App.Bootstrap.Attributes;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Dcm.UI.Hooks;
using BExIS.Modules.Dcm.UI.Models.Edit;
using BExIS.UI.Hooks;
using BExIS.UI.Hooks.Caches;
using BExIS.UI.Hooks.Logs;
using BExIS.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Telerik.Web.Mvc.Infrastructure.Implementation;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class DataController : Controller
    {
        // GET: Data
        public ActionResult Start(long id, int version)
        {
            return RedirectToAction("load", new { id, version });
        }

        [JsonNetFilter]
        public JsonResult Load(long id, int version)
        {
            DataModel model = new DataModel();
            model.Id = id;
            model.Version = version;

            HookManager hookManager = new HookManager();
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);
            EditDatasetDetailsLog log = hookManager.LoadLog<EditDatasetDetailsLog>("dataset", "details", HookMode.edit, id);


            #region settings
            var settings = ModuleManager.GetModuleSettings("dcm");
            // description
            var descrType = settings.GetValueByKey("fileuploaddescription").ToString();//
            model.DescriptionType = (DescriptionType)Enum.Parse(typeof(DescriptionType), descrType);
            #endregion

            using (var datamanager = new DatasetManager())
            {
                if(datamanager.IsDatasetCheckedIn(id))
                { 
                    var datasetversion = datamanager.GetDatasetLatestVersion(id); // get latest version of dataset

                    if (datasetversion != null) // if dataset version  
                    {
                        // check if dataset has structure
                        if (datasetversion.Dataset.DataStructure != null)
                        {
                            model.HasStructure = true;
                        }
                        else
                        {
                            model.HasStructure = false;
                            if (datasetversion.ContentDescriptors.Any()) // check if dataset has content
                            {
                                model.ExistingFiles = new List<FileInfo>();
                                foreach (var content in datasetversion.ContentDescriptors)
                                {
                                    if (content.Name.Equals("unstructuredData"))
                                    {
                                        string name = System.IO.Path.GetFileName(content.URI);
                                        if (cache.DeleteFiles == null) cache.DeleteFiles = new List<FileInfo>();
                                        if (cache.ModifiedFiles == null) cache.ModifiedFiles = new List<FileInfo>();

                                        // add only if not in delete list
                                        if (!cache.DeleteFiles.Any(f => f.Name.Equals(name)))
                                        {
                                            // check if files allready modified
                                            if (!cache.ModifiedFiles.Any(f => f.Name.Equals(name)))
                                            {
                                                // add file to list
                                                model.ExistingFiles.Add(new FileInfo()
                                                {
                                                    Name = name,
                                                    Description = content.Description,
                                                    Type = content.MimeType,
                                                    Lenght = content.FileSize
                                                });
                                            }
                                            else // exist allready modified
                                            {
                                                model.ExistingFiles.Add(cache.ModifiedFiles.FirstOrDefault(f => f.Name.Equals(name)));
                                            }
                                        }

                                        model.DeleteFiles = cache.DeleteFiles;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RemoveFile(long id, FileInfo file)
        {
            // remove file from server
            string path = System.IO.Path.Combine(AppConfiguration.DataPath, "datasets", id.ToString(), "Temp", file.Name);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

            // remove file from cache
            HookManager hookManager = new HookManager();
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);
            EditDatasetDetailsLog log = hookManager.LoadLog<EditDatasetDetailsLog>("dataset", "details", HookMode.edit, id);
            var username = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);

            if (cache.DeleteFiles == null) cache.DeleteFiles = new List<FileInfo>();
            if (!cache.DeleteFiles.Any(f => f.Name.Equals(file.Name))) cache.DeleteFiles.Add(file);
            if (cache.ModifiedFiles.Any(f => f.Name.Equals(file.Name)))
            {
                int index = cache.ModifiedFiles.FindIndex(f => f.Name.Equals(file.Name));
                cache.ModifiedFiles.RemoveAt(index);
            } 

            log.Messages.Add(new LogMessage(DateTime.Now, new List<string>() { file + " preparted for deleting by submit" }, username, "Data", "remove"));
            // update last modification time
            cache.UpdateLastModificarion(typeof(FileUploadHook));
            hookManager.Save(cache, log, "dataset", "details", HookMode.edit, id);

            return Json(true);
        }

        /// <summary>
        /// remove file from delete list
        /// </summary>
        /// <param name="id"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RevertFile(long id, FileInfo file)
        {
            // remove file from server
            string path = System.IO.Path.Combine(AppConfiguration.DataPath, "datasets", id.ToString(), "Temp", file.Name);
            if (System.IO.File.Exists(path)) System.IO.File.Delete(path);

            // remove file from cache
            HookManager hookManager = new HookManager();
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);
            EditDatasetDetailsLog log = hookManager.LoadLog<EditDatasetDetailsLog>("dataset", "details", HookMode.edit, id);
            var username = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);

            if (cache.DeleteFiles == null) cache.DeleteFiles = new List<FileInfo>();
            if (cache.DeleteFiles.Any(f => f.Name.Equals(file.Name)))
            {
                int index = cache.DeleteFiles.FindIndex(f => f.Name.Equals(file.Name));
                cache.DeleteFiles.RemoveAt(index);
            }

            // if a file will be removed from the deleteing list it should checked if the file is changed,
            // if yes it must be in the be added to the modified list
            if (isChanged(id, file.Name, file.Description))
            { 
                cache.ModifiedFiles.Add(file);
            }

            log.Messages.Add(new LogMessage(DateTime.Now, new List<string>() { file + "revert file from deleting by submit" }, username, "Data", "remove"));
            // update last modification time
            cache.UpdateLastModificarion(typeof(FileUploadHook));
            hookManager.Save(cache, log, "dataset", "details", HookMode.edit, id);

            return Json(true);
        }

        /// <summary>
        /// save file description of unstructured files
        /// </summary>
        /// <param name="id"></param>
        /// <param name="file"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult SaveFileDescription(long id, BExIS.UI.Hooks.Caches.FileInfo file)
        {
            // remove file from cache
            HookManager hookManager = new HookManager();
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);
            EditDatasetDetailsLog log = hookManager.LoadLog<EditDatasetDetailsLog>("dataset", "details", HookMode.edit, id);

            var username = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);


            if (cache.ModifiedFiles == null) cache.ModifiedFiles = new List<FileInfo>();

            if (cache.ModifiedFiles.Any(f => f.Name == file.Name))
            {
                // get file from modified list
                var f = cache.ModifiedFiles.Where(x => x.Name == file.Name).FirstOrDefault();
                if (f != null)
                {
                    // if description is the changed 
                    if (isChanged(id, file.Name, file.Description)) f.Description = file.Description; 
                    else cache.ModifiedFiles.Remove(f); // esle remove from modified list
                }
            }
            else
            {
                cache.ModifiedFiles.Add(file);
            }

            log.Messages.Add(new LogMessage(DateTime.Now, new List<string>() { file + " description updated" }, username, "Data", "save file description"));
            // update last modification time
            cache.UpdateLastModificarion(typeof(FileUploadHook));
            hookManager.SaveCache(cache, "dataset", "details", HookMode.edit, id);

            return Json(true);
        }



        private bool isChanged(long id,string name, string description)
        {
            if (string.IsNullOrEmpty(description)) description = "";

            using (var dataManager = new DatasetManager())
            {
                var datasetversion = dataManager.GetDatasetLatestVersion(id);
                string dynamicStorePath = System.IO.Path.Combine("Datasets", datasetversion.Dataset.Id.ToString(), name);
                // changeing the description can go to the default and this means the file should be removed from modifiations

                var contentdescriptor = datasetversion.ContentDescriptors.FirstOrDefault(c => c.URI.Equals(dynamicStorePath));
                if (contentdescriptor != null)
                {
                    if (description == contentdescriptor.Description) return false;

                }

                return true;
            }
        }
    }
}