using BExIS.Dcm.CreateDatasetWizard;
using BExIS.Dcm.UploadWizard;
using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.IO.DataType.DisplayPattern;
using BExIS.Modules.Dcm.UI.Models.Metadata;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using Vaiona.Persistence.Api;

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

            // check if a fixed value should block the attribute for changing
            if (locked == false && !string.IsNullOrEmpty(current.FixedValue)) locked = true;

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
            if (partySimpleMappingExist || partyComplexMappingExist)
                mappingSelectionField = MappingUtils.PartyAttrIsMain(current.Id, type);

            // in case the parent was mapped as a complex object,
            // you have to check which of the simple fields is the selection field.
            // If it is not and there is a mapping for the field, it must be blocked.
            // OR if its allready locked because of a system mapping then let it locked.
            if (locked == false && (!mappingSelectionField && partyComplexMappingExist && !partySimpleMappingExist))
            {
                locked = false;
            }

            // check if a mapping for entites exits
            entityMappingExist = MappingUtils.ExistMappingWithEntity(current.Id, type);

            // check wheter attribute has parameters nd create modesl for them
            foreach (var p in metadataAttribute.MetadataParameterUsages)
            {
                parameters.Add(FormHelper.CreateMetadataParameterModel(p, current, metadataStructureId, packageModelNumber, parentStepId));
            }

            string value = "";
            if(!string.IsNullOrEmpty(current.FixedValue)) value = current.FixedValue;
            else if (!string.IsNullOrEmpty(current.DefaultValue)) value = current.DefaultValue; 

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
                Parameters = parameters,
                DefaultValue = current.DefaultValue,
                FixedValue = current.FixedValue,
                Value = value
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
                bool locked = false;

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

                    // check if a fixed value should block the attribute for changing
                    if (locked == false && !string.IsNullOrEmpty(parameter.FixedValue)) locked = true;

                    return new MetadataParameterModel
                    {
                        Id = parameter.Id,
                        AttributeNumber = 1,
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
                        MinCardinality = parameter.MinCardinality,
                        //MaxCardinality = metadataParameter.MaxCardinality,
                        NumberOfSourceInPackage = 1,
                        DomainList = domainConstraintList,
                        ParentStepId = parentStepId,
                        LowerBoundary = lowerBoundary,
                        UpperBoundary = upperBoundary,
                        DefaultValue = parameter.DefaultValue,
                        FixedValue = parameter.FixedValue,
                        Locked = locked
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
                MappingUtils.ExistComplexMappingWithPartyCustomType(id, type))
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

        public static string GetTaskManagerKey(long entityId)
        {
            return "DatasetTaskmanager_"+ entityId;
        }

        /// <summary>
        /// get task manager from session
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public static CreateTaskmanager GetTaskManager(long entityId)
        {
            string key = GetTaskManagerKey(entityId);
            CreateTaskmanager taskManager = (CreateTaskmanager)System.Web.HttpContext.Current.Session[key];
            if (taskManager == null)
            {
                taskManager = new CreateTaskmanager();
                System.Web.HttpContext.Current.Session[key] = taskManager;
            }

            return taskManager;
        }

        /// <summary>
        /// update task manager in session
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="taskmanager"></param>
        /// <returns></returns>
        public static bool UpdateTaskManager(long entityId, CreateTaskmanager taskmanager)
        {
            string key = GetTaskManagerKey(entityId);
            System.Web.HttpContext.Current.Session[key] = taskmanager;

            return true;
        }

        /// <summary>
        /// remove usages from session
        /// </summary>
        /// <param name="entityId"></param>
        public static void ClearCache(long metadataStructureId)
        {
            System.Web.HttpContext.Current.Session["MetadataAttributeUsages_"+ metadataStructureId] = null;
            System.Web.HttpContext.Current.Session["MetadataNestedAttributeUsages_"+ metadataStructureId] = null;
            System.Web.HttpContext.Current.Session["MetadataPackageUsages_"+ metadataStructureId] = null;
        }

        public static IList<MetadataAttributeUsage> InitMetadataAttributeUsages(List<long> packages, long metadatastructureId)
        {
            string key = "MetadataAttributeUsages_" + metadatastructureId;
            using (IUnitOfWork uow = (new object()).GetUnitOfWork())
            {
                var usages = uow.GetReadOnlyRepository<MetadataAttributeUsage>().Query(mau => packages.Contains(mau.MetadataPackage.Id)).ToList();
                System.Web.HttpContext.Current.Session[key] = usages;
                return usages;
            }
        }

        public static IList<MetadataAttributeUsage> CachedMetadataAttributeUsages(CreateTaskmanager taskManager)
        {
            long metadataStructureId = getMetadataStructureId(taskManager); // get metadata structure id from task manager

            string key = "MetadataAttributeUsages_"+ metadataStructureId;
            // System.Web.HttpContext may not existing during the async upload, so check wheter the context exist
            if (System.Web.HttpContext.Current != null)
            {
                if (System.Web.HttpContext.Current.Session[key] != null)
                {
                    return (List<MetadataAttributeUsage>)System.Web.HttpContext.Current.Session[key];
                }
                else
                {
                    using (IUnitOfWork uow = (new object()).GetUnitOfWork())
                    {
                        var usages = uow.GetReadOnlyRepository<MetadataAttributeUsage>().Get();
                        System.Web.HttpContext.Current.Session[key] = usages;
                        return usages;
                    }
                }
            }
            else // if the System.Web.HttpContext is not existing, mappings need to be loaded by every call
            {
                using (IUnitOfWork uow = (new object()).GetUnitOfWork())
                {
                    return uow.GetReadOnlyRepository<MetadataAttributeUsage>().Get();
                }
            }
        }

        public static IList<MetadataNestedAttributeUsage> InitMetadataNestedAttributeUsages(long metadataStructureId)
        {
            string key = "MetadataNestedAttributeUsages_" + metadataStructureId;

            List<MetadataNestedAttributeUsage> usages = new List<MetadataNestedAttributeUsage>();

            using (var metadataAttributeManager = new MetadataAttributeManager())
            {
                usages = metadataAttributeManager.GetEffectiveMetadataNestedAttributeUsages(metadataStructureId);
                System.Web.HttpContext.Current.Session[key] = usages;
            }

            return usages;
        }

        public static IList<MetadataNestedAttributeUsage> CachedMetadataNestedAttributeUsages(CreateTaskmanager taskManager)
        {
            long metadataStructureId = getMetadataStructureId(taskManager); // get metadata structure id from task manager

            string key = "MetadataNestedAttributeUsages_" + metadataStructureId;
            // System.Web.HttpContext may not existing during the async upload, so check wheter the context exist
            if (System.Web.HttpContext.Current != null)
            {
                if (System.Web.HttpContext.Current.Session[key] != null)
                {
                    return (List<MetadataNestedAttributeUsage>)System.Web.HttpContext.Current.Session[key];
                }
                else
                {
                    using (IUnitOfWork uow = (new object()).GetUnitOfWork())
                    {
                        var usages = uow.GetReadOnlyRepository<MetadataNestedAttributeUsage>().Get();
                        System.Web.HttpContext.Current.Session[key] = usages;
                        return usages;
                    }
                }
            }
            else // if the System.Web.HttpContext is not existing, mappings need to be loaded by every call
            {
                using (IUnitOfWork uow = (new object()).GetUnitOfWork())
                {
                    return uow.GetReadOnlyRepository<MetadataNestedAttributeUsage>().Get();
                }
            }
        }

        public static IList<MetadataPackageUsage> InitMetadataPackageUsages(long metadatastructureId)
        {
            string key = "MetadataPackageUsages_" + metadatastructureId;

            using (IUnitOfWork uow = (new object()).GetUnitOfWork())
            {
                var usages = uow.GetReadOnlyRepository<MetadataPackageUsage>().Query(p => p.MetadataStructure.Id == metadatastructureId).ToList();
                System.Web.HttpContext.Current.Session[key] = usages;
                return usages;
            }
        }

        /// <summary>
        /// get cached metadata package usages for a metadata structure
        /// </summary>
        /// <param name="taskManager"></param>
        /// <returns></returns>
        public static  IList<MetadataPackageUsage> CachedMetadataPackageUsages(CreateTaskmanager taskManager)
        {
            long metadataStructureId = getMetadataStructureId(taskManager); // get metadata structure id from task manager

            string key = "MetadataPackageUsages_"+ metadataStructureId;
            // System.Web.HttpContext may not existing during the async upload, so check wheter the context exist
            if (System.Web.HttpContext.Current != null)
            {
                if (System.Web.HttpContext.Current.Session[key] != null)
                {
                    return (List<MetadataPackageUsage>)System.Web.HttpContext.Current.Session[key];
                }
                else
                {
                    using (IUnitOfWork uow = (new object()).GetUnitOfWork())
                    {
                        var usages = uow.GetReadOnlyRepository<MetadataPackageUsage>().Query().ToList();
                        System.Web.HttpContext.Current.Session[key] = usages;
                        return usages;
                    }
                }
            }
            else // if the System.Web.HttpContext is not existing, mappings need to be loaded by every call
            {
                using (IUnitOfWork uow = (new object()).GetUnitOfWork())
                {
                    return uow.GetReadOnlyRepository<MetadataPackageUsage>().Get();
                }
            }
        }

        /// <summary>
        /// get metadata structure id from task manager
        /// </summary>
        /// <param name="taskManager"></param>
        /// <returns></returns>
        private static long getMetadataStructureId(CreateTaskmanager taskManager)
        {
            long id = 0;

            if(taskManager != null && taskManager.Bus.ContainsKey(CreateTaskmanager.METADATASTRUCTURE_ID))
            {
                id = (long)taskManager.Bus[CreateTaskmanager.METADATASTRUCTURE_ID];
            }

            return id;

        }
    }
}