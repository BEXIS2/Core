using System;
using System.Web.Mvc;
using System.Web.Routing;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Dcm.UploadWizard;
using BExIS.Dcm.Wizard;
using BExIS.Web.Shell.Areas.DCM.Models;
using System.IO;
using OfficeOpenXml;
using System.Web.UI.WebControls;
using Vaiona.Logging;
using System.Collections.Generic;
using BExIS.Dlm.Entities.DataStructure;
using System.Web.Script.Serialization;
using BExIS.Dlm.Services.DataStructure;
using System.Linq;
using BExIS.IO.Transform.Validation.ValueCheck;
using BExIS.IO;
using BExIS.Dlm.Entities.Data;
using BExIS.Web.Shell.Areas.DCM.Helpers;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class EasyUploadVerificationController : Controller
    {
        private EasyUploadTaskManager TaskManager;

        //
        // GET: /DCM/SubmitSelectAreas/

        [HttpGet]
        public ActionResult Verification(int index)
        {
            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];
            SelectVerificationModel model = new SelectVerificationModel();

            //Grab all necessary managers and lists
            UnitManager unitManager = new UnitManager();
            DataTypeManager dataTypeManager = new DataTypeManager();
            List<Dlm.Entities.DataStructure.Unit> tempUnitList = unitManager.Repo.Get().ToList();
            List<DataType> allDataypes = dataTypeManager.Repo.Get().ToList();

            DataStructureManager dsm = new DataStructureManager();
            DataContainerManager dam = new DataContainerManager();
            List<DataAttribute> allDataAttributes = dam.DataAttributeRepo.Get().ToList();

            //Important for jumping back to this step
            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS))
            {
                model.AssignedHeaderUnits = (List<Tuple<int, string, UnitInfo>>)TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS];
            }



            // get all DataTypes for each Units
            foreach (Dlm.Entities.DataStructure.Unit unit in tempUnitList)
            {
                UnitInfo unitInfo = new UnitInfo();

                unitInfo.UnitId = unit.Id;
                unitInfo.Description = unit.Description;
                unitInfo.Name = unit.Name;
                unitInfo.Abbreviation = unit.Abbreviation;

                if (unit.Name.ToLower() == "none")
                {
                    foreach (DataType fullDataType in allDataypes)
                    {
                        DataTypeInfo dataTypeInfo = new DataTypeInfo();
                        dataTypeInfo.DataTypeId = fullDataType.Id;
                        dataTypeInfo.Description = fullDataType.Description;
                        dataTypeInfo.Name = fullDataType.Name;

                        unitInfo.DataTypeInfos.Add(dataTypeInfo);
                    }
                    model.AvailableUnits.Add(unitInfo);
                }
                else
                {
                    foreach (DataType dummyDataType in unit.AssociatedDataTypes)
                    {
                        DataTypeInfo dataTypeInfo = new DataTypeInfo();

                        DataType fullDataType = dataTypeManager.Repo.Get(dummyDataType.Id);
                        dataTypeInfo.DataTypeId = fullDataType.Id;
                        dataTypeInfo.Description = fullDataType.Description;
                        dataTypeInfo.Name = fullDataType.Name;

                        unitInfo.DataTypeInfos.Add(dataTypeInfo);
                    }
                    model.AvailableUnits.Add(unitInfo);
                }
            }

            if (!TaskManager.Bus.ContainsKey(EasyUploadTaskManager.VERIFICATION_AVAILABLEUNITS))
            {
                TaskManager.AddToBus(EasyUploadTaskManager.VERIFICATION_AVAILABLEUNITS, model.AvailableUnits);
            }
        
            string filePath = TaskManager.Bus[EasyUploadTaskManager.FILEPATH].ToString();
            string selectedHeaderAreaJson = TaskManager.Bus[EasyUploadTaskManager.SHEET_HEADER_AREA].ToString();

            FileStream fis = null;
            fis = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            ExcelPackage ep = new ExcelPackage(fis);
            fis.Close();

            ExcelWorkbook excelWorkbook = ep.Workbook;
            ExcelWorksheet firstWorksheet = excelWorkbook.Worksheets[1];

            string sheetFormatString = Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.SHEET_FORMAT]);

            SheetFormat sheetFormat = 0;
            Enum.TryParse<SheetFormat>(sheetFormatString, true, out sheetFormat);

            model.HeaderFields = GetExcelHeaderFields(firstWorksheet, sheetFormat, selectedHeaderAreaJson).ToArray();

            if (!TaskManager.Bus.ContainsKey(EasyUploadTaskManager.VERIFICATION_HEADERFIELDS))
            {
                TaskManager.AddToBus(EasyUploadTaskManager.VERIFICATION_HEADERFIELDS, model.HeaderFields);
            }


            model.Suggestions = new Dictionary<int, List<Tuple<string, string, string, string, string>>>();

            for (int i = 0; i < model.HeaderFields.Length; i++)
            {
                UnitInfo currentUnitInfo = (UnitInfo)model.AvailableUnits.FirstOrDefault().Clone();
                DataTypeInfo dtinfo = currentUnitInfo.DataTypeInfos.FirstOrDefault();
                currentUnitInfo.SelectedDataTypeId = dtinfo.DataTypeId;
                ViewData["defaultDatatypeID"] = dtinfo.DataTypeId;

                if (model.AssignedHeaderUnits.Where(t => t.Item1 == i).FirstOrDefault() == null)
                {
                    model.AssignedHeaderUnits.Add(new Tuple<int, string, UnitInfo>(i, model.HeaderFields[i], currentUnitInfo));
                }

                #region suggestions
                //Add a variable to the suggestions if the names are similar
                model.Suggestions.Add(i, new List<Tuple<string, string, string, string, string>>());

                //Old: Use the levenshtein distance directly
                //IEnumerable<Variable> suggestions = allVariables.Where(var => (levenshtein(var.Label, model.HeaderFields[i]) <= 3));

                //Calculate similarity metric
                //Accept suggestion if the variables have equal names or the similarity is greater than some threshold
                double threshold = 0.5;
                IEnumerable<DataAttribute> suggestions = allDataAttributes.Where(att => similarity(att.Name, model.HeaderFields[i]) >= threshold);

                //Order the suggestions according to the levenshtein metric
                /*List<Variable> ordered = suggestions.ToList<Variable>();
                ordered.Sort((x, y) => (levenshtein(x.Label, model.HeaderFields[i])).CompareTo(levenshtein(y.Label, model.HeaderFields[i])));*/

                //Order the suggestions according to the similarity (can this differ from the levenshtein metric ordering?)
                List<DataAttribute> ordered = suggestions.ToList<DataAttribute>();
                ordered.Sort((x, y) => (similarity(y.Name, model.HeaderFields[i])).CompareTo(similarity(x.Name, model.HeaderFields[i])));

                //Add the ordered suggestions to the model
                foreach (DataAttribute att in ordered)
                {
                    model.Suggestions[i].Add(new Tuple<String, String, String, String, String>(att.Name, att.Unit.Id.ToString(), att.DataType.Id.ToString(), att.Unit.Name, att.DataType.Name));
                }

                //Use the following to order suggestions alphabetically instead of ordering according to the metric
                //model.Suggestions[i] = model.Suggestions[i].Distinct().OrderBy(s => s.Item1).ToList<Tuple<String, String, String, String, String>>();

                //Each Name-Unit-Datatype-Tuple should be unique
                model.Suggestions[i] = model.Suggestions[i].Distinct().ToList<Tuple<String, String, String, String, String>>();
                #endregion
            }


            TaskManager.AddToBus(EasyUploadTaskManager.VERIFICATION_ATTRIBUTESUGGESTIONS, model.Suggestions);

            TaskManager.AddToBus(EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS, model.AssignedHeaderUnits);

            //set current stepinfo based on index
            if (TaskManager != null)
            {
                TaskManager.SetCurrent(index);

                // remove if existing
                TaskManager.RemoveExecutedStep(TaskManager.Current());
            }



            //model.JsonTableData = jsonTable;

            // when jumping back to this step
            // check if dataset selected
            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.SHEET_JSON_DATA))
            {
                if (!String.IsNullOrEmpty(Convert.ToString(TaskManager.Bus[EasyUploadTaskManager.SHEET_JSON_DATA])))
                {
                    //model.JsonTableData = TaskManager.Bus[TaskManager.SHEET_JSON_DATA].ToString();
                }
            }

            model.StepInfo = TaskManager.Current();

            return PartialView(model);

        }

        [HttpPost]
        public ActionResult Verification(object[] data)
        {
            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];
            SelectVerificationModel model = new SelectVerificationModel();
            model.StepInfo = TaskManager.Current();

            if (TaskManager != null)
            {
                TaskManager.Current().SetValid(false);



                if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS))
                {
                    List<Tuple<int, String, UnitInfo>> mappedHeaderUnits = (List<Tuple<int, String, UnitInfo>>)TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS];
                    foreach (Tuple<int, String, UnitInfo> tuple in mappedHeaderUnits)
                    {
                        if (tuple.Item3.SelectedDataTypeId < 0)
                        {
                            tuple.Item3.SelectedDataTypeId = tuple.Item3.DataTypeInfos.FirstOrDefault().DataTypeId;
                        }
                    }
                    model.AssignedHeaderUnits = mappedHeaderUnits;
                    
                    TaskManager.Current().SetValid(true);
                }
                else
                {
                    model.ErrorList.Add(new Error(ErrorType.Other, "Some Areas are not selected."));
                }

                if (TaskManager.Current().valid == true)
                {
                    TaskManager.AddExecutedStep(TaskManager.Current());
                    TaskManager.GoToNext();
                    Session["TaskManager"] = TaskManager;
                    ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                    return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
                }
                else
                {
                    TaskManager.Current().SetStatus(StepStatus.error);

                    //reload model
                    model.StepInfo = TaskManager.Current();
                }
            }

            UnitInfo currentUnitInfo = (UnitInfo)model.AvailableUnits.First().Clone();
            DataTypeInfo dtinfo = currentUnitInfo.DataTypeInfos.First();
            ViewData["defaultDatatypeID"] = dtinfo.DataTypeId;

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult SaveUnitSelection()
        {

            int? selectFieldId = null;
            int? selectOptionId = null;

            //Keys submitted by Javascript in Verification.cshtml
            foreach (string key in Request.Form.AllKeys)
            {
                if ("selectFieldId" == key)
                {

                    selectFieldId = Convert.ToInt32(Request.Form[key]);
                }
                if ("selectOptionId" == key)
                {
                    selectOptionId = Convert.ToInt32(Request.Form[key]);
                }
            }

            SelectVerificationModel model = new SelectVerificationModel();

            EasyUploadTaskManager TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS))
            {
                model.AssignedHeaderUnits = (List<Tuple<int, string, UnitInfo>>)TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS];
            }

            /*
             * Find the selected unit and adjust the AssignedHeaderUnits
             * */
            if (selectFieldId != null && selectOptionId != null)
            {
                List<UnitInfo> availableUnits = (List<UnitInfo>)TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_AVAILABLEUNITS];
                string[] headerFields = (string[])TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_HEADERFIELDS];

                string currentHeader = headerFields.ElementAt((int)selectFieldId);
                UnitInfo currentUnit = availableUnits.Where(u => u.UnitId == selectOptionId).FirstOrDefault();

                Tuple<int, string, UnitInfo> existingTuple = model.AssignedHeaderUnits.Where(t => t.Item1 == (int)selectFieldId).FirstOrDefault();
                if (existingTuple != null)
                {
                    model.AssignedHeaderUnits.Remove(existingTuple);
                }
                model.AssignedHeaderUnits.Add(new Tuple<int, string, UnitInfo>((int)selectFieldId, currentHeader, currentUnit));
            }

            TaskManager.AddToBus(EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS, model.AssignedHeaderUnits);

            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.VERIFICATION_HEADERFIELDS))
            {
                model.HeaderFields = (string[])TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_HEADERFIELDS];
            }

            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.VERIFICATION_AVAILABLEUNITS))
            {
                model.AvailableUnits = (List<UnitInfo>)TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_AVAILABLEUNITS];
            }

            model.Suggestions = (Dictionary<int, List<Tuple<String, String, String, String, String>>>)TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_ATTRIBUTESUGGESTIONS];


            Session["TaskManager"] = TaskManager;


            //create Model
            model.StepInfo = TaskManager.Current();

            //Submit default datatype id
            UnitInfo currentUnitInfo = (UnitInfo)model.AvailableUnits.FirstOrDefault().Clone();
            DataTypeInfo dtinfo = currentUnitInfo.DataTypeInfos.FirstOrDefault();
            ViewData["defaultDatatypeID"] = dtinfo.DataTypeId;

            return PartialView("Verification", model);

        }

        /*
         * Saves the selected datatype in the MappedheaderUnits and saves them on the bus
         * */
        [HttpPost]
        public ActionResult SaveDataTypeSelection()
        {

            int? selectFieldId = null;
            int? selectedDataTypeId = null;

            //Keys submitted by Javascript in Verification.cshtml
            foreach (string key in Request.Form.AllKeys)
            {
                if ("headerfieldId" == key)
                {

                    selectFieldId = Convert.ToInt32(Request.Form[key]);
                }
                if ("selectedDataTypeId" == key)
                {
                    selectedDataTypeId = Convert.ToInt32(Request.Form[key]);
                }
            }

            SelectVerificationModel model = new SelectVerificationModel();

            EasyUploadTaskManager TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS))
            {
                model.AssignedHeaderUnits = (List<Tuple<int, string, UnitInfo>>)TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS];
            }
            List<Tuple<int, string, UnitInfo>> newAssignedHeaderUnits = new List<Tuple<int, string, UnitInfo>>();
            List<Tuple<int, string, UnitInfo>> oldAssignedHeaderUnits = model.AssignedHeaderUnits;


            //Reset the name of the variable and save the new Datatype
            string[] headerFields = (string[])TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_HEADERFIELDS];
            string currentHeader = headerFields.ElementAt((int)selectFieldId);
            Tuple<int, string, UnitInfo> existingTuple = oldAssignedHeaderUnits.Where(t => t.Item1 == selectFieldId).FirstOrDefault();
            int j = model.AssignedHeaderUnits.FindIndex(i => i.Equals(existingTuple));
            model.AssignedHeaderUnits[j] = new Tuple<int, string, UnitInfo>(existingTuple.Item1, currentHeader, existingTuple.Item3);
            model.AssignedHeaderUnits[j].Item3.SelectedDataTypeId = Convert.ToInt32(selectedDataTypeId);

            TaskManager.AddToBus(EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS, oldAssignedHeaderUnits);

            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.VERIFICATION_HEADERFIELDS))
            {
                model.HeaderFields = (string[])TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_HEADERFIELDS];
            }

            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.VERIFICATION_AVAILABLEUNITS))
            {
                model.AvailableUnits = (List<UnitInfo>)TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_AVAILABLEUNITS];
            }

            model.Suggestions = (Dictionary<int, List<Tuple<String, String, String, String, String>>>)TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_ATTRIBUTESUGGESTIONS];

            Session["TaskManager"] = TaskManager;


            //create Model
            model.StepInfo = TaskManager.Current();

            //Submit default datatype id
            UnitInfo currentUnitInfo = (UnitInfo)model.AvailableUnits.FirstOrDefault().Clone();
            DataTypeInfo dtinfo = currentUnitInfo.DataTypeInfos.FirstOrDefault();
            ViewData["defaultDatatypeID"] = dtinfo.DataTypeId;

            return PartialView("Verification", model);

        }

        /*
         * Saves the name of the selected suggestion into the mapped-header-units
         * */
        [HttpPost]
        public ActionResult SaveSuggestionSelection()
        {
            SelectVerificationModel model = new SelectVerificationModel();

            int? selectFieldId = null;
            int? selectedUnitId = null;
            int? selectedDatatypeId = null;
            string selectedVariableName = null;

            //Keys submitted by Javascript in Verification.cshtml
            foreach (string key in Request.Form.AllKeys)
            {
                if ("headerfieldId" == key)
                {

                    selectFieldId = Convert.ToInt32(Request.Form[key]);
                }
                if ("selectedVariableName" == key)
                {
                    selectedVariableName = Convert.ToString(Request.Form[key]);
                }
                if ("selectedUnitId" == key)
                {
                    selectedUnitId = Convert.ToInt32(Request.Form[key]);
                }
                if ("selectedDataTypeId" == key)
                {
                    var test = Request.Form[key];
                    selectedDatatypeId = Convert.ToInt32(Request.Form[key]);
                }
            }

            EasyUploadTaskManager TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS))
            {
                model.AssignedHeaderUnits = (List<Tuple<int, string, UnitInfo>>)TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS];
            }

            /*
             * Copy the assignedHeaderUnits, change the entry where the suggestion was selected for
             * */
            if (selectFieldId != null)
            {
                //Find the position of the Tuple that is about to be changed
                Tuple<int, string, UnitInfo> exTuple = model.AssignedHeaderUnits.Where(t => t.Item1 == selectFieldId).FirstOrDefault();
                int i = model.AssignedHeaderUnits.FindIndex(t => t.Equals(exTuple));
                //Insert a new Tuple at this position
                model.AssignedHeaderUnits[i] = new Tuple<int, string, UnitInfo>(exTuple.Item1, selectedVariableName, exTuple.Item3);
            }


            //Save unit and datatype
            if (selectFieldId != null && selectedUnitId != null)
            {
                //Get all units
                List<UnitInfo> availableUnits = (List<UnitInfo>)TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_AVAILABLEUNITS];

                //Get the current unit and clone it
                UnitInfo currentUnit = (UnitInfo)availableUnits.Where(u => u.UnitId == selectedUnitId).FirstOrDefault().Clone();
                currentUnit.SelectedDataTypeId = Convert.ToInt32(selectedDatatypeId);

                //Find the index of the suggestion that is about to be changed
                Tuple<int, string, UnitInfo> existingTuple = model.AssignedHeaderUnits.Where(t => t.Item1 == (int)selectFieldId).FirstOrDefault();
                int j = model.AssignedHeaderUnits.FindIndex(t => t.Equals(existingTuple));
                //Save the new unit with the new datatype
                model.AssignedHeaderUnits[j] = new Tuple<int, string, UnitInfo>(existingTuple.Item1, selectedVariableName, currentUnit);
            }

            TaskManager.AddToBus(EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS, model.AssignedHeaderUnits);

            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.VERIFICATION_HEADERFIELDS))
            {
                model.HeaderFields = (string[])TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_HEADERFIELDS];
            }

            if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.VERIFICATION_AVAILABLEUNITS))
            {
                model.AvailableUnits = (List<UnitInfo>)TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_AVAILABLEUNITS];
            }

            model.Suggestions = (Dictionary<int, List<Tuple<String, String, String, String, String>>>)TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_ATTRIBUTESUGGESTIONS];

            Session["TaskManager"] = TaskManager;

            //create Model
            model.StepInfo = TaskManager.Current();

            //Submit default datatype id
            UnitInfo currentUnitInfo = (UnitInfo)model.AvailableUnits.FirstOrDefault().Clone();
            DataTypeInfo dtinfo = currentUnitInfo.DataTypeInfos.FirstOrDefault();
            ViewData["defaultDatatypeID"] = dtinfo.DataTypeId;

            return PartialView("Verification", model);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="excelWorksheet"></param>
        /// <param name="sheetFormat"></param>
        /// <param name="selectedAreaJsonArray"></param>
        /// <returns></returns>
        private List<String> GetExcelHeaderFields(ExcelWorksheet excelWorksheet, SheetFormat sheetFormat, string selectedAreaJsonArray)
        {
            List<String> headerValues = new List<string>();

            JavaScriptSerializer serializer = new JavaScriptSerializer();
            int[] areaValues = serializer.Deserialize<int[]>(selectedAreaJsonArray);

            if (areaValues.Length != 4)
            {
                throw new InvalidOperationException("Given JSON string for selected area got an invalid length of [" + Convert.ToString(areaValues.Length) + "]");
            }

            SheetArea selectedArea = new SheetArea(areaValues[1], areaValues[3], areaValues[0], areaValues[2]);


            switch (sheetFormat)
            {
                case SheetFormat.TopDown:
                    headerValues = GetExcelHeaderFieldsLeftRight(excelWorksheet, selectedArea);
                    break;
                case SheetFormat.LeftRight:
                    headerValues = GetExcelHeaderFieldsTopDown(excelWorksheet, selectedArea);
                    break;
                case SheetFormat.Matrix:
                    headerValues.AddRange(GetExcelHeaderFieldsLeftRight(excelWorksheet, selectedArea));
                    headerValues.AddRange(GetExcelHeaderFieldsTopDown(excelWorksheet, selectedArea));
                    break;
                default:
                    break;
            }

            return headerValues;
        }

        /// <summary>
        /// Gets all values from selected header area. This method is for left to right scheme, so the header fields are in one coulumn
        /// </summary>
        /// <param name="excelWorksheet">ExcelWorksheet with the data</param>
        /// <param name="selectedArea">Defined header area with start and end for rows and columns</param>
        /// <returns>Simple list with values of the header fields as string</returns>
        private List<String> GetExcelHeaderFieldsLeftRight(ExcelWorksheet excelWorksheet, SheetArea selectedArea)
        {
            List<String> headerValues = new List<string>();

            ExcelCellAddress SheetStartCell = excelWorksheet.Dimension.Start;
            ExcelCellAddress SheetEndCell = excelWorksheet.Dimension.End;

            // constant, because just one row is for header allowed
            int Row = selectedArea.StartRow + 1;

            #region Validation
            bool isStartColumnValid = selectedArea.StartColumn + 1 >= SheetStartCell.Column;
            bool isEndColumnValid = selectedArea.EndColumn + 1 <= SheetEndCell.Column;
            bool isStartRowValid = selectedArea.StartRow + 1 >= SheetStartCell.Row;
            bool isEndRowValid = selectedArea.EndRow + 1 <= SheetEndCell.Row;


            if (!isStartColumnValid || !isStartRowValid || !isEndColumnValid || !isEndRowValid)
            {
                throw new InvalidOperationException("Selected area is not located in given excel sheet.");
            }
            #endregion

            for (int Column = selectedArea.StartColumn + 1; Column <= selectedArea.EndColumn + 1; Column++)
            {
                ExcelRange cell = excelWorksheet.Cells[Row, Column];

                string headerText = "";

                if (cell.Value != null)
                {
                    headerText = cell.Value.ToString();
                }

                headerValues.Add(headerText);
            }


            return headerValues;
        }

        /// <summary>
        /// Gets all values from selected header area. This method is for top to down scheme, so the header fields are in one row
        /// </summary>
        /// <param name="excelWorksheet">ExcelWorksheet with the data</param>
        /// <param name="selectedArea">Defined header area with start and end for rows and columns</param>
        /// <returns>Simple list with values of the header fields as string</returns>
        private List<String> GetExcelHeaderFieldsTopDown(ExcelWorksheet excelWorksheet, SheetArea selectedArea)
        {
            List<String> headerValues = new List<string>();

            ExcelCellAddress SheetStartCell = excelWorksheet.Dimension.Start;
            ExcelCellAddress SheetEndCell = excelWorksheet.Dimension.End;

            #region Validation
            bool isStartColumnValid = selectedArea.StartColumn >= SheetStartCell.Column;
            bool isEndColumnValid = selectedArea.EndColumn <= SheetEndCell.Column;
            bool isStartRowValid = selectedArea.StartRow >= SheetStartCell.Row;
            bool isEndRowValid = selectedArea.EndRow <= SheetEndCell.Row;


            if (!isStartColumnValid || !isStartRowValid || !isEndColumnValid || !isEndRowValid)
            {
                throw new InvalidOperationException("Selected area is not located in given excel sheet.");
            }
            #endregion

            int Column = selectedArea.StartColumn;

            for (int Row = selectedArea.StartRow; Row >= selectedArea.EndRow; Row++)
            {
                ExcelRange cell = excelWorksheet.Cells[Row, Column];

                string headerText = "";

                if (cell.Value != null)
                {
                    headerText = cell.Value.ToString();
                }

                headerValues.Add(headerText);
            }

            return headerValues;
        }


        /*
         * Validates each Data row and returns a JSON-Object with the errors (if there are any)
         * */
        [HttpPost]
        public ActionResult ValidateSelection()
        {
            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

            string JsonArray = TaskManager.Bus[EasyUploadTaskManager.SHEET_JSON_DATA].ToString();




            List<Error> ErrorList = ValidateRows(JsonArray);
            List<ErrorInfo> ErrorMessageList = new List<ErrorInfo>();

            if (ErrorList.Count <= 50)
            {
                foreach (Error error in ErrorList)
                {
                    ErrorMessageList.Add(new ErrorInfo(error));
                }
            }
            else
            {
                for (int i = 0; i <= 50; i++)
                {
                    ErrorMessageList.Add(new ErrorInfo(ErrorList[i]));
                }
            }

            return Json(new { errors = ErrorMessageList.ToArray(), errorCount = (ErrorList.Count) });
        }

        /*
         * Uses a DataTypeCheck to determin, wether the selected datatypes are suitable
         * */
        private List<Error> ValidateRows(string JsonArray)
        {
            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];
            var serializer = new JavaScriptSerializer();
            string[][] DeserializedJsonArray = serializer.Deserialize<string[][]>(JsonArray);

            List<Error> ErrorList = new List<Error>();
            List<Tuple<int, string, UnitInfo>> MappedHeaders = (List<Tuple<int, string, UnitInfo>>)TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS];
            Tuple<int, string, UnitInfo>[] MappedHeadersArray = MappedHeaders.ToArray();
            DataTypeManager dtm = new DataTypeManager();


            string DataArea = TaskManager.Bus[EasyUploadTaskManager.SHEET_DATA_AREA].ToString();
            int[] IntDataArea = serializer.Deserialize<int[]>(DataArea);

            string[,] SelectedDataArea = new string[(IntDataArea[2] - IntDataArea[0]), (IntDataArea[3] - IntDataArea[1])];

            for (int y = IntDataArea[0]; y <= IntDataArea[2]; y++)
            {
                for (int x = IntDataArea[1]; x <= IntDataArea[3]; x++)
                {
                    int SelectedY = y - (IntDataArea[0]);
                    int SelectedX = x - (IntDataArea[1]);
                    string vv = DeserializedJsonArray[y][x];

                    Tuple<int, string, UnitInfo> mappedHeader = MappedHeaders.Where(t => t.Item1 == SelectedX).FirstOrDefault();

                    DataType datatype = null;

                    if (mappedHeader.Item3.SelectedDataTypeId == -1)
                    {
                        datatype = dtm.Repo.Get(mappedHeader.Item3.DataTypeInfos.FirstOrDefault().DataTypeId);
                    }
                    else
                    {
                        datatype = dtm.Repo.Get(mappedHeader.Item3.SelectedDataTypeId);
                    }


                    string datatypeName = datatype.SystemType;

                    DataTypeCheck dtc;
                    // TODO: make it nicer!
                    double DummyValue = 0;
                    if (Double.TryParse(vv, out DummyValue))
                    {
                        if (vv.Contains("."))
                        {
                            dtc = new DataTypeCheck(mappedHeader.Item2, datatypeName, DecimalCharacter.point);
                        }
                        else
                        {
                            dtc = new DataTypeCheck(mappedHeader.Item2, datatypeName, DecimalCharacter.comma);
                        }
                    }
                    else
                    {
                        dtc = new DataTypeCheck(mappedHeader.Item2, datatypeName, DecimalCharacter.point);
                    }

                    var ValidationResult = dtc.Execute(vv, y);
                    if (ValidationResult is Error)
                    {
                        ErrorList.Add((Error)ValidationResult);
                    }
                }
            }

            return ErrorList;
        }


        #region private methods

        #region helpers

        /*
         * String-distance used for making suggestions for the variable names
         * */
        private Int32 levenshtein(String a, String b)
        {

            if (string.IsNullOrEmpty(a))
            {
                if (!string.IsNullOrEmpty(b))
                {
                    return b.Length;
                }
                return 0;
            }

            if (string.IsNullOrEmpty(b))
            {
                if (!string.IsNullOrEmpty(a))
                {
                    return a.Length;
                }
                return 0;
            }

            Int32 cost;
            Int32[,] d = new int[a.Length + 1, b.Length + 1];
            Int32 min1;
            Int32 min2;
            Int32 min3;

            for (Int32 i = 0; i <= d.GetUpperBound(0); i += 1)
            {
                d[i, 0] = i;
            }

            for (Int32 i = 0; i <= d.GetUpperBound(1); i += 1)
            {
                d[0, i] = i;
            }

            for (Int32 i = 1; i <= d.GetUpperBound(0); i += 1)
            {
                for (Int32 j = 1; j <= d.GetUpperBound(1); j += 1)
                {
                    cost = Convert.ToInt32(!(a[i - 1] == b[j - 1]));

                    min1 = d[i - 1, j] + 1;
                    min2 = d[i, j - 1] + 1;
                    min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }

            return d[d.GetUpperBound(0), d.GetUpperBound(1)];

        }

        /* String-similarity computed with levenshtein-distance
         * */
        private double similarity(string a, string b)
        {
            if (a.Equals(b))
            {
                return 1.0;
            }
            else
            {
                if (!(a.Length == 0 || b.Length == 0))
                {
                    double sim = 1 - (levenshtein(a, b) / Convert.ToDouble(Math.Min(a.Length, b.Length)));
                    return sim;
                }
                else
                    return 0.0;
            }
        }

        #endregion

        #endregion
    }
}