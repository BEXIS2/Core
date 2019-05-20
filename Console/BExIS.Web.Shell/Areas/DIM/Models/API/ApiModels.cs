using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dim.UI.Models.Api
{
    public class ApiDatasetModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public long DataStructureId { get; set; }
        public long MetadataStructureId { get; set; }
    }
}