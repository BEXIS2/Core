﻿using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.IO;
using BExIS.Modules.Dcm.UI.Models.Attachments;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Vaiona.Entities.Common;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class AttachmentsController : Controller
    {
        // GET: Attachments
        public ActionResult Index()
        {
            return View();
        }

        [BExISEntityAuthorize(typeof(Dataset), "datasetId", RightType.Read)]
        public ActionResult DatasetAttachements(long datasetId, long versionId)
        {
            ViewBag.datasetId = datasetId;
            ViewBag.versionId = versionId;
            return PartialView("_datasetAttachements", LoadDatasetModel(versionId));
        }

        [BExISEntityAuthorize(typeof(Dataset), "datasetId", RightType.Read)]
        public ActionResult Download(long datasetId, String fileName)
        {
            var filePath = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetId.ToString(), "Attachments", fileName);
            return File(filePath, MimeMapping.GetMimeMapping(fileName), Path.GetFileName(filePath));
        }

        [BExISEntityAuthorize(typeof(Dataset), "datasetId", RightType.Write)]
        public ActionResult Delete(long datasetId, String fileName)
        {
            var filePath = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetId.ToString(), "Attachments", fileName);
            FileHelper.Delete(filePath);
            var dm = new DatasetManager();
            var dataset = dm.GetDataset(datasetId);
            var datasetVersion = dm.GetDatasetLatestVersion(dataset);
            var contentDescriptor = datasetVersion.ContentDescriptors.FirstOrDefault(item => item.Name == fileName);
            if (contentDescriptor == null)
                throw new Exception("There is not any content descriptor having file name '" + fileName + "'. ");

            datasetVersion.ContentDescriptors.Remove(contentDescriptor);

            datasetVersion.ModificationInfo = new EntityAuditInfo()
            {
                Performer = GetUsernameOrDefault(),
                Comment = "Attachment",
                ActionType = AuditActionType.Delete
            };

            dm.EditDatasetVersion(datasetVersion, null, null, null);
            dm.CheckInDataset(dataset.Id, fileName, GetUsernameOrDefault(), ViewCreationBehavior.None);

            dm?.Dispose();
            return RedirectToAction("showdata", "data", new { area = "ddm", id = datasetId });
        }

        private DatasetFilesModel LoadDatasetModel(long versionId)
        {
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
            EntityManager entityManager = new EntityManager();
            UserManager userManager = new UserManager();
            var dm = new DatasetManager();
            var datasetVersion = dm.GetDatasetVersion(versionId);
            var model = new DatasetFilesModel
            {
                ServerFileList = GetDatasetFileList(datasetVersion),
                FileSize = this.Session.GetTenant().MaximumUploadSize
            };

            //Parse user right

            var entity = entityManager.EntityRepository.Query(e => e.Name.ToUpperInvariant() == "Dataset".ToUpperInvariant() && e.EntityType == typeof(Dataset)).FirstOrDefault();

            var userTask = userManager.FindByNameAsync(HttpContext.User.Identity.Name);
            userTask.Wait();
            var user = userTask.Result;
            int rights = 0;
            if (user == null)
                rights = entityPermissionManager.GetEffectiveRights(null, entity.Id, datasetVersion.Dataset.Id);
            else
                rights = entityPermissionManager.GetEffectiveRights(user.Id, entity.Id, datasetVersion.Dataset.Id);
            model.UploadAccess = (((rights & (int)RightType.Write) > 0) || ((rights & (int)RightType.Grant) > 0));
            model.DeleteAccess = (((rights & (int)RightType.Delete) > 0) || ((rights & (int)RightType.Grant) > 0));
            model.DownloadAccess = ((rights & (int)RightType.Read) > 0 || ((rights & (int)RightType.Grant) > 0));
            model.ViewAccess = ((rights & (int)RightType.Read) > 0 || ((rights & (int)RightType.Grant) > 0));
            userManager?.Dispose();
            entityPermissionManager?.Dispose();
            entityManager?.Dispose();
            dm?.Dispose();
            return model;
        }

        /// <summary>
        /// read filenames from datapath/Datasets/id
        /// </summary>
        /// <returns>return a list with all names from FileStream in the folder</returns>
        private Dictionary<BasicFileInfo, String> GetDatasetFileList(DatasetVersion datasetVersion)
        {
            var fileList = new Dictionary<BasicFileInfo, String>();
            foreach (var contentDescriptor in datasetVersion.ContentDescriptors.OrderBy(c => c.OrderNo))
            {
                var contentDescriptorName = contentDescriptor.Name;
                long fileLength = 0;
                String filepath = Path.Combine(AppConfiguration.DataPath, "Datasets", contentDescriptor.DatasetVersion.Dataset.Id.ToString(), "Attachments", contentDescriptor.Name);
                if (new FileInfo(filepath).Exists)
                {  // if (System.IO.File.Exists(contentDescriptor.URI))
                    //TODO: In case a file is deleted physically from dataset folder, user should be inform maybe
                    //    //contentDescriptorName += "<span id='deletedFile' style='color:#980000;padding-left:5px;' >[deleted]</span>";
                    //    contentDescriptor.URI = "delete";
                    //else
                    fileLength = new FileInfo(filepath).Length;
                    fileList.Add(new BasicFileInfo(contentDescriptorName, contentDescriptor.URI, contentDescriptor.MimeType, "", fileLength), GetDescription(contentDescriptor.Extra));
                }
            }
            return fileList;
        }

        [HttpPost]
        [BExISEntityAuthorize(typeof(Dataset), "datasetId", RightType.Write)]
        public ActionResult ProcessSubmit(IEnumerable<HttpPostedFileBase> attachments, long datasetId, String description)
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Attach file to dataset", this.Session.GetTenant());
            // The Name of the Upload component is "attachments"
            if (attachments != null)
            {
                Session["FileInfos"] = attachments;
                uploadFiles(attachments, datasetId, description);
            }
            // Redirect to a view showing the result of the form submission.
            return RedirectToAction("showdata", "data", new { area = "ddm", id = datasetId });
        }

        [BExISEntityAuthorize(typeof(Dataset), "datasetId", RightType.Write)]
        public void uploadFiles(IEnumerable<HttpPostedFileBase> attachments, long datasetId, String description)
        {
            var filemNames = "";
            var dm = new DatasetManager();
            var dataset = dm.GetDataset(datasetId);
            // var datasetVersion = dm.GetDatasetLatestVersion(dataset);

            DatasetVersion latestVersion = dm.GetDatasetLatestVersion(datasetId);
            string status = DatasetStateInfo.NotValid.ToString();
            if (latestVersion.StateInfo != null) status = latestVersion.StateInfo.State;

            if (dm.IsDatasetCheckedOutFor(datasetId, GetUsernameOrDefault()) || dm.CheckOutDataset(datasetId, GetUsernameOrDefault()))
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


                foreach (var file in attachments)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    filemNames += fileName.ToString() + ",";
                    var dataPath = AppConfiguration.DataPath;
                    if (!Directory.Exists(Path.Combine(dataPath, "Datasets", datasetId.ToString(), "Attachments")))
                        Directory.CreateDirectory(Path.Combine(dataPath, "Datasets", datasetId.ToString(), "Attachments"));
                    var destinationPath = Path.Combine(dataPath, "Datasets", datasetId.ToString(), "Attachments", fileName);
                    file.SaveAs(destinationPath);
                    AddFileInContentDiscriptor(datasetVersion, fileName, description);
                }

                //set modification
                datasetVersion.ModificationInfo = new EntityAuditInfo()
                {
                    Performer = GetUsernameOrDefault(),
                    Comment = "Attachment",
                    ActionType = AuditActionType.Create
                };

                string filenameList = string.Join(", ", attachments.Select(f => f.FileName).ToArray());

                dm.EditDatasetVersion(datasetVersion, null, null, null);
                dm.CheckInDataset(dataset.Id, filenameList, GetUsernameOrDefault(), ViewCreationBehavior.None);
            }
            dm?.Dispose();
        }

        private string AddFileInContentDiscriptor(DatasetVersion datasetVersion, String fileName, String description)
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
                        cd.Extra = SetDescription(cd.Extra, description);
                    }
                }
            }
            else
            {
                // add file description Node
                XmlDocument doc = SetDescription(originalDescriptor.Extra, description);
                originalDescriptor.Extra = doc;
                //Add current contentdesciptor to list
                datasetVersion.ContentDescriptors.Add(originalDescriptor);
            }

            return storePath;
        }

        private XmlDocument SetDescription(XmlNode extraField, string description)
        {
            XmlNode newExtra;
            var source = (XmlDocument)extraField;
            if (source == null)
            {
                source = new XmlDocument();
                source.LoadXml("<extra><fileDescription>" + description + "</fileDescription></extra>");
            }
            else
            {
                if (XmlUtility.GetXmlNodeByName(extraField, "fileDescription") == null)
                {
                    XmlNode t = XmlUtility.CreateNode("fileDescription", source);
                    t.InnerText = description;
                    source.DocumentElement.AppendChild(t);
                }
                else
                {
                    var descNodes = source.SelectNodes("/extra/fileDescription");
                    descNodes[0].InnerText = description;
                }
            }
            return source;
        }

        private string GetDescription(XmlNode extra)
        {
            if ((XmlDocument)extra != null)
            {
                var descNode = extra.SelectNodes("/extra/fileDescription");
                if (descNode != null)
                {
                    return descNode[0].InnerText;
                }
            }
            return "";
        }

        public string GetUsernameOrDefault()
        {
            string username = string.Empty;
            try
            {
                username = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(username) ? username : "DEFAULT";
        }
    }
}