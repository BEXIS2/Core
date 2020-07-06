using BExIS.Dcm.UploadWizard;
using BExIS.Dcm.Wizard;
using BExIS.IO;
using BExIS.IO.Transform.Input;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Modules.Dcm.UI.Models;
using BExIS.UI.Helpers;
using BExIS.Utils.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using Vaiona.Logging;

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
            IOUtility iOUtility = new IOUtility();

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
                    if (iOUtility.IsSupportedAsciiFile(model.Extention))
                        model.FileInfoModel = GetFileInfoModel((AsciiFileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO], TaskManager.Bus[TaskManager.EXTENTION].ToString());

                    if (iOUtility.IsSupportedExcelFile(model.Extention))
                        model.FileInfoModel = GetFileInfoModel((ExcelFileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO], TaskManager.Bus[TaskManager.EXTENTION].ToString());
                }
                else
                {
                    if (iOUtility.IsSupportedAsciiFile(model.Extention))
                        TaskManager.Bus[TaskManager.FILE_READER_INFO] = new AsciiFileReaderInfo();

                    if (iOUtility.IsSupportedExcelFile(model.Extention))
                    {
                        TaskManager.Bus[TaskManager.FILE_READER_INFO] = new ExcelFileReaderInfo();
                        model.FileInfoModel = GetFileInfoModel((ExcelFileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO], TaskManager.Bus[TaskManager.EXTENTION].ToString());
                    }
                }

                model.FileInfoModel.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();

                return PartialView(model);
            }

            return PartialView(new GetFileInformationModel());
        }

        [HttpPost]
        public ActionResult GetFileInformation()
        {
            IOUtility iOUtility = new IOUtility();

            TaskManager = (TaskManager)Session["TaskManager"];
            TaskManager.Current().SetValid(false);

            GetFileInformationModel model = new GetFileInformationModel();

            if (TaskManager.Bus[TaskManager.IS_TEMPLATE].ToString().Equals("true"))
                TaskManager.Current().SetValid(true);

            if (TaskManager.Bus.ContainsKey(TaskManager.FILE_READER_INFO))
            {
                FileReaderInfo fri = (FileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO];

                if (fri is ExcelFileReaderInfo)
                {
                    ExcelFileReaderInfo efri = (ExcelFileReaderInfo)fri;
                    if (efri.VariablesStartRow == 0)
                    {
                        TaskManager.Current().SetValid(false);
                        model.ErrorList.Add(new Error(ErrorType.Other, "Header is not set."));
                    }

                    if (efri.DataStartRow == 0)
                    {
                        TaskManager.Current().SetValid(false);
                        model.ErrorList.Add(new Error(ErrorType.Other, "Data is not set."));
                    }

                    if (model.ErrorList.Count == 0) TaskManager.Current().SetValid(true);
                }
                else
                {
                    TaskManager.Current().SetValid(true);
                }
            }

            if (!TaskManager.Current().IsValid())
            {
                model.StepInfo = TaskManager.Current();
                model.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();
                model.FileInfoModel.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();
                model.ErrorList.Add(new Error(ErrorType.Other, "File Information not saved."));

                if (TaskManager.Bus.ContainsKey(TaskManager.FILE_READER_INFO))
                {
                    if (iOUtility.IsSupportedAsciiFile(model.Extention))
                        model.FileInfoModel = GetFileInfoModel((AsciiFileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO], TaskManager.Bus[TaskManager.EXTENTION].ToString());

                    if (iOUtility.IsSupportedExcelFile(model.Extention))
                        model.FileInfoModel = GetFileInfoModel((ExcelFileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO], TaskManager.Bus[TaskManager.EXTENTION].ToString());
                }

                model.FileInfoModel.Extention = TaskManager.Bus[TaskManager.EXTENTION].ToString();

                return PartialView(model);
            }
            else
            {
                TaskManager.AddExecutedStep(TaskManager.Current());
                TaskManager.GoToNext();
                Session["TaskManager"] = TaskManager;
                ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
            }
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

        private FileInfoModel GetFileInfoModel(ExcelFileReaderInfo info, string extention)
        {
            FileInfoModel FileReaderInfo = new FileInfoModel();

            FileReaderInfo.Data = info.Data;
            FileReaderInfo.Dateformat = info.Dateformat;
            FileReaderInfo.Decimal = info.Decimal;
            FileReaderInfo.Offset = info.Offset;
            FileReaderInfo.Orientation = info.Orientation;
            FileReaderInfo.Variables = info.Variables;

            //Use the given file and the given sheet format to create a json-table
            string filePath = TaskManager.Bus[EasyUploadTaskManager.FILEPATH].ToString();
            string jsonTable = "[]";
            FileStream fis = null;

            //FileStream for the users file
            fis = new FileStream(filePath, FileMode.Open, FileAccess.Read);

            UploadUIHelper uploadUIHelper = new UploadUIHelper(fis);

            string activeWorksheet;
            if (!TaskManager.Bus.ContainsKey(TaskManager.ACTIVE_WOKSHEET_URI))
            {
                activeWorksheet = uploadUIHelper.GetFirstWorksheetUri().ToString();
                TaskManager.AddToBus(TaskManager.ACTIVE_WOKSHEET_URI, activeWorksheet);
            }
            else
            {
                activeWorksheet = TaskManager.Bus[EasyUploadTaskManager.ACTIVE_WOKSHEET_URI].ToString();
            }

            FileReaderInfo.activeSheetUri = activeWorksheet;
            FileReaderInfo.SheetUriDictionary = uploadUIHelper.GetWorksheetUris();
            // Check if the areas have already been selected, if yes, use them (Important when jumping back to this step)
            if (TaskManager.Bus.ContainsKey(TaskManager.SHEET_DATA_AREA))
            {
                FileReaderInfo.DataArea = (List<string>)TaskManager.Bus[TaskManager.SHEET_DATA_AREA];
            }
            if (TaskManager.Bus.ContainsKey(TaskManager.SHEET_HEADER_AREA))
            {
                FileReaderInfo.HeaderArea = TaskManager.Bus[TaskManager.SHEET_HEADER_AREA].ToString();
            }

            //Generate the table for the active worksheet
            jsonTable = uploadUIHelper.GenerateJsonTable(activeWorksheet);

            if (!String.IsNullOrEmpty(jsonTable))
            {
                TaskManager.AddToBus(EasyUploadTaskManager.SHEET_JSON_DATA, jsonTable);
            }

            fis.Close();

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
            IOUtility iOUtility = new IOUtility();

            if (iOUtility.IsSupportedAsciiFile(TaskManager.Bus[TaskManager.EXTENTION].ToString()))
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

        #endregion ascii FileStream info

        #region excel FileStream info

        [HttpPost]
        public ActionResult ChangeWorksheet(string sheetIdentifier)
        {
            TaskManager = (TaskManager)Session["TaskManager"];

            #region Generate table for selected sheet

            string filePath = TaskManager.Bus[EasyUploadTaskManager.FILEPATH].ToString();
            FileStream fis = null;
            string jsonTable = "[]";

            try
            {
                //FileStream for the users file
                fis = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                //Transforms the content of the file into a 2d-json-array
                UploadUIHelper uploadUIHelper = new UploadUIHelper(fis);
                jsonTable = uploadUIHelper.GenerateJsonTable(sheetIdentifier);

                if (!String.IsNullOrEmpty(jsonTable))
                {
                    TaskManager.AddToBus(TaskManager.SHEET_JSON_DATA, jsonTable);
                }

                TaskManager.AddToBus(TaskManager.ACTIVE_WOKSHEET_URI, sheetIdentifier);
            }
            catch (Exception ex)
            {
                LoggerFactory.LogCustom(ex.Message);
            }
            finally
            {
                if (fis != null)
                {
                    fis.Close();
                }
            }

            #endregion Generate table for selected sheet

            //Send back the table-data
            return Content(jsonTable, "application/json");
        }

        /*
         * Save the selected area either as data area or as header area
         * */

        [HttpPost]
        public ActionResult SelectedAreaToBus()
        {
            string headerArea = null;
            string dataArea = null;
            string worksheeturi = "";
            TaskManager TaskManager = (TaskManager)Session["TaskManager"];

            foreach (string key in Request.Form.AllKeys)
            {
                if ("dataArea" == key)
                {
                    dataArea = Request.Form[key];
                }
                if ("headerArea" == key)
                {
                    headerArea = Request.Form[key];
                }
            }

            FileInfoModel model = new FileInfoModel();

            //read data area from request
            if (dataArea != null)
            {
                if (TaskManager.Bus.ContainsKey(TaskManager.SHEET_DATA_AREA))
                {
                    model.DataArea = (List<string>)TaskManager.Bus[TaskManager.SHEET_DATA_AREA];
                }

                //dataArea == "" means the resetButton was clicked
                if (model.DataArea == null || dataArea == "")
                {
                    model.DataArea = new List<string>();
                }
                if (dataArea != "")
                {
                    int[] newArea = JsonConvert.DeserializeObject<int[]>(dataArea);
                    Boolean contains = false;
                    foreach (string area in model.DataArea)
                    {
                        int[] oldArea = JsonConvert.DeserializeObject<int[]>(area);
                        //If one of the already selected areas contains the new one, don't add the new one to the selection (prevents duplicate selection)
                        if (oldArea[0] <= newArea[0] && oldArea[2] >= newArea[2] &&
                            oldArea[1] <= newArea[1] && oldArea[3] >= newArea[3])
                        {
                            contains = true;
                        }
                    }
                    if (!contains)
                    {
                        //If the new area contains one (or several) of the already selected areas, remove the old ones
                        for (int i = model.DataArea.Count - 1; i >= 0; i--)
                        {
                            int[] oldArea = JsonConvert.DeserializeObject<int[]>(model.DataArea[i]);

                            if (newArea[0] <= oldArea[0] && newArea[2] >= oldArea[2] &&
                                newArea[1] <= oldArea[1] && newArea[3] >= oldArea[3])
                            {
                                model.DataArea.RemoveAt(i);
                            }
                        }

                        //Insert the new area
                        model.DataArea.Add(dataArea);
                    }
                }

                TaskManager.AddToBus(TaskManager.SHEET_DATA_AREA, model.DataArea);
            }

            //read header area from request
            if (headerArea != null)
            {
                TaskManager.AddToBus(TaskManager.SHEET_HEADER_AREA, headerArea);
                model.HeaderArea = headerArea;
            }

            string filePath = TaskManager.Bus[TaskManager.FILEPATH].ToString();
            FileStream fis = null;

            //get worksheets
            try
            {
                //FileStream for the users file
                fis = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                UploadUIHelper uploadUiHelper = new UploadUIHelper(fis);

                //Get the worksheet uris and save them to the model
                model.SheetUriDictionary = uploadUiHelper.GetWorksheetUris();
            }
            catch (Exception ex)
            {
                LoggerFactory.LogCustom(ex.Message);
            }
            finally
            {
                if (fis != null)
                {
                    fis.Close();
                }
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.ACTIVE_WOKSHEET_URI))
            {
                worksheeturi = TaskManager.Bus[TaskManager.ACTIVE_WOKSHEET_URI].ToString();
            }

            // STore in the excelfilereader info

            int[] areaDataValues = null;
            int[] areaHeaderValues = null;

            //load or create FILE_READER_INFO
            ExcelFileReaderInfo excelFileReaderInfo = new ExcelFileReaderInfo();
            if (TaskManager.Bus.ContainsKey(TaskManager.FILE_READER_INFO))
            {
                excelFileReaderInfo = (ExcelFileReaderInfo)TaskManager.Bus[TaskManager.FILE_READER_INFO];
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.SHEET_DATA_AREA))
            {
                List<string> selectedDataAreaJsonArray = (List<string>)TaskManager.Bus[TaskManager.SHEET_DATA_AREA];
                List<int[]> areaDataValuesList = new List<int[]>();
                foreach (string area in selectedDataAreaJsonArray)
                {
                    areaDataValuesList.Add(JsonConvert.DeserializeObject<int[]>(area));
                }

                if (areaDataValuesList != null && areaDataValuesList.Count > 0)
                {
                    areaDataValues = areaDataValuesList[0];
                    if (areaDataValues != null)
                    {
                        excelFileReaderInfo.DataStartRow = areaDataValues[0] + 1;
                        //End row is either at the end of the batch or the end of the marked area
                        //DataEndRow = (currentBatchEndRow > areaDataValuesList[0][2] + 1) ? areaDataValues[2] + 1 : currentBatchEndRow,
                        excelFileReaderInfo.DataEndRow = areaDataValues[2] + 1;
                        //Column indices as marked in a previous step
                        excelFileReaderInfo.DataStartColumn = areaDataValues[1] + 1;
                        excelFileReaderInfo.DataEndColumn = areaDataValues[3] + 1;
                        excelFileReaderInfo.Offset = areaDataValues[1];
                    }
                }
            }

            if (TaskManager.Bus.ContainsKey(TaskManager.SHEET_HEADER_AREA))
            {
                string selectedHeaderAreaJsonArray = TaskManager.Bus[TaskManager.SHEET_HEADER_AREA].ToString();

                if (!string.IsNullOrEmpty(selectedHeaderAreaJsonArray))
                {
                    areaHeaderValues = JsonConvert.DeserializeObject<int[]>(selectedHeaderAreaJsonArray);
                    if (areaHeaderValues != null)
                    {
                        //Header area as marked in a previous step
                        excelFileReaderInfo.VariablesStartRow = areaHeaderValues[0] + 1;
                        excelFileReaderInfo.VariablesStartColumn = areaHeaderValues[1] + 1;
                        excelFileReaderInfo.VariablesEndRow = areaHeaderValues[2] + 1;
                        excelFileReaderInfo.VariablesEndColumn = areaHeaderValues[3] + 1;
                    }
                }
            }

            excelFileReaderInfo.Orientation = Orientation.columnwise;
            excelFileReaderInfo.WorksheetUri = worksheeturi;

            //reset
            if (string.IsNullOrEmpty(dataArea) && string.IsNullOrEmpty(headerArea))
            {
                excelFileReaderInfo = new ExcelFileReaderInfo();

                if (TaskManager.Bus.ContainsKey(TaskManager.SHEET_HEADER_AREA))
                {
                    TaskManager.Bus.Remove(TaskManager.SHEET_HEADER_AREA);
                }

                if (TaskManager.Bus.ContainsKey(TaskManager.SHEET_DATA_AREA))
                {
                    TaskManager.Bus.Remove(TaskManager.SHEET_DATA_AREA);
                }
            }

            TaskManager.AddToBus(TaskManager.FILE_READER_INFO, excelFileReaderInfo);

            Session["TaskManager"] = TaskManager;

            return PartialView("_xlsFormularView", model);
        }

        #endregion excel FileStream info

        #endregion private methods
    }
}