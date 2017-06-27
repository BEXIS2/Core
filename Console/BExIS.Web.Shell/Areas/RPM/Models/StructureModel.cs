using System;
using System.Linq;
using System.Collections.Generic;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using System.Data;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;

namespace BExIS.Modules.Rpm.UI.Models
{
    public class Structure
    {

        public long Id { get; set; }
        public long DataStructureId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool inUse { get; set; }
        public bool Structured { get; set; }
        public DataTable Variables { get; set; }

        public Structure()
        {
            DataStructureResultStruct dataStructureResultStruct = new DataStructureResultStruct();
            this.Id = 0;
            this.Title = dataStructureResultStruct.Title;
            this.Description = dataStructureResultStruct.Description;
            this.inUse = dataStructureResultStruct.inUse;
            this.Structured = dataStructureResultStruct.Structured;
            this.Variables = new DataTable("Variables");

            this.Variables.Columns.Add("Id");
            this.Variables.Columns.Add("Label");
            this.Variables.Columns.Add("Description");
            this.Variables.Columns.Add("isOptional");
            this.Variables.Columns.Add("Unit");
            this.Variables.Columns.Add("DataType");
            this.Variables.Columns.Add("SystemType");
        }

        public Structure(long datasetId) : this()
        {
            // Model objects MUST have no access to the services, or database!!
            DatasetManager datasetManager = new DatasetManager();
            // the dataset is retreived just for testing the NULL
            if (datasetManager.DatasetRepo.Get(datasetId) != null)
            {
                // The dataset is retrieved for the 2nd time to get its structure id!!
                this.DataStructureId = datasetManager.DatasetRepo.Get(datasetId).DataStructure.Id;
                DataStructureResultStruct dataStructureResultStruct = new DataStructureResultStruct(DataStructureId);
                // The dataset is retrieved to get its Id!!! the id was available in datasetId as well as in previous calls
                this.Id = datasetManager.DatasetRepo.Get(datasetId).Id;
                this.Title = dataStructureResultStruct.Title;
                this.Description = dataStructureResultStruct.Description;
                this.inUse = dataStructureResultStruct.inUse;
                this.Structured = dataStructureResultStruct.Structured;
                this.Variables = new DataTable("Variables");

                this.Variables.Columns.Add("Id");
                this.Variables.Columns.Add("Label");
                this.Variables.Columns.Add("Description");
                this.Variables.Columns.Add("isOptional");
                this.Variables.Columns.Add("Unit");
                this.Variables.Columns.Add("DataType");
                this.Variables.Columns.Add("SystemType");

                if (this.Structured == true)
                {
                    StructuredDataStructurePreviewModel structuredDataStructurePreviewModel = new StructuredDataStructurePreviewModel(DataStructureId);
                    DataRow dataRow;
                    foreach (VariablePreview vs in structuredDataStructurePreviewModel.VariablePreviews)
                    {
                        dataRow = this.Variables.NewRow();
                        dataRow["Id"] = vs.Id;
                        dataRow["Label"] = vs.Label;
                        dataRow["Description"] = vs.Description;
                        dataRow["isOptional"] = vs.isOptional;
                        dataRow["Unit"] = vs.Unit;
                        dataRow["DataType"] = vs.DataType;
                        dataRow["SystemType"] = vs.SystemType;
                        this.Variables.Rows.Add(dataRow);
                    }
                }
            }
        }       
    }
}
    