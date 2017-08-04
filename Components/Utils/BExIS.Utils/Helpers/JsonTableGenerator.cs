using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using OfficeOpenXml;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.IO.Transform.Validation.DSValidation;
using BExIS.IO.Transform.Validation.Exceptions;
using DocumentFormat.OpenXml.Spreadsheet;
using BExIS.Utils.Models;

namespace BExIS.Utils.Helpers
{

    public class JsonTableGenerator
    {
        private FileStream fileStream;
        private SharedStringItem[] _sharedStrings;
        private Stylesheet _stylesheet;
        private int maxCellCount = -1;
        private List<List<String>> table = new List<List<string>>();
        private Uri worksheetUri;

        public void Open(FileStream fileStream)
        {
            this.fileStream = fileStream;
        }

        public string GenerateJsonTable(SheetFormat sheetFormat)
        {
            // open excel file
            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(this.fileStream, false);

            // get workbookpart
            WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
            _sharedStrings = workbookPart.SharedStringTablePart.SharedStringTable.Elements<SharedStringItem>().ToArray();
            _stylesheet = workbookPart.WorkbookStylesPart.Stylesheet;

            //get worksheet part
            string sheetId = workbookPart.Workbook.Descendants<Sheet>().First().Id;
            WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheetId);
            this.worksheetUri = worksheetPart.Uri;

            OpenXmlReader reader = OpenXmlReader.Create(worksheetPart);

            while (reader.Read())
            {
                if (reader.ElementType == typeof(DocumentFormat.OpenXml.Spreadsheet.Row))
                {
                    do
                    {

                        DocumentFormat.OpenXml.Spreadsheet.Row row = (DocumentFormat.OpenXml.Spreadsheet.Row)reader.LoadCurrentElement();

                        List<String> rowAsStringList = new List<string>();

                        // create a new cell
                        Cell c = new Cell();

                        for (int i = 0; i < row.ChildElements.Count(); i++)
                        {
                            // get current cell at i
                            c = row.Elements<Cell>().ElementAt(i);

                            string value = "";

                            if (c != null)
                            {
                                if (c.CellValue != null)
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
                                    else
                                    {
                                        value = c.CellValue.Text;
                                    }

                                    rowAsStringList.Add(value);

                                }//end if cell value
                                else
                                {
                                    rowAsStringList.Add("");
                                }
                            }//end if cell null

                        }//for

                        maxCellCount = Math.Max(maxCellCount, rowAsStringList.Count);
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

            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(tableArray);
        }

        //Returns the Uri of the worksheet that was used to create the JsonTable
        public Uri getWorksheetUri()
        {
            return this.worksheetUri;
        }

    }
}