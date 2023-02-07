using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Utils.Data.MetadataStructure;
using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models.Metadata
{
    public class MetadataCompoundAttributeModel : AbstractMetadataStepModel
    {

        public int NumberOfSourceInPackage { get; set; }

        public bool last = false;
        public bool first = false;

        private MetadataStructureUsageHelper metadataStructureUsageHelper;

        public MetadataCompoundAttributeModel()
        {
            MetadataAttributeModels = new List<MetadataAttributeModel>();
            metadataStructureUsageHelper = new MetadataStructureUsageHelper();
        }

        public void ConvertMetadataAttributeModels(BaseUsage source, long metadataStructureId, long stepId)
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
                            if (metadataStructureUsageHelper.IsSimple(usage))
                            {
                                var metadataAtrributeModel = FormHelper.CreateMetadataAttributeModel(usage, mau, metadataStructureId, Number, stepId);
                                MetadataAttributeModels.Add(metadataAtrributeModel);
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
                            if (metadataStructureUsageHelper.IsSimple(usage))
                            {
                                var metadataAtrributeModel = FormHelper.CreateMetadataAttributeModel(usage, mnau, metadataStructureId, Number, stepId);
                                MetadataAttributeModels.Add(metadataAtrributeModel);

                            }
                        }
                    }
                }
            }
        }

    }
}