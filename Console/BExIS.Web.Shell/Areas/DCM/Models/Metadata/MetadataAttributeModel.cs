using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Dlm.Entities.MetadataStructure;

namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class MetadataAttributeModel
    {
        public long Id { get; set; }
        public long Number { get; set; }
        public long PackageModelNumber { get; set; }
        public long MetadataStructureId { get; set; }
        public MetadataAttributeUsage Source { get; set; }
        public MetadataPackageUsage Parent { get; set; }
        public String DisplayName { get; set; }
        public String Discription { get; set; }
        public int MinCardinality { get; set; }
        public int MaxCardinality { get; set; }
        public String DataType { get; set; }
        public String SystemType { get; set; }
        public int NumberOfSourceInPackage { get; set; }

        public bool last = false;
        public bool first = false;

        public object Value { get; set; }

        public static MetadataAttributeModel Convert(MetadataAttributeUsage metadataAttributeUsage , MetadataPackageUsage metadataPackageUsage, long metadataStructureId, int packageModelNumber)
        {
            return new MetadataAttributeModel
            {
               
                Id = metadataAttributeUsage.Id,
                Number = 1,
                PackageModelNumber = packageModelNumber,
                MetadataStructureId = metadataStructureId,
                Parent = metadataPackageUsage,
                Source = metadataAttributeUsage,
                DisplayName = metadataAttributeUsage.Label,
                Discription = metadataAttributeUsage.Description,
                DataType = metadataAttributeUsage.MetadataAttribute.DataType.Name,
                SystemType = metadataAttributeUsage.MetadataAttribute.DataType.SystemType,
                MinCardinality = metadataAttributeUsage.MinCardinality,
                MaxCardinality = metadataAttributeUsage.MaxCardinality,
                NumberOfSourceInPackage = 1,
                first = true,
                last = true
            };
        }

    }
}