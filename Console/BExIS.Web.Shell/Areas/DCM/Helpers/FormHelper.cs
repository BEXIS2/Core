using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Helpers.Mapping;
using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.IO.DataType.DisplayPattern;
using BExIS.Modules.Dcm.UI.Models.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Modules.Dcm.UI.Helpers
{
    public class FormHelper
    {
        public static MetadataCompoundAttributeModel CreateMetadataCompoundAttributeModel(BaseUsage metadataAttributeUsage, int number)
        {
            return new MetadataCompoundAttributeModel
            {
                Id = metadataAttributeUsage.Id,
                Number = number,
                Source = metadataAttributeUsage,
                DisplayName = metadataAttributeUsage.Label,
                Discription = metadataAttributeUsage.Description,
                MinCardinality = metadataAttributeUsage.MinCardinality,
                MaxCardinality = metadataAttributeUsage.MaxCardinality,
                NumberOfSourceInPackage = 1,
                first = true,
                last = true
            };
        }

        public static MetadataPackageModel CreateMetadataPackageModel(BaseUsage mPUsage, int number)
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

        public static MetadataAttributeModel CreateMetadataAttributeModel(BaseUsage current, BaseUsage parent, long metadataStructureId, int packageModelNumber, long parentStepId)
        {
            MetadataAttribute metadataAttribute;
            List<object> domainConstraintList = new List<object>();
            string constraintsDescription = "";
            double lowerBoundary = 0;
            double upperBoundary = 0;
            LinkElementType type = LinkElementType.MetadataNestedAttributeUsage;
            bool locked = false;
            bool entityMappingExist = false;
            bool partyMappingExist = false;

            if (current is MetadataNestedAttributeUsage)
            {
                MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)current;
                metadataAttribute = mnau.Member;
                type = LinkElementType.MetadataNestedAttributeUsage;
            }
            else
            {
                MetadataAttributeUsage mau = (MetadataAttributeUsage)current;
                metadataAttribute = mau.MetadataAttribute;
                type = LinkElementType.MetadataAttributeUsage;
            }

            if (metadataAttribute.Constraints.Where(c => (c is DomainConstraint)).Count() > 0)
                domainConstraintList = createDomainContraintList(metadataAttribute);

            if (metadataAttribute.Constraints.Count > 0)
            {
                foreach (Constraint c in metadataAttribute.Constraints)
                {
                    if (string.IsNullOrEmpty(constraintsDescription)) constraintsDescription = c.FormalDescription;
                    else constraintsDescription = String.Format("{0}\n{1}", constraintsDescription, c.FormalDescription);
                }
                if (metadataAttribute.DataType.Name == "string" && metadataAttribute.Constraints.Where(c => (c is RangeConstraint)).Count() > 0)
                {
                    foreach (RangeConstraint r in metadataAttribute.Constraints.Where(c => (c is RangeConstraint)))
                    {
                        lowerBoundary = r.Lowerbound;
                        upperBoundary = r.Upperbound;
                    }
                }
            }
            //load displayPattern
            DataTypeDisplayPattern dtdp = DataTypeDisplayPattern.Materialize(metadataAttribute.DataType.Extra);
            string displayPattern = "";
            if (dtdp != null) displayPattern = dtdp.StringPattern;

            //ToDO/Check if dim is active
            //check if its linked with a system field
            //
            locked = MappingUtils.ExistSystemFieldMappings(current.Id, type);

            // check if a mapping for parties exits
            partyMappingExist = MappingUtils.ExistMappingWithParty(current.Id, type);

            // check if a mapping for entites exits
            entityMappingExist = MappingUtils.ExistMappingWithEntity(current.Id, type);

            return new MetadataAttributeModel
            {
                Id = current.Id,
                Number = 1,
                ParentModelNumber = packageModelNumber,
                MetadataStructureId = metadataStructureId,
                Parent = parent,
                Source = current,
                DisplayName = current.Label,
                Discription = current.Description,
                ConstraintDescription = constraintsDescription,
                DataType = metadataAttribute.DataType.Name,
                SystemType = metadataAttribute.DataType.SystemType,
                DisplayPattern = displayPattern,
                MinCardinality = current.MinCardinality,
                MaxCardinality = current.MaxCardinality,
                NumberOfSourceInPackage = 1,
                first = true,
                DomainList = domainConstraintList,
                last = true,
                MetadataAttributeId = metadataAttribute.Id,
                ParentStepId = parentStepId,
                Errors = null,
                Locked = locked,
                EntityMappingExist = entityMappingExist,
                PartyMappingExist = partyMappingExist,
                LowerBoundary = lowerBoundary,
                UpperBoundary = upperBoundary,
            };
        }

        private static List<object> createDomainContraintList(MetadataAttribute attribute)
        {
            List<object> list = new List<object>();

            foreach (Constraint constraint in attribute.Constraints)
            {
                if (constraint is DomainConstraint)
                {
                    DomainConstraint domainConstraint = (DomainConstraint)constraint;
                    domainConstraint.Materialize();
                    domainConstraint.Items.ForEach(i => list.Add(i.Value));
                }
            }

            return list;
        }
    }
}
