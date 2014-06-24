using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using BExIS.Dlm.Entities.Data;
using BExIS.Io.Transform.Validation.Exceptions;
using DocumentFormat.OpenXml.Packaging;
using Vaiona.Util.Cfg;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.RegularExpressions;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Entities.DataStructure;
using DocumentFormat.OpenXml;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Io.Transform.Validation.DSValidation;
using System.Globalization;

using BExIS.RPM.Output;

namespace BExIS.Io.Transform.Output
{
    public class ExcelWriter:DataWriter
    {

        SpreadsheetDocument spreadsheetDocument;
        Worksheet worksheet;
        SharedStringItem[] _sharedStrings;
        Stylesheet _stylesheet = new Stylesheet();

        DefinedNameVal _areaOfData = new DefinedNameVal();
        DefinedNameVal _areaOfVariables = new DefinedNameVal();

        char[] alphabet = { ' ','A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };


        int startColumn = 0;
        int endColumn = 0;
        int numOfColumns = 0;
        int offset = 0;
        int numOfDataRows = 0;

        List<DataTuple> _dataTuples = new List<DataTuple>();
        StructuredDataStructure _dataStructure = new StructuredDataStructure();

        uint[] styleIndexArray = new uint[4];

        /// <summary>
        /// Add Datatuples to a Excel Template file
        /// </summary>
        /// <param name="dataTuples"> Datatuples to add</param>
        /// <param name="filePath">Path of the excel template file</param>
        /// <param name="dataStructureId">Id of datastructure</param>
        /// <returns>List of Errors or null</returns>
        public List<Error> AddDataTuplesToTemplate(List<long> dataTuplesIds, string filePath, long dataStructureId )
        {
            if (File.Exists(filePath))
            {

                //Stream file = Open(filePath);

                //_dataTuples = dataTuples;
                // loading datastructure
                _dataStructure = GetDataStructure(dataStructureId);

                // open excel file
                spreadsheetDocument = SpreadsheetDocument.Open(filePath, true);

                // get workbookpart
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

                // get all the defined area 
                List<DefinedNameVal> namesTable = BuildDefinedNamesTable(workbookPart);

                // select data area
                this._areaOfData = namesTable.Where(p => p.Key.Equals("Data")).FirstOrDefault();

                // Select variable area
                this._areaOfVariables = namesTable.Where(p => p.Key.Equals("VariableIdentifiers")).FirstOrDefault();

                // Get intergers for reading data
                startColumn = GetColumnNumber(this._areaOfData.StartColumn);
                endColumn = GetColumnNumber(this._areaOfData.EndColumn);
                
                numOfColumns = (endColumn - startColumn) + 1;
                offset = GetColumnNumber(getColumnName(this._areaOfData.StartColumn)) - 1;

                // gerneat Style for cell types
                generateStyle(spreadsheetDocument);

                // get styleSheet
                _stylesheet = workbookPart.WorkbookStylesPart.Stylesheet;

                // Get shared strings
                _sharedStrings = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ToArray();

                // select worksheetpart by selected defined name area like data in sheet
                // sheet where data area is inside
                WorksheetPart worksheetPart = GetWorkSheetPart(workbookPart, this._areaOfData);

                // Get VarioableIndentifiers
                this.VariableIndentifiers = GetVariableIdentifiers(worksheetPart, this._areaOfVariables.StartRow, this._areaOfVariables.EndRow);


                AddRows(worksheetPart, this._areaOfData.StartRow, this._areaOfData.EndRow, dataTuplesIds);

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
            

                spreadsheetDocument.WorkbookPart.Workbook.Save();
                spreadsheetDocument.Close();

            }

            return errorMessages;
        }

        /// <summary>
        /// Add Datatuples to a Excel Template file
        /// </summary>
        /// <param name="dataTuples"> Datatuples to add</param>
        /// <param name="filePath">Path of the excel template file</param>
        /// <param name="dataStructureId">Id of datastructure</param>
        /// <returns>List of Errors or null</returns>
        public List<Error> AddDataTuplesToTemplate(List<AbstractTuple> dataTuples, string filePath, long dataStructureId)
        {
            if (File.Exists(filePath))
            {

                //Stream file = Open(filePath);

                //_dataTuples = dataTuples;
                // loading datastructure
                _dataStructure = GetDataStructure(dataStructureId);

                // open excel file
                spreadsheetDocument = SpreadsheetDocument.Open(filePath, true);

                // get workbookpart
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

                // get all the defined area 
                List<DefinedNameVal> namesTable = BuildDefinedNamesTable(workbookPart);

                // select data area
                this._areaOfData = namesTable.Where(p => p.Key.Equals("Data")).FirstOrDefault();

                // Select variable area
                this._areaOfVariables = namesTable.Where(p => p.Key.Equals("VariableIdentifiers")).FirstOrDefault();

                // Get intergers for reading data
                startColumn = GetColumnNumber(this._areaOfData.StartColumn);
                endColumn = GetColumnNumber(this._areaOfData.EndColumn);

                numOfColumns = (endColumn - startColumn) + 1;
                offset = GetColumnNumber(getColumnName(this._areaOfData.StartColumn)) - 1;

                // gerneat Style for cell types
                generateStyle(spreadsheetDocument);

                // get styleSheet
                _stylesheet = workbookPart.WorkbookStylesPart.Stylesheet;

                // Get shared strings
                _sharedStrings = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ToArray();

                // select worksheetpart by selected defined name area like data in sheet
                // sheet where data area is inside
                WorksheetPart worksheetPart = GetWorkSheetPart(workbookPart, this._areaOfData);

                // Get VarioableIndentifiers
                this.VariableIndentifiers = GetVariableIdentifiers(worksheetPart, this._areaOfVariables.StartRow, this._areaOfVariables.EndRow);


                AddRows(worksheetPart, this._areaOfData.StartRow, this._areaOfData.EndRow, dataTuples);

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


                spreadsheetDocument.WorkbookPart.Workbook.Save();
                spreadsheetDocument.Close();

            }

            return errorMessages;
        }

        /// <summary>
        /// Add Rows to a WorksheetPart
        /// </summary>
        /// <param name="worksheetPart"></param>
        /// <param name="startRow"></param>
        /// <param name="endRow"></param>
        /// <param name="dataTuplesIds"></param>
        protected void AddRows(WorksheetPart worksheetPart, int startRow, int endRow, List<long> dataTuplesIds)
        {
            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();

            int rowIndex = endRow;
            //add row
            foreach (long id in dataTuplesIds)
            {
 
                // convert datatuple to row and add it to sheetdata
                Row row = DatatupleToRow(id,rowIndex);

                bool empty = true;
                foreach (Cell c in row.Elements<Cell>().ToList())
                {
                    if (!String.IsNullOrEmpty(c.InnerText))
                    {
                        empty = false;
                        break;
                    }
                }

                if (!empty)
                {
                    sheetData.Append(row);
                    if(!id.Equals(dataTuplesIds.Last()))
                        rowIndex++;
                }
            }

            numOfDataRows = rowIndex;

        }

        /// <summary>
        ///  Add Rows to a WorksheetPart
        /// </summary>
        /// <param name="worksheetPart"></param>
        /// <param name="startRow"></param>
        /// <param name="endRow"></param>
        /// <param name="dataTuples"></param>
        protected void AddRows(WorksheetPart worksheetPart, int startRow, int endRow, List<AbstractTuple> dataTuples)
        {
            Worksheet worksheet = worksheetPart.Worksheet;
            SheetData sheetData = worksheet.GetFirstChild<SheetData>();

            int rowIndex = endRow;
            //add row
            foreach (DataTuple dataTuple in dataTuples)
            {

                // convert datatuple to row and add it to sheetdata
                Row row = DatatupleToRow(dataTuple, rowIndex);

                bool empty = true;
                foreach (Cell c in row.Elements<Cell>().ToList())
                {
                    if (!String.IsNullOrEmpty(c.InnerText))
                    {
                        empty = false;
                        break;
                    }
                }

                if (!empty)
                {
                    sheetData.Append(row);
                    if (!dataTuple.Equals(dataTuples.Last()))
                        rowIndex++;
                }
            }

            numOfDataRows = rowIndex;

        }

        /// <summary>
        /// Convert a Datatuple to a Row
        /// </summary>
        /// <param name="dataTupleId">Id of the Datatuple to convert</param>
        /// <param name="rowIndex">Position of the Row</param>
        /// <returns></returns>
        protected Row DatatupleToRow(long dataTupleId, int rowIndex)
        { 
            Row row = new Row();
            row.RowIndex = Convert.ToUInt32(rowIndex);


            DataTuple dataTuple = datasetManager.DataTupleRepo.Get(dataTupleId);
            dataTuple.Materialize();
            

            int columnIndex = 0;
            columnIndex += offset;

            // need to add this empty cell to add cells to the right place
            row.AppendChild(GetEmptyCell(rowIndex, 0));    

            foreach (VariableIdentifier variableIdentifier in VariableIndentifiers)
            {
                VariableValue variableValue = dataTuple.VariableValues.Where(p => p.VariableId.Equals(variableIdentifier.id)).First();
                Cell cell = VariableValueToCell(variableValue, rowIndex, columnIndex);
                row.AppendChild(cell);
            }


            return row;
        }

        /// <summary>
        /// Convert a Datatuple to a Row
        /// </summary>
        /// <param name="dataTuple">Datatuple to convert</param>
        /// <param name="rowIndex">Position of the Row</param>
        /// <returns></returns>
        protected Row DatatupleToRow(DataTuple dataTuple, int rowIndex)
        {
            Row row = new Row();
            row.RowIndex = Convert.ToUInt32(rowIndex);

            int columnIndex = 0;
            columnIndex += offset;

            // need to add this empty cell to add cells to the right place
            row.AppendChild(GetEmptyCell(rowIndex, 0));

            foreach (VariableIdentifier variableIdentifier in VariableIndentifiers)
            {
                VariableValue variableValue = dataTuple.VariableValues.Where(p => p.VariableId.Equals(variableIdentifier.id)).First();
                Cell cell = VariableValueToCell(variableValue, rowIndex, columnIndex);
                row.AppendChild(cell);
            }


            return row;
        }

        /// <summary>
        /// Convert a VariableValue to Cell
        /// </summary>
        /// <param name="variableValue"></param>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        protected Cell VariableValueToCell(VariableValue variableValue, int rowIndex, int columnIndex)
        {
            DataContainerManager CM = new DataContainerManager();
            DataAttribute dataAttribute = CM.DataAttributeRepo.Get(variableValue.DataAttribute.Id);

            string cellRef = getColumnIndex(columnIndex);

            Cell cell = new Cell();
            cell.CellReference = cellRef;
            cell.StyleIndex = getExcelStyleIndex(dataAttribute.DataType.SystemType, styleIndexArray);
            //cell.DataType = new EnumValue<CellValues>(getExcelType(dataAttribute.DataType.SystemType));
            //cell.CellValue = new CellValue(variableValue.Value.ToString());

            CellValues cellValueType = getExcelType(dataAttribute.DataType.SystemType);
            object value = variableValue.Value;

            if (value != null && !(value is DBNull) && cellValueType == CellValues.Number)
            {
                cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                double d = Convert.ToDouble(value, System.Globalization.CultureInfo.InvariantCulture);
                cell.CellValue = new CellValue(d.ToString(System.Globalization.CultureInfo.InvariantCulture));

                return cell;
            }
            else
            {
                if (value != null && !(value is DBNull) && cellValueType == CellValues.Date)
                {

                    cell.DataType = new EnumValue<CellValues>(CellValues.Number);
                    //CultureInfo provider = CultureInfo.InvariantCulture;
                    DateTime dt = Convert.ToDateTime(value.ToString());
                    cell.CellValue = new CellValue(dt.ToOADate().ToString());
                }
                else
                {
                    cell.DataType = new EnumValue<CellValues>(CellValues.String);
                    if(value==null)
                        cell.CellValue = new CellValue("");
                    else
                        cell.CellValue = new CellValue(value.ToString());
                }
            }
           
            return cell;
        }

        /// <summary>
        /// Get a empty cell
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        protected Cell GetEmptyCell(int rowIndex, int columnIndex)
        {
            string cellRef = getColumnIndex(columnIndex);

            return  new Cell()
                    {
                        CellReference = cellRef,
                        StyleIndex = (UInt32Value)5U,
                        DataType = CellValues.String,
                        CellValue = new CellValue("")
                    };
        }

        public string CreateFile(long datasetId, long datasetVersionOrderNr, long dataStructureId, string title, string extention)
        {
            string dataPath = GetStorePath(datasetId, datasetVersionOrderNr, title, extention);

            //Template will not be filtered by columns
            if (this.visibleColumns == null)
            {
                #region generate file with full datastructure

                    string dataStructureFilePath = GetDataStructureTemplatePath(dataStructureId, extention);
                    //dataPath = GetStorePath(datasetId, datasetVersionOrderNr, title, extention);

                    try
                    {
                        SpreadsheetDocument dataStructureFile = SpreadsheetDocument.Open(dataStructureFilePath, true);
                        SpreadsheetDocument dataFile = SpreadsheetDocument.Create(dataPath, dataStructureFile.DocumentType);

                        foreach (OpenXmlPart part in dataStructureFile.GetPartsOfType<OpenXmlPart>())
                        {
                            OpenXmlPart newPart = dataFile.AddPart<OpenXmlPart>(part);
                        }

                        dataFile.WorkbookPart.Workbook.Save();
                        dataStructureFile.Dispose();
                        dataFile.Dispose();

                    }
                    catch (Exception ex)
                    {
                        throw new Exception(ex.Message.ToString());
                    }

                #endregion
            }

            // create a file with a subset of variables
            if (this.visibleColumns != null)
            { 
                /// call templateprovider from rpm
                ExcelTemplateProvider provider = new ExcelTemplateProvider();

                string path = GetOnlyStorePath(datasetId, datasetVersionOrderNr);
                string newTitle = GetNewTitle(datasetId, datasetVersionOrderNr, title, extention);

                provider.CreateTemplate(GetVariableIds(this.visibleColumns), dataStructureId, path, newTitle);

            }

            return dataPath;
        }

        public bool IsTemplate(Stream file)
        {
            this.file = file;

            // open excel file
            spreadsheetDocument = SpreadsheetDocument.Open(this.file, false);

            // get workbookpart
            WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;


            try
            {
                // get all the defined area 
                List<DefinedNameVal> namesTable = BuildDefinedNamesTable(workbookPart);

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

        private List<DefinedNameVal> BuildDefinedNamesTable(WorkbookPart workbookPart)
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

        private List<DefinedNameVal> ChangeDefinedNamesTable(WorkbookPart workbookPart)
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

        private static WorksheetPart GetWorkSheetPart(WorkbookPart workbookPart, DefinedNameVal definedName)
        {
            //get worksheet based on defined name
            string relId = workbookPart.Workbook.Descendants<Sheet>()
            .Where(s => definedName.SheetName.Equals(s.Name))
            .First()
            .Id;
            return (WorksheetPart)workbookPart.GetPartById(relId);
        }

        // Given a cell name, parses the specified cell to get the column number.
        private static int GetColumnNumber(string columnName)
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

        // Given a cell name, parses the specified cell to get the column name.
        private static string getColumnName(string cellName)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellName);

            return match.Value;
        }

        private string getColumnIndex(int index, int offset=1)
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

        private void generateStyle(SpreadsheetDocument spreadsheetDocument)
        {
            CellFormats cellFormats = spreadsheetDocument.WorkbookPart.WorkbookStylesPart.Stylesheet.Elements<CellFormats>().First();
            //number 0,00
            CellFormat cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)2U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U, ApplyNumberFormat = true };
            cellFormats.Append(cellFormat);
            styleIndexArray[0] = (uint)cellFormats.Count++;
            //number 0
            cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)1U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U, ApplyNumberFormat = true };
            cellFormats.Append(cellFormat);
            styleIndexArray[1] = (uint)cellFormats.Count++;
            //text
            cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)49U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U, ApplyNumberFormat = true };
            cellFormats.Append(cellFormat);
            styleIndexArray[2] = (uint)cellFormats.Count++;
            //date
            cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)14U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U, ApplyNumberFormat = true };
            cellFormats.Append(cellFormat);
            styleIndexArray[3] = (uint)cellFormats.Count++;
        }

        private uint getExcelStyleIndex(string systemType, uint[] styleIndex)
        {
            if (systemType == "Double" || systemType == "Decimal")
                return styleIndex[0];
            if (systemType == "Int16" || systemType == "Int32" || systemType == "Int64" || systemType == "UInt16" || systemType == "UInt32" || systemType == "UInt64")
                return styleIndex[1];
            if (systemType == "Char" || systemType == "String")
                return styleIndex[2];
            if (systemType == "DateTime")
                return styleIndex[3];
            if (systemType == "Boolean")
                return styleIndex[2];
            return styleIndex[2];
        }

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

        private List<VariableIdentifier> GetVariableIdentifiers(WorksheetPart worksheetPart, int startRow, int endRow)
        {
            //NEW OPENXMLREADER
            if (this.VariableIndentifiers == null || this.VariableIndentifiers.Count == 0)
            {

                OpenXmlReader reader = OpenXmlReader.Create(worksheetPart);
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

                                if (row.Hidden == null) variableIdentifierRows.Add(RowToList(row));
                                else if (row.Hidden != true) variableIdentifierRows.Add(RowToList(row));

                            }

                        } while (reader.ReadNextSibling() && rowNum < endRow); // Skip to the next row
                        break;
                    }
                }

                // convert variable rows to VariableIdentifiers
                if (variableIdentifierRows != null)
                {
                    foreach (List<string> l in variableIdentifierRows)
                    {
                        //create headerVariables
                        if (VariableIndentifiers.Count == 0)
                        {
                            foreach (string s in l)
                            {
                                VariableIdentifier hv = new VariableIdentifier();
                                hv.name = s;
                                VariableIndentifiers.Add(hv);
                            }
                        }
                        else
                        {
                            foreach (string s in l)
                            {
                                int id = Convert.ToInt32(s);
                                int index = l.IndexOf(s);
                                VariableIndentifiers.ElementAt(index).id = id;
                            }
                        }
                    }
                }
            }

            if (this.VariableIndentifiers != null) return this.VariableIndentifiers;
            else return null;
        }

        private List<string> RowToList(Row r)
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
                        int cellReferencAsInterger = GetColumnNumber(getColumnName(c.CellReference));
                        int start = GetColumnNumber(this._areaOfData.StartColumn);
                        int end = GetColumnNumber(this._areaOfData.EndColumn);

                        if (cellReferencAsInterger >= start && cellReferencAsInterger <= end)
                        {

                            // if Value a text
                            if (c.DataType != null && c.DataType.HasValue && c.DataType.Value == CellValues.SharedString)
                            {
                                int sharedStringIndex = int.Parse(c.CellValue.Text, CultureInfo.InvariantCulture);
                                SharedStringItem sharedStringItem = _sharedStrings[sharedStringIndex];
                                value = sharedStringItem.InnerText;

                            }
                            // not a text
                            else if (c.StyleIndex != null && c.StyleIndex.HasValue)
                            {
                                uint styleIndex = c.StyleIndex.Value;
                                CellFormat cellFormat = _stylesheet.CellFormats.ChildElements[(int)styleIndex] as CellFormat;
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
                                        if (_stylesheet.NumberingFormats != null && _stylesheet.NumberingFormats.Any(numFormat => ((NumberingFormat)numFormat).NumberFormatId.Value == numberFormatId))
                                        {
                                            NumberingFormat numberFormat = _stylesheet.NumberingFormats.First(numFormat => ((NumberingFormat)numFormat).NumberFormatId.Value == numberFormatId) as NumberingFormat;

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

        private List<long> GetVariableIds(string[] stringlist)
        {
            List<long> list = new List<long>();

            if(stringlist != null)
            {
                foreach(string id in stringlist)
                {
                    list.Add(Convert.ToInt64(id));
                }
            }

            return list;
        }

    #endregion

    }

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
