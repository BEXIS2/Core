using BExIS.Dcm.UploadWizard;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using BExIS.Modules.Dcm.UI.Models.Push;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
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

       
        public ActionResult DatasetAttachements(long datasetId)
        {
            ViewBag.datasetId = datasetId;
            return PartialView("_datasetAttachements", LoadDatasetModel(datasetId));
        }

        public ActionResult Download(long datasetId,String fileName)
        {
            var filePath = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetId.ToString(), "Attachments", fileName);
            return File(filePath, MimeMapping.GetMimeMapping(fileName), Path.GetFileName(filePath));
        }

        public ActionResult Delete(long datasetId, String fileName)
        {
            var filePath = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetId.ToString(), "Attachments", fileName);
            FileHelper.Delete(filePath);
            //TODO: What about descriptors 
            return RedirectToAction("showdata", "data", new { area = "ddm", id = datasetId });
        }

        private SendBigFilesToServerModel LoadDatasetModel(long datasetId)
        {
            var model = new SendBigFilesToServerModel
            {
                ServerFileList = GetDatasetFileList(datasetId),
                SupportedFileExtentions = new List<string>()
                    {
                        ".pdf",
                        ".txt",
                        ".csv" }
                    ,
                FileSize = this.Session.GetTenant().MaximumUploadSize
            };

            return model;
        }

        /// <summary>
        /// read filenames from datapath/Datasets/id
        /// </summary>
        /// <returns>return a list with all names from FileStream in the folder</returns>
        private List<BasicFileInfo> GetDatasetFileList(long datasetId)
        {
            var fileList = new List<BasicFileInfo>();
            var datasetDataPath = Path.Combine(AppConfiguration.DataPath, "Datasets", datasetId.ToString(), "Attachments");

            // if folder not exist
            if (!Directory.Exists(datasetDataPath))
            {
                Directory.CreateDirectory(datasetDataPath);
            }


            var dirInfo = new DirectoryInfo(datasetDataPath);


            foreach (var info in dirInfo.GetFiles())
            {
                fileList.Add(new BasicFileInfo(info.Name, info.FullName, "", info.Extension, info.Length));
            }

            return fileList;

        }

        [HttpPost]
        public ActionResult ProcessSubmit(IEnumerable<HttpPostedFileBase> attachments,long datasetId,String description)
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Attach file to dataset", this.Session.GetTenant());
            // The Name of the Upload component is "attachments"                            
            if (attachments != null)
            {
                Session["FileInfos"] = attachments;
                uploadFiles(attachments,datasetId,description);
            }
            // Redirect to a view showing the result of the form submission.
            return RedirectToAction("showdata", "data",new {area="ddm", id = datasetId });
        }

        public void uploadFiles(IEnumerable<HttpPostedFileBase> attachments, long datasetId, String description)
        {
            var filemNames = "";
            var dm = new DatasetManager();
            var dataset = dm.GetDataset(datasetId);
            var datasetVersion = dm.GetDatasetLatestVersion(dataset);
            foreach (var file in attachments)
            {
                var fileName = Path.GetFileName(file.FileName);
                filemNames += fileName.ToString() + ",";
                var dataPath = AppConfiguration.DataPath;
                if (!Directory.Exists(Path.Combine(dataPath, "Datasets", datasetId.ToString(), "Attachments")))
                    Directory.CreateDirectory(Path.Combine(dataPath, "Datasets", datasetId.ToString(), "Attachments"));
                var destinationPath = Path.Combine(dataPath, "Datasets", datasetId.ToString(), "Attachments", fileName);                
                file.SaveAs(destinationPath);
                AddFileInContentDiscriptor(datasetVersion, fileName,description);
            }
            //TODO: what about this?
            dm.CheckOutDataset(dataset.Id, GetUsernameOrDefault());
            dm.EditDatasetVersion(datasetVersion, null, null, null);
            dm.CheckInDataset(dataset.Id, "upload dataset attachements", GetUsernameOrDefault(), ViewCreationBehavior.None);
        }

        private string AddFileInContentDiscriptor(DatasetVersion datasetVersion,String fileName,String description)
        {
            
            string dataPath = AppConfiguration.DataPath; 
            string storePath = Path.Combine(dataPath, "Datasets", datasetVersion.Dataset.Id.ToString(), "Attachments");
            int lastOrderContentDescriptor = 0;
            if(datasetVersion.ContentDescriptors.Any())
                datasetVersion.ContentDescriptors.Max(cc => cc.OrderNo);            
            ContentDescriptor originalDescriptor = new ContentDescriptor()
            {
                OrderNo = lastOrderContentDescriptor+1,
                Name =fileName,
                MimeType = MimeMapping.GetMimeMapping(fileName),
                URI = Path.Combine(storePath, fileName),
                DatasetVersion = datasetVersion
            };
            //TODO: What about different versions?
            if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals(originalDescriptor.Name)) > 0)
            {   // remove the one contentdesciptor 
                foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                {
                    if (cd.Name == originalDescriptor.Name)
                    {
                        cd.URI = originalDescriptor.URI;
                    }
                }
            }
            else
            {
                // add current contentdesciptor to list
                datasetVersion.ContentDescriptors.Add(originalDescriptor);
            }

            return storePath;
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