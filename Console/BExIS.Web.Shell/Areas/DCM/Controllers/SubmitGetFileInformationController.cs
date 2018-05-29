using BExIS.Dcm.UploadWizard;
using BExIS.Dcm.Wizard;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Modules.Dcm.UI.Models;
using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class SubmitGetFileInformationController : Controller
    {

        private TaskManager TaskManager;
        //
        // GET: /DCM/GetFileInformation/

        [HttpGet]
        public ActionResult GetFileInformation(int index)
        {
            TaskManager = (TaskManager)Session["TaskManager"];

            //set current stepinfo based on index
            if (TaskManager != null)
            {
                TaskManager.SetCurrent(index);

                // remove if existing
                TaskManager.RemoveExecutedStep(TaskManager.Current());
            }


            //if its a template jumping direkt to the next step
            if (TaskManager.Bus[TaskManager.IS_TEMPLATE].ToString().Equals("true"))
            {
                TaskManager.Current().SetValid(true);
                if (TaskManager.Current().IsValid())
                {
                    TaskManager.GoToNext();
                    Session["TaskManager"] = TaskManager;
                    ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                    return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
                }
            }
            else
            {
                GetFileInformationModel model = new GetFileInformationModel();
                model.StepInfo = TaskManager.Current();
                model.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();

                if (TaskManager.Bus.ContainsKey(TaskManager.FILE_READER_INFO))
                {
                    model.FileInfoModel = GetFileInfoModel((AsciiFileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO], TaskManager.Bus[TaskManager.EXTENTION].ToString());
                }
                else
                {
                    if (UploadWizardHelper.IsSupportedAsciiFile(model.Extention))
                        TaskManager.Bus[TaskManager.FILE_READER_INFO] = new AsciiFileReaderInfo();

                    if (UploadWizardHelper.IsSupportedExcelFile(model.Extention))
                        TaskManager.Bus[TaskManager.FILE_READER_INFO] = new ExcelFileReaderInfo();
                }

                model.FileInfoModel.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();


                return PartialView(model);
            }

            return PartialView(new GetFileInformationModel());
        }

        [HttpPost]
        public ActionResult GetFileInformation()
        {
            TaskManager = (TaskManager)Session["TaskManager"];
            TaskManager.Current().SetValid(false);

            if (TaskManager.Bus[TaskManager.IS_TEMPLATE].ToString().Equals("true"))
                TaskManager.Current().SetValid(true);

            if (TaskManager.Bus.ContainsKey(TaskManager.FILE_READER_INFO))
            {
                TaskManager.Current().SetValid(true);
            }
            else
            {
                GetFileInformationModel model = new GetFileInformationModel();
                model.StepInfo = TaskManager.Current();
                model.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();
                model.FileInfoModel.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();
                model.ErrorList.Add(new Error(ErrorType.Other, "File Infomartion not saved."));

                if (TaskManager.Bus.ContainsKey(TaskManager.FILE_READER_INFO))
                {
                    model.FileInfoModel = GetFileInfoModel((AsciiFileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO], TaskManager.Bus[TaskManager.EXTENTION].ToString());
                }

                model.FileInfoModel.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();

                return PartialView(model);
            }

            if (TaskManager.Current().IsValid())
            {
                TaskManager.AddExecutedStep(TaskManager.Current());
                TaskManager.GoToNext();
                Session["TaskManager"] = TaskManager;
                ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
            }

            return PartialView();
        }

        #region private methods

        private FileInfoModel GetFileInfoModel(AsciiFileReaderInfo info, string extention)
        {
            FileInfoModel FileReaderInfo = new FileInfoModel();

            FileReaderInfo.Data = info.Data;
            FileReaderInfo.Dateformat = info.Dateformat;
            FileReaderInfo.Decimal = info.Decimal;
            FileReaderInfo.Offset = info.Offset;
            FileReaderInfo.Orientation = info.Orientation;
            FileReaderInfo.Variables = info.Variables;
            FileReaderInfo.Separator = info.Seperator;
            FileReaderInfo.TextMarker = info.TextMarker;

            return FileReaderInfo;
        }

        #region ascii FileStream info
        [HttpPost]
        public ActionResult SaveAsciiFileInfos(FileInfoModel info)
        {

            TaskManager TaskManager = (TaskManager)Session["TaskManager"];

            AsciiFileReaderInfo asciiFileReaderInfo = new AsciiFileReaderInfo();

            asciiFileReaderInfo.Data = info.Data;
            asciiFileReaderInfo.Dateformat = "";//info.Dateformat;
            asciiFileReaderInfo.Decimal = info.Decimal;
            asciiFileReaderInfo.Offset = info.Offset;
            asciiFileReaderInfo.Orientation = info.Orientation;
            asciiFileReaderInfo.Seperator = info.Separator;
            asciiFileReaderInfo.Variables = info.Variables;
            asciiFileReaderInfo.TextMarker = info.TextMarker;

            TaskManager.AddToBus(TaskManager.FILE_READER_INFO, asciiFileReaderInfo);

            GetFileInformationModel model = new GetFileInformationModel();
            model.StepInfo = TaskManager.Current();
            model.StepInfo.SetValid(true);
            model.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();
            model.FileInfoModel = info;
            model.FileInfoModel.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();
            model.IsSaved = true;

            return RedirectToAction("ReloadUploadWizard", new { restart = false });
        }

        public ActionResult ChangeAsciiFileInfo(string name, string value)
        {
            TaskManager TaskManager = (TaskManager)Session["TaskManager"];

            if (UploadWizardHelper.IsSupportedAsciiFile(TaskManager.Bus[TaskManager.EXTENTION].ToString()))
            {

                AsciiFileReaderInfo info = (AsciiFileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO];

                switch (name)
                {
                    case "Separator": { info.Seperator = AsciiFileReaderInfo.GetSeperator(value); break; }
                    case "Decimal": { info.Decimal = AsciiFileReaderInfo.GetDecimalCharacter(value); break; }
                    case "Orientation": { info.Orientation = AsciiFileReaderInfo.GetOrientation(value); break; }
                    case "TextMarker": { info.TextMarker = AsciiFileReaderInfo.GetTextMarker(value); break; }
                    case "Variables": { info.Variables = Convert.ToInt32(value); break; }
                    case "Data": { info.Data = Convert.ToInt32(value); break; }
                    case "Offset": { info.Offset = Convert.ToInt32(value); break; }
                }

                TaskManager.Bus[TaskManager.FILE_READER_INFO] = info;
                Session["TaskManager"] = TaskManager;
            }


            return Content("");
        }

        #endregion

        #region excel FileStream info

        public ActionResult SaveExcelFileInfos(FileInfoModel info)
        {
            TaskManager TaskManager = (TaskManager)Session["TaskManager"];

            ExcelFileReaderInfo excelFileReaderInfo = new ExcelFileReaderInfo();

            excelFileReaderInfo.Data = info.Data;
            excelFileReaderInfo.Dateformat = "";//info.Dateformat;
            excelFileReaderInfo.Decimal = info.Decimal;
            excelFileReaderInfo.Offset = info.Offset;
            excelFileReaderInfo.Orientation = info.Orientation;
            excelFileReaderInfo.Variables = info.Variables;

            TaskManager.AddToBus(TaskManager.FILE_READER_INFO, excelFileReaderInfo);

            GetFileInformationModel model = new GetFileInformationModel();
            model.StepInfo = TaskManager.Current();
            model.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();
            model.FileInfoModel = info;
            model.FileInfoModel.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();
            model.IsSaved = true;

            return RedirectToAction("ReloadUploadWizard", new { restart = false });

        }

        #endregion

        #endregion

    }
}
