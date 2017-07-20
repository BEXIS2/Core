using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Utils.Data.MetadataStructure;
using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models.Metadata
{
    public class MetadataPackageModel : AbstractMetadataStepModel
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
                    MetadataPackage mp = (MetadataPackage)mpu.MetadataPackage;

                    if (mp != null)
                    {
                        foreach (MetadataAttributeUsage usage in mp.MetadataAttributeUsages)
                        {
                            if (MetadataStructureUsageHelper.IsSimple(usage))
                            {
                                MetadataAttributeModels.Add(MetadataAttributeModel.Convert(usage, mpu, metadataStructureId, Number, stepId));
                            }
                        }
                    }
                }
            }
        }
    }
}