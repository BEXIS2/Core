using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using BExIS.Dcm.UploadWizard;
using BExIS.IO;
using BExIS.Web.Shell.Areas.DCM.Models.Push;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class PushController : Controller
    {
        //
        // GET: /DCM/Push/

        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Push Big File", this.Session.GetTenant());

            Session["Files"] = null;
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
            // Redirect to a view showing the result of the form submission.
            return View("Index", LoadDefaultModel());
        }

        /// <summary>
        /// read filenames from datapath/Temp/Username
        /// </summary>
        /// <returns>return a list with all names from FileStream in the folder</returns>
        private List<BasicFileInfo> GetServerFileList()
        {
            List<BasicFileInfo> fileList = new List<BasicFileInfo>();
  
            string userDataPath = Path.Combine(AppConfiguration.DataPath, "Temp", GetUsernameOrDefault());

            // if folder not exist
            if (!Directory.Exists(userDataPath))
            {
                Directory.CreateDirectory(userDataPath);
            }


            DirectoryInfo dirInfo = new DirectoryInfo(userDataPath);


            foreach(FileInfo info in  dirInfo.GetFiles())
            {
                fileList.Add(new BasicFileInfo(info.FullName, "","", info.Extension,info.Length));
            }

            return fileList;

        }

        #region helper
            // chekc if user exist
            // if true return usernamem otherwise "DEFAULT"
            private string GetUsernameOrDefault()
            {
                string username = string.Empty;
                try
                {
                    username = HttpContext.User.Identity.Name;
                }
                catch { }

                return !string.IsNullOrWhiteSpace(username) ? username : "DEFAULT";
            }

            public void uploadFiles(IEnumerable<HttpPostedFileBase> attachments)
            {
                string filemNames = "";

                Debug.WriteLine("save starts");

                foreach (var file in attachments)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    filemNames += fileName.ToString()+",";

                    string dataPath = AppConfiguration.DataPath;
                    var destinationPath = Path.Combine(dataPath, "Temp", GetUsernameOrDefault(), fileName);

                    Debug.WriteLine("contentlength :" + file.ContentLength);

                    file.SaveAs(destinationPath);
                }

            }

            private SendBigFilesToServerModel LoadDefaultModel()
            {
                SendBigFilesToServerModel model = new SendBigFilesToServerModel();
                model.ServerFileList = GetServerFileList();
                model.SupportedFileExtentions = UploadWizardHelper.GetExtentionList(DataStructureType.Unstructured);

                return model;
            }
        #endregion
        
    }
}
