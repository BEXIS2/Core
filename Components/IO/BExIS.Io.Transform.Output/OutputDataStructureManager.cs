using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO.DataType.DisplayPattern;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

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
        }

        public DataStructureDataTable(long id) : this()
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

                    if (dataStructureManager.StructuredDataStructureRepo.Get(id) != null)
                    {
                        this.Variables = new DataTable("Variables");

                        this.Variables.Columns.Add("Id", typeof(Int64));
                        this.Variables.Columns.Add("Label");
                        this.Variables.Columns.Add("Description");
                        this.Variables.Columns.Add("isOptional", typeof(Boolean));
                        this.Variables.Columns.Add("Unit");
                        this.Variables.Columns.Add("DataType");
                        this.Variables.Columns.Add("SystemType");
                        this.Variables.Columns.Add("AttributeName");
                        this.Variables.Columns.Add("AttributeDescription");
                        this.Variables.Columns.Add("DisplayPattern");
                        this.Variables.Columns.Add("MissingValues", typeof(DataTable));

                        StructuredDataStructure structuredDataStructure = dataStructureManager.StructuredDataStructureRepo.Get(id);
                        this.Structured = true;
                        DataRow dataRow;
                        foreach (VariableInstance vs in structuredDataStructure.Variables)
                        {
                            dataRow = this.Variables.NewRow();
                            dataRow["Id"] = vs.Id;
                            dataRow["Label"] = vs.Label;
                            dataRow["Description"] = vs.Description;
                            dataRow["isOptional"] = vs.IsValueOptional;
                            dataRow["Unit"] = vs.Unit.Name;
                            dataRow["DataType"] = vs.DataType.Name;
                            dataRow["SystemType"] = vs.DataType.SystemType;
                            dataRow["AttributeName"] = vs.VariableTemplate?.Label;
                            dataRow["AttributeDescription"] = vs.VariableTemplate?.Description;

                            DataTypeDisplayPattern dtdp = DataTypeDisplayPattern.Get(vs.DisplayPatternId);
                            string displayPattern = "";
                            if (dtdp != null) displayPattern = dtdp.StringPattern;

                            dataRow["DisplayPattern"] = displayPattern;

                            DataTable dtMissingValues = new DataTable("MissingValues");
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

        //public static string GetVariableListAsJson(long id)
        //{
        //    return JsonConvert.SerializeObject(new DataStructureDataList(id), Newtonsoft.Json.Formatting.Indented);
        //}

        public static DataStructureDataList GetVariableList(long id)
        {
            return new DataStructureDataList(id);
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