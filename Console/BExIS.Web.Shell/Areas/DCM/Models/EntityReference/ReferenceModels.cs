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

        public List<SimpleReferenceModel> SourceReferences { get; set; }
        public List<SimpleReferenceModel> TargetReferences { get; set; }

        public ReferencesModel()
        {
            Selected = new SimpleReferenceModel();
            SourceReferences = new List<SimpleReferenceModel>();
            TargetReferences = new List<SimpleReferenceModel>();
        }
    }

    public class SimpleReferenceModel
    {
        public long Id { get; set; }
        public long RefId { get; set; }
        public int Version { get; set; }
        public long TypeId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Context { get; set; }
        public string ReferenceType { get; set; }
        public bool LatestVersion { get; set; }

        public SimpleReferenceModel()
        {
            Id = 0;
            RefId = 0;
            Version = 0;
            TypeId = 0;
            Type = "";
            Title = "";
            Context = "";
            ReferenceType = "";
        }

        public SimpleReferenceModel(long id, int version, long typeId, string type, string title, string context, string referenceType)
        {
            Id = id;
            Version = version;
            TypeId = typeId;
            Type = type;
            Title = title;
            Context = context;
            ReferenceType = referenceType;
        }
    }

    public class CreateSimpleReferenceModel
    {
        public long SourceId { get; set; }
        public long SourceTypeId { get; set; }
        public int SourceVersion { get; set; }

        [Required]
        public long Target { get; set; }

        [Required]
        public long TargetType { get; set; }

        [Required]
        public int TargetVersion { get; set; }

        [Required]
        public String Context { get; set; }

        [Required]
        public String ReferenceType { get; set; }

        public CreateSimpleReferenceModel()
        {
        }

        public CreateSimpleReferenceModel(long sourceId, long sourceTypeId, int sourceVersion)
        {
            SourceId = sourceId;
            SourceTypeId = sourceTypeId;
            SourceVersion = sourceVersion;
        }
    }
}