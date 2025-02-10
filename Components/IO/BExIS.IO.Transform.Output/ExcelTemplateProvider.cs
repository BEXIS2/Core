using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;

namespace BExIS.IO.Transform.Output
{
    public class ExcelTemplateProvider
    {
        private string _fileName = null;

        private char[] alphabet = { ' ', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="fileName"></param>
        public ExcelTemplateProvider(string fileName)
        {
            _fileName = fileName;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public ExcelTemplateProvider()
        {
            _fileName = "BExISppTemplate_Clean.xlsx";
        }

        //private FileStream loadFile (string file)
        //{
        //    if (File.Exists(file))
        //        return File.Open(file, FileMode.Open, FileAccess.ReadWrite);

        //    else
        //        return null;
        //}

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        public void CreateTemplate(long id)
        {
            DataStructureManager dataStructureManager = null;
            try
            {
                dataStructureManager = new DataStructureManager();
                StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(id);
                CreateTemplate(dataStructure);
            }
            finally
            {
                dataStructureManager.Dispose();
            }
        }

        public List<VariableInstance> getOrderedVariables(StructuredDataStructure structuredDataStructure)
        {
            return getOrderedVariables(structuredDataStructure.Variables.ToList());
        }

        public List<VariableInstance> getOrderedVariables(List<VariableInstance> Variables)
        {
            return Variables.OrderBy(v => v.OrderNo).ToList();
        }

        public string CreateTemplate(StructuredDataStructure dataStructure)
        {
            DataStructureManager dataStructureManager = null;
            try
            {
                dataStructureManager = new DataStructureManager();

                string rgxPattern = "[<>?\":|\\\\/*]";
                string rgxReplace = "-";
                Regex rgx = new Regex(rgxPattern);

                string filename = filename = dataStructure.Id + "_" + rgx.Replace(dataStructure.Name, rgxReplace) + ".xlsx";

                string path = Path.Combine("DataStructures", dataStructure.Id.ToString());

                // create excel template based on structure

                CreateTemplate(dataStructure.Variables.Select(p => p.Id).ToList(), path, filename);

                XmlDocument resources = new XmlDocument();

                resources.LoadXml("<Resources><Resource Type=\"Excel\" Edition=\"2010\" Path=\"" + Path.Combine(path, filename) + "\"></Resource></Resources>");
                dataStructure = this.GetUnitOfWork().GetReadOnlyRepository<StructuredDataStructure>().Get(dataStructure.Id); //Javad: This line should not be here, but I could not find where oes the dataStructure come from!!
                dataStructure.TemplatePaths = resources;
                dataStructureManager.UpdateStructuredDataStructure(dataStructure);

                return Path.Combine(path, filename);
            }
            finally
            {
                dataStructureManager.Dispose();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="variableIds"></param>
        /// <param name="dataStructureId"></param>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public SpreadsheetDocument CreateTemplate(List<long> variableIds, long dataStructureId, string path, string filename)
        {
            DataStructureManager dataStructureManager = null;
            try
            {
                dataStructureManager = new DataStructureManager();
                StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);

                if (dataStructure != null)
                {
                    //List<Variable> variables = dataStructure.Variables.Where(p => variableIds.Contains(p.Id)).ToList();
                    //variables = getOrderedVariables(variables);
                    return CreateTemplate(variableIds, path, filename);
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                dataStructureManager.Dispose();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="variables"></param>
        /// <param name="path"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public SpreadsheetDocument CreateTemplate(List<long> variableIds, string path, string filename)
        {
            if (!Directory.Exists(Path.Combine(AppConfiguration.DataPath, path)))
            {
                Directory.CreateDirectory(Path.Combine(AppConfiguration.DataPath, path));
            }

            SpreadsheetDocument template = SpreadsheetDocument.Open(Path.Combine(AppConfiguration.GetModuleWorkspacePath("RPM"), "Template", _fileName), true);
            SpreadsheetDocument dataStructureFile = SpreadsheetDocument.Create(Path.Combine(AppConfiguration.DataPath, path, filename), template.DocumentType);

            foreach (OpenXmlPart part in template.GetPartsOfType<OpenXmlPart>())
            {
                OpenXmlPart newPart = dataStructureFile.AddPart<OpenXmlPart>(part);
            }

            template.Close();

            List<StyleIndexStruct> styleIndex = new List<StyleIndexStruct>();
            ExcelHelper.UpdateStylesheet(dataStructureFile.WorkbookPart.WorkbookStylesPart.Stylesheet, out styleIndex);

            Worksheet worksheet = dataStructureFile.WorkbookPart.WorksheetParts.First().Worksheet;
            List<Row> rows = GetRows(worksheet, 1, 7);

            List<VariableInstance> variables = this.GetUnitOfWork().GetReadOnlyRepository<VariableInstance>()
                                                            .Query(p => variableIds.Contains(p.Id))
                                                            .OrderBy(p => p.OrderNo)
                                                            .ToList();

            var workbookPart = dataStructureFile.WorkbookPart;

            // go throw each varaible and add a cell
            foreach (VariableInstance var in variables)
            {
                DataContainerManager CM = null;
                try
                {
                    CM = new DataContainerManager();

                    int indexVar = variables.ToList().IndexOf(var);
                    string columnIndex = GetClomunIndex(indexVar);

                    string cellRef = columnIndex + 1;

                    rows.ElementAt(0).AppendChild(generateCell(cellRef, var.Label, workbookPart, "ff9700", true, false, true));

                    //unit
                    string unit = var.Unit != null && var.Unit.Name != "none" ? var.Unit.Abbreviation : "";
                    cellRef = columnIndex + 2;
                    rows.ElementAt(1).AppendChild(generateCell(cellRef, unit, workbookPart, "ffd599"));

                    //description
                    cellRef = columnIndex + 3;
                    rows.ElementAt(2).AppendChild(generateCell(cellRef, var.Description, workbookPart, "ffd599"));

                    // datatype
                    string dataType = var.DataType != null ? var.DataType.Name : "";
                    cellRef = columnIndex + 4;
                    rows.ElementAt(3).AppendChild(generateCell(cellRef, dataType, workbookPart, "ffd599"));

                    // optional
                    cellRef = columnIndex + 5;
                    rows.ElementAt(4).AppendChild(generateCell(cellRef, var.IsValueOptional ? "optional" : "mandatory", workbookPart, "ffd599"));

                    // missing values
                    var mvs = new List<string>();
                    if (var.MissingValues != null)
                    {
                        foreach (var mv in var.MissingValues)
                        {
                            mvs.Add(mv.DisplayName);
                        }
                    }
                    string mvsToString = string.Join(", ", mvs);

                    cellRef = columnIndex + 6;
                    rows.ElementAt(5).AppendChild(generateCell(cellRef, mvsToString, workbookPart, "ffd599"));

                    // is key
                    cellRef = columnIndex + 7;
                    rows.ElementAt(6).AppendChild(generateCell(cellRef, var.IsKey ? "key" : "", workbookPart, "ffd599", false, true));
                }
                finally
                {
                    CM.Dispose();
                }
            }

            dataStructureFile.WorkbookPart.Workbook.Save();

            dataStructureFile.Close();

            return dataStructureFile;
        }

        private Cell generateCell(string cellRef, string value, WorkbookPart workbookPart, string rgb, bool first = false, bool last = false, bool bold = false)
        {
            Cell cell = new Cell()
            {
                CellReference = cellRef,
                //StyleIndex = (UInt32Value)1U,
                DataType = CellValues.String,
                CellValue = new CellValue(value)
            };

            // sett cellformat
            CellFormat cellFormat = cell.StyleIndex != null ? GetCellFormat(workbookPart, cell.StyleIndex).CloneNode(true) as CellFormat : new CellFormat();
            cellFormat.FillId = InsertFill(workbookPart, GenerateFill(rgb));
            if (first || last) cellFormat.BorderId = InsertBorder(workbookPart, GenerateBottomBorder());
            if (bold) cellFormat.FontId = InsertFont(workbookPart, GenerateFont());
            cell.StyleIndex = InsertCellFormat(workbookPart, cellFormat);

            return cell;
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
        /// <param name="worksheet"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private List<Row> GetRows(Worksheet worksheet, int start, int end)
        {
            List<Row> temp = new List<Row>();

            for (int i = start; i <= end; i++)
            {
                temp.Add(worksheet.GetFirstChild<SheetData>().Elements<Row>().Where(r => r.RowIndex == i).First());
            }

            return temp;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="index"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        private string GetClomunIndex(int index, int offset = 1)
        {
            //if (index <= 25) return alphabet[index].ToString();
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
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataStrctureId"></param>
        public void deleteTemplate(long dataStrctureId)
        {
            DataStructureManager DSM = null;
            try
            {
                DSM = new DataStructureManager();
                StructuredDataStructure dataStructure = DSM.StructuredDataStructureRepo.Get(dataStrctureId);
                string path = "";
                try
                {
                    XmlNode resources = dataStructure.TemplatePaths.FirstChild;

                    XmlNodeList resource = resources.ChildNodes;

                    foreach (XmlNode x in resource)
                    {
                        path = Path.Combine(AppConfiguration.DataPath, x.Attributes.GetNamedItem("Path").Value);
                        if (File.Exists(path))
                            File.Delete(path);
                    }

                    path = Path.Combine(AppConfiguration.DataPath, "DataStructures", dataStructure.Id.ToString());

                    if (Directory.Exists(path) && !(Directory.EnumerateFileSystemEntries(path).Any()))
                        Directory.Delete(path);
                }
                catch
                {
                }
            }
            finally
            {
                DSM.Dispose();
            }
        }

        #region styles

        public Border GenerateBottomBorder()
        {
            Border border2 = new Border();

            BottomBorder bottomBorder2 = new BottomBorder() { Style = BorderStyleValues.Thin };
            Color color1 = new Color() { Indexed = (UInt32Value)64U };

            bottomBorder2.Append(color1);
            border2.Append(bottomBorder2);

            return border2;
        }

        public Font GenerateFont()
        {
            Font font = new Font(new Bold());

            return font;
        }

        public Fill GenerateFill(string rgb)
        {
            Fill fill = new Fill();

            PatternFill patternFill = new PatternFill() { PatternType = PatternValues.Solid };
            ForegroundColor foregroundColor1 = new ForegroundColor() { Rgb = rgb };
            BackgroundColor backgroundColor1 = new BackgroundColor() { Rgb = rgb };

            patternFill.Append(foregroundColor1);
            patternFill.Append(backgroundColor1);

            fill.Append(patternFill);

            return fill;
        }

        public uint InsertBorder(WorkbookPart workbookPart, Border border)
        {
            Borders borders = workbookPart.WorkbookStylesPart.Stylesheet.Elements<Borders>().First();
            borders.Append(border);
            return (uint)borders.Count++;
        }

        public uint InsertFill(WorkbookPart workbookPart, Fill fill)
        {
            Fills fills = workbookPart.WorkbookStylesPart.Stylesheet.Elements<Fills>().First();
            fills.Append(fill);
            return (uint)fills.Count++;
        }

        public Cell GetCell(WorksheetPart workSheetPart, string cellAddress)
        {
            return workSheetPart.Worksheet.Descendants<Cell>()
                                        .SingleOrDefault(c => cellAddress.Equals(c.CellReference));
        }

        public CellFormat GetCellFormat(WorkbookPart workbookPart, uint styleIndex)
        {
            return workbookPart.WorkbookStylesPart.Stylesheet.Elements<CellFormats>().First().Elements<CellFormat>().ElementAt((int)styleIndex);
        }

        public uint InsertCellFormat(WorkbookPart workbookPart, CellFormat cellFormat)
        {
            CellFormats cellFormats = workbookPart.WorkbookStylesPart.Stylesheet.Elements<CellFormats>().First();
            cellFormats.Append(cellFormat);
            return (uint)cellFormats.Count++;
        }

        public uint InsertFont(WorkbookPart workbookPart, Font font)
        {
            var fonts = workbookPart.WorkbookStylesPart.Stylesheet.Elements<Fonts>().First();
            fonts.Append(font);
            return (uint)fonts.Count++;
        }

        #endregion styles
    }
}