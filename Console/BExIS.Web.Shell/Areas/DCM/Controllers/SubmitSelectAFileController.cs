using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using BExIS.Io.Transform.Input;
using BExIS.Io.Transform.Validation.Exceptions;
using BExIS.Dcm.UploadWizard;
using BExIS.Dcm.Wizard;
using BExIS.Web.Shell.Areas.DCM.Models;
using Vaiona.Util.Cfg;
using BExIS.Io;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class SubmitSelectAFileController : Controller
    {

        private TaskManager TaskManager;
        private FileStream Stream;

        //
        // GET: /DCM/Step1/

        [HttpGet]
        public ActionResult SelectAFile(int index)
        {
            TaskManager = (TaskManager)Session["TaskManager"];

            //set current stepinfo based on index
            if (TaskManager != null)
                TaskManager.SetCurrent(index);

            //Get Bus infomations
            SelectFileViewModel model = new SelectFileViewModel();
            if (TaskManager.Bus.ContainsKey(TaskManager.FILENAME))
            {
                model.SelectedFileName = TaskManager.Bus[TaskManager.FILENAME].ToString();
            }

            //get datastuctureType
            model.DataStructureType = GetDataStructureType();
            model.SupportedFileExtentions = UploadWizardHelper.GetExtentionList(model.DataStructureType);

            //Get StepInfo
            model.StepInfo = TaskManager.Current();

            model.serverFileList = GetServerFileList();



            return PartialView(model);
        }

        [HttpPost]
        public ActionResult SelectAFile(object[] data)
        {

            SelectFileViewModel model = new SelectFileViewModel();

            TaskManager = (TaskManager)Session["TaskManager"];

            if (data != null) TaskManager.AddToBus(data);

            model.StepInfo = TaskManager.Current();

            TaskManager.Current().SetValid(false);

            if (TaskManager != null)
            {
                // is path of file exist
                if (TaskManager.Bus.ContainsKey(TaskManager.FILEPATH))
                {
                    if (IsSupportedExtention(TaskManager))
                    {
                        try
                        {
                            if (GetDataStructureType().Equals(DataStructureType.Structured))
                            {
                                #region structured datastructure
                                    //try save file
                                    string filePath = TaskManager.Bus[TaskManager.FILEPATH].ToString();
                                    //if extention like a makro excel file
                                    if (TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".xlsm"))
                                    {
                                        // open file
                                        ExcelReader reader = new ExcelReader();
                                        Stream = reader.Open(filePath);
                                        //Session["Stream"] = Stream;

                                        //check is it template

                                        if (reader.IsTemplate(Stream))
                                        {
                                            TaskManager.Current().SetValid(true);
                                            TaskManager.AddToBus(TaskManager.IS_TEMPLATE, "true");
                                        }
                                        else
                                        {
                                            model.ErrorList.Add(new Error(ErrorType.Other, "File is not a Template"));
                                            TaskManager.AddToBus(TaskManager.IS_TEMPLATE, "false");
                                        }

                                        Stream.Close();
                                    }
                                    else
                                    {
                                        TaskManager.AddToBus(TaskManager.IS_TEMPLATE, "false");
                                        // excel file
                                        if (TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".xls"))
                                        {

                                            // open file
                                            ExcelReader reader = new ExcelReader();
                                            Stream = reader.Open(filePath);
                                            //Session["Stream"] = Stream;
                                            TaskManager.Current().SetValid(true);

                                            Stream.Close();
                                        }
                                        // text ór csv file
                                        else if (TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".csv") || TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".txt"))
                                        {
                                            // open file
                                            AsciiReader reader = new AsciiReader();
                                            Stream = reader.Open(filePath);
                                            //Session["Stream"] = Stream;
                                            TaskManager.Current().SetValid(true);

                                            Stream.Close();
                                        }
                                    }
                                #endregion
                            }

                            if (GetDataStructureType().Equals(DataStructureType.Unstructured))
                            {
                                #region unstructured datastructure
                                //try save file
                                string filePath = TaskManager.Bus[TaskManager.FILEPATH].ToString();
                              
                                if(FileHelper.FileExist( filePath ))
                                {
                                    TaskManager.Current().SetValid(true);
                                }

                                
                                #endregion
                            }
                            
                        }
                        catch
                        {
                            model.ErrorList.Add(new Error(ErrorType.Other, "Cannot access file on server."));
                        }
                    }
                    else
                    {
                        model.ErrorList.Add(new Error(ErrorType.Other, "File is not supported."));
                    }


                }
                else
                {
                    model.ErrorList.Add(new Error(ErrorType.Other, "No file selected or submitted."));
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
            //get datastuctureType
            model.DataStructureType = GetDataStructureType();
            model.SupportedFileExtentions = UploadWizardHelper.GetExtentionList(model.DataStructureType);

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
            TaskManager TaskManager = (TaskManager)Session["TaskManager"];
            if (SelectFileUploader != null)
            {
                //data/datasets/1/1/
                string dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");
                string storepath = Path.Combine(dataPath, "Temp", GetUserNameOrDefault());

                // if folder not exist
                if (!Directory.Exists(storepath))
                {
                    Directory.CreateDirectory(storepath);
                }

                string path = Path.Combine(storepath, SelectFileUploader.FileName);



                SelectFileUploader.SaveAs(path);
                TaskManager.AddToBus(TaskManager.FILEPATH, path);

                TaskManager.AddToBus(TaskManager.FILENAME, SelectFileUploader.FileName);
                TaskManager.AddToBus(TaskManager.EXTENTION, SelectFileUploader.FileName.Split('.').Last());
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
            TaskManager TaskManager = (TaskManager)Session["TaskManager"];
            if (fileName != null)
            {
                //string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "ServerFiles",fileName);

                //data/datasets/1/1/
                string dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");
                string path = Path.Combine(dataPath, "Temp", GetUserNameOrDefault(), fileName);

                TaskManager.AddToBus(TaskManager.FILEPATH, path);

                TaskManager.AddToBus(TaskManager.FILENAME, fileName);
                TaskManager.AddToBus(TaskManager.EXTENTION, "." + fileName.Split('.').Last());
                Session["TaskManager"] = TaskManager;
            }

            //return RedirectToAction("UploadWizard");
            return Content("");
        }


        /// <summary>
        /// read filenames from datapath/Temp/Username
        /// </summary>
        /// <returns>return a list with all names from file in the folder</returns>
        private List<String> GetServerFileList()
        {

            string userDataPath = Path.Combine(AppConfiguration.DataPath, "Temp", GetUserNameOrDefault());

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
        public string GetUserNameOrDefault()
        {
            string userName = string.Empty;
            try
            {
                userName = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(userName) ? userName : "DEFAULT";
        }

        /// <summary>
        /// returns true if Extention in the Bus will supported
        /// </summary>
        /// <param name="taskManager"></param>
        /// <returns></returns>
        private bool IsSupportedExtention(TaskManager taskManager)
        {
            if (taskManager.Bus.ContainsKey(TaskManager.EXTENTION))
            {
                string ext = taskManager.Bus[TaskManager.EXTENTION].ToString();
                DataStructureType type = (DataStructureType)taskManager.Bus[TaskManager.DATASTRUCTURE_TYPE];

                if (UploadWizardHelper.GetExtentionList(type).Contains(ext)) return true;

            }

            return false;
        }

        private DataStructureType GetDataStructureType()
        {
            if (TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_TYPE))
            {
                return (DataStructureType)TaskManager.Bus[TaskManager.DATASTRUCTURE_TYPE];
            }

            return DataStructureType.Structured;
        }

        #endregion
    }
}
