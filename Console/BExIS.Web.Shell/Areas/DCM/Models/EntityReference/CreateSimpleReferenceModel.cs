using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Models.EntityReference
{
    public class CreateSimpleReferenceModel
    {
        public long SourceId { get; set; }
        public long SourceTypeId { get; set; }

        [Required]
        public long Target { get; set; }

        [Required]
        public long TargetType { get; set; }

        [Required]
        public String Context { get; set; }

        public CreateSimpleReferenceModel()
        {
        }

        public CreateSimpleReferenceModel(long sourceId, long sourceTypeId)
        {
            SourceId = sourceId;
            SourceTypeId = sourceTypeId;
        }
    }
}