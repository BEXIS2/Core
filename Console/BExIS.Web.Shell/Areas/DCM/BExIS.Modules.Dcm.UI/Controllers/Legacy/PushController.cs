using BExIS.IO;
using BExIS.Modules.Dcm.UI.Models.Push;
using BExIS.Utils.Data.Upload;
using BExIS.Utils.Upload;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class PushController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Push Big File", this.Session.GetTenant());

            Session["Files"] = null;

            // get max file name length
            // get max file lenght
            var dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");
            var storepath = Path.Combine(dataPath, "Temp", GetUsernameOrDefault());

            ViewBag.maxFileNameLength = 260 - storepath.Length - 2;

            return View(LoadDefaultModel());
        }

        public ActionResult Delete(string path)
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Push Big File", this.Session.GetTenant());
            FileHelper.Delete(path);

            return View("Index", LoadDefaultModel());
        }

        public ActionResult Reload()
        {
            return View("_fileListView", GetServerFileList());
        }

        public ActionResult Remove()
        {
            return Content("");
        }

        [HttpPost]
        public ActionResult ProcessSubmit(IEnumerable<HttpPostedFileBase> attachments)
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Push Big File", this.Session.GetTenant());
            // The Name of the Upload component is "attachments"
            if (attachments != null)
            {
                Session["FileInfos"] = attachments;
                uploadFiles(attachments);
            }

            var dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");
            var storepath = Path.Combine(dataPath, "Temp", GetUsernameOrDefault());

            ViewBag.maxFileNameLength = 260 - storepath.Length - 2;
            // Redirect to a view showing the result of the form submission.
            return View("Index", LoadDefaultModel());
        }

        /// <summary>
        /// read filenames from datapath/Temp/UserName
        /// </summary>
        /// <returns>return a list with all names from FileStream in the folder</returns>
        private List<BasicFileInfo> GetServerFileList()
        {
            var fileList = new List<BasicFileInfo>();

            var userDataPath = Path.Combine(AppConfiguration.DataPath, "Temp", GetUsernameOrDefault());

            // if folder not exist
            if (!Directory.Exists(userDataPath))
            {
                Directory.CreateDirectory(userDataPath);
            }

            var dirInfo = new DirectoryInfo(userDataPath);

            foreach (var info in dirInfo.GetFiles())
            {
                fileList.Add(new BasicFileInfo(info.Name, info.FullName, "", info.Extension, info.Length));
            }

            return fileList;
        }

        #region helper

        // chekc if user exist
        // if true return usernamem otherwise "DEFAULT"
        private string GetUsernameOrDefault()
        {
            var username = string.Empty;
            try
            {
                username = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(username) ? username : "DEFAULT";
        }

        public void uploadFiles(IEnumerable<HttpPostedFileBase> attachments)
        {
            var filemNames = "";

            Debug.WriteLine("save starts");

            foreach (var file in attachments)
            {
                var fileName = Path.GetFileName(file.FileName);
                filemNames += fileName.ToString() + ",";

                var dataPath = AppConfiguration.DataPath;
                var destinationPath = Path.Combine(dataPath, "Temp", GetUsernameOrDefault(), fileName);

                Debug.WriteLine("contentlength :" + file.ContentLength);

                file.SaveAs(destinationPath);
            }
        }

        private SendBigFilesToServerModel LoadDefaultModel()
        {
            var model = new SendBigFilesToServerModel
            {
                ServerFileList = GetServerFileList(),
                SupportedFileExtentions = UploadHelper.GetExtentionList(DataStructureType.Unstructured, this.Session.GetTenant()),
                // FileSize = this.Session.GetTenant().MaximumUploadSize
            };

            return model;
        }

        #endregion helper
    }
}