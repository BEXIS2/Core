using System;
using System.Linq;
using System.Collections.Generic;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using System.Data;

namespace BExIS.Web.Shell.Areas.RPM.Models
{
    public class dataStructure
    {

        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool inUse { get; set; }
        public bool Structured { get; set; }
        public DataTable Variables { get; set; }

        public dataStructure()
        {
            DataStructureResultStruct dataStructureResultStruct = new DataStructureResultStruct();
            this.Id = dataStructureResultStruct.Id;
            this.Title = dataStructureResultStruct.Title;
            this.Description = dataStructureResultStruct.Description;
            this.inUse = dataStructureResultStruct.inUse;
            this.Structured = dataStructureResultStruct.Structured;
            this.Variables = new DataTable("Variables");
        }

        public dataStructure(long dataStructureId)
        {
            DataStructureResultStruct dataStructureResultStruct = new DataStructureResultStruct(dataStructureId);
            this.Id = dataStructureResultStruct.Id;
            this.Title = dataStructureResultStruct.Title;
            this.Description = dataStructureResultStruct.Description;
            this.inUse = dataStructureResultStruct.inUse;
            this.Structured = dataStructureResultStruct.Structured;
            this.Variables = new DataTable("Variables");

            this.Variables.Columns.Add("Id");
            this.Variables.Columns.Add("Lable");
            this.Variables.Columns.Add("Description");
            this.Variables.Columns.Add("isOptional");
            this.Variables.Columns.Add("Unit");
            this.Variables.Columns.Add("DataType");

            if (this.Structured == true)
            {
                StructuredDataStructurePreviewModel structuredDataStructurePreviewModel = new StructuredDataStructurePreviewModel(dataStructureId);
                DataRow dataRow;
                foreach (VariablePreviewStruct vs in structuredDataStructurePreviewModel.VariablePreviews)
                {
                    dataRow = this.Variables.NewRow();
                    dataRow["Id"] = vs.Id;
                    dataRow["Lable"] = vs.Lable;
                    dataRow["Description"] = vs.Description;
                    dataRow["isOptional"] = vs.isOptional;
                    dataRow["Unit"] = vs.Unit;
                    dataRow["DataType"] = vs.DataType;
                    this.Variables.Rows.Add(dataRow);
                }
            }
        }
    }
}
    