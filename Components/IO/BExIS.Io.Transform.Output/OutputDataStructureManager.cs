using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.TypeSystem;
using BExIS.IO.DataType.DisplayPattern;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;

namespace BExIS.IO.Transform.Output
{
    public class DataStructureDataTable
    {

        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool inUse { get; set; }
        public bool Structured { get; set; }
        public DataTable Variables { get; set; }

        public DataStructureDataTable()
        {
            this.Id = 0;
            this.Title = null;
            this.Description = null;
            this.inUse = false;
            this.Structured = false;
            this.Variables = new DataTable("Variables");

            this.Variables.Columns.Add("Id");
            this.Variables.Columns.Add("Label");
            this.Variables.Columns.Add("Description");
            this.Variables.Columns.Add("isOptional");
            this.Variables.Columns.Add("Unit");
            this.Variables.Columns.Add("DataType");
            this.Variables.Columns.Add("SystemType");
        }

        public DataStructureDataTable(long datasetId) : this()
        {
            var dataset = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>().Get(datasetId);
            if (dataset != null && dataset.DataStructure.Id != 0)
            {
                DataStructureManager dataStructureManager = null;
                try
                {
                    dataStructureManager = new DataStructureManager();
                    DataStructure dataStructure = dataStructureManager.AllTypesDataStructureRepo.Get(dataset.DataStructure.Id);
                    if (dataStructure != null)
                    {
                        this.Id = dataStructure.Id;
                        this.Title = dataStructure.Name;
                        this.Description = dataStructure.Description;

                        if (dataStructure.Datasets.Count > 0)
                            this.inUse = true;
                        else
                            this.inUse = false;

                        this.Structured = false;
                        this.Variables = new DataTable("Variables");

                        this.Variables.Columns.Add("Id");
                        this.Variables.Columns.Add("Label");
                        this.Variables.Columns.Add("Description");
                        this.Variables.Columns.Add("isOptional");
                        this.Variables.Columns.Add("Unit");
                        this.Variables.Columns.Add("DataType");
                        this.Variables.Columns.Add("SystemType");

                        if (dataStructureManager.StructuredDataStructureRepo.Get(dataset.DataStructure.Id) != null)
                        {
                            StructuredDataStructure structuredDataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataset.DataStructure.Id);
                            this.Structured = true;
                            DataRow dataRow;
                            foreach (Variable vs in structuredDataStructure.Variables)
                            {
                                dataRow = this.Variables.NewRow();
                                dataRow["Id"] = vs.Id;
                                dataRow["Label"] = vs.Label;
                                dataRow["Description"] = vs.Description;
                                dataRow["isOptional"] = vs.IsValueOptional;
                                dataRow["Unit"] = vs.Unit.Name;
                                dataRow["DataType"] = vs.DataAttribute.DataType.Name;
                                dataRow["SystemType"] = vs.DataAttribute.DataType.SystemType;
                                this.Variables.Rows.Add(dataRow);
                            }
                        }
                    }
                }
                finally
                {
                    dataStructureManager.Dispose();
                }
            }
        }
    }

    public class VariableElement
    {
        public long Id { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public bool isOptional { get; set; }
        public UnitElement unit { get; set; }
        public DataTypeElement dataType { get; set; }

        public VariableElement(Variable variable)
        {

            Id = variable.Id;
            Label = variable.Label;
            Description = variable.Description;
            isOptional = variable.IsValueOptional;
            unit = new UnitElement(variable.Unit.Id);
            dataType = new DataTypeElement(variable.DataAttribute.DataType.Id);
        }

    }

    public class UnitElement
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DimensionElement Dimension { get; set; }
        public string MeasurementSystem { get; set; }

        public UnitElement(long unitId)
        {
            UnitManager um = null;
            try
            {
                um = new UnitManager();
                Unit unit = um.Repo.Get(unitId);
                var dim = um.DimensionRepo.Get(unit.Dimension.Id);

                Id = unit.Id;
                Name = unit.Name;
                Description = unit.Description;
                Dimension = new DimensionElement(dim.Name, dim.Description, dim.Specification);
                MeasurementSystem = unit.MeasurementSystem.ToString();
            }
            finally
            {
                um.Dispose();
            }
        }

    }

    public class DimensionElement
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Specification { get; set; }

        public DimensionElement(string name, string description, string specification)
        {
            Name = name;
            Description = description;
            Specification = specification;
        }

    }

    public class DataTypeElement
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SystemType { get; set; }

        public DataTypeElement(long dataTypeId)
        {
            DataTypeManager dtm = null;
            try
            {
                dtm = new DataTypeManager();
                Dlm.Entities.DataStructure.DataType dataType = dtm.Repo.Get(dataTypeId);
                Id = dataType.Id;
                Name = dataType.Name;
                Description = dataType.Description;
                SystemType = dataType.SystemType;
            }
            finally
            {
                dtm.Dispose();
            }

        }
    }

    public class DataStructureDataList
    {

        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool inUse { get; set; }
        public bool Structured { get; set; }
        public List<VariableElement> Variables { get; set; }

        public DataStructureDataList()
        {
            this.Id = 0;
            this.Title = null;
            this.Description = null;
            this.inUse = false;
            this.Structured = false;
            this.Variables = new List<VariableElement>();
        }

        public DataStructureDataList(long datasetId) : this()
        {
            var dataset = this.GetUnitOfWork().GetReadOnlyRepository<Dataset>().Get(datasetId);
            if (dataset != null && dataset.DataStructure.Id != 0)
            {
                DataStructureManager dataStructureManager = null;
                try
                {
                    dataStructureManager = new DataStructureManager();
                    DataStructure dataStructure = dataStructureManager.AllTypesDataStructureRepo.Get(dataset.DataStructure.Id);
                    if (dataStructure != null)
                    {
                        this.Id = dataStructure.Id;
                        this.Title = dataStructure.Name;
                        this.Description = dataStructure.Description;

                        if (dataStructure.Datasets.Count > 0)
                            this.inUse = true;
                        else
                            this.inUse = false;

                        this.Structured = false;
                        this.Variables = new List<VariableElement>();

                        if (dataStructureManager.StructuredDataStructureRepo.Get(dataset.DataStructure.Id) != null)
                        {
                            StructuredDataStructure structuredDataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataset.DataStructure.Id);
                            this.Structured = true;
                            foreach (Variable vs in structuredDataStructure.Variables)
                            {
                                vs.Materialize();
                                this.Variables.Add(new VariableElement(vs));
                            }
                        }
                    }
                }
                finally
                {
                    dataStructureManager.Dispose();
                }
            }
        }
    }



    public class OutputDataStructureManager
    {
        /// <summary>
        /// generate a text file with JSON from a datastructure of a dataset
        /// and stored this file on the server
        /// and store the path in the content discriptor
        /// </summary>
        /// <param name="datasetId"></param>
        /// <returns>dynamic filepath</returns>

        public static string GetDataStructureAsJson(long datasetId)
        {
            return JsonConvert.SerializeObject(new DataStructureDataTable(datasetId));
        }

        public static string GetVariableListAsJson(long datasetId)
        {
            return JsonConvert.SerializeObject(new DataStructureDataList(datasetId), Newtonsoft.Json.Formatting.Indented);
        }

        public static DataStructureDataList GetVariableList(long datasetId)
        {
            return new DataStructureDataList(datasetId);
        }

        public static string GenerateDataStructure(long datasetId)
        {
            string path = "";
            try
            {
                DatasetManager datasetManager = null;
                try
                {
                    datasetManager = new DatasetManager();
                    DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);

                    DataStructureDataTable dataStructureDataTable = new DataStructureDataTable(datasetId);

                    // store in content descriptor
                    path = storeGeneratedFilePathToContentDiscriptor(datasetId, datasetVersion, "datastructure", ".txt");
                }
                finally
                {
                    datasetManager.Dispose();
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }

            return path;
        }


        private static string storeGeneratedFilePathToContentDiscriptor(long datasetId, DatasetVersion datasetVersion, string title, string ext)
        {

            string name = "";
            string mimeType = "";

            if (ext.Contains("csv"))
            {
                name = "datastructure";
                mimeType = "text/comma-separated-values";
            }


            // create the generated FileStream and determine its location
            string dynamicPath = OutputDatasetManager.GetDynamicDatasetStorePath(datasetId, datasetVersion.Id, title, ext);
            //Register the generated data FileStream as a resource of the current dataset version
            //ContentDescriptor generatedDescriptor = new ContentDescriptor()
            //{
            //    OrderNo = 1,
            //    Name = name,
            //    MimeType = mimeType,
            //    URI = dynamicPath,
            //    DatasetVersion = datasetVersion,
            //};

            DatasetManager dm = null;
            try
            {
                dm = new DatasetManager();
                if (datasetVersion.ContentDescriptors.Count(p => p.Name.Equals(name)) > 0)
                {   // remove the one contentdesciptor 
                    foreach (ContentDescriptor cd in datasetVersion.ContentDescriptors)
                    {
                        if (cd.Name == name)
                        {
                            cd.URI = dynamicPath;
                            dm.UpdateContentDescriptor(cd);
                        }
                    }
                }
                else
                {
                    // add current contentdesciptor to list
                    //datasetVersion.ContentDescriptors.Add(generatedDescriptor);
                    dm.CreateContentDescriptor(name, mimeType, dynamicPath, 1, datasetVersion);
                }

                //dm.EditDatasetVersion(datasetVersion, null, null, null);
                return dynamicPath;
            }
            finally
            {
                dm.Dispose();
            }
        }

    }

    public struct StyleIndexStruct
    {
        public string Name { get; set; }
        public uint Index { get; set; }
        public DataTypeDisplayPattern DisplayPattern { get; set; }
    }
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class ExcelTemplateProvider
    {
        string _fileName = null;

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

        public List<Variable> getOrderedVariables(StructuredDataStructure structuredDataStructure)
        {
            return getOrderedVariables(structuredDataStructure.Variables.ToList());
        }

        public List<Variable> getOrderedVariables(List<Variable> Variables)
        {
            return Variables.OrderBy(v => v.OrderNo).ToList();
        }

        public string CreateTemplate(StructuredDataStructure dataStructure)
        {
            DataStructureManager dataStructureManager = null;
            try
            {
                dataStructureManager = new DataStructureManager();
                //List<Variable> variables = getOrderedVariables(dataStructure);

                string rgxPattern = "[<>?\":|\\\\/*]";
                string rgxReplace = "-";
                Regex rgx = new Regex(rgxPattern);

                string filename = filename = dataStructure.Id + "_" + rgx.Replace(dataStructure.Name, rgxReplace) + ".xlsm";

                string path = Path.Combine("DataStructures", dataStructure.Id.ToString());

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
            //dataStructureFile = SpreadsheetDocument.Open(Path.Combine(AppConfiguration.GetModuleWorkspacePath("RPM"), "Template", filename), true);


            foreach (OpenXmlPart part in template.GetPartsOfType<OpenXmlPart>())
            {
                OpenXmlPart newPart = dataStructureFile.AddPart<OpenXmlPart>(part);
            }

            template.Close();

            //uint iExcelIndex = 164;
            List<StyleIndexStruct> styleIndex = new List<StyleIndexStruct>();
            ExcelHelper.UpdateStylesheet(dataStructureFile.WorkbookPart.WorkbookStylesPart.Stylesheet,out styleIndex);

            Worksheet worksheet = dataStructureFile.WorkbookPart.WorksheetParts.First().Worksheet;
            List<Row> rows = GetRows(worksheet, 1, 11);

            List<Variable> variables = this.GetUnitOfWork().GetReadOnlyRepository<Variable>()
                                                            .Query(p => variableIds.Contains(p.Id))
                                                            .OrderBy(p => p.OrderNo)
                                                            .ToList();
            foreach (Variable var in variables)
            {
                DataContainerManager CM = null;
                try
                {
                    CM = new DataContainerManager();
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
                        StyleIndex = ExcelHelper.GetExcelStyleIndex(dataAttribute.DataType, styleIndex),
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
                finally
                {
                    CM.Dispose();
                }
            }

            foreach (DefinedName name in dataStructureFile.WorkbookPart.Workbook.GetFirstChild<DefinedNames>())
            {
                if (name.Name == "Data" || name.Name == "VariableIdentifiers")
                {
                    string[] tempArr = name.InnerText.Split('$');
                    string temp = "";
                    tempArr[tempArr.Count() - 2] = GetClomunIndex(variables.Count());
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
            //return "";
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

    }
}
