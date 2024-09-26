using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.IO.Transform.Validation.DSValidation;
using BExIS.IO.Transform.Validation.Exceptions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

/// <summary>
///
/// </summary>
namespace BExIS.IO.Transform.Input
{
    /// <summary>
    /// http://blogs.msdn.com/b/brian_jones/archive/2010/05/27/parsing-and-reading-large-excel-files-with-the-open-xml-sdk.aspx
    /// </summary>

    /// <summary>
    /// this class is used to read and validate excel files
    /// </summary>
    /// <remarks></remarks>
    public class ExcelReader : DataReader
    {
        public static List<string> SUPPORTED_APPLICATIONS = new List<string>() { "Microsoft Excel" };

        protected SharedStringItem[] _sharedStrings;
        protected Stylesheet _stylesheet = new Stylesheet();
        protected SpreadsheetDocument spreadsheetDocument;
        protected Worksheet worksheet;
        protected DefinedNameVal _areaOfData = new DefinedNameVal();
        protected DefinedNameVal _areaOfVariables = new DefinedNameVal();

        // read data
        protected int startColumn = 0;

        protected int endColumn = 0;
        protected int numOfColumns = 0;
        protected int offset = 0;

        protected DecimalCharacter decimalCharacter = 0;

        protected int rows = 0;

        protected char[] alphabet = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        //Properties from File
        public string Application = "";

        public string ApplicationVersion = "";

        public ExcelReader(StructuredDataStructure structuredDatastructure, ExcelFileReaderInfo fileReaderInfo) : base(structuredDatastructure, fileReaderInfo)
        {
            NumberOfRows = 0;
            NumberOSkippedfRows = 0;
        }

        public ExcelReader(StructuredDataStructure structuredDatastructure, ExcelFileReaderInfo fileReaderInfo, IOUtility iOUtility) : base(structuredDatastructure, fileReaderInfo, iOUtility)
        {
            NumberOSkippedfRows = 0;
            NumberOfRows = 0;
        }

        public ExcelReader(StructuredDataStructure structuredDatastructure, ExcelFileReaderInfo fileReaderInfo, IOUtility iOUtility, DatasetManager datasetManager) : base(structuredDatastructure, fileReaderInfo, iOUtility, datasetManager)
        {
            NumberOfRows = 0;
            NumberOSkippedfRows = 0;
        }

        public override FileStream Open(string fileName)
        {
            FileStream tmp = base.Open(fileName);
            loadProperties(tmp);

            return tmp;
        }

        #region read file

        /// <summary>
        /// return true if the Excel file based on a template
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="file"> file as stream</param>
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

        private void loadProperties(Stream file)
        {
            try
            {
                this.FileStream = file;

                // open excel file
                spreadsheetDocument = SpreadsheetDocument.Open(this.FileStream, false);

                if (spreadsheetDocument != null)
                {
                    if (spreadsheetDocument.ExtendedFilePropertiesPart.Properties.Application != null)
                    {
                        Application = spreadsheetDocument.ExtendedFilePropertiesPart.Properties.Application.InnerText;
                    }

                    if (spreadsheetDocument.ExtendedFilePropertiesPart.Properties.ApplicationVersion != null)
                    {
                        ApplicationVersion = spreadsheetDocument.ExtendedFilePropertiesPart.Properties.ApplicationVersion.InnerText;
                    }
                }
            }
            catch (Exception)
            {
                this.FileStream.Close();
                throw new NotSupportedException("This Excel file is not created by common programs. Please open the file in Excel, save it and try again.");
            }
        }

        /// <summary>
        /// Read a Excel Template file
        /// Convert the rows into a data tuple based on the data structure.
        /// Return a list of data tuples
        /// </summary>
        /// <remarks>Only when excel template is in use</remarks>
        /// <seealso cref=""/>
        /// <param name="file">File as stream</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="sds">StructuredDataStructure of a dataset</param>
        /// <param name="datasetId">Datasetid of a dataset</param>
        /// <returns>List of DataTuples</returns>
        public List<DataTuple> ReadFile(Stream file, string fileName, long datasetId)
        {
            this.FileStream = file;
            this.FileName = fileName;

            //this.info = efri;
            this.DatasetId = datasetId;

            // open excel file
            spreadsheetDocument = SpreadsheetDocument.Open(this.FileStream, false);

            // get workbookpart
            WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

            // get all the defined area
            List<DefinedNameVal> namesTable = BuildDefinedNamesTable(workbookPart);

            // select data area
            this._areaOfData = namesTable.Where(p => p.Key.Equals("Data")).FirstOrDefault();

            // Select variable area
            this._areaOfVariables = namesTable.Where(p => p.Key.Equals("VariableIdentifiers")).FirstOrDefault();

            // Get integers for reading data
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

            if (GetSubmitedVariableIdentifier(worksheetPart, this._areaOfVariables.StartRow, this._areaOfVariables.EndRow) != null)
            {
                ReadRows(worksheetPart, this._areaOfData.StartRow, this._areaOfData.EndRow);
            }

            return this.DataTuples;
        }

        /// <summary>
        /// Read a Excel Template file wth package size
        /// Convert the rows into a data tuple based on the data structure.
        /// Return a list of data tuples
        /// </summary>
        /// <remarks>Only when excel template is in use</remarks>
        /// <seealso cref=""/>
        /// <param name="file">File as stream</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="sds">StructuredDataStructure of a dataset</param>
        /// <param name="datasetId">Datasetid of a dataset</param>
        /// <returns>List of DataTuples</returns>
        //[MeasurePerformance]
        public List<DataTuple> ReadTemplateFile(Stream file, string fileName, long datasetId, int packageSize)
        {
            this.FileStream = file;
            this.FileName = fileName;

            // clear lsit of datatuples for the next package
            this.DataTuples = new List<DataTuple>();

            //this.StructuredDataStructure = sds;
            //this.info = efri;
            this.DatasetId = datasetId;

            // open excel file
            spreadsheetDocument = SpreadsheetDocument.Open(this.FileStream, false);

            // get workbookpart
            WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

            // get all the defined area
            List<DefinedNameVal> namesTable = BuildDefinedNamesTable(workbookPart);

            // select data area
            this._areaOfData = namesTable.Where(p => p.Key.Equals("Data")).FirstOrDefault();

            // Select variable area
            this._areaOfVariables = namesTable.Where(p => p.Key.Equals("VariableIdentifiers")).FirstOrDefault();

            // Get integers for reading data
            startColumn = GetColumnNumber(this._areaOfData.StartColumn);
            endColumn = GetColumnNumber(this._areaOfData.EndColumn);

            numOfColumns = (endColumn - startColumn) + 1;
            offset = GetColumnNumber(GetColumnName(this._areaOfData.StartColumn)) - 1;

            if (Info == null) Info = new FileReaderInfo();
            Info.Decimal = DecimalCharacter.point;

            // select worksheetpart by selected defined name area like data in sheet
            // sheet where data area is inside
            WorksheetPart worksheetPart = GetWorkSheetPart(workbookPart, this._areaOfData);
            //worksheet = worksheetPart.Worksheet;
            // get styleSheet
            _stylesheet = workbookPart.WorkbookStylesPart.Stylesheet;

            // Get shared strings
            _sharedStrings = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ToArray();

            if (Position == 1)
            {
                Position = this._areaOfData.StartRow;
            }

            int endPosition = (Position + packageSize) - 1;

            if (endPosition > this._areaOfData.EndRow)
                endPosition = this._areaOfData.EndRow;

            if (ValidateDatastructure(worksheetPart, this._areaOfVariables.StartRow, this._areaOfVariables.EndRow))
            {
                ReadRows(worksheetPart, Position, endPosition);
                Position += packageSize;
            }

            return this.DataTuples;
        }

        /// <summary>
        /// Read a Excel row by row
        /// Convert the rows into a data tuple based on the data structure.
        /// Return a list of data tuples
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="file">File as stream</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="fri">FileReaderInfo (ExcelFileReaderInfo) for additional information to read the file</param>
        /// <param name="sds">StructuredDataStructure of a dataset</param>
        /// <param name="datasetId">Datasetid of a dataset</param>
        /// <returns>List of DataTuples</returns>
        public List<DataTuple> ReadFile(Stream file, string fileName, long datasetId, int packageSize)
        {
            this.FileStream = file;
            this.FileName = fileName;
            this.DatasetId = datasetId;

            // clear list of data tuples for the next package
            this.DataTuples = new List<DataTuple>();

            ExcelFileReaderInfo fri = (ExcelFileReaderInfo)Info;

            // Check params
            if (this.FileStream == null)
            {
                this.ErrorMessages.Add(new Error(ErrorType.Other, "File not exist"));
            }
            if (!this.FileStream.CanRead)
            {
                this.ErrorMessages.Add(new Error(ErrorType.Other, "File is not readable"));
            }
            if (fri.VariablesStartRow <= 0)
            {
                this.ErrorMessages.Add(new Error(ErrorType.Other, "Startrow of Variables can´t be 0"));
            }
            if (fri.DataStartRow <= 0)
            {
                this.ErrorMessages.Add(new Error(ErrorType.Other, "Startrow of Data can´t be 0"));
            }

            if (this.ErrorMessages.Count == 0)
            {
                // open excel file
                spreadsheetDocument = SpreadsheetDocument.Open(this.FileStream, false);

                // get workbookpart
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

                startColumn = fri.DataStartColumn;
                endColumn = fri.DataEndColumn;

                numOfColumns = (endColumn - startColumn) + 1;
                offset = fri.Offset;

                int endRowData = fri.DataEndRow;

                // select worksheetpart by selected defined name area like data in sheet
                // sheet where data area is inside
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.First(); //GetWorkSheetPart(workbookPart, this._areaOfData);

                if (!string.IsNullOrEmpty(fri.WorksheetUri))
                {
                    worksheetPart = workbookPart.WorksheetParts.Where(ws => ws.Uri.ToString() == fri.WorksheetUri).FirstOrDefault();
                }

                // get styleSheet
                _stylesheet = workbookPart.WorkbookStylesPart.Stylesheet;

                // Get shared strings
                _sharedStrings = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ToArray();

                if (Position == 1)
                {
                    Position = fri.DataStartRow;
                }
                //else
                //    Position++;

                int endPosition = (Position + packageSize) - 1;

                if (endPosition > endRowData)
                    endPosition = endRowData;

                if (ValidateDatastructure(worksheetPart, fri.VariablesStartRow, fri.VariablesEndRow))
                {
                    ReadRows(worksheetPart, Position, endPosition);
                    Position += packageSize;
                }

                return this.DataTuples;
            }

            return this.DataTuples;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="file"></param>
        /// <param name="fileName"></param>
        /// <param name="sds"></param>
        /// <param name="datasetId"></param>
        /// <param name="variableList"></param>
        /// <param name="packageSize"></param>
        /// <returns></returns>
        public List<List<string>> ReadValuesFromFile(Stream file, string fileName, long datasetId, List<long> variableList, int packageSize)
        {
            List<List<string>> listOfSelectedvalues = new List<List<string>>();

            this.FileStream = file;
            this.FileName = fileName;
            this.DatasetId = datasetId;

            // open excel file
            spreadsheetDocument = SpreadsheetDocument.Open(this.FileStream, false);

            // get workbookpart
            WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

            // get all the defined area
            List<DefinedNameVal> namesTable = BuildDefinedNamesTable(workbookPart);

            // select data area
            this._areaOfData = namesTable.Where(p => p.Key.Equals("Data")).FirstOrDefault();

            // Select variable area
            this._areaOfVariables = namesTable.Where(p => p.Key.Equals("VariableIdentifiers")).FirstOrDefault();

            // Get integers for reading data
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

            if (Position == 1)
            {
                Position = this._areaOfData.StartRow;
            }

            if (GetSubmitedVariableIdentifier(worksheetPart, this._areaOfVariables.StartRow, this._areaOfVariables.EndRow) != null)
            {
                listOfSelectedvalues = GetValuesFromRows(worksheetPart, variableList, Position, (Position + packageSize) - 1);
                Position += packageSize;
            }

            return listOfSelectedvalues;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="worksheetPart"></param>
        /// <param name="variableList"></param>
        /// <param name="startRow"></param>
        /// <param name="endRow"></param>
        /// <returns></returns>
        private List<List<string>> GetValuesFromRows(WorksheetPart worksheetPart, List<long> variableList, int startRow, int endRow)
        {
            List<List<string>> temp = new List<List<string>>();

            using (OpenXmlReader reader = OpenXmlReader.Create(worksheetPart))
            {
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

                                    temp.Add(RowToList(row, variableList));

                                    count++;
                                }
                            }
                            else
                            {
                                if (rowNum >= startRow && rowNum <= endRow)
                                {
                                    Row row = (Row)reader.LoadCurrentElement();

                                    temp.Add(RowToList(row, variableList));
                                    count++;
                                }
                            }
                        } while (reader.ReadNextSibling()); // Skip to the next row

                        break;
                    }
                }
            }

            return temp;
        }

        /// <summary>
        /// Read rows from worksheetPart starts from a start row and ends on the end row
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="worksheetPart">part of a excel worksheet</param>
        /// <param name="startRow">start row</param>
        /// <param name="endRow">end row</param>
        protected void ReadRows(WorksheetPart worksheetPart, int startRow, int endRow)
        {
            using (OpenXmlReader reader = OpenXmlReader.Create(worksheetPart))
            {
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

                            if (rowNum >= startRow && ((rowNum <= endRow) || (endRow == 0)))
                            {
                                Row row = (Row)reader.LoadCurrentElement();

                                if (!IsEmpty(row))
                                {
                                    this.DataTuples.Add(ReadRow(RowToList(row), Convert.ToInt32(row.RowIndex.ToString())));
                                    count++;
                                }
                            }
                        } while (reader.ReadNextSibling()); // Skip to the next row

                        break;
                    }
                }
                // (12 - 2) - 10 = 0
                NumberOSkippedfRows += ((endRow + 1) - startRow) - count;
                Debug.WriteLine("NumberOSkippedfRows += ((endRow+1) - startRow) - count");
                Debug.WriteLine(NumberOSkippedfRows + " += (" + endRow + 1 + " - " + startRow + ") - " + count + ");");
            }
        }

        #endregion read file

        #region validate

        /// <summary>
        /// Validate a Excel Template file
        /// </summary>
        /// <remarks>Only when excel template is in use</remarks>
        /// <seealso cref=""/>
        /// <param name="file">File as stream</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="sds">StructuredDataStructure of a dataset</param>
        /// <param name="datasetId">Datasetid of a dataset</param>
        public void ValidateTemplateFile(Stream file, string fileName, long datasetId)
        {
            this.FileStream = file;
            this.FileName = fileName;

            //this.Info = efri;
            this.DatasetId = datasetId;

            // open excel file
            spreadsheetDocument = SpreadsheetDocument.Open(this.FileStream, false);

            // get workbookpart
            WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

            // get all the defined area
            List<DefinedNameVal> namesTable = BuildDefinedNamesTable(workbookPart);

            // select data area
            this._areaOfData = namesTable.Where(p => p.Key.Equals("Data")).FirstOrDefault();
            if (this._areaOfData == null) this.ErrorMessages.Add(new Error(ErrorType.Other, "Data area is not defined in the excel template."));

            // Select variable area
            this._areaOfVariables = namesTable.Where(p => p.Key.Equals("VariableIdentifiers")).FirstOrDefault();
            if (this._areaOfVariables == null) this.ErrorMessages.Add(new Error(ErrorType.Other, "VariableIdentifiers area is not defined in the excel template."));

            // Get integers for reading data
            startColumn = GetColumnNumber(this._areaOfData.StartColumn);
            endColumn = GetColumnNumber(this._areaOfData.EndColumn);

            numOfColumns = (endColumn - startColumn) + 1;
            offset = GetColumnNumber(GetColumnName(this._areaOfData.StartColumn)) - 1;

            // select worksheet part by selected defined name area like data in sheet
            // sheet where data area is inside
            WorksheetPart worksheetPart = GetWorkSheetPart(workbookPart, this._areaOfData);
            //worksheet = worksheetPart.Worksheet;
            // get styleSheet
            _stylesheet = workbookPart.WorkbookStylesPart.Stylesheet;

            // Get shared strings
            _sharedStrings = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ToArray();

            if (this.ErrorMessages.Count == 0)
            {
                if (ValidateDatastructure(worksheetPart, this._areaOfVariables.StartRow, this._areaOfVariables.EndRow))
                {
                    ValidateRows(worksheetPart, this._areaOfData.StartRow, this._areaOfData.EndRow);
                }
            }
            // close fehlt
        }

        /// <summary>
        /// Validate a Excel Template file
        /// </summary>
        /// <remarks>Only when excel template is in use</remarks>
        /// <seealso cref=""/>
        /// <param name="file">File as stream</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="sds">StructuredDataStructure of a dataset</param>
        /// <param name="datasetId">Datasetid of a dataset</param>
        public void ValidateFile(Stream file, string fileName, long datasetId)
        {
            this.FileStream = file;
            this.FileName = fileName;
            this.DatasetId = datasetId;
            ExcelFileReaderInfo fri = (ExcelFileReaderInfo)Info;

            // open excel file
            spreadsheetDocument = SpreadsheetDocument.Open(this.FileStream, false);

            // get workbookpart
            WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

            startColumn = fri.DataStartColumn;
            endColumn = fri.DataEndColumn;

            numOfColumns = (endColumn - startColumn) + 1;
            offset = fri.Offset;

            int endRowData = fri.DataEndRow;

            // select worksheetpart by selected defined name area like data in sheet
            // select worksheetpart by Uri
            WorksheetPart worksheetPart = workbookPart.WorksheetParts.Where(ws => ws.Uri.ToString() == fri.WorksheetUri).FirstOrDefault();

            // get styleSheet
            _stylesheet = workbookPart.WorkbookStylesPart.Stylesheet;

            // Get shared strings
            _sharedStrings = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ToArray();

            if (this.ErrorMessages.Count == 0)
            {
                if (ValidateDatastructure(worksheetPart, fri.VariablesStartRow, fri.VariablesEndRow))
                {
                    ValidateRows(worksheetPart, fri.DataStartRow, endRowData);
                }
            }
            // close fehlt
        }

        /// <summary>
        /// Validate Datastructure from file
        /// true = valid
        /// false = not valid
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="worksheetPart">Part of a excel worksheet where the data structure is located</param>
        /// <param name="startRow">Row where the DataStructure is starting</param>
        /// <param name="endRow">Row where the DataStructure is ending</param>
        protected bool ValidateDatastructure(WorksheetPart worksheetPart, int startRow, int endRow)
        {
            List<Error> errorList;

            errorList = ValidateComparisonWithDatatsructure(GetSubmitedVariableIdentifier(worksheetPart, startRow, endRow));

            if (errorList != null)
            {
                if (errorList.Distinct().ToList().Count > 0)
                {
                    this.ErrorMessages = this.ErrorMessages.Concat(errorList).ToList();
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Validate rows from file
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="worksheetPart">Part of a excel worksheet where the datastructure is located</param>
        /// <param name="startRow">Rows starting</param>
        /// <param name="endRow">Rows ending</param>
        protected void ValidateRows(WorksheetPart worksheetPart, int startRow, int endRow)
        {
            //NEW OPENXMLREADER

            using (OpenXmlReader reader = OpenXmlReader.Create(worksheetPart))
            {
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

                                this.ErrorMessages = this.ErrorMessages.Union(ValidateRow(RowToList(row), rowNum)).ToList();
                                count++;
                            }
                        } while (reader.ReadNextSibling()); // Skip to the next row

                        break;
                    }
                }
            }
        }

        #endregion validate

        #region helper methods

        ///// <summary>
        ///// Convert a excel row to a list of strings
        ///// Every cell is one value
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param name="r"> row from a excel file</param>
        ///// <returns>list of string for each cell in the row</returns>
        //private List<string> RowToList(Row r)
        //{
        //    string[] rowAsStringArray = new string[numOfColumns];

        //    // create a new cell
        //    Cell c = new Cell();

        //    for (int i = 0; i < r.ChildElements.Count(); i++)
        //    {
        //        // get current cell at i
        //        c = r.Elements<Cell>().ElementAt(i);

        //        string value = "";

        //        if (c != null)
        //        {
        //            int cellReferencAsInterger = GetColumnNumber(GetColumnName(c.CellReference));

        //            if (c.CellValue != null)
        //            {
        //                // if cell reference in range of the area
        //                int start = startColumn;//GetColumnNumber(this._areaOfData.StartColumn);
        //                int end = endColumn;//GetColumnNumber(this._areaOfData.EndColumn);

        //                if (cellReferencAsInterger >= start && cellReferencAsInterger <= end)
        //                {
        //                    // shared string
        //                    if (c.DataType != null && c.DataType.HasValue && c.DataType.Value == CellValues.SharedString)
        //                    {
        //                        int sharedStringIndex = int.Parse(c.CellValue.Text, CultureInfo.InvariantCulture);
        //                        SharedStringItem sharedStringItem = _sharedStrings[sharedStringIndex];
        //                        value = sharedStringItem.InnerText;
        //                    }
        //                    else {
        //                        value = c.CellValue.Text;
        //                    }

        //                    // define index based on cell refernce - offset
        //                    int index = cellReferencAsInterger - offset - 1;
        //                    rowAsStringArray[index] = value;
        //                }
        //            }//end if cell value
        //            else
        //            {
        //                if (cellReferencAsInterger >= startColumn && cellReferencAsInterger <= endColumn)
        //                {
        //                    int index = cellReferencAsInterger - offset - 1;
        //                    rowAsStringArray[index] = "";
        //                }
        //            }
        //        }//end if cell null
        //    }//for

        //    // replace all null values with "";
        //    for (int i = 0; i < rowAsStringArray.Length; i++)
        //    {
        //        if (rowAsStringArray[i] == null) rowAsStringArray[i] = "";
        //    }

        //    return rowAsStringArray.ToList();
        //}

        /// <summary>
        /// Convert a excel row to a list of strings
        /// Every cell is one value
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="r"> row from a excel file</param>
        /// <param name="varIds">if null all columns will return otherwise only the columns with the varids returned</param>
        /// <returns>list of string for each cell in the row</returns>
        private List<string> RowToList(Row r, List<long> varIds = null)
        {
            List<int> columns = null;
            int currentNumOfCloumns = numOfColumns;
            //get a subset of a row
            if (varIds != null)
            {
                columns = new List<int>();
                foreach (long id in varIds)
                {
                    //get the index of the variable identifier where id equals id from varids
                    int columnPosition = GetColumnNumber(this._areaOfData.StartColumn) + this.SubmitedVariableIdentifiers.IndexOf(this.SubmitedVariableIdentifiers.Where(p => p.id.Equals(id)).FirstOrDefault());
                    columns.Add(columnPosition);
                }

                currentNumOfCloumns = columns.Count;
            }

            string[] rowAsStringArray = new string[currentNumOfCloumns];

            // create a new cell
            Cell c = new Cell();

            for (int i = 0; i < r.ChildElements.Count(); i++)
            {
                // get current cell at i
                c = r.Elements<Cell>().ElementAt(i);

                string value = "";

                if (c != null)
                {
                    int cellReferencAsInterger = GetColumnNumber(GetColumnName(c.CellReference));

                    // create the index for the position in the rowAsStringArray
                    int index = cellReferencAsInterger - offset - 1;

                    // the columnPosition of the cell must be in the startColumn and endColumn range
                    // otherwise jump over it
                    if (cellReferencAsInterger >= startColumn && cellReferencAsInterger <= endColumn)
                    {
                        if (c.CellValue != null)
                        {
                            //if columns is not null means the function returns a subset of all columns based on the incoming varids
                            if ((columns == null || columns.Contains(cellReferencAsInterger)) && index >= 0)
                            {
                                // if Value a text
                                if (c.DataType != null && c.DataType.HasValue &&
                                   (c.DataType.Value == CellValues.SharedString || c.DataType.Value == CellValues.String || c.DataType.Value == CellValues.InlineString))
                                {
                                    // if the datatype is a shared string, then the value comes from a shared string table and the value inside is a
                                    // id for the shared string table
                                    if (c.DataType.Value == CellValues.SharedString)
                                    {
                                        int sharedStringIndex = int.Parse(c.CellValue.Text, CultureInfo.InvariantCulture);
                                        SharedStringItem sharedStringItem = _sharedStrings[sharedStringIndex];
                                        value = sharedStringItem.InnerText;
                                    }
                                    // if the excel cell datatype a CellValues.String or CellValues.Inline
                                    // then set the value by the value from the cell direct without some extra work
                                    else
                                    {
                                        value = c.CellValue.Text;
                                    }
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
                                            double tmp = 0;
                                            if (double.TryParse(c.CellValue.Text, out tmp)) value = ExcelHelper.FromExcelSerialDate(tmp).ToString();
                                            else value = c.CellValue.Text;
                                        }
                                        else // not a date
                                        {
                                            if (_stylesheet.NumberingFormats != null && _stylesheet.NumberingFormats.Any(numFormat => ((NumberingFormat)numFormat).NumberFormatId.Value == numberFormatId))
                                            {
                                                NumberingFormat numberFormat = _stylesheet.NumberingFormats.First(numFormat => ((NumberingFormat)numFormat).NumberFormatId.Value == numberFormatId) as NumberingFormat;

                                                if (numberFormat != null && numberFormat.FormatCode != null && numberFormat.FormatCode.HasValue)
                                                {
                                                    string formatCode = numberFormat.FormatCode.Value;
                                                    if ((formatCode.Contains("h") && formatCode.Contains("m")) || (formatCode.Contains("m") && formatCode.Contains("d")))
                                                    {
                                                        double tmp = 0;
                                                        if (double.TryParse(c.CellValue.Text, out tmp)) value = ExcelHelper.FromExcelSerialDate(tmp).ToString();
                                                        else value = c.CellValue.Text;
                                                    }
                                                    else
                                                    {
                                                        // check format code and round the value based on the decimal place
                                                        // contains e.g. #.## & 0.00
                                                        // contains e.g. 1,23E+02
                                                        // 1234567890,43759 = 1,23E+09

                                                        value = ExcelHelper.ConvertWithFormat(c.CellValue.Text, formatCode);
                                                    }
                                                }
                                                else //formatCode = null
                                                {
                                                    value = ExcelHelper.ConvertWithFormat(c.CellValue.Text, "");
                                                }
                                            }
                                            else //number format not exist in the stylesheet but may numberformatid exist
                                            {
                                                string formatcode = ExcelHelper.GetFormatCode(numberFormatId);

                                                value = ExcelHelper.ConvertWithFormat(c.CellValue.Text, formatcode);
                                            }
                                        }//numbers end
                                    }
                                    else
                                    {
                                        value = ExcelHelper.ConvertWithFormat(c.CellValue.Text, "");
                                    }
                                }
                                else //standard
                                {
                                    value = ExcelHelper.ConvertWithFormat(c.CellValue.Text, "");
                                }

                                // define index based on cell reference - offset
                                //int index = cellReferencAsInterger - offset - 1;
                                if ((columns == null || columns.Contains(cellReferencAsInterger)) && index >= 0)
                                {
                                    rowAsStringArray[index] = value;
                                }
                            }
                        }//end if cell value
                        else
                        {
                            if (columns == null || columns.Contains(cellReferencAsInterger))
                            {
                                rowAsStringArray[index] = value;
                            }
                        }//end if cell null
                    }
                }
            }//for

            // replace all null values with "";
            for (int i = 0; i < rowAsStringArray.Length; i++)
            {
                if (rowAsStringArray[i] == null) rowAsStringArray[i] = "";
            }

            return rowAsStringArray.ToList();
        }

        /// <summary>
        /// Generate a list of VariableIdentifiers from the excel file
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="worksheetPart">part of a excel worksheet</param>
        /// <param name="startRow">start row</param>
        /// <param name="endRow">end row</param>
        /// <returns>List of variableIdentifier</returns>
        protected List<VariableIdentifier> GetSubmitedVariableIdentifier(WorksheetPart worksheetPart, int startRow, int endRow)
        {
            //NEW OPENXMLREADER
            if (this.SubmitedVariableIdentifiers == null || this.SubmitedVariableIdentifiers.Count == 0)
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

                                    if (row.Hidden == null) VariableIdentifierRows.Add(RowToList(row));
                                    else if (row.Hidden != true) VariableIdentifierRows.Add(RowToList(row));
                                }
                            } while (reader.ReadNextSibling() && rowNum < endRow); // Skip to the next row
                            break;
                        }
                    }

                    // convert variable rows to VariableIdentifiers
                    if (VariableIdentifierRows != null)
                    {
                        foreach (List<string> l in VariableIdentifierRows)
                        {
                            //create headerVariables
                            if (SubmitedVariableIdentifiers.Count == 0)
                            {
                                foreach (string s in l)
                                {
                                    VariableIdentifier hv = new VariableIdentifier();
                                    hv.name = s;
                                    SubmitedVariableIdentifiers.Add(hv);
                                }
                            }
                            else
                            {
                                foreach (string s in l)
                                {
                                    if (!string.IsNullOrEmpty(s))
                                    {
                                        int id = 0;
                                        if (int.TryParse(s, out id))
                                        {
                                            int index = l.IndexOf(s);
                                            SubmitedVariableIdentifiers.ElementAt(index).id = id;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (this.SubmitedVariableIdentifiers != null) return this.SubmitedVariableIdentifiers;
            else return null;
        }

        /// <summary>
        /// generate a list of DefinedNameVal which includes all information
        /// about areas in a Excel template file like Data and VariableIdentifiers from the excel file
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="workbookPart">part of the workbook</param>
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

        /// <summary>
        /// Return the worksheet which include a specific area
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="workbookPart">Workbook part</param>
        /// <param name="definedName">Object with information about a area</param>
        /// <returns>Worksheet where the area is inside</returns>
        private static WorksheetPart GetWorkSheetPart(WorkbookPart workbookPart, DefinedNameVal definedName)
        {
            //get worksheet based on defined name
            string relId = workbookPart.Workbook.Descendants<Sheet>()
            .Where(s => definedName.SheetName.Equals(s.Name))
            .First()
            .Id;
            return (WorksheetPart)workbookPart.GetPartById(relId);
        }

        /// <summary>
        /// Get the cloumn name from a cell
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="cellName">Name of the cell</param>
        /// <returns>column name as string</returns>
        private static string GetColumnName(string cellName)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellName);

            return match.Value;
        }

        /// <summary>
        /// Get the row number from a cell
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="cellName">Name of the cell</param>
        /// <returns>row number as int</returns>
        private static int GetRowNumber(string cellName)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[0-9]+");
            Match match = regex.Match(cellName);

            return Convert.ToInt32(match.Value);
        }

        /// <summary>
        /// Get Column number based on a column name
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="columnName"></param>
        /// <returns>Column number as int</returns>
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

        private bool IsEmpty(Row row)
        {
            Cell c;

            for (int i = 0; i < row.ChildElements.Count(); i++)
            {
                // get current cell at i
                c = row.Elements<Cell>().ElementAt(i);
                if (c.CellValue != null)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion helper methods
    }

    /// <summary>
    /// Represent a area in a excel file
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