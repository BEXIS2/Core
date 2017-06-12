using System.Collections.Generic;
using BExIS.Dcm.CreateDatasetWizard;
using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.MetadataStructure;

namespace BExIS.Modules.Dcm.UI.Models.Metadata
{
    public class MetadataCompoundAttributeModel:AbstractMetadataStepModel
    {

        public int NumberOfSourceInPackage { get; set; }

        public bool last = false;
        public bool first = false;

        public MetadataCompoundAttributeModel()
        {
            MetadataAttributeModels = new List<MetadataAttributeModel>();
        }

        public static MetadataCompoundAttributeModel ConvertToModel(BaseUsage metadataAttributeUsage, int number)
        {
            
            return new MetadataCompoundAttributeModel
            {

                Id = metadataAttributeUsage.Id,
                Number = number,
                //PackageModelNumber = packageModelNumber,
                //MetadataStructureId = metadataStructureId,
                //Parent = metadataPackageUsage,
                Source = metadataAttributeUsage,
                DisplayName = metadataAttributeUsage.Label,
                Discription = metadataAttributeUsage.Description,
                //DataType = metadataAttributeUsage..MetadataAttribute.DataType.Name,
                //SystemType = metadataAttributeUsage.MetadataAttribute.DataType.SystemType,
                MinCardinality = metadataAttributeUsage.MinCardinality,
                MaxCardinality = metadataAttributeUsage.MaxCardinality,
                NumberOfSourceInPackage = 1,
                first = true,
                ////DomainList = domainConstraintList,
                last = true
            };
        }

        public void ConvertMetadataAttributeModels( BaseUsage source, long metadataStructureId, long stepId)
        {
            Source = source;

            if (Source is MetadataAttributeUsage)
            {

                MetadataAttributeUsage mau = (MetadataAttributeUsage)Source;

                if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
                {
                    MetadataCompoundAttribute mca = (MetadataCompoundAttribute)mau.MetadataAttribute.Self;

                    if (mca != null)
                    {
                        foreach (MetadataNestedAttributeUsage usage in mca.MetadataNestedAttributeUsages)
                        {
                            if (UsageHelper.IsSimple(usage))
                            {
                                MetadataAttributeModels.Add(MetadataAttributeModel.Convert(usage, mau, metadataStructureId, Number, stepId));
                            }
                        }
                    }
                }
            }

            if (Source is MetadataNestedAttributeUsage)
            {
                MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)Source;
                if (mnau.Member.Self is MetadataCompoundAttribute)
                {
                    MetadataCompoundAttribute mca = (MetadataCompoundAttribute)mnau.Member.Self;

                    if (mca != null)
                    {
                        foreach (MetadataNestedAttributeUsage usage in mca.MetadataNestedAttributeUsages)
                        {
                            if (UsageHelper.IsSimple(usage))
                            {
                                MetadataAttributeModels.Add(MetadataAttributeModel.Convert(usage, mnau, metadataStructureId, Number, stepId));
                            }
                        }
                    }
                }
            }
        }
    }
}