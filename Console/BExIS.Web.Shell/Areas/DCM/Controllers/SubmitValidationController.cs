using BExIS.Dcm.UploadWizard;
using BExIS.Dcm.Wizard;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Vaiona.Logging.Aspects;
using Vaiona.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class SubmitValidationController : BaseController
    {
        private TaskManager TaskManager;
        private FileStream Stream;
        //
        // GET: /DCM/Validation/

        [HttpGet]
        public ActionResult Validation(int index)
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            //set current stepinfo based on index
            if (TaskManager != null)
            {
                TaskManager.SetCurrent(index);
                // remove if existing
                TaskManager.RemoveExecutedStep(TaskManager.Current());

                // set valid to false if exist
                if (TaskManager.Bus.ContainsKey(TaskManager.VALID))
                {
                    TaskManager.Bus.Remove(TaskManager.VALID);
                }
            }

            ValidationModel model = new ValidationModel();
            model.StepInfo = TaskManager.Current();

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Validation(object[] data)
        {
            TaskManager = (BExIS.Dcm.UploadWizard.TaskManager)Session["TaskManager"];
            ValidationModel model = new ValidationModel();


            if (TaskManager != null)
            {
                TaskManager.Current().SetValid(false);

                if (data != null) TaskManager.AddToBus(data);

                if (TaskManager.Bus.ContainsKey(TaskManager.VALID))
                {
                    bool valid = Convert.ToBoolean(TaskManager.Bus[TaskManager.VALID]);

                    if (valid)
                    {
                        TaskManager.Current().SetValid(true);
                    }
                    else
                    {
                        model.ErrorList.Add(new Error(ErrorType.Other, "Not Valid."));
                    }

                }
                else
                {
                    model.ErrorList.Add(new Error(ErrorType.Other, "Validation failed! Please check that the information you provided  in the previous steps conforms to your data FileStream."));
                    model.StepInfo = TaskManager.Current();
                }

                if (TaskManager.Current().valid == true)
                {
                    TaskManager.AddExecutedStep(TaskManager.Current());
                    TaskManager.GoToNext();
                    Session["TaskManager"] = TaskManager;
                    ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                    return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
                }
            }

            model.StepInfo = TaskManager.Current();

            return PartialView(model);
        }


        [HttpPost]
        [MeasurePerformance]
        public ActionResult ValidateFile()
        {
            ViewData["SortedErrors"] = null;

            DataStructureManager dsm = new DataStructureManager();
            IOUtility iOUtility = new IOUtility();
            try
            {

                BExIS.Dcm.UploadWizard.TaskManager TaskManager = (BExIS.Dcm.UploadWizard.TaskManager)Session["TaskManager"];
                ValidationModel model = new ValidationModel();
                model.StepInfo = TaskManager.Current();

                if (TaskManager.Bus.ContainsKey(TaskManager.DATASET_ID) && TaskManager.Bus.ContainsKey(TaskManager.DATASTRUCTURE_ID))
                {
                    try
                    {
                        long id = (long)Convert.ToInt32(TaskManager.Bus[TaskManager.DATASET_ID]);
                        long iddsd = (long)Convert.ToInt32(TaskManager.Bus[TaskManager.DATASTRUCTURE_ID]);
                        StructuredDataStructure sds = dsm.StructuredDataStructureRepo.Get(iddsd);
                        dsm.StructuredDataStructureRepo.LoadIfNot(sds.Variables);

                        // Add Number of Variables to the BUS
                        if(sds != null)
                            TaskManager.AddToBus(TaskManager.NUMBERSOFVARIABLES, sds.Variables.Count);


                        if (TaskManager.Bus[TaskManager.EXTENTION].ToString().Equals(".xlsm"))
                        {
                            // open FileStream
                            ExcelReader reader = new ExcelReader(sds, new ExcelFileReaderInfo());
                            Stream = reader.Open(TaskManager.Bus[TaskManager.FILEPATH].ToString());
                            reader.ValidateTemplateFile(Stream, TaskManager.Bus[TaskManager.FILENAME].ToString(), id);
                            model.ErrorList = reader.ErrorMessages;

                            TaskManager.AddToBus(TaskManager.NUMBERSOFROWS, reader.NumberOfRows);

                        }

                        if (iOUtility.IsSupportedExcelFile(TaskManager.Bus[TaskManager.EXTENTION].ToString()))
                        {
                            // open FileStream
                            ExcelReader reader = new ExcelReader(sds, (ExcelFileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO]);
                            Stream = reader.Open(TaskManager.Bus[TaskManager.FILEPATH].ToString());
                            reader.ValidateFile(Stream, TaskManager.Bus[TaskManager.FILENAME].ToString(), id);
                            model.ErrorList = reader.ErrorMessages;

                            TaskManager.AddToBus(TaskManager.NUMBERSOFROWS, reader.NumberOfRows);

                        }


                        if (iOUtility.IsSupportedAsciiFile(TaskManager.Bus[TaskManager.EXTENTION].ToString()))
                        {
                            AsciiReader reader = new AsciiReader(sds, (AsciiFileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO]);
                            Stream = reader.Open(TaskManager.Bus[TaskManager.FILEPATH].ToString());
                            reader.ValidateFile(Stream, TaskManager.Bus[TaskManager.FILENAME].ToString(), id);
                            model.ErrorList = reader.ErrorMessages;

                            TaskManager.AddToBus(TaskManager.NUMBERSOFROWS, reader.NumberOfRows);

                        }

                    }
                    catch (Exception ex)
                    {
                        model.ErrorList.Add(new Error(ErrorType.Other, "Can not valid. :  " + ex.Message));
                        TaskManager.AddToBus(TaskManager.VALID, false);

                    }
                    finally
                    {
                        Stream.Close();
                    }
                }
                else
                {
                    model.ErrorList.Add(new Error(ErrorType.Dataset, "Dataset is not selected."));
                    TaskManager.AddToBus(TaskManager.VALID, false);
                }

                if (model.ErrorList.Count() == 0)
                {
                    model.Validated = true;
                    TaskManager.AddToBus(TaskManager.VALID, true);
                }

                List<Error> errorList = new List<Error>();
                for (int i = 0; i < model.ErrorList.Count; i++)
                {
                    // Assume not duplicate.
                    bool duplicate = false;
                    for (int z = 0; z < i; z++)
                    {
                        if (model.ErrorList[z].ToString() == model.ErrorList[i].ToString())
                        {
                            // This is a duplicate.
                            duplicate = true;
                            break;
                        }
                    }
                    // If not duplicate, add to result.
                    if (!duplicate)
                    {
                        errorList.Add(model.ErrorList[i]);
                    }
                }

                model.ErrorList = errorList;

                if (errorList.Count > 0)
                {
                    // split up the error messages for a btter overview-- >
                    // set all value error with the same var name, datatypoe and issue-- >
                    // create a dictionary for error messages

                    // variable issues
                    var varNames = errorList.Where(e=> e.GetType().Equals(ErrorType.Value)).Select(e => e.getName() ).Distinct();
                    var varIssues = errorList.Where(e => e.GetType().Equals(ErrorType.Value)).Select(e => e.GetMessage()).Distinct();

                    List<Tuple<string, int, string>> sortedErrors = new List<Tuple<string, int, string>>();

                    foreach (string vn in varNames)
                    {
                        foreach (string i in varIssues)
                        {
                            int c = errorList.Where(e => e.getName().Equals(vn) && e.GetMessage().Equals(i)).Count();

                            if (c > 0)
                            {
                                sortedErrors.Add(new Tuple<string, int, string>(vn, c, i));
                            }
                        }
                    }

                    if (sortedErrors.Count > 0)
                    {
                        ViewData["SortedValueErrors"] = sortedErrors;
                    }
                }

                return PartialView(TaskManager.Current().GetActionInfo.ActionName, model);
            }
            finally
            {
                dsm.Dispose();
            }
        }
    }
}
