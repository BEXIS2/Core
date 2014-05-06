using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dcm.Wizard;
using BExIS.Io.Transform.Validation.Exceptions;
using BExIS.Dlm.Entities.MetadataStructure;

namespace BExIS.Web.Shell.Areas.DCM.Models.Metadata
{
    public class MetadataPackageModel
    {
        public String DisplayName { get; set; }
        public String Discription { get; set; }
        public int Number { get; set; }
        public int MinCardinality { get; set; }
        public int MaxCardinality { get; set; }
        public List<MetadataAttributeModel> MetadataAttributeModels { get; set; }

        public MetadataPackageUsage Source { get; set; }

        //public StepInfo StepInfo { get; set; }
        public List<Error> ErrorList { get; set; }

        public MetadataPackageModel()
        {
            //ErrorList = new List<Error>();
        }

        public static MetadataPackageModel Convert(MetadataPackageUsage metadataPackageUsage)
        {
            return new MetadataPackageModel
            {
                Source = metadataPackageUsage,
                Number = 1,
                MetadataAttributeModels = new List<MetadataAttributeModel>(),
                DisplayName = metadataPackageUsage.Label,
                Discription = metadataPackageUsage.MetadataPackage.Description,
                MinCardinality = metadataPackageUsage.MinCardinality,
                MaxCardinality = metadataPackageUsage.MaxCardinality
            };
        }

        public void ConvertMetadataAttributeModels(ICollection<MetadataAttributeUsage> metadataAttributeUsages)
        { 
            metadataAttributeUsages.ToList().ForEach(a=> MetadataAttributeModels.Add(MetadataAttributeModel.Convert(a, Source, Source.MetadataStructure.Id, Number)));
        }

    }
}