using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using BExIS.Dcm.Wizard;
using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Xml.Helpers;

namespace BExIS.Web.Shell.Areas.DCM.Models.Metadata
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

        public void ConvertMetadataAttributeModels()
        {
            if (Source is MetadataAttributeUsage)
            {

                MetadataAttributeUsage mau = (MetadataAttributeUsage)Source;

                if (mau.MetadataAttribute.Self is MetadataCompoundAttribute)
                {
                    MetadataCompoundAttribute mca = (MetadataCompoundAttribute)mau.MetadataAttribute.Self;

                    if (mca != null)
                    {
                        mca.MetadataNestedAttributeUsages.ToList().ForEach(a => MetadataAttributeModels.Add(MetadataAttributeModel.Convert(a, mau, 1, Number)));
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
                        mca.MetadataNestedAttributeUsages.ToList().ForEach(a => MetadataAttributeModels.Add(MetadataAttributeModel.Convert(a, mnau, 1, Number)));
                    }
                }
            }
        }
    }
}