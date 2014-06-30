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
using BExIS.Io;
using BExIS.Web.Shell.Areas.DCM.Models.Push;
using Vaiona.Util.Cfg;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class PushController : Controller
    {
        //
        // GET: /DCM/Push/

        public ActionResult Index()
        {
            Session["Files"] = null;
            SendBigFilesToServerModel model = new SendBigFilesToServerModel();
            model.ServerFileList = GetServerFileList();

            return View(model);
        }

        public ActionResult Delete(string path)
        {

            FileHelper.Delete(path);

            SendBigFilesToServerModel model = new SendBigFilesToServerModel();
            model.ServerFileList = GetServerFileList();

            return View("Index", model);
        }

        public ActionResult Reload()
        {

            SendBigFilesToServerModel model = new SendBigFilesToServerModel();
            model.ServerFileList = GetServerFileList();

            return View("_fileListView", model.ServerFileList);
        }

        public ActionResult Remove()
        {


            return Content("");
        }

        [HttpPost]
        public ActionResult ProcessSubmit(IEnumerable<HttpPostedFileBase> attachments)
        {
            // The Name of the Upload component is "attachments"                            
            if (attachments != null)
            {
                Session["FileInfos"] = attachments;
                Thread t = new Thread(() => uploadFiles((IEnumerable<HttpPostedFileBase>)Session["FileInfos"]));
                
                try
                {
                   
                    t.Start();
                 
                }
                catch (Exception e)
                {

                    //Send mail
                    SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
                    {
                        Credentials = new NetworkCredential("david.blaa@googlemail.com", "since2094"),
                        EnableSsl = true
                    };

                    string email = "david.blaa@googlemail.com";
                    MailMessage msg = new MailMessage();
                    msg.From = new MailAddress("david.blaa@googlemail.com");

                    msg.Subject = "Error";
                    msg.Body = e.Message;

                    msg.To.Add(email);
                    client.Send(msg);

                    Debug.WriteLine(e.Message);
                    
                }

            }

            SendBigFilesToServerModel model = new SendBigFilesToServerModel();
            model.ServerFileList = GetServerFileList();
            // Redirect to a view showing the result of the form submission.
            return View("Index", model);
        }

        /// <summary>
        /// read filenames from datapath/Temp/Username
        /// </summary>
        /// <returns>return a list with all names from file in the folder</returns>
        private List<BasicFileInfo> GetServerFileList()
        {
            List<BasicFileInfo> fileList = new List<BasicFileInfo>();
  
            string userDataPath = Path.Combine(AppConfiguration.DataPath, "Temp", GetUserNameOrDefault());

            // if folder not exist
            if (!Directory.Exists(userDataPath))
            {
                Directory.CreateDirectory(userDataPath);
            }


            DirectoryInfo dirInfo = new DirectoryInfo(userDataPath);


            foreach(FileInfo info in  dirInfo.GetFiles())
            {
                fileList.Add(new BasicFileInfo(info.FullName, ""));
            }

            return fileList;

        }

        #region helper
            // chekc if user exist
            // if true return usernamem otherwise "DEFAULT"
            private string GetUserNameOrDefault()
            {
                string userName = string.Empty;
                try
                {
                    userName = HttpContext.User.Identity.Name;
                }
                catch { }

                return !string.IsNullOrWhiteSpace(userName) ? userName : "DEFAULT";
            }


            public void uploadFiles(IEnumerable<HttpPostedFileBase> attachments)
            {
                string filemNames = "";

                if (Session["FileInfos"] != null)
                {
                    attachments = (IEnumerable<HttpPostedFileBase>)Session["FileInfos"];
                }

                Debug.WriteLine("save starts");

                foreach (var file in attachments)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    filemNames += fileName.ToString()+",";

                    string dataPath = AppConfiguration.DataPath;
                    var destinationPath = Path.Combine(dataPath, "Temp", GetUserNameOrDefault(), fileName);

                    file.SaveAs(destinationPath);
                }

                //Send mail
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("david.blaa@googlemail.com", "since2094"),
                    EnableSsl = true
                };

                string email = "david.blaa@googlemail.com";
                MailMessage msg = new MailMessage();
                msg.From = new MailAddress("david.blaa@googlemail.com");

                msg.Subject = "Push to server complete";
                msg.Body = filemNames + "  --> uploaded";

                msg.To.Add(email);
                client.Send(msg);
                
                Debug.WriteLine("save complete");
            }
        #endregion
        
    }
}
