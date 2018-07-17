using BExIS.Dcm.UploadWizard;
using BExIS.Dcm.Wizard;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.IO.Transform.Validation.ValueCheck;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Modules.Dcm.UI.Models;
using BExIS.Utils.Models;
using F23.StringSimilarity;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.WebControls;
using Vaiona.Persistence.Api;
using Vaiona.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class EasyUploadVerificationController : BaseController
    {
        private EasyUploadTaskManager TaskManager;

        private static string UNIT_ID = "UNIT_ID";
        private static string ATTRIBUTE_ID = "ATTRIBUTE_ID";
        private static string DATATYPE_ID = "DATATYPE_ID";

        //
        // GET: /DCM/SubmitSelectAreas/

        [HttpGet]
        public ActionResult Verification(int index)
        {

            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

            List<Dlm.Entities.DataStructure.Unit> tempUnitList = new List<Dlm.Entities.DataStructure.Unit>();
            List<DataType> allDataypes = new List<DataType>();
            List<DataAttribute> allDataAttributes = new List<DataAttribute>();

            using (IUnitOfWork unitOfWork = this.GetUnitOfWork())
            {

                //set current stepinfo based on index
                if (TaskManager != null)
                {
                    TaskManager.SetCurrent(index);

                    // remove if existing
                    TaskManager.RemoveExecutedStep(TaskManager.Current());
                }

                SelectVerificationModel model = new SelectVerificationModel();
                List<DataTypeInfo> dataTypeInfos = new List<DataTypeInfo>();
                List<UnitInfo> unitInfos = new List<UnitInfo>();
                List<DataAttrInfo> dataAttributeInfos = new List<DataAttrInfo>();
                List<EasyUploadSuggestion> suggestions = new List<EasyUploadSuggestion>();
                List<string> headers = new List<string>();


                tempUnitList = unitOfWork.GetReadOnlyRepository<Dlm.Entities.DataStructure.Unit>().Get().ToList();
                allDataypes = unitOfWork.GetReadOnlyRepository<DataType>().Get().ToList();
                allDataAttributes = unitOfWork.GetReadOnlyRepository<DataAttribute>().Get().ToList();


                //Important for jumping back to this step
                if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.ROWS))
                {
                    model.Rows = (List<RowModel>)TaskManager.Bus[EasyUploadTaskManager.ROWS];
                }

                // get all DataTypes for each Units
                foreach (Dlm.Entities.DataStructure.Unit unit in tempUnitList)
                {
                    UnitInfo unitInfo = new UnitInfo();

                    unitInfo.UnitId = unit.Id;
                    unitInfo.Description = unit.Description;
                    unitInfo.Name = unit.Name;
                    unitInfo.Abbreviation = unit.Abbreviation;
                    unitInfo.DimensionId = unit.Dimension.Id;

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

                        unitInfos.Add(unitInfo);
                    }
                    else
                    {
                        Boolean hasDatatype = false; //Make sure only units that have at least one datatype are shown

                        foreach (DataType dummyDataType in unit.AssociatedDataTypes)
                        {
                            if (!hasDatatype)
                                hasDatatype = true;

                            DataTypeInfo dataTypeInfo = new DataTypeInfo();

                            DataType fullDataType = allDataypes.Where(p => p.Id == dummyDataType.Id).FirstOrDefault();
                            dataTypeInfo.DataTypeId = fullDataType.Id;
                            dataTypeInfo.Description = fullDataType.Description;
                            dataTypeInfo.Name = fullDataType.Name;

                            unitInfo.DataTypeInfos.Add(dataTypeInfo);
                        }
                        if (hasDatatype)
                            unitInfos.Add(unitInfo);
                    }
                }

                //Sort the units by name
                unitInfos.Sort(delegate (UnitInfo u1, UnitInfo u2)
                {
                    return String.Compare(u1.Name, u2.Name, StringComparison.InvariantCultureIgnoreCase);
                });


                TaskManager.AddToBus(EasyUploadTaskManager.VERIFICATION_AVAILABLEUNITS, unitInfos);

                // all datatypesinfos 
                dataTypeInfos = unitInfos.SelectMany(u => u.DataTypeInfos).GroupBy(d => d.DataTypeId).Select(g => g.Last()).ToList();
                TaskManager.AddToBus(EasyUploadTaskManager.ALL_DATATYPES, dataTypeInfos);


                //Setall Data AttrInfos to Session -> default
                allDataAttributes.ForEach(d => dataAttributeInfos.Add(new DataAttrInfo(d.Id, d.Unit.Id, d.DataType.Id, d.Description, d.Name, d.Unit.Dimension.Id)));
                Session["DataAttributes"] = dataAttributeInfos;



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

                headers = GetExcelHeaderFields(firstWorksheet, sheetFormat, selectedHeaderAreaJson);

                if (!TaskManager.Bus.ContainsKey(EasyUploadTaskManager.VERIFICATION_HEADERFIELDS))
                {
                    TaskManager.AddToBus(EasyUploadTaskManager.VERIFICATION_HEADERFIELDS, headers);
                }


                suggestions = new List<EasyUploadSuggestion>();

                foreach (string varName in headers)
                {
                    #region suggestions

                    //Add a variable to the suggestions if the names are similar
                    suggestions = getSuggestions(varName, dataAttributeInfos);

                    #endregion

                    //set rowmodel
                    RowModel row = new RowModel(
                        headers.IndexOf(varName),
                        varName,
                        null,
                        null,
                        null,
                        suggestions,
                        unitInfos,
                        dataAttributeInfos,
                        dataTypeInfos
                        );

                    model.Rows.Add(row);

                    TaskManager.AddToBus(EasyUploadTaskManager.ROWS, model.Rows);
                    TaskManager.AddToBus(EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS, RowsToTuples());
                }

                TaskManager.AddToBus(EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS, headers);

                model.StepInfo = TaskManager.Current();

                return PartialView(model);
            }
        }

        [HttpPost]
        public ActionResult Verification(object[] data)
        {
            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];
            SelectVerificationModel model = new SelectVerificationModel();
            model.StepInfo = TaskManager.Current();

            //convert all rowmodels to tuples for the next steps
            if (TaskManager != null)
            {
                TaskManager.Current().SetValid(false);

                if (TaskManager.Bus.ContainsKey(EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS))
                {
                    List<Tuple<int, String, UnitInfo>> mappedHeaderUnits = RowsToTuples();
                    TaskManager.AddToBus(EasyUploadTaskManager.VERIFICATION_MAPPEDHEADERUNITS, mappedHeaderUnits);

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


            return PartialView(model);
        }

        [HttpPost]
        public ActionResult SaveSelection(int index, long selectedUnit, long selectedDataType, long selectedAttribute, string varName, string lastSelection)
        {

            List<DataTypeInfo> dataTypeInfos = new List<DataTypeInfo>();
            List<UnitInfo> unitInfos = new List<UnitInfo>();
            List<DataAttrInfo> dataAttributeInfos = new List<DataAttrInfo>();
            List<EasyUploadSuggestion> suggestions = new List<EasyUploadSuggestion>();
            List<string> headers = new List<string>();


            EasyUploadTaskManager TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

            //suggestions

            #region load all lists

            //dataattr
            if (Session["DataAttributes"] != null)
            {
                dataAttributeInfos = (List<DataAttrInfo>)Session["DataAttributes"];
            }

            if (!string.IsNullOrEmpty(varName))
            {
                suggestions = getSuggestions(varName, dataAttributeInfos);
            }

            unitInfos = (List<UnitInfo>)TaskManager.Bus[EasyUploadTaskManager.VERIFICATION_AVAILABLEUNITS];

            dataTypeInfos = (List<DataTypeInfo>)TaskManager.Bus[EasyUploadTaskManager.ALL_DATATYPES];

            #endregion


            #region load current seleted items

            UnitInfo currentUnit = unitInfos.FirstOrDefault(u => u.UnitId == selectedUnit);
            DataTypeInfo currentDataTypeInfo = dataTypeInfos.FirstOrDefault(d => d.DataTypeId.Equals(selectedDataType));
            DataAttrInfo currentDataAttrInfo = dataAttributeInfos.FirstOrDefault(da => da.Id.Equals(selectedAttribute));

            #endregion

            #region filtering



            if (currentUnit != null)
            {
                unitInfos = unitInfos.Where(u => u.UnitId.Equals(currentUnit.UnitId) || u.DimensionId.Equals(currentUnit.DimensionId)).ToList();

                // filtering data attrs where the attr has the unit or there dimension
                if (selectedAttribute == 0) dataAttributeInfos = dataAttributeInfos.Where(d => d.UnitId.Equals(currentUnit.UnitId) || d.DimensionId.Equals(currentUnit.DimensionId)).ToList();
                else dataAttributeInfos = dataAttributeInfos.Where(d => d.Id.Equals(currentDataAttrInfo.Id)).ToList();

                if (selectedDataType == 0) dataTypeInfos = dataTypeInfos.Where(d => currentUnit.DataTypeInfos.Any(ud => ud.DataTypeId.Equals(d.DataTypeId))).ToList();
                else
                {
                    dataTypeInfos = dataTypeInfos.Where(dt => dt.DataTypeId.Equals(currentDataTypeInfo.DataTypeId)).ToList();

                    // not checkt
                    var datatype = dataAttributeInfos.First();
                    var unit = unitInfos.First();

                    unit.SelectedDataTypeId = datatype.Id;
                }
            }

            if (currentDataTypeInfo != null)
            {
                dataTypeInfos = dataTypeInfos.Where(dt => dt.DataTypeId.Equals(currentDataTypeInfo.DataTypeId)).ToList();

                if (selectedAttribute == 0) dataAttributeInfos = dataAttributeInfos.Where(d => d.DataTypeId.Equals(currentDataTypeInfo.DataTypeId)).ToList();
                else dataAttributeInfos = dataAttributeInfos.Where(d => d.Id.Equals(currentDataAttrInfo.Id)).ToList();

                if (selectedUnit == 0) unitInfos = unitInfos.Where(u => u.DataTypeInfos.Any(d => d.DataTypeId.Equals(currentDataTypeInfo.DataTypeId))).ToList();
                else unitInfos.Where(u => u.UnitId.Equals(currentUnit.UnitId) || u.DimensionId.Equals(currentUnit.DimensionId)).ToList();
            }


            if (currentDataAttrInfo != null)
            {
                // is the seletced currentDataAttrInfo a suggestion then overrigth all selected items
                if (suggestions.Any(s => s.attributeName.Equals(currentDataAttrInfo.Name)))
                {
                    dataAttributeInfos = dataAttributeInfos.Where(d => d.Id.Equals(currentDataAttrInfo.Id)).ToList();
                    unitInfos = unitInfos.Where(u => u.UnitId.Equals(currentDataAttrInfo.UnitId) || u.DimensionId.Equals(currentDataAttrInfo.DimensionId)).ToList();
                    dataTypeInfos = unitInfos.SelectMany(u => u.DataTypeInfos).GroupBy(d => d.DataTypeId).Select(g => g.Last()).ToList();

                    if (lastSelection == "Suggestions")
                    {
                        currentUnit = unitInfos.FirstOrDefault(u => u.UnitId.Equals(dataAttributeInfos.First().UnitId));
                        currentDataTypeInfo = dataTypeInfos.FirstOrDefault(d => d.DataTypeId.Equals(dataAttributeInfos.First().DataTypeId));
                    }

                }
                else
                {

                    dataAttributeInfos = dataAttributeInfos.Where(d => d.Id.Equals(currentDataAttrInfo.Id)).ToList();

                    //filtering units when data attr is selected, if id or dimension is the same
                    if (selectedUnit == 0) unitInfos = unitInfos.Where(u => u.UnitId.Equals(currentDataAttrInfo.UnitId) || u.DimensionId.Equals(currentDataAttrInfo.DimensionId)).ToList();
                    else unitInfos = unitInfos.Where(u => u.UnitId.Equals(currentUnit.UnitId) || u.DimensionId.Equals(currentUnit.DimensionId)).ToList();

                    if (selectedDataType == 0) dataTypeInfos = unitInfos.SelectMany(u => u.DataTypeInfos).GroupBy(d => d.DataTypeId).Select(g => g.Last()).ToList();
                    else dataTypeInfos = dataTypeInfos.Where(dt => dt.DataTypeId.Equals(currentDataTypeInfo.DataTypeId)).ToList();

                }
            }



            #endregion


            RowModel model = new RowModel(
                    index,
                    varName,
                    currentDataAttrInfo,
                    currentUnit,
                    currentDataTypeInfo,
                    suggestions,
                    unitInfos,
                    dataAttributeInfos,
                    dataTypeInfos
                );

            //update row in the bus of the taskmanager
            UpdateRowInBus(model);

            return PartialView("Row", model);

        }

        #region excel stuff

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

            int[] areaValues = JsonConvert.DeserializeObject<int[]>(selectedAreaJsonArray);

            if (areaValues.Length != 4)
            {
                throw new InvalidOperationException("Given JSON string for selected area got an invalid length of [" + Convert.ToString(areaValues.Length) + "]");
            }

            SheetArea selectedArea = new SheetArea(areaValues[1], areaValues[3], areaValues[0], areaValues[2]);


            switch (sheetFormat)
            {
                case SheetFormat.TopDown:
                    headerValues = GetExcelHeaderFieldsTopDown(excelWorksheet, selectedArea);
                    break;
                case SheetFormat.LeftRight:
                    headerValues = GetExcelHeaderFieldsLeftRight(excelWorksheet, selectedArea);
                    break;
                case SheetFormat.Matrix:
                    headerValues.AddRange(GetExcelHeaderFieldsTopDown(excelWorksheet, selectedArea));
                    headerValues.AddRange(GetExcelHeaderFieldsLeftRight(excelWorksheet, selectedArea));
                    break;
                default:
                    break;
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

            String jsonTableString = TaskManager.Bus[EasyUploadTaskManager.SHEET_JSON_DATA].ToString();
            String[][] jsonTable = JsonConvert.DeserializeObject<string[][]>(jsonTableString);

            for (int i = selectedArea.StartColumn; i <= selectedArea.EndColumn; i++)
            {
                headerValues.Add(jsonTable[selectedArea.StartRow][i]);
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

            String jsonTableString = TaskManager.Bus[EasyUploadTaskManager.SHEET_JSON_DATA].ToString();
            String[][] jsonTable = JsonConvert.DeserializeObject<string[][]>(jsonTableString);

            for (int row = selectedArea.StartRow; row <= selectedArea.EndColumn; row++)
            {
                headerValues.Add(jsonTable[row][selectedArea.StartColumn]);
            }

            return headerValues;
        }

        #endregion

        /*
         * Validates each Data row and returns a JSON-Object with the errors (if there are any)
         * */
        [HttpPost]
        public ActionResult ValidateSelection()
        {
            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

            string JsonArray = TaskManager.Bus[EasyUploadTaskManager.SHEET_JSON_DATA].ToString();

            List<Tuple<int, Error>> ErrorList = ValidateRows(JsonArray);
            List<Tuple<int, ErrorInfo>> ErrorMessageList = new List<Tuple<int, ErrorInfo>>();

            foreach (Tuple<int, Error> error in ErrorList)
            {
                ErrorMessageList.Add(new Tuple<int, ErrorInfo>(error.Item1, new ErrorInfo(error.Item2)));
            }

            return Json(new { errors = ErrorMessageList.ToArray(), errorCount = (ErrorList.Count) });
        }

        #region private methods

        /// <summary>
        /// Determin whether the selected datatypes are suitable
        /// </summary>
        private List<Tuple<int, Error>> ValidateRows(string JsonArray)
        {
            DataTypeManager dtm = new DataTypeManager();

            try
            {
                const int maxErrorsPerColumn = 20;
                TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

                string[][] DeserializedJsonArray = JsonConvert.DeserializeObject<string[][]>(JsonArray);

                List<Tuple<int, Error>> ErrorList = new List<Tuple<int, Error>>();
                List<RowModel> Rows = (List<RowModel>)TaskManager.Bus[EasyUploadTaskManager.ROWS];
                RowModel[] MappedRowsArray = Rows.ToArray();


                List<string> DataArea = (List<string>)TaskManager.Bus[EasyUploadTaskManager.SHEET_DATA_AREA];
                List<int[]> IntDataAreaList = new List<int[]>();
                foreach (string area in DataArea)
                {
                    IntDataAreaList.Add(JsonConvert.DeserializeObject<int[]>(area));
                }

                foreach (int[] IntDataArea in IntDataAreaList)
                {
                    string[,] SelectedDataArea = new string[(IntDataArea[2] - IntDataArea[0]), (IntDataArea[3] - IntDataArea[1])];

                    for (int x = IntDataArea[1]; x <= IntDataArea[3]; x++)
                    {
                        int errorsInColumn = 0;
                        for (int y = IntDataArea[0]; y <= IntDataArea[2]; y++)
                        {
                            int SelectedY = y - (IntDataArea[0]);
                            int SelectedX = x - (IntDataArea[1]);
                            string vv = DeserializedJsonArray[y][x];

                            RowModel mappedHeader = MappedRowsArray.Where(t => t.Index == SelectedX).FirstOrDefault();

                            DataType datatype = null;
                            if (mappedHeader.SelectedDataType != null && mappedHeader.SelectedDataAttribute != null)
                            {
                                datatype = dtm.Repo.Get(mappedHeader.SelectedDataType.DataTypeId);
                                string datatypeName = datatype.SystemType;

                                #region DataTypeCheck
                                DataTypeCheck dtc;
                                double DummyValue = 0;
                                if (Double.TryParse(vv, out DummyValue))
                                {
                                    if (vv.Contains("."))
                                    {
                                        dtc = new DataTypeCheck(mappedHeader.SelectedDataAttribute.Name, datatypeName, DecimalCharacter.point);
                                    }
                                    else
                                    {
                                        dtc = new DataTypeCheck(mappedHeader.SelectedDataAttribute.Name, datatypeName, DecimalCharacter.comma);
                                    }
                                }
                                else
                                {
                                    dtc = new DataTypeCheck(mappedHeader.SelectedDataAttribute.Name, datatypeName, DecimalCharacter.point);
                                }
                                #endregion

                                var ValidationResult = dtc.Execute(vv, y);
                                if (ValidationResult is Error)
                                {
                                    ErrorList.Add(new Tuple<int, Error>(SelectedX, (Error)ValidationResult));
                                    errorsInColumn++;
                                }

                                if (errorsInColumn >= maxErrorsPerColumn)
                                {
                                    //Break inner (row) loop to jump to the next column
                                    break;
                                }
                            }
                            else
                            {
                                ErrorList.Add(new Tuple<int, Error>(SelectedX, new Error(ErrorType.Other, "No datatype or data attribute selected in row " + SelectedX + 1)));
                            }
                        }
                    }
                }

                return ErrorList;
            }
            finally
            {
                dtm.Dispose();
            }
        }

        ///// <summary>
        ///// Calcualtes the Levenshtein distance between two strings
        ///// </summary>
        ///// Source: https://en.wikibooks.org/wiki/Algorithm_Implementation/Strings/Levenshtein_distance#C.23
        ///// Explanation: https://en.wikipedia.org/wiki/Levenshtein_distance
        //private Int32 levenshtein(String a, String b)
        //{

        //    if (string.IsNullOrEmpty(a))
        //    {
        //        if (!string.IsNullOrEmpty(b))
        //        {
        //            return b.Length;
        //        }
        //        return 0;
        //    }

        //    if (string.IsNullOrEmpty(b))
        //    {
        //        if (!string.IsNullOrEmpty(a))
        //        {
        //            return a.Length;
        //        }
        //        return 0;
        //    }

        //    Int32 cost;
        //    Int32[,] d = new int[a.Length + 1, b.Length + 1];
        //    Int32 min1;
        //    Int32 min2;
        //    Int32 min3;

        //    for (Int32 i = 0; i <= d.GetUpperBound(0); i += 1)
        //    {
        //        d[i, 0] = i;
        //    }

        //    for (Int32 i = 0; i <= d.GetUpperBound(1); i += 1)
        //    {
        //        d[0, i] = i;
        //    }

        //    for (Int32 i = 1; i <= d.GetUpperBound(0); i += 1)
        //    {
        //        for (Int32 j = 1; j <= d.GetUpperBound(1); j += 1)
        //        {
        //            cost = Convert.ToInt32(!(a[i - 1] == b[j - 1]));

        //            min1 = d[i - 1, j] + 1;
        //            min2 = d[i, j - 1] + 1;
        //            min3 = d[i - 1, j - 1] + cost;
        //            d[i, j] = Math.Min(Math.Min(min1, min2), min3);
        //        }
        //    }

        //    return d[d.GetUpperBound(0), d.GetUpperBound(1)];

        //}

        ///// <summary>
        ///// String-similarity computed with levenshtein-distance
        ///// </summary>
        //private double similarityLevenshtein(string a, string b)
        //{
        //    if (a.Equals(b))
        //    {
        //        return 1.0;
        //    }
        //    else
        //    {
        //        if (!(a.Length == 0 || b.Length == 0))
        //        {
        //            double sim = 1 - (levenshtein(a, b) / Convert.ToDouble(Math.Min(a.Length, b.Length)));
        //            return sim;
        //        }
        //        else
        //            return 0.0;
        //    }
        //}

        ///// <summary>
        ///// String-similarity computed with Dice Coefficient
        ///// </summary>
        ///// Source: https://en.wikibooks.org/wiki/Algorithm_Implementation/Strings/Dice%27s_coefficient#C.23
        ///// Explanation: https://en.wikipedia.org/wiki/S%C3%B8rensen%E2%80%93Dice_coefficient
        //private double similarityDiceCoefficient(string a, string b)
        //{
        //    //Workaround for |a| == |b| == 1
        //    if (a.Length <= 1 && b.Length <= 1)
        //    {
        //        if (a.Equals(b))
        //            return 1.0;
        //        else
        //            return 0.0;
        //    }

        //    HashSet<string> setA = new HashSet<string>();
        //    HashSet<string> setB = new HashSet<string>();

        //    for (int i = 0; i < a.Length - 1; ++i)
        //        setA.Add(a.Substring(i, 2));

        //    for (int i = 0; i < b.Length - 1; ++i)
        //        setB.Add(b.Substring(i, 2));

        //    HashSet<string> intersection = new HashSet<string>(setA);
        //    intersection.IntersectWith(setB);

        //    return (2.0 * intersection.Count) / (setA.Count + setB.Count);
        //}

        /// <summary>
        /// Combines multiple String-similarities with equal weight
        /// </summary>
        private double similarity(string a, string b)
        {
            List<double> similarities = new List<double>();
            double output = 0.0;

            var l = new NormalizedLevenshtein();
            similarities.Add(l.Similarity(a, b));
            var jw = new JaroWinkler();
            similarities.Add(jw.Similarity(a, b));
            var jac = new Jaccard();
            similarities.Add(jac.Similarity(a, b));

            foreach (double sim in similarities)
            {
                output += sim;
            }

            return output / similarities.Count;
        }

        private List<EasyUploadSuggestion> getSuggestions(string varName, List<DataAttrInfo> allDataAttributes)
        {
            #region suggestions
            //Add a variable to the suggestions if the names are similar
            List<EasyUploadSuggestion> suggestions = new List<EasyUploadSuggestion>();

            //Calculate similarity metric
            //Accept suggestion if the similarity is greater than some threshold
            double threshold = 0.4;
            IEnumerable<DataAttrInfo> suggestionAttrs = allDataAttributes.Where(att => similarity(att.Name, varName) >= threshold);

            //Order the suggestions according to the similarity
            List<DataAttrInfo> ordered = suggestionAttrs.ToList<DataAttrInfo>();
            ordered.Sort((x, y) => (similarity(y.Name, varName)).CompareTo(similarity(x.Name, varName)));

            //Add the ordered suggestions to the model
            foreach (DataAttrInfo att in ordered)
            {
                suggestions.Add(new EasyUploadSuggestion(att.Id, att.Name, att.UnitId, att.DataTypeId, true));
            }

            //Use the following to order suggestions alphabetically instead of ordering according to the metric
            //model.Suggestions[i] = model.Suggestions[i].Distinct().OrderBy(s => s.attributeName).ToList<EasyUploadSuggestion>();

            //Each Name-Unit-Datatype-Tuple should be unique
            suggestions = suggestions.Distinct().ToList<EasyUploadSuggestion>();
            #endregion

            return suggestions;
        }

        private List<Tuple<int, String, UnitInfo>> RowsToTuples()
        {
            List<Tuple<int, String, UnitInfo>> tmp = new List<Tuple<int, string, UnitInfo>>();
            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

            if (TaskManager != null && TaskManager.Bus[EasyUploadTaskManager.ROWS] != null)
            {
                List<RowModel> rows = (List<RowModel>)TaskManager.Bus[EasyUploadTaskManager.ROWS];

                foreach (var row in rows)
                {
                    tmp.Add(RowToTuple(row));
                }
            }

            return tmp;
        }

        private Tuple<int, String, UnitInfo> RowToTuple(RowModel row)
        {
            return new Tuple<int, string, UnitInfo>((int)row.Index, row.Name, row.SelectedUnit);
        }

        private void UpdateRowInBus(RowModel row)
        {
            TaskManager = (EasyUploadTaskManager)Session["TaskManager"];

            if (TaskManager != null && TaskManager.Bus[EasyUploadTaskManager.ROWS] != null)
            {
                List<RowModel> rows = (List<RowModel>)TaskManager.Bus[EasyUploadTaskManager.ROWS];
                if (rows.Any(r => r.Index.Equals(row.Index)))
                {
                    for (int i = 0; i < rows.Count; i++)
                    {
                        RowModel tmp = rows.ElementAt(i);
                        if (tmp.Index.Equals(row.Index))
                        {
                            rows[i] = row;
                            break;
                        }
                    }
                }

                TaskManager.AddToBus(EasyUploadTaskManager.ROWS, rows);
            }

        }

        #endregion
    }
}