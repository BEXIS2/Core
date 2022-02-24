﻿using BExIS.App.Bootstrap.Attributes;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.IO;
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

            FileUploader model = new FileUploader();

            # region settings

            var settings = ModuleManager.GetModuleSettings("dcm");

            // description
            var descrType = settings.GetEntryValue("attachmentDescription").ToString();//
            model.DescriptionType = (DescriptionType)Enum.Parse(typeof(DescriptionType), descrType);

            // max size
            model.MaxSize = Session.GetTenant().MaximumUploadSize; // need to load from tenant
            //multifileupload
            model.Multiple = Boolean.Parse(settings.GetEntryValue("allowMultiAttachmentUpload").ToString());

            #endregion

            model.Accept = UploadHelper.GetExtentionList(DataStructureType.None, this.Session.GetTenant());
            // load filelist from database and checlk against the folder
            using (DatasetManager dm = new DatasetManager())
            {
                // load specuific version
                var datasetVersion = dm.GetDatasetLatestVersion(id);
                if (datasetVersion != null)
                {
                    model.ExistingFiles = getDatasetFileList(datasetVersion);
                }
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult Upload(long id)
        {
            // load edit dataset cache
            HookManager hookManager = new HookManager();
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);

            List<string> filesNames = new List<string>();

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
            // load edit dataset cache
            HookManager hookManager = new HookManager();
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);

            using (var dm = new DatasetManager())
            {
                var username = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);

                var filePath = Path.Combine(AppConfiguration.DataPath, "Datasets", id.ToString(), "Attachments", file);
                FileHelper.Delete(filePath);
                var dataset = dm.GetDataset(id);
                var datasetVersion = dm.GetDatasetLatestVersion(dataset);
                var contentDescriptor = datasetVersion.ContentDescriptors.FirstOrDefault(item => item.Name == file);
                if (contentDescriptor == null)
                    throw new Exception("There is not any content descriptor having file name '" + file + "'. ");

                datasetVersion.ContentDescriptors.Remove(contentDescriptor);

                datasetVersion.ModificationInfo = new EntityAuditInfo()
                {
                    Performer = username,
                    Comment = "Attachment",
                    ActionType = AuditActionType.Delete
                };

                dm.EditDatasetVersion(datasetVersion, null, null, null);
                dm.CheckInDataset(dataset.Id, file, username, ViewCreationBehavior.None);

                var es = new EmailService();

                es.Send(MessageHelper.GetAttachmentDeleteHeader(id, typeof(Dataset).Name),
                                MessageHelper.GetAttachmentDeleteMessage(id, file, username),
                                GeneralSettings.SystemEmail
                                );

                // add message to the cache
                cache.Messages.Add(new ResultMessage(DateTime.Now, new List<string>() { file + " removed" }));
                hookManager.SaveCache(cache, "dataset", "details", HookMode.edit, id);
            }

            return Json(true);
        }

        [HttpPost]
        public JsonResult SaveFileDescription(long id, string file, string description)
        {
            HookManager hookManager = new HookManager();
            EditDatasetDetailsCache cache = hookManager.LoadCache<EditDatasetDetailsCache>("dataset", "details", HookMode.edit, id);

            using (var dm = new DatasetManager())
            {
                var username = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);

                var filePath = Path.Combine(AppConfiguration.DataPath, "Datasets", id.ToString(), "Attachments", file);
                var dataset = dm.GetDataset(id);
                var datasetVersion = dm.GetDatasetLatestVersion(dataset);
                var contentDescriptor = datasetVersion.ContentDescriptors.FirstOrDefault(item => item.Name == file);
                if (contentDescriptor == null)
                    throw new Exception("There is not any content descriptor having file name '" + file + "'. ");

                contentDescriptor.Description = description;

                dm.UpdateContentDescriptor(contentDescriptor);
            }

            cache.Messages.Add(new ResultMessage(DateTime.Now, new List<string>() { file + " description updated" }));
            hookManager.SaveCache(cache, "dataset", "details", HookMode.edit, id);

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
            var filemNames = "";
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
                            filemNames += fileName.ToString() + ",";
                            var dataPath = AppConfiguration.DataPath;
                            if (!Directory.Exists(Path.Combine(dataPath, "Datasets", datasetId.ToString(), "Attachments")))
                                Directory.CreateDirectory(Path.Combine(dataPath, "Datasets", datasetId.ToString(), "Attachments"));
                            var destinationPath = Path.Combine(dataPath, "Datasets", datasetId.ToString(), "Attachments", fileName);
                            file.SaveAs(destinationPath);
                            AddFileInContentDiscriptor(datasetVersion, fileName);
                        }

                        var es = new EmailService();
                        es.Send(MessageHelper.GetAttachmentUploadHeader(datasetId, typeof(Dataset).Name),
                        MessageHelper.GetAttachmentUploadMessage(datasetId, filemNames, user.DisplayName),
                        GeneralSettings.SystemEmail
                        );

                        //set modification
                        datasetVersion.ModificationInfo = new EntityAuditInfo()
                        {
                            Performer = user.Name,
                            Comment = "Attachment",
                            ActionType = AuditActionType.Create
                        };

                        dm.EditDatasetVersion(datasetVersion, null, null, null);
                        dm.CheckInDataset(dataset.Id, filemNames, user.Name, ViewCreationBehavior.None);
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
                DatasetVersion = datasetVersion,
                Description = ""
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