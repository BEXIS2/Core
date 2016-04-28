using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Vaiona.Utils.Cfg;
using System.Text.RegularExpressions;
using System;
using BExIS.IO.DataType.DisplayPattern;
using BExIS.Dlm.Services.TypeSystem;

/// <summary>
///
/// </summary>        
namespace BExIS.RPM.Output
{
    struct StyleIndexStruct
    {
        public string Name { get; set; }
        public uint Index { get; set; }
        public DataTypeDisplayPattern DisplayPattern { get; set;}
    }
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class ExcelTemplateProvider
    {
        string _fileName = null;

        private char[] alphabet = {' ','A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };

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
            _fileName = "BExISppTemplate_Clean.xlsm"; 
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
            DataStructureManager dataStructureManager = new DataStructureManager();
            StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(id);
            CreateTemplate(dataStructure);
        }

        public List<Variable> getOrderedVariables(StructuredDataStructure structuredDataStructure)
        {
            return getOrderedVariables(structuredDataStructure.Id, structuredDataStructure.Variables.ToList());
        }

        public List<Variable> getOrderedVariables(long dataStructureID, List<Variable> Variables)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            StructuredDataStructure structuredDataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureID);
            XmlDocument doc = (XmlDocument)structuredDataStructure.Extra;
            XmlNode order;
            XmlNodeList temp;
            if (doc == null)
            {
                doc = new XmlDocument();
                XmlNode root = doc.CreateNode(System.Xml.XmlNodeType.Element, "extra", null);
                doc.AppendChild(root);
            }
            if (doc.GetElementsByTagName("order").Count == 0)
                {

                if (structuredDataStructure.Variables.Count != 0)
                    {
                    order = doc.CreateNode(System.Xml.XmlNodeType.Element, "order", null);
                    foreach (Variable v in structuredDataStructure.Variables)
                    {

                        XmlNode variable = doc.CreateNode(System.Xml.XmlNodeType.Element, "variable", null);
                        variable.InnerText = v.Id.ToString();
                        order.AppendChild(variable);
                    }
                    doc.FirstChild.AppendChild(order);
                    structuredDataStructure.Extra = doc;
                    dataStructureManager.UpdateStructuredDataStructure(structuredDataStructure);
                }
            }

            temp = doc.GetElementsByTagName("order");
            order = temp[0];
            List<Variable> orderedVariables = new List<Variable>();
            if (Variables.Count != 0)
            {
                foreach (XmlNode x in order)
                {
                    foreach (Variable v in Variables)
                    {
                        if (v.Id == Convert.ToInt64(x.InnerText))
                            orderedVariables.Add(v);

                    }
                }
            }
            return orderedVariables;
            }
            
        public void CreateTemplate(StructuredDataStructure dataStructure)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            List<Variable> variables = getOrderedVariables(dataStructure);
            
            string rgxPattern = "[<>?\":|\\\\/*]";
            string rgxReplace = "-";
            Regex rgx = new Regex(rgxPattern);

            string filename = filename = dataStructure.Id + "_" + rgx.Replace(dataStructure.Name, rgxReplace) +".xlsm";
            
            string path = Path.Combine("DataStructures", dataStructure.Id.ToString());

            CreateTemplate(variables, path, filename);

            XmlDocument resources = new XmlDocument();

            resources.LoadXml("<Resources><Resource Type=\"Excel\" Edition=\"2010\" Path=\"" + Path.Combine(path, filename) + "\"></Resource></Resources>");
            dataStructure.TemplatePaths = resources;
            dataStructureManager.UpdateStructuredDataStructure(dataStructure);

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
        public SpreadsheetDocument CreateTemplate(List<long> variableIds,long dataStructureId, string path, string filename)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);

            if (dataStructure != null)
            {
                List<Variable> variables = dataStructure.Variables.Where(p => variableIds.Contains(p.Id)).ToList();
                variables = getOrderedVariables(dataStructureId, variables);
                return CreateTemplate(variables, path, filename);
            }
            else
            {
                return null;
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
            List<StyleIndexStruct> styleIndex = new List<StyleIndexStruct>();
            CellFormats cellFormats = dataStructureFile.WorkbookPart.WorkbookStylesPart.Stylesheet.Elements<CellFormats>().First();
            //number 0,00
            CellFormat cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)2U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)1U, ApplyNumberFormat = true};
            cellFormat.Protection = new Protection();
            cellFormat.Protection.Locked = false;
            cellFormats.Append(cellFormat);
            styleIndex.Add(new StyleIndexStruct() { Name = "Decimal", Index = (uint)cellFormats.Count++, DisplayPattern = null });
            //number 0
            cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)1U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)1U, ApplyNumberFormat = true};
            cellFormat.Protection = new Protection();
            cellFormat.Protection.Locked = false;
            cellFormats.Append(cellFormat);
            styleIndex.Add(new StyleIndexStruct() { Name = "Number", Index = (uint)cellFormats.Count++, DisplayPattern = null });
            //text
            cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)49U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)1U, ApplyNumberFormat = true};
            cellFormat.Protection = new Protection();
            cellFormat.Protection.Locked = false;
            cellFormats.Append(cellFormat);
            styleIndex.Add(new StyleIndexStruct() { Name = "Text", Index = (uint)cellFormats.Count++, DisplayPattern = null });
            //DateTime
            cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)22U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)1U, ApplyNumberFormat = true};
            cellFormat.Protection = new Protection();
            cellFormat.Protection.Locked = false;
            cellFormats.Append(cellFormat);
            styleIndex.Add(new StyleIndexStruct() { Name = "DateTime", Index = (uint)cellFormats.Count++, DisplayPattern = DataTypeDisplayPattern.Pattern.Where(p => p.Systemtype.Equals(DataTypeCode.DateTime) && p.Name.Equals("DateTime")).FirstOrDefault() });
            //Date
            cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)14U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)1U, ApplyNumberFormat = true };
            cellFormat.Protection = new Protection();
            cellFormat.Protection.Locked = false;
            cellFormats.Append(cellFormat);
            styleIndex.Add(new StyleIndexStruct() { Name = "Date", Index = (uint)cellFormats.Count++, DisplayPattern = DataTypeDisplayPattern.Pattern.Where(p => p.Systemtype.Equals(DataTypeCode.DateTime) && p.Name.Equals("Date")).FirstOrDefault() });
            //Time
            cellFormat = new CellFormat() { NumberFormatId = (UInt32Value)21U, FontId = (UInt32Value)0U, FillId = (UInt32Value)0U, BorderId = (UInt32Value)0U, FormatId = (UInt32Value)1U, ApplyNumberFormat = true };
            cellFormat.Protection = new Protection();
            cellFormat.Protection.Locked = false;
            cellFormats.Append(cellFormat);
            styleIndex.Add(new StyleIndexStruct() { Name = "Time", Index = (uint)cellFormats.Count++, DisplayPattern = DataTypeDisplayPattern.Pattern.Where(p => p.Systemtype.Equals(DataTypeCode.DateTime) && p.Name.Equals("Time")).FirstOrDefault() });

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
                        StyleIndex = (UInt32Value)4U,
                        DataType = CellValues.String,
                        CellValue = new CellValue(var.Label)                        
                    };
                    rows.ElementAt(0).AppendChild(cell);

                    cellRef = columnIndex + 2;
                    cell = new Cell()
                    {                        
                        CellReference = cellRef,
                        DataType = CellValues.String,
                        StyleIndex = getExcelStyleIndex(dataAttribute.DataType, styleIndex),                        
                        CellValue = new CellValue("")
                    };
                    rows.ElementAt(1).AppendChild(cell);

                    cellRef = columnIndex + 3;
                    cell = new Cell()
                    {
                        CellReference = cellRef,
                        StyleIndex = (UInt32Value)4U,
                        DataType = CellValues.String,
                        CellValue = new CellValue(var.Id.ToString())
                    };
                    rows.ElementAt(2).AppendChild(cell);


                    
                    cellRef = columnIndex + 4;
                    cell = new Cell()
                    {
                        CellReference = cellRef,
                        StyleIndex = (UInt32Value)4U,
                        DataType = CellValues.String,
                        CellValue = new CellValue(dataAttribute.ShortName)
                    };
                    rows.ElementAt(3).AppendChild(cell);

                    // description from variable 
                    // if not then from attribute
                    string description = "";
                    description = String.IsNullOrEmpty(var.Description) ? dataAttribute.Description : var.Description;
   
                    cellRef = columnIndex + 5;
                    cell = new Cell()
                    {
                        CellReference = cellRef,
                        StyleIndex = (UInt32Value)4U,
                        DataType = CellValues.String,
                        CellValue = new CellValue(description)
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
                        StyleIndex = (UInt32Value)4U,
                        DataType = CellValues.String,
                        CellValue = new CellValue(classification)
                    };
                    rows.ElementAt(5).AppendChild(cell);
                    
                    string unit = "";

                    if (var.Unit != null)
                    {
                        unit = var.Unit.Name;
                    }

                    cellRef = columnIndex + 7;
                    cell = new Cell()
                    {
                        CellReference = cellRef,
                        StyleIndex = (UInt32Value)4U,
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
                        StyleIndex = (UInt32Value)4U,
                        DataType = CellValues.String,
                        CellValue = new CellValue(dataType)
                    };
                    rows.ElementAt(7).AppendChild(cell);

                    cellRef = columnIndex + 9;
                    cell = new Cell()
                    {
                        CellReference = cellRef,
                        StyleIndex = (UInt32Value)4U,
                        DataType = CellValues.String,
                        CellValue = new CellValue(var.IsValueOptional.ToString())
                    };
                    rows.ElementAt(8).AppendChild(cell);

                    cellRef = columnIndex + 10;
                    cell = new Cell()
                    {
                        CellReference = cellRef,
                        StyleIndex = (UInt32Value)4U,                     
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
        /// <param name="systemType"></param>
        /// <param name="styleIndex"></param>
        /// <returns></returns>
        private uint getExcelStyleIndex(DataType dataType, List<StyleIndexStruct> styleIndex)
        {
            if (dataType.SystemType == DataTypeCode.Double.ToString() || dataType.SystemType == DataTypeCode.Decimal.ToString())
                return styleIndex.Where(p => p.Name.Equals("Decimal")).FirstOrDefault().Index;
            if (dataType.SystemType == DataTypeCode.Int16.ToString() || dataType.SystemType == DataTypeCode.Int32.ToString() || dataType.SystemType == DataTypeCode.Int64.ToString() || dataType.SystemType == DataTypeCode.UInt16.ToString() || dataType.SystemType == DataTypeCode.Int32.ToString() || dataType.SystemType == DataTypeCode.Int64.ToString())
                return styleIndex.Where(p => p.Name.Equals("Number")).FirstOrDefault().Index;
            if (dataType.SystemType == DataTypeCode.String.ToString() || dataType.SystemType == DataTypeCode.Char.ToString())
                return styleIndex.Where(p => p.Name.Equals("Text")).FirstOrDefault().Index;
            if (dataType.SystemType == DataTypeCode.DateTime.ToString())
            {
                if (DataTypeDisplayPattern.Materialize(dataType.Extra).Name == "DateTime" && DataTypeDisplayPattern.Materialize(dataType.Extra).Systemtype == DataTypeCode.DateTime)
                {
                    return styleIndex.Where(p => p.Name.Equals("DateTime")).FirstOrDefault().Index;
                }
                if (DataTypeDisplayPattern.Materialize(dataType.Extra).Name == "Date" && DataTypeDisplayPattern.Materialize(dataType.Extra).Systemtype == DataTypeCode.DateTime)
                {
                    return styleIndex.Where(p => p.Name.Equals("Date")).FirstOrDefault().Index;
                }
                if (DataTypeDisplayPattern.Materialize(dataType.Extra).Name == "Time" && DataTypeDisplayPattern.Materialize(dataType.Extra).Systemtype == DataTypeCode.DateTime)
                {
                    return styleIndex.Where(p => p.Name.Equals("Time")).FirstOrDefault().Index;
                }
                return styleIndex.Where(p => p.Name.Equals("DateTime")).FirstOrDefault().Index;
            }
            if (dataType.SystemType == DataTypeCode.Boolean.ToString())
                return styleIndex.Where(p => p.Name.Equals("Text")).FirstOrDefault().Index;
            return styleIndex.Where(p => p.Name.Equals("Text")).FirstOrDefault().Index;
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

            for(int i=start;i<=end;i++)
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

        /// <summary>
        /// 
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataStrctureId"></param>
        public void deleteTemplate(long dataStrctureId)
        {
            DataStructureManager DSM = new DataStructureManager();
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

    }
}
