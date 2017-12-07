using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Helpers;
using BExIS.Utils.Data.MetadataStructure;
using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models.Metadata
{
    public class MetadataPackageModel : AbstractMetadataStepModel
    {
        private MetadataStructureUsageHelper metadataStructureUsageHelper;

        public MetadataPackageModel()
        {
            ErrorList = new List<Error>();
            metadataStructureUsageHelper = new MetadataStructureUsageHelper();
        }

        public void ConvertMetadataAttributeModels(BaseUsage source, long metadataStructureId, int stepId)
        {
            Source = source;



            //if (Source is MetadataAttributeUsage)
            //{

            //    MetadataAttributeUsage mau = (MetadataAttributeUsage)Source;

            //    if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
            //    {
            //        MetadataCompoundAttribute mca = (MetadataCompoundAttribute)mau.MetadataAttribute.Self;

            //        if (mca != null)
            //        {
            //            foreach (MetadataNestedAttributeUsage usage in mca.MetadataNestedAttributeUsages)
            //            {
            //                if (UsageHelper.IsSimple(usage))
            //                {
            //                    MetadataAttributeModels.Add(MetadataAttributeModel.Convert(usage, mau, metadataStructureId, Number));
            //                }
            //            }
            //        }
            //    }
            //}

            //if (Source is MetadataNestedAttributeUsage)
            //{
            //    MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)Source;
            //    if (mnau.Member.Self is MetadataCompoundAttribute)
            //    {
            //        MetadataCompoundAttribute mca = (MetadataCompoundAttribute)mnau.Member.Self;

            //        if (mca != null)
            //        {
            //            foreach (MetadataNestedAttributeUsage usage in mca.MetadataNestedAttributeUsages)
            //            {
            //                if (UsageHelper.IsSimple(usage))
            //                {
            //                    MetadataAttributeModels.Add(MetadataAttributeModel.Convert(usage, mnau, metadataStructureId, Number));
            //                }
            //            }
            //        }
            //    }
            //}

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
    }
}