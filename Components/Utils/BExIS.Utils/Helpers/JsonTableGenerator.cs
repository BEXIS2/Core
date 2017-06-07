using System;
using System.Collections.Generic;
using System.IO;
using BExIS.Dlm.Entities.Data;
using BExIS.IO.Transform.Validation.DSValidation;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Dlm.Entities.DataStructure;
using OfficeOpenXml;
using System.Web.UI.WebControls;
using System.Web.Script.Serialization;
using BExIS.Utils.Models;

namespace BExIS.Utils.Helpers
{

    public class JsonTableGenerator
    {
        private FileStream fileStream;

        public void Open(FileStream fileStream)
        {
            this.fileStream = fileStream;
        }

        public string GenerateJsonTable(SheetFormat sheetFormat)
        {
            ExcelPackage ep = new ExcelPackage(this.fileStream);
            ExcelWorkbook excelWorkbook = ep.Workbook;
            ExcelWorksheet firstWorksheet = excelWorkbook.Worksheets[1];
            ExcelCellAddress StartCell = firstWorksheet.Dimension.Start;
            ExcelCellAddress EndCell = firstWorksheet.Dimension.End;
            string[][] arr = new string[EndCell.Row][];
            for (int Row = StartCell.Row; Row <= EndCell.Row; Row++)
            {
                TableRow tRow = new TableRow();

                string[] currentRow = new string[EndCell.Column];
                for (int Column = StartCell.Column; Column <= EndCell.Column; Column++)
                {
                    ExcelRange cell = firstWorksheet.Cells[Row, Column];
                    //richTextBox1.Text += "Cell: " + cell.Address + ",";
                    TableCell tCell = new TableCell();
                    //tCell.Text = cell.Address + ": Value [" + cell.Value + "]<br>" + "BGColorRGB [" + colorRgb + "]<br>Formula [" + cell.Formula + "] ";
                    if (cell.Value != null)
                    {
                        tCell.Text = cell.Value.ToString();
                        currentRow[Column - 1] = cell.Value.ToString();
                    }
                    else
                    {
                        tCell.Text = "";
                        currentRow[Column - 1] = "";
                    }
                    tRow.Cells.Add(tCell);
                }
                arr[Row - 1] = currentRow;
            }
            var serializer = new JavaScriptSerializer();
            return serializer.Serialize(arr);
        }

    }
}