using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO.DataType.DisplayPattern;
using BExIS.IO.Transform.Validation.DSValidation;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;

/// <summary>
///
/// </summary>
namespace BExIS.IO.Transform.Output
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class ExcelWriter : DataWriter
    {
        #region constants

        // number types in datatable
        private static System.TypeCode[] numberTypes = new[] {
                          TypeCode.Decimal, TypeCode.Double, TypeCode.Single,
                          TypeCode.Int16, TypeCode.Int32, TypeCode.Int64,
                          TypeCode.UInt16, TypeCode.UInt32, TypeCode.UInt64,
                        };

        #endregion constants

        #region private

        private SpreadsheetDocument spreadsheetDocument;
        private SharedStringItem[] sharedStrings;
        private Stylesheet stylesheet = new Stylesheet();

        private DefinedNameVal areaOfData = new DefinedNameVal();
        private DefinedNameVal areaOfVariables = new DefinedNameVal();

        private char[] alphabet = { ' ', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        private int startColumn = 0;
        private int endColumn = 0;
        private int numOfColumns = 0;
        private int offset = 0;
        private int numOfDataRows = 0;

        private List<DataTuple> dataTuples = new List<DataTuple>();

        private uint[] styleIndexArray = new uint[6];
        private List<StyleIndexStruct> styleIndex = new List<StyleIndexStruct>();

        private WorkbookPart workbookPart;
        private WorksheetPart worksheetPart;
        private SheetData sheetData;
        private bool Template = false;

        #endregion private

        public ExcelWriter(bool isTemplate = false)
        {
            Template = isTemplate;
        }

        public ExcelWriter(IOUtility iOUtility, bool isTemplate = false) : base(iOUtility)
        {
            Template = isTemplate;
        }

        public ExcelWriter(DatasetManager datasetManager, bool isTemplate = false) : base(datasetManager)
        {
            Template = isTemplate;
        }

        public ExcelWriter(IOUtility iOUtility, DatasetManager datasetManager, bool isTemplate = false) : base(iOUtility, datasetManager)
        {
            Template = isTemplate;
        }

        /// <summary>
        /// Convert a Datatuple to a Row
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataTupleId">Id of the Datatuple to convert</param>
        /// <param name="rowIndex">Position of the Row</param>
        /// <returns></returns>
        protected Row DatatupleToRow(long dataTupleId, int rowIndex)
        {
            DataTuple dataTuple = DatasetManager.DataTupleRepo.Query(d => d.Id.Equals(dataTupleId)).FirstOrDefault();
            dataTuple.Materialize();

            return DatatupleToRow(dataTuple, rowIndex);
        }

        /// <summary>
        /// Convert a Datatuple to a Row
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataTuple">Datatuple to convert</param>
        /// <param name="rowIndex">Position of the Row</param>
        /// <returns></returns>
        protected Row DatatupleToRow(AbstractTuple dataTuple, int rowIndex)
        {
            Row row = new Row();
            row.RowIndex = Convert.ToUInt32(rowIndex);

            int columnIndex = 0;
            columnIndex += offset;

            // need to add this empty cell to add cells to the right place
            row.AppendChild(GetEmptyCell(rowIndex, 0));

            foreach (VariableIdentifier variableIdentifier in VariableIdentifiers)
            {
                VariableValue variableValue = dataTuple.VariableValues.Where(p => p.VariableId.Equals(variableIdentifier.id)).First();
                Cell cell = VariableValueToCell(variableValue, rowIndex, columnIndex);
                row.AppendChild(cell);
            }

            return row;
        }

        /// <summary>
        /// convert a DataRow to a Row
        /// </summary>
        /// <param name="src"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        protected Row DatarowToRow(DataRow src, int rowIndex)
        {
            Row row = new Row();
            row.RowIndex = Convert.ToUInt32(rowIndex);

            int columnIndex = 0;
            columnIndex += offset;

            // need to add this empty cell to add cells to the right place
            if (Template) row.AppendChild(GetEmptyCell(rowIndex, 0));

            foreach (object variable in src.ItemArray)
            {
                Cell cell = VariableValueToCell(variable, rowIndex, columnIndex);
                row.AppendChild(cell);
                columnIndex += 1;
            }

            return row;
        }

        /// <summary>
        /// convert a DataRow to a Row
        /// </summary>
        /// <param name="src"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        protected Row DataColumnCollectionToRow(DataColumnCollection src)
        {
            Row row = new Row();
            row.RowIndex = 1;
            int columnIndex = 0;
            columnIndex += offset;

            foreach (DataColumn variable in src)
            {
                string cellRef = getColumnIndex(columnIndex) + row.RowIndex;
                string type = typeof(string).Name;

                Cell cell = new Cell();
                cell.CellReference = new StringValue(cellRef);
                cell.DataType = CellValues.String;
                //cell.InlineString = new InlineString() { Text = new Text(variable.Caption) };
                cell.CellValue = new CellValue(variable.Caption);

                row.AppendChild(cell);
                columnIndex += 1;
            }

            return row;
        }

        protected Row StringArrayToRow(string[] src)
        {
            Row row = new Row();
            row.RowIndex = Convert.ToUInt32(rowIndex);
            int columnIndex = 0;
            columnIndex += offset;

            foreach (string value in src)
            {
                string cellRef = getColumnIndex(columnIndex) + row.RowIndex;
                string type = typeof(string).Name;

                Cell cell = new Cell();
                cell.CellReference = new StringValue(cellRef);
                cell.DataType = CellValues.String;
                //cell.InlineString = new InlineString() { Text = new Text(variable.Caption) };
                cell.CellValue = new CellValue(value);

                row.AppendChild(cell);
                columnIndex += 1;
            }

            return row;
        }

        ///// <summary>
        ///// Convert a VariableValue to Cell
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param name="variableValue"></param>
        ///// <param name="rowIndex"></param>
        ///// <param name="columnIndex"></param>
        ///// <returns></returns>
        //protected Cell VariableValueToCell(VariableValue variableValue, int rowIndex, int columnIndex)
        //{
        //    using (var uow = this.GetUnitOfWork())
        //    {
        //        DataAttribute dataAttribute = uow.GetReadOnlyRepository<Variable>().Query(p => p.Id == variableValue.VariableId).Select(p => p.DataAttribute).FirstOrDefault();

        //        string message = "row :" + rowIndex + "column:" + columnIndex;
        //        Debug.WriteLine(message);

        //        string cellRef = getColumnIndex(columnIndex) + rowIndex;

        //        Cell cell = new Cell();
        //        cell.CellReference = cellRef;
        //        cell.StyleIndex = ExcelHelper.GetExcelStyleIndex(dataAttribute.DataType, styleIndex);
        //        //cell.DataType = new EnumValue<CellValues>(getExcelType(dataAttribute.DataType.SystemType));
        //        //cell.CellValue = new CellValue(variableValue.Value.ToString());

        //        CellValues cellValueType = getExcelType(dataAttribute.DataType.SystemType);
        //        object value = variableValue.Value;

        //        //missing value
        //        // check if the value is a missing value and should be replaced
        //        if (variableValue.Variable.MissingValues.Any(mv => mv.Placeholder.Equals(value.ToString())))
        //        {
        //            value = variableValue.Variable.MissingValues.FirstOrDefault(mv => mv.Placeholder.Equals(value.ToString())).DisplayName;
        //            cell.DataType = new EnumValue<CellValues>(CellValues.String);
        //            cell.CellValue = new CellValue(value.ToString());

        //            return cell;
        //        }

        //        // number
        //        if (value != null && !(value is DBNull) && cellValueType == CellValues.Number)
        //        {
        //            cell.DataType = new EnumValue<CellValues>(CellValues.Number);

        //            try
        //            {
        //                if (value.ToString() != "")
        //                {
        //                    double d = Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
        //                    cell.CellValue = new CellValue(d.ToString(System.Globalization.CultureInfo.InvariantCulture));
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                throw new Exception(ex.Message + "\n|" + message);
        //            }

        //            return cell;
        //        }

        //        // Date
        //        if (value != null && !(value is DBNull) && cellValueType == CellValues.Date)
        //        {
        //            cell.DataType = new EnumValue<CellValues>(CellValues.Number);
        //            //CultureInfo provider = CultureInfo.InvariantCulture;
        //            try
        //            {
        //                if (value.ToString() != "")
        //                {
        //                    DateTime dt;
        //                    if (dataAttribute.DataType != null && dataAttribute.DataType.Extra != null)
        //                    {
        //                        DataTypeDisplayPattern pattern = DataTypeDisplayPattern.Materialize(dataAttribute.DataType.Extra);
        //                        if (!string.IsNullOrEmpty(pattern.StringPattern))
        //                        {
        //                            IOUtility.ExportDateTimeString(value.ToString(), pattern.StringPattern, out dt);
        //                            cell.CellValue = new CellValue(dt.ToOADate().ToString());
        //                        }
        //                        else
        //                        {
        //                            if (IOUtility.IsDate(value.ToString(), out dt))
        //                                cell.CellValue = new CellValue(dt.ToOADate().ToString());
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (IOUtility.IsDate(value.ToString(), out dt))
        //                            cell.CellValue = new CellValue(dt.ToOADate().ToString());
        //                    }
        //                }

        //                return cell;
        //            }
        //            catch (Exception ex)
        //            {
        //                throw new Exception(ex.Message + "|" + message);
        //            }
        //        }

        //        // String
        //        cell.DataType = new EnumValue<CellValues>(CellValues.String);
        //        if (value == null)
        //            cell.CellValue = new CellValue("");
        //        else
        //            cell.CellValue = new CellValue(value.ToString());

        //        return cell;
        //    }
        //}

        /// <summary>
        /// convert any given value to a Cell or Convert a VariableValue to Cell
        /// </summary>
        /// <param name="value">values as object as Variable Value </param>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        protected Cell VariableValueToCell(object value, int rowIndex, int columnIndex)
        {
            using (var uow = this.GetUnitOfWork())
            {
                string message = "row :" + rowIndex + "column:" + columnIndex;

                // define Cell
                string cellRef = getColumnIndex(columnIndex) + rowIndex;
                string type = "";
                Cell cell = new Cell();
                cell.CellReference = cellRef;

                // get Variable
                var variable = new VariableInstance();

                if (value is VariableValue)
                {
                    var id = ((VariableValue)value).VariableId;
                    variable = ((VariableValue)value).Variable;
                    type = variable.DataType.SystemType;
                }
                else
                {
                    variable = dataStructure.Variables.ElementAt(columnIndex - offset);
                    type = value.GetType().Name;
                }

                // set stylindex baed on type and index
                cell.StyleIndex = ExcelHelper.GetExcelStyleIndex(type, styleIndex);

                // set cell value type
                CellValues cellValueType = getExcelType(type);

                // missing value
                // check if the value is a missing value and should be replaced

                if (variable.MissingValues.Any(mv => mv.Placeholder.Equals(value.ToString())))
                {
                    value = variable.MissingValues.FirstOrDefault(mv => mv.Placeholder.Equals(value.ToString())).DisplayName;
                    cell.DataType = new EnumValue<CellValues>(CellValues.String);

                    cell.CellValue = new CellValue(value.ToString());

                    return cell;
                }

                // Number
                if (value != null && !(value is DBNull) && cellValueType == CellValues.Number)
                {
                    cell.DataType = new EnumValue<CellValues>(CellValues.Number);

                    if (value.ToString() != "")
                    {
                        double d = Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
                        cell.CellValue = new CellValue(d.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    }

                    return cell;
                }

                // Date
                if (value != null && !(value is DBNull) && cellValueType == CellValues.Date)
                {
                    cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                    //CultureInfo provider = CultureInfo.InvariantCulture;
                    try
                    {
                        if (value.ToString() != "")
                        {
                            DateTime dt;
                            if (variable.DataType != null)
                            {
                                DataTypeDisplayPattern pattern = DataTypeDisplayPattern.Get(variable.DisplayPatternId);
                                if (!string.IsNullOrEmpty(pattern.StringPattern))
                                {
                                    IOUtility.ExportDateTimeString(value.ToString(), pattern.StringPattern, out dt);
                                    cell.CellValue = new CellValue(dt.ToOADate().ToString());
                                }
                                else
                                {
                                    if (IOUtility.IsDate(value.ToString(), out dt))
                                        cell.CellValue = new CellValue(dt.ToOADate().ToString());
                                }
                            }
                            else
                            {
                                if (IOUtility.IsDate(value.ToString(), out dt))
                                    cell.CellValue = new CellValue(dt.ToOADate().ToString());
                            }
                        }

                        return cell;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message + "|" + message);
                    }
                }

                // String
                cell.DataType = new EnumValue<CellValues>(CellValues.String);
                if (value == null)
                    cell.CellValue = new CellValue("");
                else
                    cell.CellValue = new CellValue(value.ToString());

                return cell;
            }
        }

        /// <summary>
        /// Get a empty cell
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        protected Cell GetEmptyCell(int rowIndex, int columnIndex)
        {
            string cellRef = getColumnIndex(columnIndex);

            //CellFormat cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)2U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U, ApplyNumberFormat = true };
            //cellFormat.Protection = new Protection();
            //cellFormat.Protection.Locked = false;

            //Sty

            return new Cell()
            {
                CellReference = cellRef,
                StyleIndex = (UInt32Value)5U,
                DataType = CellValues.String,
                CellValue = new CellValue("")
            };
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="datasetId"></param>
        /// <param name="datasetVersionOrderNr"></param>
        /// <param name="dataStructureId"></param>
        /// <param name="title"></param>
        /// <param name="extention"></param>
        /// <returns></returns>
        public string CreateFile(long datasetId, long datasetVersionOrderNr, long dataStructureId, string title, string extention)
        {
            DataStructureManager dsm = new DataStructureManager();

            try
            {
                string dataPath = GetFullStorePath(datasetId, datasetVersionOrderNr, title, extention);

                if (Template)
                {
                    #region template

                    //Template will not be filtered by columns
                    if (this.VisibleColumns == null)
                    {
                        #region generate file with full datastructure

                        string dataStructureFilePath = GetDataStructureTemplatePath(dataStructureId, extention);
                        //dataPath = GetStorePath(datasetId, datasetVersionOrderNr, title, extention);

                        createTemplateFile(dataPath, dataStructureId, extention);

                        #endregion generate file with full datastructure
                    }

                    // create a file with a subset of variables
                    if (this.VisibleColumns != null)
                    {
                        /// call templateprovider from rpm
                        ExcelTemplateProvider provider = new ExcelTemplateProvider();

                        string path = GetStorePath(datasetId, datasetVersionOrderNr);
                        string newTitle = GetNewTitle(datasetId, datasetVersionOrderNr, title, extention);

                        StructuredDataStructure ds = dsm.StructuredDataStructureRepo.Get(dataStructureId);

                        List<long> ids = GetSubsetOfVariableIds(ds.Variables, this.VisibleColumns);

                        provider.CreateTemplate(ids, dataStructureId, path, newTitle);
                    }

                    #endregion template
                }
                else
                {
                    createEmptyFile(dataPath, extention);
                }

                return dataPath;
            }
            finally
            {
                dsm.Dispose();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ns"></param>
        /// <param name="table"></param>
        /// <param name="title"></param>
        /// <param name="extention"></param>
        /// <returns></returns>
        public string CreateFile(string ns, long dataStructureId, string title, string extension)
        {
            string dataPath = GetFullStorePath(ns, title, extension);

            if (Template)
            {
                #region template

                //Template will not be filtered by columns
                if (this.VisibleColumns == null)
                {
                    #region generate file with full datastructure

                    string dataStructureFilePath = GetDataStructureTemplatePath(dataStructureId, extension);
                    //dataPath = GetStorePath(datasetId, datasetVersionOrderNr, title, extention);

                    createTemplateFile(dataPath, dataStructureId, extension);

                    #endregion generate file with full datastructure
                }

                // create a file with a subset of variables
                if (this.VisibleColumns != null)
                {
                    /// call templateprovider from rpm
                    ExcelTemplateProvider provider = new ExcelTemplateProvider();

                    string path = ns;
                    string newTitle = title + extension;

                    provider.CreateTemplate(getVariableIds(this.VisibleColumns), dataStructureId, path, newTitle);
                }

                #endregion template
            }
            else
            {
                createEmptyFile(dataPath, extension);
            }

            return dataPath;
        }

        private bool createTemplateFile(string dataPath, long dataStructureId, string extension)
        {
            try
            {
                string dataStructureFilePath = GetDataStructureTemplatePath(dataStructureId, extension);

                SpreadsheetDocument dataStructureFile = SpreadsheetDocument.Open(dataStructureFilePath, true);
                SpreadsheetDocument dataFile = SpreadsheetDocument.Create(dataPath,
                    dataStructureFile.DocumentType);

                foreach (OpenXmlPart part in dataStructureFile.GetPartsOfType<OpenXmlPart>())
                {
                    OpenXmlPart newPart = dataFile.AddPart<OpenXmlPart>(part);
                }

                dataFile.WorkbookPart.Workbook.Save();
                dataStructureFile.Dispose();
                dataFile.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Can´t create excel template file.", ex);
            }
        }

        private bool createEmptyFile(string dataPath, string extension)
        {
            try
            {
                string emptyExcelTemplatePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "Template", "empty" + extension);

                SpreadsheetDocument emptyTemplate = SpreadsheetDocument.Open(emptyExcelTemplatePath, true);
                SpreadsheetDocument dataFile = SpreadsheetDocument.Create(dataPath,
                    emptyTemplate.DocumentType);

                foreach (OpenXmlPart part in emptyTemplate.GetPartsOfType<OpenXmlPart>())
                {
                    OpenXmlPart newPart = dataFile.AddPart<OpenXmlPart>(part);
                }

                //uint iExcelIndex = 164;
                //List<StyleIndexStruct> styleIndex = new List<StyleIndexStruct>();
                ExcelHelper.UpdateStylesheet(dataFile.WorkbookPart.WorkbookStylesPart.Stylesheet, out styleIndex);

                dataFile.WorkbookPart.Workbook.Save();
                emptyTemplate.Dispose();
                dataFile.Dispose();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Can´t create excel template file.", ex);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool IsTemplate(Stream file)
        {
            this.FileStream = file;

            // open excel file
            spreadsheetDocument = SpreadsheetDocument.Open(this.FileStream, false);

            // get workbookpart
            WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

            try
            {
                // get all the defined area
                List<DefinedNameVal> namesTable = buildDefinedNamesTable(workbookPart);

                if (namesTable.Where(p => p.Key.Equals("Data")).FirstOrDefault() != null
                    && namesTable.Where(p => p.Key.Equals("VariableIdentifiers")).FirstOrDefault() != null)
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        #region helper

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="workbookPart"></param>
        /// <returns></returns>
        private List<DefinedNameVal> buildDefinedNamesTable(WorkbookPart workbookPart)
        {
            //Build a list
            List<DefinedNameVal> definedNames = new List<DefinedNameVal>();
            foreach (DefinedName name in workbookPart.Workbook.GetFirstChild<DefinedNames>())
            {
                //Parse defined name string...
                string key = name.Name;

                if (key.Equals("Data") || key.Equals("VariableIdentifiers"))
                {
                    string reference = name.InnerText;
                    string sheetName = reference.Split('!')[0];
                    sheetName = sheetName.Trim('\'');

                    //Assumption: None of my defined names are relative defined names (i.e. A1)
                    string range = reference.Split('!')[1];
                    string[] rangeArray = range.Split('$');
                    string startCol = rangeArray[1];
                    int startRow = Convert.ToInt32(rangeArray[2].TrimEnd(':'));
                    string endCol = null;
                    int endRow = 0;
                    if (rangeArray.Length > 3)
                    {
                        endCol = rangeArray[3];
                        endRow = Convert.ToInt32(rangeArray[4]);
                    }
                    definedNames.Add(new DefinedNameVal() { Key = key, SheetName = sheetName, StartColumn = startCol, StartRow = startRow, EndColumn = endCol, EndRow = endRow });
                }
            }

            return definedNames;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="workbookPart"></param>
        /// <returns></returns>
        private List<DefinedNameVal> changeDefinedNamesTable(WorkbookPart workbookPart)
        {
            //Build a list
            List<DefinedNameVal> definedNames = new List<DefinedNameVal>();
            foreach (DefinedName name in workbookPart.Workbook.GetFirstChild<DefinedNames>())
            {
                //Parse defined name string...
                string key = name.Name;

                if (key.Equals("Data") || key.Equals("VariableIdentifiers"))
                {
                    string reference = name.InnerText;
                    string sheetName = reference.Split('!')[0];
                    sheetName = sheetName.Trim('\'');

                    //Assumption: None of my defined names are relative defined names (i.e. A1)
                    string range = reference.Split('!')[1];
                    string[] rangeArray = range.Split('$');
                    string startCol = rangeArray[1];
                    int startRow = Convert.ToInt32(rangeArray[2].TrimEnd(':'));
                    string endCol = null;
                    int endRow = 0;
                    if (rangeArray.Length > 3)
                    {
                        endCol = rangeArray[3];
                        endRow = Convert.ToInt32(rangeArray[4]);
                    }
                    definedNames.Add(new DefinedNameVal() { Key = key, SheetName = sheetName, StartColumn = startCol, StartRow = startRow, EndColumn = endCol, EndRow = endRow });
                }
            }

            return definedNames;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="workbookPart"></param>
        /// <param name="definedName"></param>
        /// <returns></returns>
        private static WorksheetPart getWorkSheetPart(WorkbookPart workbookPart, DefinedNameVal definedName)
        {
            //get worksheet based on defined name
            string relId = workbookPart.Workbook.Descendants<Sheet>()
            .Where(s => definedName.SheetName.Equals(s.Name))
            .First()
            .Id;
            return (WorksheetPart)workbookPart.GetPartById(relId);
        }

        /// <summary>
        /// Given a cell name, parses the specified cell to get the column number.
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="columnName"></param>
        /// <returns></returns>
        private static int getColumnNumber(string columnName)
        {
            char[] chars = columnName.ToCharArray();

            if (chars.Length == 1)
            {
                int colNr1 = (int)(chars[0] - 64);
                return colNr1;
            }
            else if (chars.Length == 2)
            {
                int colNr1 = (int)(chars[1] - 64);
                int colNr2 = (int)(chars[0] - 64);
                return colNr1 + (26 * colNr2);
            }
            else if (chars.Length == 3)
            {
                int colNr1 = (int)(chars[2] - 64);
                int colNr2 = (int)(chars[1] - 64);
                int colNr3 = (int)(chars[0] - 64);
                return colNr1 + (26 * colNr2) + (26 * 26 * colNr3);
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Given a cell name, parses the specified cell to get the column name.
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="cellName"></param>
        /// <returns></returns>
        private static string getColumnName(string cellName)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellName);

            return match.Value;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="index"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private string getColumnIndex(int index, int offset = 1)
        {
            int residual = 0;
            string column = "";
            bool firstRun = true;
            do
            {
                if (firstRun == true)
                {
                    residual = ((index % 26)) + offset;
                    column = alphabet[residual].ToString() + column;
                    index = (index / 26);
                    firstRun = false;
                }
                else
                {
                    residual = ((index % 26)) + offset;
                    column = alphabet[residual - 1].ToString() + column;
                    index = (index / 26);
                }
            } while (index > 0);
            return column;
        }

        /// <summary>
        /// 1 0
        /// 2 0.00
        /// 3 #,##0
        /// 4 #,##0.00
        /// 5 $#,##0_);($#,##0)
        /// 6 $#,##0_);[Red]($#,##0)
        /// 7 $#,##0.00_);($#,##0.00)
        /// 8 $#,##0.00_);[Red]($#,##0.00)
        /// 9 0%
        /// 10 0.00%
        /// 11 0.00E+00
        /// 12 # ?/?
        /// 13 # ??/??
        /// 14 m/d/yyyy
        /// 15 d-mmm-yy
        /// 16 d-mmm
        /// 17 mmm-yy
        /// 18 h:mm AM/PM
        /// 19 h:mm:ss AM/PM
        /// 20 h:mm
        /// 21 h:mm:ss
        /// 22 m/d/yyyy h:mm
        /// 37 #,##0_);(#,##0)
        /// 38 #,##0_);[Red](#,##0)
        /// 39 #,##0.00_);(#,##0.00)
        /// 40 #,##0.00_);[Red](#,##0.00)
        /// 45 mm:ss
        /// 46 [h]:mm:ss
        /// 47 mm:ss.0
        /// 48 ##0.0E+0
        /// 49 @
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="spreadsheetDocument"></param>
        private void generateStyle(SpreadsheetDocument spreadsheetDocument)
        {
            ExcelHelper.UpdateStylesheet(spreadsheetDocument.WorkbookPart.WorkbookStylesPart.Stylesheet, out styleIndex);
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="systemType"></param>
        /// <returns></returns>
        private CellValues getExcelType(string systemType)
        {
            if (systemType == "Int16" || systemType == "Int32" || systemType == "Int64" || systemType == "UInt16" || systemType == "UInt32" || systemType == "UInt64" || systemType == "Double" || systemType == "Decimal")
                return CellValues.Number;
            if (systemType == "Char" || systemType == "String")
                return CellValues.SharedString;
            if (systemType == "DateTime")
                return CellValues.Date;
            if (systemType == "Boolean")
                return CellValues.Boolean;
            return CellValues.SharedString;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="worksheetPart"></param>
        /// <param name="startRow"></param>
        /// <param name="endRow"></param>
        /// <returns></returns>
        private List<VariableIdentifier> getVariableIdentifiers(WorksheetPart worksheetPart, int startRow, int endRow)
        {
            //NEW OPENXMLREADER
            if (this.VariableIdentifiers == null || this.VariableIdentifiers.Count == 0)
            {
                using (OpenXmlReader reader = OpenXmlReader.Create(worksheetPart))
                {
                    int rowNum = 0;

                    // read variable rows to get name and id from area variable
                    while (reader.Read())
                    {
                        if (reader.ElementType == typeof(Row))
                        {
                            do
                            {
                                if (reader.HasAttributes)
                                    rowNum = Convert.ToInt32(reader.Attributes.First(a => a.LocalName == "r").Value);

                                if (rowNum >= startRow && rowNum <= endRow)
                                {
                                    Row row = (Row)reader.LoadCurrentElement();

                                    if (row.Hidden == null) VariableIdentifierRows.Add(rowToList(row));
                                    else if (row.Hidden != true) VariableIdentifierRows.Add(rowToList(row));
                                }
                            } while (reader.ReadNextSibling() && rowNum < endRow); // Skip to the next row
                            break;
                        }
                    }
                }

                // convert variable rows to VariableIdentifiers
                if (VariableIdentifierRows != null)
                {
                    foreach (List<string> l in VariableIdentifierRows)
                    {
                        //create headerVariables
                        if (VariableIdentifiers.Count == 0)
                        {
                            foreach (string s in l)
                            {
                                VariableIdentifier hv = new VariableIdentifier();
                                hv.name = s;
                                VariableIdentifiers.Add(hv);
                            }
                        }
                        else
                        {
                            foreach (string s in l)
                            {
                                int id = Convert.ToInt32(s);
                                int index = l.IndexOf(s);
                                VariableIdentifiers.ElementAt(index).id = id;
                            }
                        }
                    }
                }
            }

            if (this.VariableIdentifiers != null) return this.VariableIdentifiers;
            else return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="r"></param>
        /// <returns></returns>
        private List<string> rowToList(Row r)
        {
            string[] rowAsStringArray = new string[numOfColumns];

            // create a new cell
            Cell c = new Cell();

            for (int i = 0; i < r.ChildElements.Count(); i++)
            {
                // get current cell at i
                c = r.Elements<Cell>().ElementAt(i);

                string value = "";

                if (c != null)
                {
                    if (c.CellValue != null)
                    {
                        // if cell reference in range of the area
                        int cellReferencAsInterger = getColumnNumber(getColumnName(c.CellReference));
                        int start = getColumnNumber(this.areaOfData.StartColumn);
                        int end = getColumnNumber(this.areaOfData.EndColumn);

                        if (cellReferencAsInterger >= start && cellReferencAsInterger <= end)
                        {
                            // if Value a text
                            if (c.DataType != null && c.DataType.HasValue && c.DataType.Value == CellValues.SharedString)
                            {
                                int sharedStringIndex = int.Parse(c.CellValue.Text, CultureInfo.InvariantCulture);
                                SharedStringItem sharedStringItem = sharedStrings[sharedStringIndex];
                                value = sharedStringItem.InnerText;
                            }
                            // not a text
                            else if (c.StyleIndex != null && c.StyleIndex.HasValue)
                            {
                                uint styleIndex = c.StyleIndex.Value;
                                CellFormat cellFormat = stylesheet.CellFormats.ChildElements[(int)styleIndex] as CellFormat;
                                if (cellFormat.ApplyNumberFormat != null && cellFormat.ApplyNumberFormat.HasValue && cellFormat.ApplyNumberFormat.Value && cellFormat.NumberFormatId != null && cellFormat.NumberFormatId.HasValue)
                                {
                                    uint numberFormatId = cellFormat.NumberFormatId.Value;

                                    // Number format 14-22 and 45-47 are built-in date and/or time formats
                                    if ((numberFormatId >= 14 && numberFormatId <= 22) || (numberFormatId >= 45 && numberFormatId <= 47))
                                    {
                                        DateTime dateTime = DateTime.FromOADate(double.Parse(c.CellValue.Text, CultureInfo.InvariantCulture));
                                        value = dateTime.ToString();
                                    }
                                    else
                                    {
                                        if (stylesheet.NumberingFormats != null && stylesheet.NumberingFormats.Any(numFormat => ((NumberingFormat)numFormat).NumberFormatId.Value == numberFormatId))
                                        {
                                            NumberingFormat numberFormat = stylesheet.NumberingFormats.First(numFormat => ((NumberingFormat)numFormat).NumberFormatId.Value == numberFormatId) as NumberingFormat;

                                            if (numberFormat != null && numberFormat.FormatCode != null && numberFormat.FormatCode.HasValue)
                                            {
                                                string formatCode = numberFormat.FormatCode.Value;
                                                if ((formatCode.Contains("h") && formatCode.Contains("m")) || (formatCode.Contains("m") && formatCode.Contains("d")))
                                                {
                                                    DateTime dateTime = DateTime.FromOADate(double.Parse(c.CellValue.Text, CultureInfo.InvariantCulture));
                                                    value = dateTime.ToString();
                                                }
                                                else
                                                {
                                                    value = c.CellValue.Text;
                                                }
                                            }
                                            else
                                            {
                                                value = c.CellValue.Text;
                                            }
                                        }
                                        else
                                        {
                                            value = c.CellValue.Text;
                                        }
                                    }
                                }
                                else
                                {
                                    value = c.CellValue.Text;
                                }
                            }

                            // define index based on cell refernce - offset
                            int index = cellReferencAsInterger - offset - 1;
                            rowAsStringArray[index] = value;
                        }
                    }//end if cell value
                }//end if cell null
            }//for

            return rowAsStringArray.ToList();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="stringlist"></param>
        /// <returns></returns>
        private List<long> getVariableIds(string[] stringlist)
        {
            List<long> list = new List<long>();

            if (stringlist != null)
            {
                foreach (string id in stringlist)
                {
                    list.Add(Convert.ToInt64(id));
                }
            }

            return list;
        }

        #endregion helper

        #region setup / close actions

        /// <summary>
        /// prepare existing excel file to append data
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="dataStructureId"></param>
        protected override void Init(string filePath, long dataStructureId)
        {
            // loading datastructure
            //dataStructure = GetDataStructure(dataStructureId);

            // open excel file
            spreadsheetDocument = SpreadsheetDocument.Open(filePath, true);

            // get workbookpart
            workbookPart = spreadsheetDocument.WorkbookPart;

            // if the writer should create a template, more preperation is needed
            if (Template)
            {
                // get all the defined area
                List<DefinedNameVal> namesTable = buildDefinedNamesTable(workbookPart);

                // select data area
                this.areaOfData = namesTable.Where(p => p.Key.Equals("Data")).FirstOrDefault();

                // set starting row number
                rowIndex = areaOfData.EndRow;

                // Select variable area
                this.areaOfVariables = namesTable.Where(p => p.Key.Equals("VariableIdentifiers")).FirstOrDefault();

                // Get integers for reading data
                startColumn = getColumnNumber(this.areaOfData.StartColumn);
                endColumn = getColumnNumber(this.areaOfData.EndColumn);

                numOfColumns = (endColumn - startColumn) + 1;
                offset = getColumnNumber(getColumnName(this.areaOfData.StartColumn)) - 1;

                // generate Style for cell types
                generateStyle(spreadsheetDocument);

                // get styleSheet
                stylesheet = workbookPart.WorkbookStylesPart.Stylesheet;

                // Get shared strings
                sharedStrings = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ToArray();

                // select worksheetpart by selected defined name area like data in sheet
                // sheet where data area is inside
                worksheetPart = getWorkSheetPart(workbookPart, this.areaOfData);

                // Get VariableIndentifiers
                VariableIdentifiers = getVariableIdentifiers(worksheetPart, this.areaOfVariables.StartRow, this.areaOfVariables.EndRow);
            }
            else
            {
                worksheetPart = workbookPart.WorksheetParts.FirstOrDefault();
            }

            // get sheetData object for adding data to
            if (worksheetPart != null && !worksheetPart.Worksheet.HasChildren)
                worksheetPart.Worksheet.AppendChild(new SheetData());

            sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
        }

        /// <summary>
        /// finish up and close the Excel file
        /// </summary>
        /// <param name="workbookPart"></param>
        protected override void Close()
        {
            if (Template)
            {
                // adjust count of data rows
                numOfDataRows = rowIndex;

                // set data area
                foreach (DefinedName name in workbookPart.Workbook.GetFirstChild<DefinedNames>())
                {
                    if (name.Name == "Data")
                    {
                        string[] tempArr = name.InnerText.Split('$');
                        string temp = "";
                        //$A$10:$C$15

                        tempArr[tempArr.Count() - 1] = numOfDataRows.ToString();

                        foreach (string t in tempArr)
                        {
                            if (t == tempArr.First())
                            {
                                temp = temp + t;
                            }
                            else
                            {
                                temp = temp + "$" + t;
                            }
                        }

                        name.Text = temp;
                    }
                }
            }

            spreadsheetDocument.WorkbookPart.Workbook.Save();
            spreadsheetDocument.Close();
        }

        #endregion setup / close actions

        #region addRow

        /// <summary>
        /// add a row to the excel sheet
        /// </summary>
        /// <param name="tuple"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        protected override bool AddRow(AbstractTuple tuple, long rowIndex)
        {
            // convert datatuple to row
            Row row = DatatupleToRow(tuple, (int)rowIndex);

            // skip rows with only empty cells
            var hasNonEmptyCell = row.Elements<Cell>()
                                       .Any<Cell>(cell => !String.IsNullOrEmpty(cell.InnerText));
            if (!hasNonEmptyCell)
            {
                return false;
            }

            // add row
            sheetData.Append(row);

            return true;
        }

        /// <summary>
        /// add a row to the excel sheet
        /// </summary>
        /// <param name="row"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        protected override bool AddRow(DataRow row, long rowIndex, bool idInclude = false)
        {
            // column count
            int colCount = row.Table.Columns.Count;

            // skip rows with only empty cells
            bool empty = true;
            for (int i = 0; i < colCount; i++)
            {
                if (!String.IsNullOrEmpty(row[i].ToString()))
                {
                    empty = false;
                    break;
                }
            }
            if (empty)
            {
                return false;
            }

            // create excel row
            Row excelRow = DatarowToRow(row, (int)rowIndex);

            // add row
            sheetData.Append(excelRow);

            return true;
        }

        protected override bool AddRow(string[] row, long rowIndex)
        {
            throw new NotImplementedException();
        }

        #endregion addRow

        #region addHeader

        protected override bool AddHeader(string[] header)
        {
            throw new NotImplementedException();
        }

        protected override bool AddHeader(StructuredDataStructure header)
        {
            return false;
        }

        protected override bool AddHeader(DataColumnCollection header)
        {
            // column count
            int colCount = header.Count;

            // skip rows with only empty cells
            bool empty = true;
            for (int i = 0; i < colCount; i++)
            {
                if (!String.IsNullOrEmpty(header[i].ToString()))
                {
                    empty = false;
                    break;
                }
            }
            if (empty)
            {
                return false;
            }

            // create excel row
            Row excelRow = DataColumnCollectionToRow(header);
            rowIndex++;
            // add row
            sheetData.Append(excelRow);

            return true;
        }

        #endregion addHeader

        #region AddUnits

        protected override bool AddUnits(string[] units)
        {
            try
            {
                // create excel row
                Row excelRow = StringArrayToRow(units);
                rowIndex++;
                // add row
                sheetData.Append(excelRow);

                return true;
            }
            catch
            {
                return false;
            }
        }

        protected override bool AddUnits(StructuredDataStructure structrue)
        {
            return false;
        }

        #endregion AddUnits
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class DefinedNameVal
    {
        public string Key = "";
        public string SheetName = "";
        public string StartColumn = "";
        public int StartRow = 0;
        public string EndColumn = "";
        public int EndRow = 0;
    }
}