using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Vaiona.Util.Cfg;


namespace BExIS.RPM.Output
{
    public class ExcelTemplateProvider
    {
        string _fileName = null;

        private char[] alphabet = {' ','A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

        public ExcelTemplateProvider(string fileName)
        {
            _fileName = fileName;
        }

        public ExcelTemplateProvider()
        {
            _fileName = "BExISppTemplate_Clean.xlsm"; 
        }

        //private FileStream loadFile (string file)
        //{
        //    if (File.Exists(file))
        //        return File.Open(file, FileMode.Open, FileAccess.ReadWrite);

        //    else
        //        return null;
        //}

        public void CreateTemplate(long id)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(id);
            CreateTemplate(dataStructure);
        }

        public void CreateTemplate(StructuredDataStructure dataStructure)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            dataStructureManager.StructuredDataStructureRepo.LoadIfNot(dataStructure.Variables);
            string filename = dataStructure.Name + ".xlsm";
            string path = Path.Combine("DataStructures", dataStructure.Id.ToString());

            CreateTemplate(dataStructure.Variables.ToList(), path, filename);

            XmlDocument resources = new XmlDocument();

            resources.LoadXml("<Resources><Resource Type=\"Excel\" Edition=\"2010\" Path=\"" + Path.Combine(path, filename) + "\"></Resource></Resources>");
            dataStructure.TemplatePaths = resources;
            dataStructureManager.UpdateStructuredDataStructure(dataStructure);

        }

        public SpreadsheetDocument CreateTemplate(List<long> variableIds,long dataStructureId, string path, string filename)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);

            if (dataStructure != null)
            {
                List<Variable> variables = dataStructure.Variables.Where(p => variableIds.Contains(p.Id)).ToList();
                return CreateTemplate(variables, path, filename);
            }
            else
            {
                return null;
            }
        }

        public SpreadsheetDocument CreateTemplate(List<Variable> variables, string path, string filename)
        {            
            if (!Directory.Exists(Path.Combine(AppConfiguration.DataPath, path)))
            {
                Directory.CreateDirectory(Path.Combine(AppConfiguration.DataPath, path));
            }

            SpreadsheetDocument template = SpreadsheetDocument.Open(Path.Combine(AppConfiguration.GetModuleWorkspacePath("RPM"),"Template",_fileName),true);
            SpreadsheetDocument dataStructureFile = SpreadsheetDocument.Create(Path.Combine(AppConfiguration.DataPath, path, filename), template.DocumentType);
            //dataStructureFile = SpreadsheetDocument.Open(Path.Combine(AppConfiguration.GetModuleWorkspacePath("RPM"), "Template", filename), true);
 

            foreach (OpenXmlPart part in template.GetPartsOfType<OpenXmlPart>())
            {
                OpenXmlPart newPart = dataStructureFile.AddPart<OpenXmlPart>(part);
            }

            template.Close();

            // get worksheet
            uint[] styleIndex = new uint[4];
            CellFormats cellFormats = dataStructureFile.WorkbookPart.WorkbookStylesPart.Stylesheet.Elements<CellFormats>().First();
            //number 0,00
            CellFormat cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)2U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U, ApplyNumberFormat = true };           
            cellFormats.Append(cellFormat);
            styleIndex[0] = (uint)cellFormats.Count++;
            //number 0
            cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)1U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U, ApplyNumberFormat = true };
            cellFormats.Append(cellFormat);
            styleIndex[1] = (uint)cellFormats.Count++;
            //text
            cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)49U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U, ApplyNumberFormat = true };
            cellFormats.Append(cellFormat);
            styleIndex[2] = (uint)cellFormats.Count++;
            //date
            cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)14U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)0U, ApplyNumberFormat = true };
            cellFormats.Append(cellFormat);
            styleIndex[3] = (uint)cellFormats.Count++;

            Worksheet worksheet = dataStructureFile.WorkbookPart.WorksheetParts.First().Worksheet;



                List<Row> rows = GetRows(worksheet,1,11);

                foreach (Variable var in variables)
                {
                    DataContainerManager CM = new DataContainerManager();
                    DataAttribute dataAttribute = CM.DataAttributeRepo.Get(var.DataAttribute.Id);

                    int indexVar = variables.ToList().IndexOf(var) + 1;
                    string columnIndex = GetClomunIndex(indexVar);

                    string cellRef = columnIndex + 1;
                    Cell cell = new Cell()
                    {
                        CellReference = cellRef,
                        StyleIndex = (UInt32Value)5U,
                        DataType = CellValues.String,
                        CellValue = new CellValue(var.Label)
                    };
                    rows.ElementAt(0).AppendChild(cell);

                    cellRef = columnIndex + 2;
                    cell = new Cell()
                    {
                        CellReference = cellRef,
                        DataType = CellValues.String,
                        StyleIndex = getExcelStyleIndex(dataAttribute.DataType.SystemType, styleIndex),
                        CellValue = new CellValue("")
                    };
                    rows.ElementAt(1).AppendChild(cell);

                    cellRef = columnIndex + 3;
                    cell = new Cell()
                    {
                        CellReference = cellRef,
                        StyleIndex = (UInt32Value)5U,
                        DataType = CellValues.String,
                        CellValue = new CellValue(var.Id.ToString())
                    };
                    rows.ElementAt(2).AppendChild(cell);

                    cellRef = columnIndex + 4;
                    cell = new Cell()
                    {
                        CellReference = cellRef,
                        StyleIndex = (UInt32Value)5U,
                        DataType = CellValues.String,
                        CellValue = new CellValue(dataAttribute.ShortName)
                    };
                    rows.ElementAt(3).AppendChild(cell);

                    cellRef = columnIndex + 5;
                    cell = new Cell()
                    {
                        CellReference = cellRef,
                        StyleIndex = (UInt32Value)5U,
                        DataType = CellValues.String,
                        CellValue = new CellValue(dataAttribute.Description)
                    };
                    rows.ElementAt(4).AppendChild(cell);

                    string classification = "";

                    if (dataAttribute.Classification != null)
                    {
                        classification = dataAttribute.Classification.Name;
                    }

                    cellRef = columnIndex + 6;
                    cell = new Cell()
                    {
                        CellReference = cellRef,
                        StyleIndex = (UInt32Value)5U,
                        DataType = CellValues.String,
                        CellValue = new CellValue(classification)
                    };
                    rows.ElementAt(5).AppendChild(cell);
                    
                    string unit = "";

                    if (dataAttribute.Unit != null)
                    {
                        unit = dataAttribute.Unit.Name;
                    }

                    cellRef = columnIndex + 7;
                    cell = new Cell()
                    {
                        CellReference = cellRef,
                        StyleIndex = (UInt32Value)5U,
                        DataType = CellValues.String,
                        CellValue = new CellValue(unit)
                    };
                    rows.ElementAt(6).AppendChild(cell);

                    string dataType = "";

                    if (dataAttribute.DataType != null)
                    {
                        dataType = dataAttribute.DataType.Name;
                    }

                    cellRef = columnIndex + 8;
                    cell = new Cell()
                    {
                        CellReference = cellRef,
                        StyleIndex = (UInt32Value)5U,
                        DataType = CellValues.String,
                        CellValue = new CellValue(dataType)
                    };
                    rows.ElementAt(7).AppendChild(cell);

                    cellRef = columnIndex + 9;
                    cell = new Cell()
                    {
                        CellReference = cellRef,
                        StyleIndex = (UInt32Value)5U,
                        DataType = CellValues.String,
                        CellValue = new CellValue(var.IsValueOptional.ToString())
                    };
                    rows.ElementAt(8).AppendChild(cell);

                    cellRef = columnIndex + 10;
                    cell = new Cell()
                    {
                        CellReference = cellRef,
                        StyleIndex = (UInt32Value)5U,
                        DataType = CellValues.String,
                        CellValue = new CellValue(dataAttribute.IsMultiValue.ToString())
                    };
                    rows.ElementAt(9).AppendChild(cell);

                }

                foreach (DefinedName name in dataStructureFile.WorkbookPart.Workbook.GetFirstChild<DefinedNames>())
                {
                    if (name.Name == "Data" || name.Name == "VariableIdentifiers")
                    {
                        string[] tempArr = name.InnerText.Split('$');
                        string temp = "";
                        tempArr[tempArr.Count() - 2] = GetClomunIndex(variables.Count());
                        foreach(string t in tempArr)
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
            
            //WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
            //WorksheetPart worksheetPart = workbookPart.WorksheetParts.First();
        
            dataStructureFile.WorkbookPart.Workbook.Save();
            
            dataStructureFile.Close();

            return dataStructureFile;
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

        private List<Row> GetRows(Worksheet worksheet, int start, int end)
        {
            List<Row> temp = new List<Row>();

            for(int i=start;i<=end;i++)
            {
                temp.Add(worksheet.GetFirstChild<SheetData>().Elements<Row>().Where(r => r.RowIndex == i).First());
            }

            return temp;
        }

        private string GetClomunIndex(int index, int offset = 1)
        {

            //if (index <= 25) return alphabet[index].ToString();
            //return "";
            int residual = 0;
            string column = "";
            bool firstRun= true;
            do
            {
                if(firstRun == true)
                {
                    residual = ((index % 26)) + offset;
                    column = alphabet[residual].ToString() + column;
                    index = (index / 26);
                    firstRun = false;
                }
                else
                {
                    residual = ((index % 26)) + offset;
                    column = alphabet[residual-1].ToString() + column;
                    index = (index / 26);
                }

            } while (index > 0);
            return column;
        }

        public void deleteTemplate(long dataStrctureId)
        {
            DataStructureManager DSM = new DataStructureManager();
            StructuredDataStructure dataStructure = DSM.StructuredDataStructureRepo.Get(dataStrctureId);
            string path = "";

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

    }
}
