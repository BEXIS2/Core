﻿using System;
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
        public bool HasEditRights { get; set; }
        public SimpleSourceReferenceModel Selected { get; set; }

        public List<ReferenceModel> SourceReferences { get; set; }
        public List<ReferenceModel> TargetReferences { get; set; }

        public ReferencesModel()
        {
            Selected = new SimpleSourceReferenceModel();
            SourceReferences = new List<ReferenceModel>();
            TargetReferences = new List<ReferenceModel>();
            HasEditRights = false;
        }
    }

    public class SimpleSourceReferenceModel
    {
        public long Id { get; set; }
        public int Version { get; set; }
        public string Title { get; set; }

        public long TypeId { get; set; }
        public string Type { get; set; }
    }

    public class ReferenceModel
    {
        public long RefId { get; set; }

        public ReferenceElementModel Target { get; set; }
        public ReferenceElementModel Source { get; set; }

        public string Context { get; set; }
        public string ReferenceType { get; set; }

        public ReferenceModel()
        {
            RefId = 0;
            Target = new ReferenceElementModel();
            Source = new ReferenceElementModel();
            Context = "";
            ReferenceType = "";
        }
    }

    public class ReferenceElementModel
    {
        public long Id { get; set; }
        public int Version { get; set; }
        public long TypeId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public bool LatestVersion { get; set; }

        public ReferenceElementModel()
        {
            Id = 0;
            Version = 0;
            TypeId = 0;
            Title = "";
            Type = "";
        }

        public ReferenceElementModel(long id, int version, long typeId, string title, string type, bool latestVersionc)
        {
            Id = id;
            Version = version;
            TypeId = typeId;
            Title = title;
            Type = type;
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

        public String Context { get; set; }

        [Required]
        public String ReferenceType { get; set; }

        public CreateSimpleReferenceModel()
        {
            Context = "";
        }

        public CreateSimpleReferenceModel(long sourceId, long sourceTypeId, int sourceVersion)
        {
            SourceId = sourceId;
            SourceTypeId = sourceTypeId;
            SourceVersion = sourceVersion;
            Context = "";
        }
    }
}