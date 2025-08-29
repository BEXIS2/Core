using BExIS.App.Bootstrap.Attributes;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.IO;
using BExIS.Modules.Dcm.UI.Hooks;
using BExIS.Modules.Dcm.UI.Models.Edit;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.UI.Hooks;
using BExIS.UI.Hooks.Caches;
using BExIS.UI.Hooks.Logs;
using BExIS.UI.Models;
using BExIS.Utils.Config;
using BExIS.Utils.Data.Upload;
using BExIS.Utils.Upload;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using Vaiona.Entities.Common;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Modularity;

using Cache = BExIS.UI.Hooks.Caches;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class AttachmentUploadController : Controller
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

            FileUploadModel model = new FileUploadModel();

            # region settings

            var settings = ModuleManager.GetModuleSettings("dcm");

            // description
            var descrType = settings.GetValueByKey("attachmentDescription").ToString();//
            model.FileUploader.DescriptionType = (DescriptionType)Enum.Parse(typeof(DescriptionType), descrType);

            // max size
            model.FileUploader.MaxSize = Session.GetTenant().MaximumUploadSize; // need to load from tenant
            //multifileupload
            model.FileUploader.Multiple = Boolean.Parse(settings.GetValueByKey("allowMultiAttachmentUpload").ToString());

            #endregion

            model.FileUploader.Accept = UploadHelper.GetExtentionList(DataStructureType.None, this.Session.GetTenant());
            // load filelist from database and checlk against the folder
            using (DatasetManager dm = new DatasetManager())
            {
                // load specuific version
                var datasetVersion = dm.GetDatasetLatestVersion(id);
                if (datasetVersion != null)
                {
                    model.FileUploader.ExistingFiles = getDatasetFileList(datasetVersion);
                }
            }

            HookManager hookManager = new HookManager();
            // load cache to check existing files
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);
            // set modification date
            model.LastModification = cache.GetLastModificarion(typeof(AttachmentEditHook));

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenOnPost]
        public JsonResult Upload(long id)
        {
            // load edit dataset cache
            HookManager hookManager = new HookManager();
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);
            EditDatasetDetailsLog logs = hookManager.LoadLog<EditDatasetDetailsLog>("dataset", "details", HookMode.edit, id);

            List<string> filesNames = new List<string>();
            var username = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);

            string folder = "Attachments"; // folder name inside dataset - temp or attachments

            if (Request.Files.Count > 0)
            {
                try
                {
                    //  Get all files from Request object and store it on the server

                    #region store on server

                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";
                        //string filename = Path.GetFileName(Request.Files[i].FileName);

                        HttpPostedFileBase file = files[i];
                        string fname = getFileName(file);
                        //data/datasets/1/1/
                        var dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");
                        var storepath = Path.Combine(dataPath, "Datasets", id.ToString(), folder);

                        // if folder not exist
                        if (!Directory.Exists(storepath))
                        {
                            Directory.CreateDirectory(storepath);
                        }

                        var path = Path.Combine(storepath, fname);

                        file.SaveAs(path);
                    }

                    #endregion store on server

                    // save it in to the database
                    saveAttachments(files, id);

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

                    logs.Messages.Add(new LogMessage(DateTime.Now, messages, username, "Attachment upload", "upload"));
                    hookManager.Save(cache, logs, "dataset", "details", HookMode.edit, id);
                }
            }
            else
            {
                return Json("No files selected.");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryTokenOnPost]
        public JsonResult RemoveFile(long id, BExIS.UI.Hooks.Caches.FileInfo file)
        {
            // load edit dataset cache
            HookManager hookManager = new HookManager();
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);
            EditDatasetDetailsLog log = hookManager.LoadLog<EditDatasetDetailsLog>("dataset", "details", HookMode.edit, id);

            using (var dm = new DatasetManager())
            {
                var username = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);

                var filePath = Path.Combine(AppConfiguration.DataPath, "Datasets", id.ToString(), "Attachments", file.Name);
                FileHelper.Delete(filePath);
                var dataset = dm.GetDataset(id);
      

                if (dm.IsDatasetCheckedOutFor(dataset.Id, username) || dm.CheckOutDataset(dataset.Id, username))
                {
                    var datasetVersion = dm.GetDatasetWorkingCopy(dataset.Id);

                    var contentDescriptor = datasetVersion.ContentDescriptors.FirstOrDefault(item => item.Name == file.Name);
                    if (contentDescriptor == null)
                        throw new Exception("There is not any content descriptor having file name '" + file + "'. ");

                    datasetVersion.ContentDescriptors.Remove(contentDescriptor);

                    datasetVersion.ModificationInfo = new EntityAuditInfo()
                    {
                        Performer = username,
                        Comment = "Attachment",
                        ActionType = AuditActionType.Delete,
                        Timestamp = DateTime.Now
                    };


                    // create attachment comment
                    // single case
                    string comment = "Attachment deleted (" + file.Name + ")";

                    dm.EditDatasetVersion(datasetVersion, null, null, null);
                    dm.CheckInDataset(dataset.Id, comment, username, ViewCreationBehavior.None);

                    using (var emailService = new EmailService())
                    {
                        emailService.Send(MessageHelper.GetAttachmentDeleteHeader(id, typeof(Dataset).Name),
                                        MessageHelper.GetAttachmentDeleteMessage(id, file.Name, username),
                                        GeneralSettings.SystemEmail
                                        );
                    }

                    // add message to the cache
                    log.Messages.Add(new LogMessage(DateTime.Now, new List<string>() { file + " removed" }, username, "Attachment upload", "remove"));

                    // update last modification time
                    cache.UpdateLastModificarion(typeof(AttachmentEditHook));

                    hookManager.Save(cache, log, "dataset", "details", HookMode.edit, id);
                }
            }

            return Json(true);
        }

        [HttpPost]
        [ValidateAntiForgeryTokenOnPost]
        public JsonResult SaveFileDescription(long id, BExIS.UI.Hooks.Caches.FileInfo file, string description)
        {
            HookManager hookManager = new HookManager();
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);
            EditDatasetDetailsLog log = hookManager.LoadLog<EditDatasetDetailsLog>("dataset", "details", HookMode.edit, id);

            using (var dm = new DatasetManager())
            {
                var filePath = Path.Combine(AppConfiguration.DataPath, "Datasets", id.ToString(), "Attachments", file.Name);
                var dataset = dm.GetDataset(id);
                var datasetVersion = dm.GetDatasetLatestVersion(dataset);
                var contentDescriptor = datasetVersion.ContentDescriptors.FirstOrDefault(item => item.Name == file.Name);
                if (contentDescriptor == null)
                    throw new Exception("There is not any content descriptor having file name '" + file + "'. ");

                contentDescriptor.Description = file.Description;

                dm.UpdateContentDescriptor(contentDescriptor);
            }

            var username = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);
            //new LogMessage(DateTime.Now, new List<string>() { file + " removed" }, username, "Attachment", "remove")
            log.Messages.Add(new LogMessage(DateTime.Now, new List<string>() { file + " description updated" }, username, "Attachment upload", "save description"));
            // update last modification time
            cache.UpdateLastModificarion(typeof(AttachmentEditHook));

            // save cache and logs
            hookManager.Save(cache, log, "dataset", "details", HookMode.edit, id);

            return Json(true);
        }

        #region helper

        /// <summary>
        /// get all files as attachments
        /// </summary>
        /// <returns>return a list with all names from FileStream in the folder</returns>
        private List<Cache.FileInfo> getDatasetFileList(DatasetVersion datasetVersion)
        {
            var fileList = new List<Cache.FileInfo>();
            foreach (var contentDescriptor in datasetVersion.ContentDescriptors.OrderBy(c => c.OrderNo))
            {
                var contentDescriptorName = contentDescriptor.Name;
                String filepath = Path.Combine(AppConfiguration.DataPath, "Datasets", contentDescriptor.DatasetVersion.Dataset.Id.ToString(), "Attachments", contentDescriptor.Name);

                if (System.IO.File.Exists(filepath))
                {
                    fileList.Add(new Cache.FileInfo(contentDescriptor.Name, contentDescriptor.MimeType, contentDescriptor.FileSize, contentDescriptor.Description));
                }
            }
            return fileList;
        }

        //[BExISEntityAuthorize(typeof(Dataset), "datasetId", RightType.Write)]
        public void saveAttachments(HttpFileCollectionBase attachments, long datasetId)
        {
            var fileNames = "";
            var user = BExISAuthorizeHelper.GetUserFromAuthorization(HttpContext);
            using (var dm = new DatasetManager())
            {
                var dataset = dm.GetDataset(datasetId);
                // var datasetVersion = dm.GetDatasetLatestVersion(dataset);

                DatasetVersion latestVersion = dm.GetDatasetLatestVersion(datasetId);
                string status = DatasetStateInfo.NotValid.ToString();
                if (latestVersion.StateInfo != null) status = latestVersion.StateInfo.State;

                if (dm.IsDatasetCheckedOutFor(datasetId, user.Name) || dm.CheckOutDataset(datasetId, user.Name))
                {
                    try
                    {
                        DatasetVersion datasetVersion = dm.GetDatasetWorkingCopy(datasetId);

                        //set StateInfo of the previus version
                        if (datasetVersion.StateInfo == null)
                        {
                            datasetVersion.StateInfo = new Vaiona.Entities.Common.EntityStateInfo()
                            {
                                State = status
                            };
                        }
                        else
                        {
                            datasetVersion.StateInfo.State = status;
                        }

                        for (int i = 0; i < attachments.Count; i++)
                        {
                            HttpPostedFileBase file = attachments[i];
                            var fileName = getFileName(file);
                            fileNames += fileName.ToString() + ",";
                            var dataPath = AppConfiguration.DataPath;
                            if (!Directory.Exists(Path.Combine(dataPath, "Datasets", datasetId.ToString(), "Attachments")))
                                Directory.CreateDirectory(Path.Combine(dataPath, "Datasets", datasetId.ToString(), "Attachments"));
                            var destinationPath = Path.Combine(dataPath, "Datasets", datasetId.ToString(), "Attachments", fileName);
                            file.SaveAs(destinationPath);
                            AddFileInContentDiscriptor(datasetVersion, fileName);
                        }

                        using (var emailService = new EmailService())
                        {
                            emailService.Send(MessageHelper.GetAttachmentUploadHeader(datasetId, typeof(Dataset).Name),
                                                        MessageHelper.GetAttachmentUploadMessage(datasetId, fileNames, user.DisplayName),
                                                        GeneralSettings.SystemEmail
                                                        );
                        }

                        // create attachment comment
                        // single case
                        string comment = "Attachment uploaded (" + fileNames+")";
                        // multiple case
                        if(attachments.Count>1)
                            comment = "Attachments uploaded (" + fileNames + ")";

                        //set modification
                        datasetVersion.ModificationInfo = new EntityAuditInfo()
                        {
                            Performer = user.Name,
                            Comment = "Attachment",
                            ActionType = AuditActionType.Create,
                            Timestamp = DateTime.Now
                        };

                        dm.EditDatasetVersion(datasetVersion, null, null, null);
                        dm.CheckInDataset(dataset.Id, comment, user.Name, ViewCreationBehavior.None);
                    }
                    catch (Exception ex)
                    {
                        dm.UndoCheckoutDataset(dataset.Id, user.Name);
                    }
                }
            }
        }

        private string AddFileInContentDiscriptor(DatasetVersion datasetVersion, String fileName)
        {
            string dataPath = AppConfiguration.DataPath;
            string storePath = Path.Combine(dataPath, "Datasets", datasetVersion.Dataset.Id.ToString(), "Attachments");
            int lastOrderContentDescriptor = 0;

            if (datasetVersion.ContentDescriptors.Any())
                lastOrderContentDescriptor = datasetVersion.ContentDescriptors.Max(cc => cc.OrderNo);
            ContentDescriptor originalDescriptor = new ContentDescriptor()
            {
                OrderNo = lastOrderContentDescriptor + 1,
                Name = fileName,
                MimeType = MimeMapping.GetMimeMapping(fileName),
                URI = Path.Combine("Datasets", datasetVersion.Dataset.Id.ToString(), "Attachments", fileName),
                DatasetVersion = datasetVersion
            };
            // replace the URI and description in case they have a same name
            if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals(originalDescriptor.Name)) > 0)
            {
                //
                foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                {
                    if (cd.Name == originalDescriptor.Name)
                    {
                        cd.URI = originalDescriptor.URI;
                        // cd.Extra = SetDescription(cd.Extra, description);
                        cd.Description = originalDescriptor.Description;
                    }
                }
            }
            else
            {
                // add file description Node
                //XmlDocument doc = SetDescription(originalDescriptor.Extra, description);
                originalDescriptor.Description = "";
                //Add current contentdesciptor to list
                datasetVersion.ContentDescriptors.Add(originalDescriptor);
            }

            return storePath;
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

        #endregion helper
    }
}