using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dcm.Wizard;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Entities.Common;

namespace BExIS.Web.Shell.Areas.DCM.Models.Metadata
{
    public class MetadataPackageModel:AbstractMetadataStepModel
    {

        public MetadataPackageModel()
        {
            ErrorList = new List<Error>();

        }

        public static MetadataPackageModel Convert(BaseUsage mPUsage, int number)
        {
            MetadataPackageUsage metadataPackageUsage = (MetadataPackageUsage)mPUsage;
            if (metadataPackageUsage != null)
            {
                return new MetadataPackageModel
                {
                    Source = metadataPackageUsage,
                    Number = number,
                    MetadataAttributeModels = new List<MetadataAttributeModel>(),
                    DisplayName = metadataPackageUsage.Label,
                    Discription = metadataPackageUsage.Description,
                    MinCardinality = metadataPackageUsage.MinCardinality,
                    MaxCardinality = metadataPackageUsage.MaxCardinality
                };
            }
            else
                return null;
        }

        public void ConvertMetadataAttributeModels(ICollection<MetadataAttributeUsage> metadataAttributeUsages)
        {
            metadataAttributeUsages.ToList().ForEach(a => MetadataAttributeModels.Add(MetadataAttributeModel.Convert(a, Source, ((MetadataPackageUsage)Source).MetadataStructure.Id, Number)));
        }

    }
}