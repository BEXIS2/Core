using BExIS.IO.DataType.DisplayPattern;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace BExIS.Modules.Dcm.UI.Helpers
{
    public class UploadUIHelper
    {
        private static List<char> Letters = new List<char>() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', ' ' };
        private FileStream fileStream;
        private SharedStringItem[] _sharedStrings;
        private Stylesheet _stylesheet;
        private int maxCellCount = -1;
        private List<List<String>> table = new List<List<string>>();

        public UploadUIHelper(FileStream fileStream)
        {
            this.fileStream = fileStream;
        }

        public string GenerateJsonTable(String worksheetUri)
        {
            // open excel file
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(this.fileStream, false))
            {
                // get workbookpart
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                _sharedStrings = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ToArray();
                _stylesheet = workbookPart.WorkbookStylesPart.Stylesheet;

                WorksheetPart worksheetPart = null;
                foreach (Sheet worksheet in workbookPart.Workbook.Descendants<Sheet>())
                {
                    //Get the current worksheetpart and see if it is the correct one
                    WorksheetPart tmp = (WorksheetPart)workbookPart.GetPartById(worksheet.Id);
                    if (tmp.Uri.ToString() == worksheetUri)
                    {
                        //Found the correct WorksheetPart
                        worksheetPart = tmp;
                    }
                }

                using (OpenXmlReader reader = OpenXmlReader.Create(worksheetPart))
                {
                    int expectedRowIndex = 1;
                    while (reader.Read())
                    {
                        if (reader.ElementType == typeof(DocumentFormat.OpenXml.Spreadsheet.Row))
                        {
                            do
                            {
                                DocumentFormat.OpenXml.Spreadsheet.Row row = (DocumentFormat.OpenXml.Spreadsheet.Row)reader.LoadCurrentElement();

                                List<String> rowAsStringList = new List<string>();

                                //Since this library will ignore empty rows, check if we skipped some and add empty rows if necessary
                                //This will still ignore empty rows at the end of the file but those wouldn't have any influence on the indices of data & header anyway
                                while (row.RowIndex > expectedRowIndex)
                                {
                                    List<String> dummyRow = new List<string>();
                                    dummyRow.Add("");
                                    table.Add(dummyRow);
                                    expectedRowIndex++;
                                }

                                // create a new cell
                                Cell c = new Cell();

                                int expectedIndex = 0; //To check whether we skipped cells because they were empty
                                for (int i = 0; i < row.ChildElements.Count(); i++)
                                {
                                    // get current cell at i
                                    c = row.Elements<Cell>().ElementAt(i);

                                    string value = "";

                                    if (c != null)
                                    {
                                        //See if cells have been skipped (empty cells are not contained in the xml and therefore not contained in row.ChildElements)
                                        //See: https://stackoverflow.com/a/3981249

                                        // Gets the column index of the cell with data
                                        int cellColumnIndex = (int)GetColumnIndexFromName(GetColumnName(c.CellReference));
                                        if (expectedIndex < cellColumnIndex)
                                        {
                                            //We skipped one or more cells so add some blank data
                                            do
                                            {
                                                rowAsStringList.Add(""); //Insert blank data
                                                expectedIndex++;
                                            }
                                            while (expectedIndex < cellColumnIndex);
                                        }

                                        //We now have the correct index and can grab the value of the cell
                                        if (c.CellValue != null && !string.IsNullOrEmpty(c.CellValue.Text))
                                        {
                                            // if Value a text
                                            if (c.DataType != null && c.DataType.HasValue && c.DataType.Value == CellValues.SharedString)
                                            {
                                                int sharedStringIndex = int.Parse(c.CellValue.Text, CultureInfo.InvariantCulture);
                                                SharedStringItem sharedStringItem = _sharedStrings[sharedStringIndex];
                                                value = sharedStringItem.InnerText;
                                            }
                                            //If cell contains boolean (doesn't always work for files saved with libre office)
                                            else if (c.DataType != null && c.DataType.HasValue && c.DataType.Value == CellValues.Boolean)
                                            {
                                                if (c.InnerText == "1")
                                                {
                                                    value = "true";
                                                }
                                                else
                                                {
                                                    value = "false";
                                                }
                                            }
                                            // not a text
                                            else if (c.StyleIndex != null && c.StyleIndex.HasValue)
                                            {
                                                uint styleIndex = c.StyleIndex.Value;
                                                CellFormat cellFormat = _stylesheet.CellFormats.ChildElements[(int)styleIndex] as CellFormat;
                                                if (cellFormat != null && cellFormat.NumberFormatId != null && cellFormat.NumberFormatId.HasValue)
                                                {
                                                    uint numberFormatId = cellFormat.NumberFormatId.Value;

                                                    if (_stylesheet.NumberingFormats != null)
                                                    {
                                                        NumberingFormat numberFormat = _stylesheet.NumberingFormats.FirstOrDefault(numFormat => ((NumberingFormat)numFormat).NumberFormatId.Value == numberFormatId) as NumberingFormat;

                                                        //
                                                        if (numberFormat != null)
                                                        {
                                                            if (numberFormat != null && numberFormat.FormatCode != null && numberFormat.FormatCode.HasValue)
                                                            {
                                                                string formatCode = numberFormat.FormatCode.Value;
                                                                if ((formatCode.ToLower().Contains("d") && formatCode.ToLower().Contains("m")) ||
                                                                    (formatCode.ToLower().Contains("m") && formatCode.ToLower().Contains("y")) ||
                                                                    (formatCode.ToLower().Contains("m") && formatCode.ToLower().Contains("d")) ||
                                                                    (formatCode.ToLower().Contains("h") && formatCode.ToLower().Contains("m")) ||
                                                                    (formatCode.ToLower().Contains("m") && formatCode.ToLower().Contains("s"))
                                                                    )
                                                                {
                                                                    DateTime dateTime = DateTime.FromOADate(double.Parse(c.CellValue.Text, CultureInfo.InvariantCulture));
                                                                    //value = dateTime.ToString(new CultureInfo("en-us"));
                                                                    //get c# display pattern

                                                                    DataTypeDisplayPattern dataTypeDisplayPattern = DataTypeDisplayPattern.GetByExcelPattern(formatCode);
                                                                    value = dataTypeDisplayPattern != null ? dateTime.ToString(dataTypeDisplayPattern.StringPattern) : dateTime.ToString(new CultureInfo("en-us"));

                                                                    //Debug.WriteLine("----");
                                                                    //Debug.WriteLine(formatCode);
                                                                }
                                                            }
                                                        }
                                                    }// check numberformat not null
                                                }// check cell format

                                                //It may happen that values are in a cell, but the associated information such as numberformat or style are missing.
                                                // In this case, we decide to display the values, even if they are incorrect.
                                                if (string.IsNullOrEmpty(value) && (!string.IsNullOrEmpty(c?.CellValue?.Text))) value = c.CellValue.Text;
                                            }
                                            else { value = c.CellValue.Text; }

                                            rowAsStringList.Add(value);
                                        }//end if cell value null
                                        else
                                        {
                                            rowAsStringList.Add("");
                                        }
                                    }//end if cell null

                                    expectedIndex++;
                                }//for children of row

                                //Check if there's a new max length for the length of a row
                                maxCellCount = Math.Max(maxCellCount, rowAsStringList.Count);

                                //Just read a row, so increase the expected index for the next one
                                expectedRowIndex++;

                                table.Add(rowAsStringList);
                            } while (reader.ReadNextSibling()); // Skip to the next row

                            break;
                        }
                    }

                    //Make sure each row has the same number of values in it
                    foreach (List<String> row in table)
                    {
                        while (row.Count < maxCellCount)
                        {
                            row.Add("");
                        }
                    }

                    //Convert the Lists to Arrays
                    List<String>[] rowArray = table.ToArray(); //The elements of the Array are the rows in form of String-lists
                    String[][] tableArray = new String[rowArray.Length][];
                    for (int i = 0; i < rowArray.Length; i++)
                    {
                        tableArray[i] = rowArray[i].ToArray();
                    }

                    return JsonConvert.SerializeObject(tableArray);
                }
            }
        }

        //Solution from https://stackoverflow.com/a/3981249
        /// <summary>
        /// Given a cell name, parses the specified cell to get the column name.
        /// </summary>
        /// <param name="cellReference">Address of the cell (ie. B2)</param>
        /// <returns>Column Name (ie. B)</returns>
        public static string GetColumnName(string cellReference)
        {
            // Create a regular expression to match the column name portion of the cell name.
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellReference);

            return match.Value;
        }

        //Solution from https://stackoverflow.com/a/3981249
        /// <summary>
        /// Given just the column name (no row index), it will return the zero based column index.
        /// Note: This method will only handle columns with a length of up to two (ie. A to Z and AA to ZZ).
        /// A length of three can be implemented when needed.
        /// </summary>
        /// <param name="columnName">Column Name (ie. A or AB)</param>
        /// <returns>Zero based index if the conversion was successful; otherwise null</returns>
        public static int? GetColumnIndexFromName(string columnName)
        {
            int? columnIndex = null;

            string[] colLetters = Regex.Split(columnName, "([A-Z]+)");
            colLetters = colLetters.Where(s => !string.IsNullOrEmpty(s)).ToArray();

            if (colLetters.Count() <= 2)
            {
                int index = 0;
                foreach (string col in colLetters)
                {
                    List<char> col1 = colLetters.ElementAt(index).ToCharArray().ToList();
                    int? indexValue = Letters.IndexOf(col1.ElementAt(index));

                    if (indexValue != -1)
                    {
                        // The first letter of a two digit column needs some extra calculations
                        if (index == 0 && colLetters.Count() == 2)
                        {
                            columnIndex = columnIndex == null ? (indexValue + 1) * 26 : columnIndex + ((indexValue + 1) * 26);
                        }
                        else
                        {
                            columnIndex = columnIndex == null ? indexValue : columnIndex + indexValue;
                        }
                    }

                    index++;
                }
            }

            return columnIndex;
        }

        /*
         * Returns a dictionary, containing the Uris of all worksheets as keys and their names (displaynames) as values
         * */

        public Dictionary<Uri, String> GetWorksheetUris()
        {
            if (this.fileStream != null)
            {
                Dictionary<Uri, String> output = new Dictionary<Uri, String>();

                // open excel file
                using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(this.fileStream, false))
                {
                    // get workbookpart
                    WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

                    //get worksheet part
                    //Save uris and names of all worksheets
                    foreach (Sheet worksheet in workbookPart.Workbook.Descendants<Sheet>())
                    {
                        WorksheetPart tmp = (WorksheetPart)workbookPart.GetPartById(worksheet.Id);
                        output.Add(tmp.Uri, worksheet.Name);
                    }
                    return output;
                }
            }
            return null;
        }

        //Returns the Uri of the worksheet that was used to create the JsonTable
        public Uri GetFirstWorksheetUri()
        {
            // open excel file
            using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(this.fileStream, false))
            {
                // get workbookpart
                WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;

                //get first uri
                string id = workbookPart.Workbook.Descendants<Sheet>().First().Id;
                return workbookPart.GetPartById(id).Uri;
            }
        }
    }
}