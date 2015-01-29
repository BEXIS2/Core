using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.DCM.Models.Metadata
{
    public class MetadataInstanceModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
        public long UsageId { get; set; }
    }
}