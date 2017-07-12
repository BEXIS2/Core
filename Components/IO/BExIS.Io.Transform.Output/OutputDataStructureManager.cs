using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Xml.Helpers.Mapping;
using Vaiona.Utils.Cfg;
using System.Data;
using Newtonsoft.Json;

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
            DatasetManager datasetManager = new DatasetManager();
            var dataset = datasetManager.DatasetRepo.Get(datasetId);
            if (dataset != null && dataset.DataStructure.Id != 0)
            {
                DataStructureManager dataStructureManager = new DataStructureManager();
                DataStructure dataStructure = dataStructureManager.AllTypesDataStructureRepo.Get(dataset.DataStructure.Id);
                if (dataStructure != null)
                { 
                    this.Id = dataStructure.Id;
                    this.Title = dataStructure.Name;
                    this.Description = dataStructure.Description;

                    if(dataStructure.Datasets.Count > 0)
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
                            dataRow["Unit"] = vs.Unit;
                            dataRow["DataType"] = vs.DataAttribute.DataType.Name;
                            dataRow["SystemType"] = vs.DataAttribute.DataType.SystemType;
                            this.Variables.Rows.Add(dataRow);
                        }
                    }
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
        public static string GenerateDataStructure(long datasetId)
        {
            string path = "";
            try
            {
                DatasetManager datasetManager = new DatasetManager();
                DatasetVersion datasetVersion = datasetManager.GetDatasetLatestVersion(datasetId);

                DataStructureDataTable dataStructureDataTable = new DataStructureDataTable(datasetId);

                // store in content descriptor
                path = storeGeneratedFilePathToContentDiscriptor(datasetId, datasetVersion, "datastructure", ".txt");

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

            DatasetManager dm = new DatasetManager();
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

    }
}
