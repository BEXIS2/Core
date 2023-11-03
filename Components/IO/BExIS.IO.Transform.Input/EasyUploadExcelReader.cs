using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.IO.Transform.Validation.DSValidation;
using BExIS.IO.Transform.Validation.Exceptions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BExIS.IO.Transform.Input
{
    public class EasyUploadExcelReader : ExcelReader
    {
        public EasyUploadExcelReader(StructuredDataStructure structuredDatastructure, EasyUploadFileReaderInfo fileReaderInfo) : base(structuredDatastructure, fileReaderInfo)
        {
        }

        public DataTuple[] ReadFile(Stream file, string fileName, EasyUploadFileReaderInfo fri, long datasetId, String worksheetUri)
        {
            this.FileStream = file;
            this.FileName = fileName;
            this.Info = fri;
            this.DatasetId = datasetId;

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

                //SheetDimension dimension = workbookPart.WorksheetParts.First().Worksheet.GetFirstChild<SheetDimension>();

                // get all the defined area
                //List<DefinedNameVal> namesTable = BuildDefinedNamesTable(workbookPart);

                /*
                 * Markus: Fixed it for column names greater than Z - leaving the old code commented out in case something goes wrong
                 * */
                //this._areaOfVariables.StartColumn = alphabet[fri.VariablesStartColumn - 1].ToString();
                this._areaOfVariables.StartColumn = columnToLetter(fri.VariablesStartColumn);
                //this._areaOfVariables.EndColumn = alphabet[fri.VariablesEndColumn - 1].ToString();
                this._areaOfVariables.EndColumn = columnToLetter(fri.VariablesEndColumn);
                this._areaOfVariables.StartRow = fri.VariablesStartRow;
                this._areaOfVariables.EndRow = fri.VariablesEndRow;

                //this._areaOfData.StartColumn = alphabet[fri.DataStartColumn - 1].ToString();
                this._areaOfData.StartColumn = columnToLetter(fri.DataStartColumn);
                //this._areaOfData.EndColumn = alphabet[fri.DataEndColumn - 1].ToString();
                this._areaOfData.EndColumn = columnToLetter(fri.DataEndColumn);
                this._areaOfData.StartRow = fri.DataStartRow;
                this._areaOfData.EndRow = fri.DataEndRow;

                // Get intergers for reading data
                startColumn = fri.VariablesStartColumn;
                endColumn = fri.VariablesEndColumn;

                numOfColumns = (endColumn - startColumn) + 1;
                offset = this.Info.Offset;

                int endRowData = fri.DataEndRow;

                // select worksheetpart by Uri
                WorksheetPart worksheetPart = workbookPart.WorksheetParts.Where(ws => ws.Uri.ToString() == worksheetUri).FirstOrDefault();

                // get styleSheet
                _stylesheet = workbookPart.WorkbookStylesPart.Stylesheet;

                // Get shared strings
                _sharedStrings = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ToArray();

                if (ValidateDatastructure(worksheetPart, this._areaOfVariables.StartRow, this._areaOfVariables.EndRow))
                {
                    ReadRows(worksheetPart, fri.DataStartRow, endRowData);
                }

                return this.DataTuples.ToArray();
            }

            return this.DataTuples.ToArray();
        }

        public void setSubmittedVariableIdentifiers(List<VariableIdentifier> vi)
        {
            this.SubmitedVariableIdentifiers = vi;
        }

        private string columnToLetter(int column)
        {
            int temp = 0;
            string letter = "";
            while (column > 0)
            {
                temp = (column - 1) % 26;
                letter = alphabet[temp] + letter;
                column = (column - temp - 1) / 26;
            }
            return letter;
        }
    }
}