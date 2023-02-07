using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Helpers.Mapping;
using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
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
            bool mappingSelectionField = false;

            List<MetadataParameterModel> parameters = new List<MetadataParameterModel>();

            string metadataAttributeName = "";

            //simple
            bool partySimpleMappingExist = false;
            //complex
            bool partyComplexMappingExist = false;

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

            //set metadata attr name
            metadataAttributeName = metadataAttribute.Name;

            //load displayPattern
            DataTypeDisplayPattern dtdp = DataTypeDisplayPattern.Materialize(metadataAttribute.DataType.Extra);
            string displayPattern = "";
            if (dtdp != null) displayPattern = dtdp.StringPattern;

            //ToDO/Check if dim is active
            //check if its linked with a system field
            locked = MappingUtils.ExistSystemFieldMappings(current.Id, type);

            // check if a mapping for parties exits
            partyMappingExist = MappingUtils.ExistMappingWithParty(current.Id, type);


            // check if mapping to this metadata attribute is simple or complex.
            // complex means, that the attribute is defined in the context of the parent
            // e.g. name of User
            // simple means, that the attribute is not defined in the context of the
            // e.g. DataCreator Name in Contacts as list of contacts
            partySimpleMappingExist = hasSimpleMapping(current.Id, type);
            partyComplexMappingExist = hasComplexMapping(current.Id, type);

            // set the flag tru if the attribute is one where the complex object will be fill from
            // e.g. User: name -> name is a main attribute, so its possible so select user by name
            mappingSelectionField = MappingUtils.PartyAttrIsMain(current.Id, type);

            // in case the parent was mapped as a complex object, 
            // you have to check which of the simple fields is the selection field. 
            // If it is not and there is a mapping for the field, it must be blocked.
            // OR if its allready locked because of a system mapping then let it locked.
            if (locked == false && (!mappingSelectionField && partyComplexMappingExist && !partySimpleMappingExist)) { 
                locked = false;
            }

            // check if a mapping for entites exits
            entityMappingExist = MappingUtils.ExistMappingWithEntity(current.Id, type);

            // check wheter attribute has parameters nd create modesl for them
            foreach (var p in metadataAttribute.MetadataParameterUsages)
            {
                parameters.Add(FormHelper.CreateMetadataParameterModel(p, current, metadataStructureId, packageModelNumber, parentStepId));
            }

            return new MetadataAttributeModel
            {
                Id = current.Id,
                Number = 1,
                ParentModelNumber = packageModelNumber,
                MetadataStructureId = metadataStructureId,
                MetadataAttributeName = metadataAttributeName,
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
                PartySimpleMappingExist = partySimpleMappingExist,
                PartyComplexMappingExist = partyComplexMappingExist,
                LowerBoundary = lowerBoundary,
                UpperBoundary = upperBoundary,
                MappingSelectionField = mappingSelectionField,
                Parameters = parameters
            };
        }

        public static MetadataParameterModel CreateMetadataParameterModel(BaseUsage parameter, BaseUsage attribute, long metadataStructureId, int packageModelNumber, long parentStepId)
        {
            using (var metadataAttributeManager = new MetadataAttributeManager())
            {
                MetadataParameter metadataParameter = null; ;
                List<object> domainConstraintList = new List<object>();
                string constraintsDescription = "";
                double lowerBoundary = 0;
                double upperBoundary = 0;


                if (parameter is MetadataParameterUsage)
                {
                    MetadataParameterUsage mpu = (MetadataParameterUsage)parameter;
                    metadataParameter = metadataAttributeManager.GetParameter(mpu.Member.Id);

                    if (metadataParameter.Constraints.Where(c => (c is DomainConstraint)).Count() > 0)
                        domainConstraintList = createDomainContraintList(metadataParameter);

                    if (metadataParameter.Constraints.Count > 0)
                    {
                        foreach (Constraint c in metadataParameter.Constraints)
                        {
                            if (string.IsNullOrEmpty(constraintsDescription)) constraintsDescription = c.FormalDescription;
                            else constraintsDescription = String.Format("{0}\n{1}", constraintsDescription, c.FormalDescription);
                        }
                        if (metadataParameter.DataType.Name == "string" && metadataParameter.Constraints.Where(c => (c is RangeConstraint)).Count() > 0)
                        {
                            foreach (RangeConstraint r in metadataParameter.Constraints.Where(c => (c is RangeConstraint)))
                            {
                                lowerBoundary = r.Lowerbound;
                                upperBoundary = r.Upperbound;
                            }
                        }
                    }

                    //load displayPattern
                    DataTypeDisplayPattern dtdp = DataTypeDisplayPattern.Materialize(metadataParameter.DataType.Extra);
                    string displayPattern = "";
                    if (dtdp != null) displayPattern = dtdp.StringPattern;



                    return new MetadataParameterModel
                    {
                        Id = parameter.Id,
                        Number = 1,
                        ParentModelNumber = packageModelNumber,
                        MetadataStructureId = metadataStructureId,
                        MetadataParameterName = parameter.Label,
                        MetadataParameterId = metadataParameter.Id,
                        Parent = attribute,
                        Source = parameter,
                        DisplayName = parameter.Label,
                        Discription = parameter.Description,
                        ConstraintDescription = constraintsDescription,
                        DataType = metadataParameter.DataType.Name,
                        SystemType = metadataParameter.DataType.SystemType,
                        DisplayPattern = displayPattern,
                        //MinCardinality = metadataParameter.MinCardinality,
                        //MaxCardinality = metadataParameter.MaxCardinality,
                        NumberOfSourceInPackage = 1,
                        DomainList = domainConstraintList,
                        ParentStepId = parentStepId,
                        LowerBoundary = lowerBoundary,
                        UpperBoundary = upperBoundary
                    };
                }

                return null;
            }
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

        private static bool hasComplexMapping(long id, LinkElementType type)
        {
            if (MappingUtils.ExistComplexMappingWithParty(id, type) || 
                MappingUtils.ExistComplexMappingWithPartyCustomType(id,type))
            {
                return true;
            }

            return false;
        }

        private static bool hasSimpleMapping(long id, LinkElementType type)
        {
            if (MappingUtils.ExistSimpleMappingWithParty(id, type) ||
                MappingUtils.ExistSimpleMappingWithPartyCustomType(id, type))
            {
                return true;
            }

            return false;
        }
    }
}
