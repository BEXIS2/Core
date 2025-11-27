using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Meanings;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.Meanings;
using BExIS.IO.DataType.DisplayPattern;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Math;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using Vaiona.Utils.Cfg;
using DataTable = System.Data.DataTable;

namespace BExIS.IO.Transform.Output
{
    public class DataStructureDataTable
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("desciption")]
        public string Description { get; set; }

        [JsonProperty("inUse")]
        public bool inUse { get; set; }

        [JsonProperty("variables")]
        public DataTable Variables { get; set; }

        public DataStructureDataTable()
        {
            this.Id = 0;
            this.Title = null;
            this.Description = null;
            this.inUse = false;
            this.Variables = new DataTable("variables");
        }

        public DataStructureDataTable(long id) : this()
        {

            using (MeaningManager meaningManager = new MeaningManager())
            using (DataStructureManager dataStructureManager = new DataStructureManager())
            {
                DataStructure dataStructure = dataStructureManager.AllTypesDataStructureRepo.Get(id);
                if (dataStructure != null)
                {
                    this.Id = dataStructure.Id;
                    this.Title = dataStructure.Name;
                    this.Description = string.IsNullOrEmpty(dataStructure.Description) ? "" :  dataStructure.Description;

                    if (dataStructure.Datasets.Count > 0)
                        this.inUse = true;
                    else
                        this.inUse = false;



                    if (dataStructureManager.StructuredDataStructureRepo.Get(id) != null)
                    {
                        this.Variables = new DataTable("Variables");

                        this.Variables.Columns.Add("id", typeof(Int64));
                        this.Variables.Columns.Add("label");
                        this.Variables.Columns.Add("description");
                        this.Variables.Columns.Add("isOptional", typeof(Boolean));
                        this.Variables.Columns.Add("dataType");
                        this.Variables.Columns.Add("systemType");
                        this.Variables.Columns.Add("displayPattern");
                        this.Variables.Columns.Add("unit", typeof(UnitElement));
                        this.Variables.Columns.Add("missingValues", typeof(DataTable));
                        // template
                        this.Variables.Columns.Add("template",typeof(Template));
                        this.Variables.Columns.Add("meanings", typeof(DataTable));
                        this.Variables.Columns.Add("constraints", typeof(DataTable));

                        StructuredDataStructure structuredDataStructure = dataStructureManager.StructuredDataStructureRepo.Get(id);
         
                        DataRow dataRow;
                        foreach (VariableInstance vs in structuredDataStructure.Variables)
                        {
                            dataRow = this.Variables.NewRow();
                            dataRow["id"] = vs.Id;
                            dataRow["label"] = vs.Label;
                            dataRow["description"] = vs.Description;
                            dataRow["isOptional"] = vs.IsValueOptional;
                            dataRow["dataType"] = vs.DataType.Name;

                            DataTypeDisplayPattern dtdp = DataTypeDisplayPattern.Get(vs.DisplayPatternId);
                            string displayPattern = "";
                            if (dtdp != null) displayPattern = dtdp.StringPattern;
                            dataRow["displayPattern"] = displayPattern;

                            dataRow["systemType"] = vs.DataType.SystemType;
                            dataRow["unit"] = new UnitElement(vs.Unit.Id);

                            // template
                            Template dtTepmplate = new Template();
                            dtTepmplate.Id = vs.VariableTemplate?.Id ?? 0;
                            dtTepmplate.Name = vs.VariableTemplate?.Label;
                            dtTepmplate.Description = vs.VariableTemplate?.Description;

                            dataRow["template"] = dtTepmplate;

                            // displayPattern
                            DataTable dtMissingValues = new DataTable("missingValues");
                            dtMissingValues.Columns.Add("placeholder", typeof(String));
                            dtMissingValues.Columns.Add("displayName", typeof(String));

                            foreach (var missingValue in vs.MissingValues)
                            {
                                DataRow workRow = dtMissingValues.NewRow();
                                workRow["placeholder"] = missingValue.Placeholder;
                                workRow["displayName"] = missingValue.DisplayName;
                                dtMissingValues.Rows.Add(workRow);
                            }
                            dataRow["missingValues"] = dtMissingValues;

                            // meanings
                            DataTable dtMeanings = new DataTable("meanings");
                            dtMeanings.Columns.Add("id", typeof(Int64));
                            dtMeanings.Columns.Add("name", typeof(String));
                            dtMeanings.Columns.Add("description", typeof(String));
                            dtMeanings.Columns.Add("externallinks", typeof(DataTable));

                            foreach (var meaning in vs.Meanings)
                            {
                                DataRow workRowMeaning = dtMeanings.NewRow();
                                workRowMeaning["id"] = meaning.Id;
                                workRowMeaning["name"] = meaning.Name;
                                workRowMeaning["description"] = string.IsNullOrEmpty(meaning.Description) ? "" : meaning.Description;

                                DataTable dtExternalLinks = new DataTable("externalLinks");
                                dtExternalLinks.Columns.Add("label", typeof(String));
                                dtExternalLinks.Columns.Add("releation", typeof(String));
                                dtExternalLinks.Columns.Add("link", typeof(String));

                                foreach (var el in meaning.ExternalLinks)
                                {
                                    foreach (var l in el.MappedLinks)
                                    {
                                        string label = l.Name;
                                        if (l.Prefix != null) label = l.Prefix.Name + ":" + l.Name;
                                        DataRow workRow = dtExternalLinks.NewRow();
                                        workRow["label"] = label;
                                        workRow["releation"] = el.MappingRelation?.Name;
                                        workRow["link"] = meaningManager.GetfullUri(l);
                                        dtExternalLinks.Rows.Add(workRow);
                                    }
                                }

                                workRowMeaning["externallinks"] = dtExternalLinks;

                                dtMeanings.Rows.Add(workRowMeaning);
                            }

                            dataRow["Meanings"] = dtMeanings;

                            // constrains
                            DataTable dtConstraints = new DataTable("constrants");
                            dtConstraints.Columns.Add("id", typeof(Int64));
                            dtConstraints.Columns.Add("name", typeof(String));
                            dtConstraints.Columns.Add("type", typeof(String));
                            dtConstraints.Columns.Add("description", typeof(String));

                            foreach (var constraint in vs.VariableConstraints)
                            {
                                DataRow workRow = dtConstraints.NewRow();
                                workRow["id"] = constraint.Id;
                                workRow["name"] = constraint.Name;
                                workRow["type"] = getConstraintType(constraint);
                                workRow["description"] = constraint.FormalDescription;
                                dtConstraints.Rows.Add(workRow);
                            }

                            dataRow["Constraints"] = dtConstraints;

                            this.Variables.Rows.Add(dataRow);
                        }
                    }
                }

            }
        }

        private string getConstraintType(Dlm.Entities.DataStructure.Constraint c)
        {
            if (c is DomainConstraint) return "Domain";
            if (c is RangeConstraint) return "Range";
            if (c is PatternConstraint) return "Pattern";

            return string.Empty;
        }

    }

    public class Template
    {
        [JsonProperty("id")]
        public long Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("description")]

        public string Description { get; set; }
    }

    public class VariableElement
    {
        public long Id { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public bool isOptional { get; set; }
        public UnitElement unit { get; set; }
        public DataTypeElement dataType { get; set; }

        public List<MissingValueElement> missingValues { get; set; }
        public List<MeaningElement> meanings { get; set; }

        public VariableElement(Variable variable)
        {
            Id = variable.Id;
            Label = variable.Label;
            Description = variable.Description;
            isOptional = variable.IsValueOptional;
            unit = new UnitElement(variable.Unit.Id);
            dataType = new DataTypeElement(variable.DataType.Id);
            missingValues = new List<MissingValueElement>();
            if (variable.MissingValues.Any())
                variable.MissingValues.ToList().ForEach(m => missingValues.Add(new MissingValueElement(m.DisplayName, m.Description, m.Placeholder)));

            meanings = new List<MeaningElement>();
            if (variable.Meanings.Any())
                variable.Meanings.ToList().ForEach(m => meanings.Add(new MeaningElement(m.Name, m.Description)));
        }
    }

    public class UnitElement
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("abbrevation")]

        public string Abbrevation { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("dimension")]
        public DimensionElement Dimension { get; set; }
        
        [JsonProperty("measurementSystem")]
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
                Abbrevation = unit.Abbreviation;
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
        [JsonProperty("name")]

        public string Name { get; set; }
        [JsonProperty("description")]

        public string Description { get; set; }
        [JsonProperty("specification")]

        public string Specification { get; set; }

        public DimensionElement(string name, string description, string specification)
        {
            Name = name;
            Description = description;
            Specification = specification;
        }
    }

    public class MissingValueElement
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Placeholder { get; set; }

        public MissingValueElement(string displayname, string description, string placeholder)
        {
            DisplayName = displayname;
            Description = description;
            Placeholder = placeholder;
        }
    }

    public class MeaningElement
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public MeaningElement(string name, string description)
        {
            Description = description;
            Name = name;
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

        public DataStructureDataList(long id) : this()
        {
            DataStructureManager dataStructureManager = null;
            try
            {
                dataStructureManager = new DataStructureManager();
                DataStructure dataStructure = dataStructureManager.AllTypesDataStructureRepo.Get(id);
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

                    if (dataStructureManager.StructuredDataStructureRepo.Get(id) != null)
                    {
                        StructuredDataStructure structuredDataStructure = dataStructureManager.StructuredDataStructureRepo.Get(id);
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

    public class OutputDataStructureManager
    {
        /// <summary>
        /// generate a text file with JSON from a datastructure
        /// and stored this file on the server
        /// and store the path in the content discriptor
        /// </summary>
        /// <param name="id"></param>
        /// <returns>dynamic filepath</returns>

        public static string GetDataStructureAsJson(long id)
        {
            return JsonConvert.SerializeObject(new DataStructureDataTable(id));
        }


        public static DataStructureDataList GetVariableList(long id)
        {
            return new DataStructureDataList(id);
        }

        public static string GenerateDataStructureAsText(long datastructureId)
        {
            StringBuilder stringBuilder = new StringBuilder();

            using (var dataStructureManager = new DataStructureManager())
            {
                StructuredDataStructure dataStructure = new StructuredDataStructure();
                dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(datastructureId);

                if (dataStructure != null)
                {
                    stringBuilder.AppendLine(String.Join(",", dataStructure.Variables.Select(v => v.Label)));
                    stringBuilder.AppendLine(String.Join(",", dataStructure.Variables.Select(v => v.Unit?.Name)));
                    stringBuilder.AppendLine(String.Join(",", dataStructure.Variables.Select(v => v.Description)));
                    stringBuilder.AppendLine(String.Join(",", dataStructure.Variables.Select(v => v.DataType?.Name)));
                    stringBuilder.AppendLine(String.Join(",", dataStructure.Variables.Select(v => v.IsValueOptional? "optional" : "mandatory")));
                    stringBuilder.AppendLine(String.Join(",", dataStructure.Variables.Select(v => v.IsKey?"primary key":"")));
                }
            }

            return stringBuilder.ToString();
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
}