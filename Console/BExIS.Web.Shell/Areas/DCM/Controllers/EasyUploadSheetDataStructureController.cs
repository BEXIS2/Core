using BExIS.Dcm.UploadWizard;
using BExIS.Dcm.Wizard;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Models;
using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class EasyUploadSheetDataStructureController : Controller
    {
        private EasyUploadTaskManager TaskManager;

        [HttpGet]
        public ActionResult SheetDataStructure(int index)
        {
            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

            //set current stepinfo based on index
            if (TaskManager != null)
            {
                TaskManager.SetCurrent(index);

                // remove if existing
                TaskManager.RemoveExecutedStep(TaskManager.Current());
            }

            SelectSheetFormatModel model = new SelectSheetFormatModel();

            // when jumping back to this step
            // check if sheet format is selected
            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_FORMAT))
            {
                if (!String.IsNullOrEmpty(Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.SHEET_FORMAT])))
                {
                    model.SelectedSheetFormat = TaskManager.Bus[EasyUploadTaskManager.SHEET_FORMAT].ToString();
                }
            }

            model.StepInfo = TaskManager.Current();

            return PartialView(model);

        }

        /*
         * Is called when the "next"-Button on the SheetDataStructure-View is pressed
         * */
        [HttpPost]
        public ActionResult SheetDataStructure(object[] data)
        {
            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];
            SelectSheetFormatModel model = new SelectSheetFormatModel();
            model.StepInfo = TaskManager.Current();

            if (TaskManager != null)
            {
                TaskManager.Current().SetValid(false);

                if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_FORMAT))  //Check if there is a Sheet Format on the bus
                                                                                      //it's only added to the bus if it's valid so there's no need to double check that
                {
                    TaskManager.Current().SetValid(true);
                }
                else
                {
                    model.ErrorList.Add(new Error(ErrorType.Other, "Please select a sheet format."));   //No Format selected yet
                }

                if (TaskManager.Current().valid == true) //Jump to the next step
                {
                    TaskManager.AddExecutedStep(TaskManager.Current());
                    TaskManager.GoToNext();
                    Session["TaskManager"] = TaskManager;
                    ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                    return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
                }
                else //Stay on the same page
                {
                    TaskManager.Current().SetStatus(StepStatus.error);

                    //reload model
                    model.StepInfo = TaskManager.Current();
                }
            }

            return PartialView(model);
        }

        /*
         * If a valid sheet format was submitted via the radio buttons,
         * add it to the bus
         * */
        [HttpPost]
        public ActionResult AddSelectedDatasetToBus(string format)
        {

            SelectSheetFormatModel model = new SelectSheetFormatModel();

            EasyUploadTaskManager TaskManager = (EasyUploadTaskManager)Session["TaskManager"];


            if (!String.IsNullOrEmpty(format) && validateSheetFormat(format))
            {
                //Valid sheet format was selected, add it to the bus
                TaskManager.AddToBus(EasyUploadTaskManager.SHEET_FORMAT, format);
            }
            else
            {
                //No valid format was selected, display an error
                model.ErrorList.Add(new Error(ErrorType.Other, "Please select a sheet format."));
            }

            Session["TaskManager"] = TaskManager;


            //create Model
            model.StepInfo = TaskManager.Current();

            //Make sure the selected sheet is correctly displayed
            model.SelectedSheetFormat = format;

            //Stay on the same page
            return PartialView("SheetDataStructure", model);

        }

        #region private methods


        #region helper

        /* Check if a valid sheet format was selected
         * */
        private bool validateSheetFormat(string sheetFormat)
        {
            switch (sheetFormat)
            {
                case "TopDown":
                    return true;
                case "LeftRight":
                    return true;
                case "Matrix":
                    return true;
                default:
                    return false;
            }
        }

        #endregion

        #endregion
    }
}