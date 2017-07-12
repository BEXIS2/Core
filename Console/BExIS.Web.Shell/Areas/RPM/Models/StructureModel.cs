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
            DatasetManager datasetManager = new DatasetManager();
            var dataset = datasetManager.DatasetRepo.Get(datasetId);
            if (dataset != null && dataset.DataStructure.Id != 0)
            {
                DataStructureResultStruct dataStructureResultStruct = new DataStructureResultStruct(this.Id);
                this.Id = dataStructureResultStruct.Id;
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
                    StructuredDataStructurePreviewModel structuredDataStructurePreviewModel = new StructuredDataStructurePreviewModel(this.Id);
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
    