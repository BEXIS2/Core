using BExIS.Dcm.UploadWizard;
using BExIS.Dcm.Wizard;
using BExIS.IO;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Models;
using BExIS.Utils.Data.Upload;
using BExIS.Utils.Upload;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;

namespace BExIS.Modules.Dcm.UI.Controllers
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
            var model = new SelectFileViewModel();
            if (TaskManager.Bus.ContainsKey(TaskManager.FILENAME))
            {
                model.SelectedFileName = TaskManager.Bus[TaskManager.FILENAME].ToString();
            }


            //get datastuctureType
            model.DataStructureType = GetDataStructureType();
            model.SupportedFileExtentions = UploadHelper.GetExtentionList(model.DataStructureType, this.Session.GetTenant());

            //Get StepInfo
            model.StepInfo = TaskManager.Current();

            model.serverFileList = GetServerFileList();

            // get max file lenght
            var dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");
            var storepath = Path.Combine(dataPath, "Temp", GetUsernameOrDefault());

            model.MaxFileLength = 260 - storepath.Length - 2;

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult SelectAFile(object[] data)
        {
            IOUtility iOUtility = new IOUtility();

            var model = new SelectFileViewModel();

            TaskManager = (TaskManager)Session["TaskManager"];

            if (data != null) TaskManager.AddToBus(data);

            model.StepInfo = TaskManager.Current();

            TaskManager.Current().SetValid(false);

            if (TaskManager != null)
            {
                // is path of FileStream exist
                if (TaskManager.Bus.ContainsKey(TaskManager.FILEPATH))
                {
                    if (IsSupportedExtention(TaskManager))
                    {
                        try
                        {
                            if (GetDataStructureType().Equals(DataStructureType.Structured))
                            {
                                #region structured datastructure
                                //try save FileStream
                                var filePath = TaskManager.Bus[TaskManager.FILEPATH].ToString();
                                //if extention like a makro excel FileStream
                                if (TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".xlsm"))
                                {
                                    // open FileStream
                                    var reader = new ExcelReader(null, null);
                                    using (Stream = reader.Open(filePath))
                                    {
                                        //Session["Stream"] = Stream;

                                        //check is it template

                                        if (reader.IsTemplate(Stream))
                                        {
                                            TaskManager.Current().SetValid(true);
                                            TaskManager.AddToBus(TaskManager.IS_TEMPLATE, "true");
                                            TaskManager.AddToBus(TaskManager.FILE_READER_INFO, new ExcelFileReaderInfo()
                                            {
                                                Offset = 1,
                                                Variables = 1,
                                                Data = 13,
                                                Decimal = DecimalCharacter.point
                                            });
                                        }
                                        else
                                        {
                                            model.ErrorList.Add(new Error(ErrorType.Other, "File is not a Template"));
                                            TaskManager.AddToBus(TaskManager.IS_TEMPLATE, "false");
                                        }

                                        if (!ExcelReader.SUPPORTED_APPLICATIONS.Contains(reader.Application))
                                        {
                                            model.ErrorList.Add(new Error(ErrorType.Other, "The document was created in an application " + reader.Application + " that will currently not support"));
                                        }
                                    }
                                }
                                else
                                {
                                    TaskManager.AddToBus(TaskManager.IS_TEMPLATE, "false");
                                    // excel FileStream
                                    if (iOUtility.IsSupportedExcelFile(TaskManager.Bus[TaskManager.EXTENTION].ToString()))
                                    {

                                        // open FileStream
                                        var reader = new ExcelReader(null, null);
                                        Stream = reader.Open(filePath);
                                        TaskManager.Current().SetValid(true);
                                        Stream.Close();
                                    }
                                    // text ór csv FileStream
                                    else if (iOUtility.IsSupportedAsciiFile(TaskManager.Bus[TaskManager.EXTENTION].ToString()))
                                    {
                                        // open FileStream
                                        var reader = new AsciiReader(null, null, new IOUtility());
                                        using (Stream = reader.Open(filePath))
                                        {
                                            //Session["Stream"] = Stream;
                                            TaskManager.Current().SetValid(true);
                                        }
                                    }
                                }
                                #endregion
                            }

                            if (GetDataStructureType().Equals(DataStructureType.Unstructured))
                            {
                                #region unstructured datastructure
                                //try save FileStream
                                var filePath = TaskManager.Bus[TaskManager.FILEPATH].ToString();

                                if (FileHelper.FileExist(filePath))
                                {
                                    TaskManager.Current().SetValid(true);
                                }


                                #endregion
                            }

                        }
                        catch (NotSupportedException ex)
                        {
                            model.ErrorList.Add(new Error(ErrorType.Other, ex.Message));
                        }
                        catch (Exception ex)
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
            //get datastuctureType
            model.DataStructureType = GetDataStructureType();
            model.SupportedFileExtentions = UploadHelper.GetExtentionList(model.DataStructureType, this.Session.GetTenant());

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
            var TaskManager = (TaskManager)Session["TaskManager"];
            if (SelectFileUploader != null)
            {
                //data/datasets/1/1/
                var dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");
                var storepath = Path.Combine(dataPath, "Temp", GetUsernameOrDefault());

                // if folder not exist
                if (!Directory.Exists(storepath))
                {
                    Directory.CreateDirectory(storepath);
                }

                var path = Path.Combine(storepath, SelectFileUploader.FileName);

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
            var TaskManager = (TaskManager)Session["TaskManager"];
            if (fileName != null)
            {
                //string path = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "ServerFiles",fileName);

                //data/datasets/1/1/
                var dataPath = AppConfiguration.DataPath; //Path.Combine(AppConfiguration.WorkspaceRootPath, "Data");
                var path = Path.Combine(dataPath, "Temp", GetUsernameOrDefault(), fileName);

                TaskManager.AddToBus(TaskManager.FILEPATH, path);

                TaskManager.AddToBus(TaskManager.FILENAME, fileName);
                TaskManager.AddToBus(TaskManager.EXTENTION, "." + fileName.Split('.').Last());
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

            var userDataPath = Path.Combine(AppConfiguration.DataPath, "Temp", GetUsernameOrDefault());

            // if folder not exist
            if (!Directory.Exists(userDataPath))
            {
                Directory.CreateDirectory(userDataPath);
            }


            var dirInfo = new DirectoryInfo(userDataPath);
            return dirInfo.GetFiles().Select(i => i.Name).ToList();

        }

        // chekc if user exist
        // if true return usernamem otherwise "DEFAULT"
        public string GetUsernameOrDefault()
        {
            var username = string.Empty;
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
        private bool IsSupportedExtention(TaskManager taskManager)
        {
            if (taskManager.Bus.ContainsKey(TaskManager.EXTENTION))
            {
                var ext = taskManager.Bus[TaskManager.EXTENTION].ToString();
                var type = (DataStructureType)taskManager.Bus[TaskManager.DATASTRUCTURE_TYPE];

                if (UploadHelper.GetExtentionList(type, this.Session.GetTenant()).Contains(ext.ToLower())) return true;

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
