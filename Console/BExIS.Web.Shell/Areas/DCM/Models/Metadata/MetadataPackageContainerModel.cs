using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dcm.Wizard;
using BExIS.Io.Transform.Validation.Exceptions;
using BExIS.Dlm.Entities.MetadataStructure;

namespace BExIS.Web.Shell.Areas.DCM.Models.Metadata
{
    public class MetadataPackageContainerModel
    {

        public String DisplayName { get; set; }
        public String Discription { get; set; }
        public int Number { get; set; }
        public int MinCardinality { get; set; }
        public int MaxCardinality { get; set; }

        public MetadataPackageUsage Source { get; set; }
        public List<MetadataPackageModel> MetadataPackageModel { get; set; }

        public StepInfo StepInfo { get; set; }
        public List<Error> ErrorList { get; set; }

        public MetadataPackageContainerModel()
        {
            ErrorList = new List<Error>();
        }

        public static MetadataPackageContainerModel Convert(MetadataPackageUsage metadataPackageUsage)
        {
            return new MetadataPackageContainerModel
            {
                Source = metadataPackageUsage,
                Number = 1,
                MetadataPackageModel = new List<MetadataPackageModel>(),
                DisplayName = metadataPackageUsage.Label,
                Discription = metadataPackageUsage.MetadataPackage.Description,
                MinCardinality = metadataPackageUsage.MinCardinality,
                MaxCardinality = metadataPackageUsage.MaxCardinality
            };
        }
    }
}