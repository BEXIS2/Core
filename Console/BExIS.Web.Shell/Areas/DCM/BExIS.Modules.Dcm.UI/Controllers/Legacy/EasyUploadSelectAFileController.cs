using BExIS.Dcm.UploadWizard;
using BExIS.Dcm.Wizard;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class EasyUploadSelectAFileController : Controller
    {

        private EasyUploadTaskManager TaskManager;
        private List<String> supportedExtensions = new List<string>() { ".xlsx", ".xlsm" };

        [HttpGet]
        public ActionResult SelectAFile(int index)
        {
            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

            //set current stepinfo based on index
            if (TaskManager != null)
                TaskManager.SetCurrent(index);

            //Get Bus infomations
            SelectFileViewModel model = new SelectFileViewModel();
            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.FILENAME))
            {
                model.SelectedFileName = TaskManager.Bus[EasyUploadTaskManager.FILENAME].ToString();
            }

            //get datastuctureType
            model.SupportedFileExtentions = supportedExtensions;

            //Get StepInfo
            model.StepInfo = TaskManager.Current();

            model.serverFileList = GetServerFileList();

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult SelectAFile(object[] data)
        {

            SelectFileViewModel model = new SelectFileViewModel();

            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

            if (data != null) TaskManager.AddToBus(data);

            model.StepInfo = TaskManager.Current();

            TaskManager.Current().SetValid(false);

            if (TaskManager != null)
            {
                // is path of FileStream exist
                if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.FILEPATH))
                {
                    if (IsSupportedExtention(TaskManager))
                    {
                        try
                        {
                            string filePath = TaskManager.Bus[EasyUploadTaskManager.FILEPATH].ToString();

                            //TaskManager.AddToBus(EasyUploadTaskManager.IS_TEMPLATE, "false");

                            TaskManager.Current().SetValid(true);

                        }
                        catch
                        {
                            model.ErrorList.Add(new Error(ErrorType.Other, "Cannot access FileStream on server."));
                        }
                    }
                    else
                    {
                        model.ErrorList.Add(new Error(ErrorType.Other, "File is not supported."));
                    }


                }
                else
                {
                    model.ErrorList.Add(new Error(ErrorType.Other, "No FileStream selected or submitted."));
                }

                if (TaskManager.Current().IsValid())
                {
                    TaskManager.AddExecutedStep(TaskManager.Current());
                    TaskManager.GoToNext();
                    Session["TaskManager"] = TaskManager;
                    ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                    return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
                }
            }

            model.serverFileList = GetServerFileList();
            model.SupportedFileExtentions = supportedExtensions;

            return PartialView(model);
        }

        #region private methods

        /// <summary>
        /// Selected File store din the BUS
        /// </summary>
        /// <param name="SelectFileUploader"></param>
        /// <returns></returns>
        public ActionResult SelectFileProcess(HttpPostedFileBase SelectFileUploader)
        {
            EasyUploadTaskManager TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

            #region Remove selected information from the bus
            TaskManager.Bus.Remove(EasyUploadTaskManager.SHEET_JSON_DATA);
            TaskManager.Bus.Remove(EasyUploadTaskManager.SHEET_HEADER_AREA);
            TaskManager.Bus.Remove(EasyUploadTaskManager.SHEET_DATA_AREA);
            TaskManager.Bus.Remove(EasyUploadTaskManager.SHEET_FORMAT);
            TaskManager.Bus.Remove(EasyUploadTaskManager.VERIFICATION_AVAILABLEUNITS);
            TaskManager.Bus.Remove(EasyUploadTaskManager.VERIFICATION_HEADERFIELDS);
            TaskManager.Bus.Remove(EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS);
            TaskManager.Bus.Remove(EasyUploadTaskManager.VERIFICATION_ATTRIBUTESUGGESTIONS);
            #endregion

            if (SelectFileUploader != null)
            {
                //data/datasets/1/1/
                string dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");
                string storepath = Path.Combine(dataPath, "Temp", GetUsernameOrDefault());

                // if folder not exist
                if (!Directory.Exists(storepath))
                {
                    Directory.CreateDirectory(storepath);
                }

                string path = Path.Combine(storepath, SelectFileUploader.FileName);

                SelectFileUploader.SaveAs(path);
                TaskManager.AddToBus(EasyUploadTaskManager.FILEPATH, path);

                TaskManager.AddToBus(EasyUploadTaskManager.FILENAME, SelectFileUploader.FileName);
                TaskManager.AddToBus(EasyUploadTaskManager.EXTENTION, SelectFileUploader.FileName.Split('.').Last());
                Session["TaskManager"] = TaskManager;
            }

            //return RedirectToAction("UploadWizard");
            return Content("");
        }

        /// <summary>
        /// Selected File from server and store into BUS
        /// </summary>
        /// <param name="SelectFileUploader"></param>
        /// <returns></returns>
        public ActionResult SelectFileFromServerProcess(string fileName)
        {
            EasyUploadTaskManager TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

            #region Remove selected information from the bus
            TaskManager.Bus.Remove(EasyUploadTaskManager.SHEET_JSON_DATA);
            TaskManager.Bus.Remove(EasyUploadTaskManager.SHEET_HEADER_AREA);
            TaskManager.Bus.Remove(EasyUploadTaskManager.SHEET_DATA_AREA);
            TaskManager.Bus.Remove(EasyUploadTaskManager.SHEET_FORMAT);
            TaskManager.Bus.Remove(EasyUploadTaskManager.VERIFICATION_AVAILABLEUNITS);
            TaskManager.Bus.Remove(EasyUploadTaskManager.VERIFICATION_HEADERFIELDS);
            TaskManager.Bus.Remove(EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS);
            TaskManager.Bus.Remove(EasyUploadTaskManager.VERIFICATION_ATTRIBUTESUGGESTIONS);
            #endregion

            if (fileName != null)
            {
                //string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "ServerFiles",fileName);

                //data/datasets/1/1/
                string dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");
                string path = Path.Combine(dataPath, "Temp", GetUsernameOrDefault(), fileName);

                TaskManager.AddToBus(EasyUploadTaskManager.FILEPATH, path);

                TaskManager.AddToBus(EasyUploadTaskManager.FILENAME, fileName);
                TaskManager.AddToBus(EasyUploadTaskManager.EXTENTION, "." + fileName.Split('.').Last());
                Session["TaskManager"] = TaskManager;
            }

            //return RedirectToAction("UploadWizard");
            return Content("");
        }


        /// <summary>
        /// read filenames from datapath/Temp/UserName
        /// </summary>
        /// <returns>return a list with all names from FileStream in the folder</returns>
        private List<String> GetServerFileList()
        {

            string userDataPath = Path.Combine(AppConfiguration.DataPath, "Temp", GetUsernameOrDefault());

            // if folder not exist
            if (!Directory.Exists(userDataPath))
            {
                Directory.CreateDirectory(userDataPath);
            }


            DirectoryInfo dirInfo = new DirectoryInfo(userDataPath);
            return dirInfo.GetFiles().Select(i => i.Name).ToList();

        }

        // chekc if user exist
        // if true return usernamem otherwise "DEFAULT"
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

        /// <summary>
        /// returns true if Extention in the Bus will supported
        /// </summary>
        /// <param name="taskManager"></param>
        /// <returns></returns>
        private bool IsSupportedExtention(EasyUploadTaskManager taskManager)
        {
            if (taskManager.Bus.ContainsKey(EasyUploadTaskManager.EXTENTION))
            {
                string ext = taskManager.Bus[EasyUploadTaskManager.EXTENTION].ToString();

                if (supportedExtensions.Contains(ext.ToLower())) return true;

            }

            return false;
        }


        #endregion
    }
}
