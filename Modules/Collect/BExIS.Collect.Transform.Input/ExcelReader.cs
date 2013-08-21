using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.DCM.Transform.Input;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Globalization;
using System.Text.RegularExpressions;
using BExIS.DCM.Transform.Validation.Exceptions;
using BExIS.DCM.Transform.Validation.DSValidation;



namespace BExIS.DCM.Transform.Input
{
    /// <summary>
    /// http://blogs.msdn.com/b/brian_jones/archive/2010/05/27/parsing-and-reading-large-excel-files-with-the-open-xml-sdk.aspx
    /// </summary>
    

    public class ExcelReader:DataReader
    {
        private SharedStringItem[] _sharedStrings;
        private Stylesheet _stylesheet = new Stylesheet();
        SpreadsheetDocument spreadsheetDocument;
        private Worksheet worksheet;

        //private List<VariableValue> _variableValues = new List<VariableValue>();
        
        private DefinedNameVal _areaOfData = new DefinedNameVal();
        private DefinedNameVal _areaOfVariables = new DefinedNameVal();


        
       
        // read data 
        int startColumn = 0;
        int endColumn = 0;
        int numOfColumns = 0;
        int offset = 0;

        int rows = 0;

        private char[] alphabet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        #region read file
        
        // read file using template
        public List<DataTuple> ReadFile(Stream file, string fileName, StructuredDataStructure sds, long datasetId)
        {

            this.file = file;
            this.fileName = fileName;

            this.structuredDataStructure = sds;
            //this.info = efri;
            this._datasetId = datasetId;

            // open excel file
            spreadsheetDocument = SpreadsheetDocument.Open(this.file, false);

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
            offset = GetColumnNumber(GetColumnName(this._areaOfData.StartColumn)) - 1;

            // select worksheetpart by selected defined name area like data in sheet
            // sheet where data area is inside
            WorksheetPart worksheetPart = GetWorkSheetPart(workbookPart, this._areaOfData);
            //worksheet = worksheetPart.Worksheet;
            // get styleSheet
            _stylesheet = workbookPart.WorkbookStylesPart.Stylesheet;

            // Get shared strings
            _sharedStrings = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ToArray();

            if (GetSubmitedVariableIdentifier(worksheetPart, this._areaOfVariables.StartRow, this._areaOfVariables.EndRow)!=null)
            {
                ReadRows(worksheetPart, this._areaOfData.StartRow, this._areaOfData.EndRow);
            }

            return this.dataTuples;
        }

        // read file without using template
        public List<DataTuple> ReadFile(Stream file, string fileName, FileReaderInfo fri, StructuredDataStructure sds, long datasetId)
        {
            this.file = file;
            this.fileName = fileName;

            this.structuredDataStructure = sds;
            this.info = fri;
            this._datasetId = datasetId;

             // Check params
            if (this.file == null)
            {
                this.errorMessages.Add(new Error(ErrorType.Other, "File not exist"));
            }
            if (!this.file.CanRead)
            {
                this.errorMessages.Add(new Error(ErrorType.Other, "File is not readable"));
            }
            if (this.info.Variables <= 0)
            {
                this.errorMessages.Add(new Error(ErrorType.Other, "Startrow of Variable can´t be 0"));
            }
            if (this.info.Data <= 0)
            {
                this.errorMessages.Add(new Error(ErrorType.Other, "Startrow of Data can´t be 0"));
            }

            if (this.errorMessages.Count == 0)
            {
                // open excel file
                spreadsheetDocument = SpreadsheetDocument.Open(this.file, false);

                // get workbookpart
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

                SheetDimension dimension = workbookPart.WorksheetParts.First().Worksheet.GetFirstChild<SheetDimension>();

               string s =  dimension.Reference.Value;

               string[] references = s.Split(':'); 
                 

                // get all the defined area 
                //List<DefinedNameVal> namesTable = BuildDefinedNamesTable(workbookPart);


                // Get intergers for reading data
                startColumn = GetColumnNumber(GetColumnName(references[0]));
                endColumn = GetColumnNumber(GetColumnName(references[1]));
                
                numOfColumns = (endColumn - startColumn) + 1;
                offset = this.info.Offset;

                int endRowData = GetRowNumber(references[1]);
                
                // select worksheetpart by selected defined name area like data in sheet
                // sheet where data area is inside
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.First(); //GetWorkSheetPart(workbookPart, this._areaOfData);

                // get styleSheet
                _stylesheet = workbookPart.WorkbookStylesPart.Stylesheet;

                // Get shared strings
                _sharedStrings = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ToArray();

                if (GetSubmitedVariableIdentifier(worksheetPart, this.info.Variables, this.info.Variables) != null)
                {
                    ReadRows(worksheetPart, this.info.Data, endRowData);
                }

                return this.dataTuples;


            }

            return this.dataTuples;
        }

        protected void ReadRows(WorksheetPart worksheetPart, int startRow, int endRow)
        {
            OpenXmlReader reader = OpenXmlReader.Create(worksheetPart);
            int count = 0;
            int rowNum = 0;

            while (reader.Read())
            {
                if (reader.ElementType == typeof(Row))
                {
                    do
                    {
                        if (reader.HasAttributes)
                            rowNum = Convert.ToInt32(reader.Attributes.First(a => a.LocalName == "r").Value);

                        if (endRow == 0)
                        {
                            if (rowNum >= startRow)
                            {
                                Row row = (Row)reader.LoadCurrentElement();

                                //this.errorMessages = this.errorMessages.Union(Validate(RowToList(row), Convert.ToInt32(row.RowIndex.ToString()))).ToList();
                                this.dataTuples.Add(ReadRow(RowToList(row), Convert.ToInt32(row.RowIndex.ToString())));
                                count++;
                            }
                        }
                        else
                        {
                            if (rowNum >= startRow && rowNum <= endRow)
                            {
                                Row row = (Row)reader.LoadCurrentElement();

                                //this.errorMessages = this.errorMessages.Union(Validate(RowToList(row), Convert.ToInt32(row.RowIndex.ToString()))).ToList();
                                this.dataTuples.Add(ReadRow(RowToList(row), Convert.ToInt32(row.RowIndex.ToString())));
                                count++;

                            }
                        }


                    } while (reader.ReadNextSibling()); // Skip to the next row

                    break;

                }

            }
        }

        #endregion

        #region validate
        
        // read file using template
        public void ValidateFile(Stream file, string fileName, StructuredDataStructure sds, long datasetId)
        {
            this.file = file;
            this.fileName = fileName;

            this.structuredDataStructure = sds;
            //this.info = efri;
            this._datasetId = datasetId;

            // open excel file
            spreadsheetDocument = SpreadsheetDocument.Open(this.file, false);

            // get workbookpart
            WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

            // get all the defined area 
            List<DefinedNameVal> namesTable = BuildDefinedNamesTable(workbookPart);

            // select data area
            this._areaOfData = namesTable.Where(p => p.Key.Equals("Data")).FirstOrDefault();
            if(this._areaOfData == null) this.errorMessages.Add(new Error(ErrorType.Other,"Data area is not defined in the excel template."));

            // Select variable area
            this._areaOfVariables = namesTable.Where(p => p.Key.Equals("VariableIdentifiers")).FirstOrDefault();
            if(this._areaOfVariables == null) this.errorMessages.Add(new Error(ErrorType.Other,"VariableIdentifiers area is not defined in the excel template."));

            // Get intergers for reading data
            startColumn = GetColumnNumber(this._areaOfData.StartColumn);
            endColumn = GetColumnNumber(this._areaOfData.EndColumn);

            numOfColumns = (endColumn - startColumn) + 1;
            offset = GetColumnNumber(GetColumnName(this._areaOfData.StartColumn)) - 1;

            // select worksheetpart by selected defined name area like data in sheet
            // sheet where data area is inside
            WorksheetPart worksheetPart = GetWorkSheetPart(workbookPart, this._areaOfData);
            //worksheet = worksheetPart.Worksheet;
            // get styleSheet
            _stylesheet = workbookPart.WorkbookStylesPart.Stylesheet;

            // Get shared strings
            _sharedStrings = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ToArray();

            if (this.errorMessages.Count == 0)
            {
                if (ValidateDatastructure(worksheetPart, this._areaOfVariables.StartRow, this._areaOfVariables.EndRow))
                {
                    ValidateRows(worksheetPart, this._areaOfData.StartRow, this._areaOfData.EndRow);
                }
            }
           
        
            // close fehlt
        }

        protected bool ValidateDatastructure(WorksheetPart worksheetPart, int startRow, int endRow)
        {
            List<Error> errorList;

            errorList = ValidateComparisonWithDatatsructure(GetSubmitedVariableIdentifier(worksheetPart, startRow, endRow));

            if (errorList != null)
            {
                if (errorList.Count > 0)
                {
                    this.errorMessages = this.errorMessages.Concat(errorList).ToList();
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <returns>rows as list of strings</returns>
        protected void ValidateRows(WorksheetPart worksheetPart, int startRow, int endRow)
        {
            //NEW OPENXMLREADER

            OpenXmlReader reader = OpenXmlReader.Create(worksheetPart);
            int count = 0;
            int rowNum = 0;

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

                            this.errorMessages = this.errorMessages.Union(ValidateRow(RowToList(row), rowNum)).ToList();
                            count++;


                        }
                    } while (reader.ReadNextSibling()); // Skip to the next row

                    break;

                }

            }
        }

        #endregion

        #region helper methods
        
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
                        int cellReferencAsInterger = GetColumnNumber(GetColumnName(c.CellReference));
                        int start = GetColumnNumber(this._areaOfData.StartColumn);
                        int end = GetColumnNumber(this._areaOfData.EndColumn);

                        if (cellReferencAsInterger >= start && cellReferencAsInterger<=end)
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
                            int index = cellReferencAsInterger - offset-1;
                            rowAsStringArray[index] = value;
                        }
                    }//end if cell value
                }//end if cell null

            }//for

            return rowAsStringArray.ToList();
        }

        private List<VariableIdentifier> GetSubmitedVariableIdentifier(WorksheetPart worksheetPart, int startRow, int endRow)
        {
            //NEW OPENXMLREADER
            if (this.SubmitedVariableIndentifiers == null || this.SubmitedVariableIndentifiers.Count == 0)
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
                        if (SubmitedVariableIndentifiers.Count == 0)
                        {
                            foreach (string s in l)
                            {
                                VariableIdentifier hv = new VariableIdentifier();
                                hv.name = s;
                                SubmitedVariableIndentifiers.Add(hv);
                            }
                        }
                        else
                        {
                            foreach (string s in l)
                            {
                                int id = Convert.ToInt32(s);
                                int index = l.IndexOf(s);
                                SubmitedVariableIndentifiers.ElementAt(index).id = id;
                            }
                        }
                    }
                }
            }

            if (this.SubmitedVariableIndentifiers != null) return this.SubmitedVariableIndentifiers;
            else return null;
        }

        private List<DefinedNameVal> BuildDefinedNamesTable(WorkbookPart workbookPart)
        {
            //Build a list
            List<DefinedNameVal> definedNames = new List<DefinedNameVal>();
            foreach (DefinedName name in workbookPart.Workbook.GetFirstChild<DefinedNames>())
            {
                //Parse defined name string...
                string key = name.Name;
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

         // Given a cell name, parses the specified cell to get the column name.
        private static string GetColumnName(string cellName)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellName);

            return match.Value;
        }

        // Given a cell name, parses the specified cell to get the column name.
        private static int GetRowNumber(string cellName)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[0-9]+");
            Match match = regex.Match(cellName);

            return Convert.ToInt32(match.Value);
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

        #endregion

    }

    public class DefinedNameVal
    {
        public string Key ="";
        public string SheetName="";
        public string StartColumn="";
        public int StartRow=0;
        public string EndColumn="";
        public int EndRow=0;
    }

}
