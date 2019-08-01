using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Models.EntityReference
{
    public class ReferencesModel
    {
        public SimpleReferenceModel Selected { get; set; }
        public DataTable MetadataReferences { get; set; }

        public DataTable SystemReferences { get; set; }

        public ReferencesModel()
        {
            Selected = new SimpleReferenceModel();
            SystemReferences = new DataTable();
            MetadataReferences = new DataTable();
        }
    }

    public class SimpleReferenceModel
    {
        public long Id { get; set; }
        public int Version { get; set; }
        public long TypeId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }

        public SimpleReferenceModel()
        {
            Id = 0;
            Version = 0;
            TypeId = 0;
            Type = "";
            Title = "";
        }

        public SimpleReferenceModel(long id, int version, long typeId, string type, string title)
        {
            Id = id;
            Version = version;
            TypeId = typeId;
            Type = type;
            Title = title;
        }
    }

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