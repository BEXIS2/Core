using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Utils.Data.MetadataStructure;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Modules.Dcm.UI.Models.Metadata
{
    public class MetadataPackageModel : AbstractMetadataStepModel
    {
        private MetadataStructureUsageHelper metadataStructureUsageHelper;

        public MetadataPackageModel()
        {
            ErrorList = new List<Error>();
            metadataStructureUsageHelper = new MetadataStructureUsageHelper();
            MetadataParameterModels = new List<MetadataParameterModel>();
        }

        public void ConvertMetadataAttributeModels(BaseUsage source, long metadataStructureId, int stepId)
        {
            Source = source;

            if (Source is MetadataPackageUsage)
            {
                MetadataPackageUsage mpu = (MetadataPackageUsage)Source;
                if (mpu.MetadataPackage is MetadataPackage)
                {
                    MetadataPackage mp = mpu.MetadataPackage;

                    if (mp != null)
                    {
                        foreach (MetadataAttributeUsage usage in mp.MetadataAttributeUsages)
                        {
                            if (metadataStructureUsageHelper.IsSimple(usage))
                            {
                                MetadataAttributeModels.Add(FormHelper.CreateMetadataAttributeModel(usage, mpu, metadataStructureId, Number, stepId));
                            }
                        }
                    }
                }
            }
        }

        public void ConvertMetadataParameterModels(BaseUsage source, long metadataStructureId, long stepId)
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
                        foreach (MetadataParameterUsage usage in mca.MetadataParameterUsages)
                        {

                            var metadataParameterModel = FormHelper.CreateMetadataParameterModel(usage, mau, metadataStructureId, Number, stepId);
                            MetadataParameterModels.Add(metadataParameterModel);

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
                                var metadataParameterModel = FormHelper.CreateMetadataParameterModel(usage, mnau, metadataStructureId, Number, stepId);
                                MetadataParameterModels.Add(metadataParameterModel);

                            }
                        }
                    }
                }
            }
        }
    }
}