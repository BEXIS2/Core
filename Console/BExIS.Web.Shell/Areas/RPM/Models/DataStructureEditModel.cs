using System;
using System.Linq;
using System.Collections.Generic;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;

namespace BExIS.Web.Shell.Areas.RPM.Models
{
    public class AttributePreviewStruct
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public string DataType { get; set; }

        public AttributePreviewStruct()
        {
            this.Id = 0;
            this.Name = "";
            this.Description = "";
            this.Unit = "";
            this.DataType = "";
        }
    }
}
    