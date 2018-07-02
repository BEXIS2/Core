﻿using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Dim.UI.Models.Mapping;
using BExIS.Utils.Data.MetadataStructure;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.Dim.UI.Helper
{
    public class MappingHelper
    {
        private static MetadataStructureUsageHelper metadataStructureUsageHelper = new MetadataStructureUsageHelper();


        #region load rootList

        public static List<LinkElementRootListItem> LoadSelectionList()
        {
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();

            try
            {
                List<LinkElementRootListItem> tmp = new List<LinkElementRootListItem>();

                //load System
                LinkElementRootListItem li = new LinkElementRootListItem(0, "System", LinkElementType.System);


                tmp.Add(li);

                //load Metadata Strutcure
                IEnumerable<MetadataStructure> metadataStructures = metadataStructureManager.Repo.Get();

                foreach (var metadataStructure in metadataStructures)
                {
                    li = new LinkElementRootListItem(
                        metadataStructure.Id,
                        metadataStructure.Name,
                        LinkElementType.MetadataStructure
                        );

                    tmp.Add(li);
                }


                return tmp;
            }
            finally
            {
                metadataStructureManager.Dispose();
            }
        }

        #endregion


        #region load Model from metadataStructure

        public static LinkElementRootModel LoadFromMetadataStructure(long id, LinkElementPostion rootModelType, MappingManager mappingManager)
        {

            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();

            try
            {
                MetadataStructure metadataStructure = metadataStructureManager.Repo.Get(id);

                LinkElementRootModel model = new LinkElementRootModel(LinkElementType.MetadataStructure, id, metadataStructure.Name, rootModelType);

                if (metadataStructure != null)
                {

                    LinkElement metadataStructureLinkElement = mappingManager.GetLinkElement(metadataStructure.Id,
                            LinkElementType.MetadataStructure);

                    long metadataStructureLinkElementId = 0;
                    if (metadataStructureLinkElement != null)
                        metadataStructureLinkElementId = metadataStructureLinkElement.Id;


                    LinkElementModel LEModel = new LinkElementModel(
                        metadataStructureLinkElementId,
                        metadataStructure.Id,
                        LinkElementType.MetadataStructure,
                        metadataStructure.Name, "Metadata",
                        rootModelType,
                        LinkElementComplexity.Complex,
                        metadataStructure.Description);


                    foreach (var pUsage in metadataStructure.MetadataPackageUsages)
                    {
                        addUsageAsLinkElement(pUsage, "Metadata", model, LEModel);
                    }

                    model = CreateLinkElementContainerModels(model);
                    model.Id = id;
                }
                return model;
            }
            finally
            {
                metadataStructureManager.Dispose();
            }
        }

        public static void addUsageAsLinkElement(BaseUsage usage, string parentXpath, LinkElementRootModel rootModel,
            LinkElementModel parent)
        {
            int min = usage.MinCardinality;
            string childName = "";



            //MappingManager mappingManager = new MappingManager();


            string usageName = usage.Label;
            string typeName = "x";
            long typeId = 0;
            string typeDescription = "";
            string xPath = parentXpath;
            LinkElementType type = LinkElementType.ComplexMetadataAttribute;
            LinkElementComplexity complexity = LinkElementComplexity.Complex;

            bool addTypeAsLinkElement = false;

            if (usage is MetadataPackageUsage)
            {
                type = LinkElementType.MetadataPackageUsage;
                typeName = ((MetadataPackageUsage)usage).MetadataPackage.Name;
            }
            else if (usage is MetadataNestedAttributeUsage)
            {
                type = LinkElementType.MetadataNestedAttributeUsage;
                MetadataNestedAttributeUsage n = (MetadataNestedAttributeUsage)usage;
                typeName = n.Member.Name;

                if (n.Member.Self is MetadataCompoundAttribute)
                {
                    addTypeAsLinkElement = true;
                    typeId = n.Member.Self.Id;
                    typeDescription = n.Member.Self.Description;

                }

                if (n.Member.Self is MetadataSimpleAttribute)
                {
                    complexity = LinkElementComplexity.Simple;
                }
            }
            else if (usage is MetadataAttributeUsage)
            {
                type = LinkElementType.MetadataAttributeUsage;
                MetadataAttributeUsage u = (MetadataAttributeUsage)usage;
                typeName = u.MetadataAttribute.Name;

                if (u.MetadataAttribute.Self is MetadataCompoundAttribute)
                {
                    addTypeAsLinkElement = true;
                    typeId = u.MetadataAttribute.Self.Id;
                    typeDescription = u.MetadataAttribute.Self.Description;

                }

                if (u.MetadataAttribute.Self is MetadataSimpleAttribute)
                {
                    complexity = LinkElementComplexity.Simple;
                }
            }

            // add usage
            xPath = parentXpath + "/" + usageName.Replace(" ", string.Empty) + "/" + typeName;

            long linkElementId = 0;
            string mask = "";

            LinkElement linkElement = type.GetUnitOfWork().GetReadOnlyRepository<LinkElement>().Get()
                    .FirstOrDefault(le => le.ElementId.Equals(usage.Id) && le.Type.Equals(type));

            if (linkElement != null)
            {
                linkElementId = linkElement.Id;
            }


            LinkElementModel LEModel = new LinkElementModel(
                linkElementId,
                usage.Id,
                type, usage.Label, xPath, rootModel.Position, complexity, usage.Description);
            LEModel.Parent = parent;
            rootModel.LinkElements.Add(LEModel);



            //add type
            if (addTypeAsLinkElement)
            {
                linkElementId = 0;

                linkElement =
                    type.GetUnitOfWork().GetReadOnlyRepository<LinkElement>().Get()
                        .FirstOrDefault(
                            le =>
                                le.ElementId.Equals(typeId) &&
                                le.Type.Equals(LinkElementType.ComplexMetadataAttribute));

                if (linkElement != null)
                {
                    linkElementId = linkElement.Id;
                }

                LEModel = new LinkElementModel(
                    linkElementId,
                    typeId,
                    LinkElementType.ComplexMetadataAttribute,
                    typeName,
                    xPath,
                    rootModel.Position,
                    complexity,
                    typeDescription);

                LEModel.Parent = parent;


                if (!rootModel.LinkElements.Any(le => le.ElementId.Equals(typeId) &&
                                                      le.Type.Equals(LinkElementType.ComplexMetadataAttribute)))
                {
                    rootModel.LinkElements.Add(LEModel);
                }
            }


            //Debug.WriteLine("1: " + LEModel.Name + " " + LEModel.Type);

            List<BaseUsage> childrenUsages = metadataStructureUsageHelper.GetChildren(usage.Id, usage.GetType());

            if (childrenUsages.Count > 0)
            {
                foreach (BaseUsage childUsage in childrenUsages)
                {
                    addUsageAsLinkElement(childUsage, xPath, rootModel, LEModel);
                }

                //AddChildrens
                //addLinkElementsFromChildrens(usage, xPath, rootModel);
            }

        }

        #endregion

        #region Load Model From System

        public static LinkElementRootModel LoadfromSystem(LinkElementPostion rootModelType, MappingManager mappingManager)
        {
            //get all parties - complex
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            PartyRelationshipTypeManager partyRelationshipTypeManager = new PartyRelationshipTypeManager();

            try
            {
                LinkElementRootModel model = new LinkElementRootModel(LinkElementType.System, 0, "System", rootModelType);

                LinkElement SystemRoot = mappingManager.GetLinkElement(0, LinkElementType.System);

                long id = 0;
                long elementId = 0;
                if (SystemRoot != null)
                {
                    id = SystemRoot.Id;
                    elementId = SystemRoot.ElementId;
                }


                LinkElementModel LEParent = new LinkElementModel(
                       id,
                       elementId,
                       LinkElementType.System,
                       "System", "",
                       rootModelType,
                       LinkElementComplexity.Complex,
                       "");


                #region get party types
                IEnumerable<PartyType> partyTypes = partyTypeManager.PartyTypeRepository.Get();

                foreach (var pt in partyTypes)
                {
                    LinkElementModel ptModel = createLinkElementModelType(pt, model, LEParent, mappingManager);
                    model.LinkElements.Add(ptModel);
                    //get all partyCustomTypeAttr -> simple
                    model.LinkElements.AddRange(createLinkElementModelPartyCustomType(pt, model, ptModel, mappingManager));
                }

                #endregion

                #region keys
                //get all keys -> simple
                foreach (Key value in Key.GetValues(typeof(Key)))
                {
                    long linkElementId = GetId((int)value, LinkElementType.Key, mappingManager);
                    //string mask = GetMask((int)value, LinkElementType.Key);

                    LinkElementModel LEModel = new LinkElementModel(
                            linkElementId,
                            (int)value,
                            LinkElementType.Key, value.ToString(), "", model.Position, LinkElementComplexity.Simple, "");

                    LEModel.Parent = LEParent;

                    model.LinkElements.Add(LEModel);

                }

                #endregion

                #region get all relationships

                IEnumerable<PartyRelationshipType> relationshipTypes = partyRelationshipTypeManager.PartyRelationshipTypeRepository.Get();

                foreach (PartyRelationshipType partyRelationshipType in relationshipTypes)
                {
                    long value = partyRelationshipType.Id;
                    long linkElementId = GetId(partyRelationshipType.Id, LinkElementType.Key, mappingManager);

                    LinkElementModel LEModel = new LinkElementModel(
                            linkElementId,
                            partyRelationshipType.Id,
                            LinkElementType.PartyRelationshipType, 
                            partyRelationshipType.DisplayName, 
                            "", 
                            model.Position, 
                            LinkElementComplexity.Simple, 
                            "");

                    LEModel.Parent = LEParent;

                    model.LinkElements.Add(LEModel);
                }

                #endregion

                //create container
                model = CreateLinkElementContainerModels(model);

                return model;
            }
            finally
            {
                partyTypeManager.Dispose();
            }

        }

        private static LinkElementModel createLinkElementModelType(
            PartyType partyType,
            LinkElementRootModel rootModel,
            LinkElementModel parent, MappingManager mappingManager)
        {

            long linkElementId = GetId(partyType.Id, LinkElementType.PartyType, mappingManager);

            LinkElementModel LEModel = new LinkElementModel(
                        linkElementId,
                        partyType.Id,
                        LinkElementType.PartyType, partyType.Title, "", rootModel.Position, LinkElementComplexity.Complex, partyType.Description);

            LEModel.Parent = parent;

            return LEModel;
        }

        private static List<LinkElementModel> createLinkElementModelPartyCustomType(PartyType partyType, LinkElementRootModel rootModel, LinkElementModel parent, MappingManager mappingManager)
        {
            List<LinkElementModel> tmp = new List<LinkElementModel>();

            PartyTypeManager partyTypeManager = new PartyTypeManager();

            try
            {

                foreach (var partyCustomType in partyType.CustomAttributes)
                {
                    long linkElementId = GetId(partyCustomType.Id, LinkElementType.PartyCustomType, mappingManager);

                    LinkElementModel LEModel = new LinkElementModel(
                                linkElementId,
                                partyCustomType.Id,
                                LinkElementType.PartyCustomType, partyCustomType.Name, partyType.Title + "/" + partyCustomType.Name, rootModel.Position, LinkElementComplexity.Simple, partyCustomType.Description);
                    LEModel.Parent = parent;

                    tmp.Add(LEModel);
                }


                return tmp;
            }
            finally
            {
                partyTypeManager.Dispose();
            }
        }


        #endregion

        #region loadMapping

        public static List<ComplexMappingModel> LoadMappings(Mapping rootMapping)
        {
            MappingManager mappingManager = new MappingManager();
            try
            {
                List<ComplexMappingModel> tmp = new List<ComplexMappingModel>();

                //get all complex mappings
                List<Mapping> mappings = mappingManager.GetChildMapping(rootMapping.Id, 1).ToList();

                foreach (var mapping in mappings)
                {

                    ComplexMappingModel model = CreateComplexMappingModel(mapping);


                    //get all complex mappings
                    List<Mapping> childMappings = mappingManager.GetChildMapping(mapping.Id, 2).ToList();


                    foreach (var cm in childMappings)
                    {
                        //ToDO Add transformation rule
                        model.SimpleMappings.Add(CreateSimpleMappingModel(cm, cm.Source, cm.Target));
                    }

                    tmp.Add(model);
                }
                return tmp;
            }
            finally
            {
                mappingManager.Dispose();
            }
        }



        #endregion

        public static LinkElementRootModel CreateLinkElementContainerModels(LinkElementRootModel model)
        {

            foreach (LinkElementComplexity value in LinkElementComplexity.GetValues(typeof(LinkElementComplexity)))
            {
                LinkElementContainerModel tmp = CreateLinkeContainerModel(value, model.LinkElements, model.Position);
                if (tmp != null) model.LinkElementContainers.Add(tmp);
            }


            return model;
        }

        public static LinkElementContainerModel CreateLinkeContainerModel(LinkElementComplexity complexity,
            List<LinkElementModel> linkElements, LinkElementPostion position)
        {
            if (linkElements.Any(l => l.Complexity.Equals(complexity)))
            {
                LinkElementContainerModel cModel = new LinkElementContainerModel(complexity, position);

                cModel.LinkElements = linkElements.Where(l => l.Complexity.Equals(complexity)).ToList();

                return cModel;
            }
            else
            {
                return null;
            }
        }


        public static LinkElementModel LoadChildren(LinkElementModel model)
        {
            switch (model.Type)
            {
                case LinkElementType.PartyType:
                    {
                        model.Children = getChildrenFromPartyType(model);
                        break;
                    }
                case LinkElementType.ComplexMetadataAttribute:
                    {
                        model.Children = getChildrenFromComplexMetadataAttribute(model);
                        break;
                    }
                case LinkElementType.MetadataNestedAttributeUsage:
                    {
                        //ToDo load childrens from nestedUsage
                        model.Children = getChildrenFromMetadataNestedUsage(model);
                        break;
                    }
                case LinkElementType.MetadataPackage:
                    {
                        //ToDo load childrens from packageUsage
                        model.Children = getChildrenFromMetadataPackage(model);
                        break;
                    }
                case LinkElementType.MetadataPackageUsage:
                    {
                        //ToDo load childrens from packageUsage
                        model.Children = getChildrenFromMetadataPackageUsage(model);
                        break;
                    }
                case LinkElementType.MetadataAttributeUsage:
                    {
                        //ToDo load childrens from packageUsage
                        model.Children = getChildrenFromMetadataAttributeUsage(model);
                        break;
                    }
            }

            return model;
        }

        private static List<LinkElementModel> getChildrenFromPartyType(LinkElementModel model)
        {
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            try
            {
                IEnumerable<PartyCustomAttribute> ptAttr = partyTypeManager.PartyCustomAttributeRepository.Get().Where(p => p.PartyType.Id.Equals(model.ElementId));

                foreach (var attr in ptAttr)
                {
                    model.Children.Add(
                        new LinkElementModel(
                            0,
                            attr.Id,
                            LinkElementType.PartyCustomType, attr.Name, "", model.Position, LinkElementComplexity.Simple, attr.Description)
                        );
                }

                return model.Children;
            }
            finally
            {
                partyTypeManager.Dispose();
            }
        }

        private static List<LinkElementModel> getChildrenFromComplexMetadataAttribute(LinkElementModel model)
        {
            return getChildrenFromComplexMetadataAttribute(model.ElementId, model.Position);
        }

        private static List<LinkElementModel> getChildrenFromComplexMetadataAttribute(long metadataCompountAttributeId, LinkElementPostion position)
        {
            List<LinkElementModel> tmp = new List<LinkElementModel>();

            MetadataAttributeManager metadataAttributeManager = new MetadataAttributeManager();

            try
            {

                MetadataCompoundAttribute mca = metadataAttributeManager.MetadataCompoundAttributeRepo.Get(metadataCompountAttributeId);

                foreach (var attr in mca.MetadataNestedAttributeUsages)
                {
                    LinkElementComplexity complexity = LinkElementComplexity.None;
                    LinkElementType type = LinkElementType.ComplexMetadataAttribute;

                    complexity = attr.Member.Self is MetadataSimpleAttribute
                        ? LinkElementComplexity.Simple
                        : LinkElementComplexity.Complex;

                    //type = attr.Member.Self is MetadataSimpleAttribute
                    //    ? LinkElementType.SimpleMetadataAttribute
                    //    : LinkElementType.ComplexMetadataAttribute;

                    type = LinkElementType.MetadataNestedAttributeUsage;


                    tmp.Add(
                            new LinkElementModel(
                                0,
                                attr.Id,
                                type, attr.Label, "", position, complexity, attr.Description)
                            );
                }

                return tmp;
            }
            finally
            {
                metadataAttributeManager.Dispose();
            }
        }

        private static List<LinkElementModel> getChildrenFromMetadataAttributeUsage(LinkElementModel model)
        {
            MetadataAttributeManager metadataAttributeManager = new MetadataAttributeManager();
            try
            {

                MetadataAttributeUsage metadataAttributeUsage = metadataAttributeManager.MetadataAttributeUsageRepo.Get(model.ElementId);

                LinkElementComplexity complexity = LinkElementComplexity.None;
                LinkElementType type = LinkElementType.ComplexMetadataAttribute;

                complexity = metadataAttributeUsage.MetadataAttribute.Self is MetadataSimpleAttribute
                    ? LinkElementComplexity.Simple
                    : LinkElementComplexity.Complex;

                if (complexity == LinkElementComplexity.Complex)
                {
                    return getChildrenFromComplexMetadataAttribute(metadataAttributeUsage.MetadataAttribute.Id, model.Position);
                }

                return new List<LinkElementModel>();
            }
            finally
            {
                metadataAttributeManager.Dispose();
            }

        }

        private static List<LinkElementModel> getChildrenFromMetadataNestedUsage(LinkElementModel model)
        {
            MetadataAttributeManager metadataAttributeManager = new MetadataAttributeManager();

            try
            {

                MetadataNestedAttributeUsage metadataNestedAttributeUsage =
                    metadataAttributeManager.MetadataNestedAttributeUsageRepo.Get(model.ElementId);


                LinkElementComplexity complexity = LinkElementComplexity.None;
                LinkElementType type = LinkElementType.ComplexMetadataAttribute;

                complexity = metadataNestedAttributeUsage.Member.Self is MetadataSimpleAttribute
                    ? LinkElementComplexity.Simple
                    : LinkElementComplexity.Complex;

                if (complexity == LinkElementComplexity.Complex)
                {
                    return getChildrenFromComplexMetadataAttribute(metadataNestedAttributeUsage.Member.Id, model.Position);
                }

                return new List<LinkElementModel>();
            }
            finally
            {
                metadataAttributeManager.Dispose();
            }
        }


        private static List<LinkElementModel> getChildrenFromMetadataPackage(LinkElementModel model)
        {
            return getChildrenFromMetadataPackage(model.ElementId, model.Position);
        }

        private static List<LinkElementModel> getChildrenFromMetadataPackageUsage(LinkElementModel model)
        {

            MetadataStructureManager msm = new MetadataStructureManager();
            try
            {
                MetadataPackageUsage metadataPackageUsage = msm.PackageUsageRepo.Get(model.ElementId);

                return getChildrenFromMetadataPackage(metadataPackageUsage.MetadataPackage.Id, model.Position);
            }
            finally
            {
                msm.Dispose();
            }
        }

        private static List<LinkElementModel> getChildrenFromMetadataPackage(long metadataPackageId, LinkElementPostion pos)
        {
            MetadataPackageManager metadataPackageManager = new MetadataPackageManager();

            try
            {

                MetadataPackage metadataPackage = metadataPackageManager.MetadataPackageRepo.Get(metadataPackageId);

                List<LinkElementModel> tmp = new List<LinkElementModel>();
                foreach (var attr in metadataPackage.MetadataAttributeUsages)
                {
                    LinkElementComplexity complexity = LinkElementComplexity.None;
                    LinkElementType type = LinkElementType.ComplexMetadataAttribute;

                    complexity = attr.MetadataAttribute.Self is MetadataSimpleAttribute
                        ? LinkElementComplexity.Simple
                        : LinkElementComplexity.Complex;

                    //type = attr.Member.Self is MetadataSimpleAttribute
                    //    ? LinkElementType.SimpleMetadataAttribute
                    //    : LinkElementType.ComplexMetadataAttribute;

                    type = LinkElementType.MetadataNestedAttributeUsage;


                    tmp.Add(
                            new LinkElementModel(
                                0,
                                attr.Id,
                                type, attr.Label, "", pos, complexity, attr.Description)
                            );

                }

                return tmp;
            }
            finally
            {
                metadataPackageManager.Dispose();

            }
        }

        #region create

        public static ComplexMappingModel CreateComplexMappingModel(Mapping mapping)
        {
            LinkElementModel sourceModel = CreateLinkElementModel(mapping.Source, LinkElementPostion.Source);
            LinkElementModel targetModel = CreateLinkElementModel(mapping.Target, LinkElementPostion.Target);

            sourceModel = LoadChildren(sourceModel);
            targetModel = LoadChildren(targetModel);

            long parentId = 0;
            if (mapping.Parent != null)
                parentId = mapping.Parent.Id;

            return new ComplexMappingModel()
            {
                Id = mapping.Id,
                ParentId = parentId,
                Source = sourceModel,
                Target = targetModel

            };
        }

        public static SimpleMappingModel CreateSimpleMappingModel(Mapping mapping, LinkElement source, LinkElement target)
        {
            LinkElementModel sourceModel = CreateLinkElementModel(source, LinkElementPostion.Source);
            LinkElementModel targetModel = CreateLinkElementModel(target, LinkElementPostion.Target);

            TransformationRuleModel transformationRuleModel = null;


            if (mapping.TransformationRule != null)
            {
                transformationRuleModel = new TransformationRuleModel();
                transformationRuleModel.Id = mapping.TransformationRule.Id;
                transformationRuleModel.RegEx = mapping.TransformationRule.RegEx;
                transformationRuleModel.Mask = mapping.TransformationRule.Mask;
            }
            else
            {
                transformationRuleModel = new TransformationRuleModel();
            }

            long parentId = 0;
            if (mapping.Parent != null)
                parentId = mapping.Parent.Id;


            //ToDo Load Rules
            return new SimpleMappingModel()
            {
                Id = mapping.Id,
                Source = sourceModel,
                Target = targetModel,
                TransformationRule = transformationRuleModel,
                ParentId = parentId

            };
        }

        public static LinkElementModel CreateLinkElementModel(LinkElement le, LinkElementPostion position)
        {


            return new LinkElementModel()
            {
                Id = le.Id,
                ElementId = le.ElementId,
                XPath = le.XPath,
                Name = le.Name,
                Position = position,
                Type = le.Type,
                Complexity = le.Complexity

            };

        }

        public static LinkElement CreateLinkElement(LinkElementModel model)
        {
            MappingManager mappingManager = new MappingManager();
            try
            {

                Debug.WriteLine("CreateLinkElement");
                Debug.WriteLine(model.ElementId);
                Debug.WriteLine(model.Type);
                Debug.WriteLine(model.Name);

                if (model.Parent != null)
                {
                    Debug.WriteLine("Parent");
                    Debug.WriteLine(model.Parent.ElementId);
                    Debug.WriteLine(model.Parent.Type);
                    Debug.WriteLine(model.Parent.Name);
                    Debug.WriteLine("------------------");
                }

                return mappingManager.CreateLinkElement(
                            model.ElementId,
                            model.Type,
                            model.Complexity,
                            model.Name,
                            model.XPath
                            );
            }
            finally
            {
                mappingManager.Dispose();
            }
        }

        public static LinkElement CreateLinkElement(LinkElementModel model, long parentId)
        {
            MappingManager mappingManager = new MappingManager();
            try
            {

                return mappingManager.CreateLinkElement(
                        model.ElementId,
                        model.Type,
                        model.Complexity,
                        model.Name,
                        model.XPath,
                        false
                        );
            }
            finally
            {
                mappingManager.Dispose();
            }
        }

        public static LinkElement CreateIfNotExistLinkElement(LinkElementModel leModel, MappingManager mappingManager)
        {

            if (ExistLinkElement(leModel))
            {
                return mappingManager.LinkElementRepo.Get()
                    .FirstOrDefault(le => le.ElementId.Equals(leModel.ElementId) && le.Type.Equals(leModel.Type));
            }
            else
            {
                return CreateLinkElement(leModel);
            }
        }

        public static LinkElement CreateIfNotExistLinkElement(LinkElementModel leModel, long parentId, MappingManager mappingManager)
        {
            if (ExistLinkElement(leModel))
            {
                return mappingManager.LinkElementRepo.Get()
                    .FirstOrDefault(le =>
                    le.ElementId.Equals(leModel.ElementId) &&
                    le.Type.Equals(leModel.Type) &&
                    le.Complexity.Equals(leModel.Complexity)
                    //le.Parent.Id.Equals(parentId)
                    );
            }
            else
            {
                return CreateLinkElement(leModel, parentId);
            }
        }

        public static Mapping CreateIfNotExistMapping(LinkElement source, LinkElement target, long level, TransformationRule rule, Mapping parent, MappingManager mappingManager)
        {
            object tmp = new object();
            IEnumerable<Mapping> mappings = tmp.GetUnitOfWork().GetReadOnlyRepository<Mapping>().Get();

            Mapping mapping = null;

            if (parent != null)
            {
                mapping = mappings.FirstOrDefault(
                    m => m.Parent != null && m.Parent.Id.Equals(parent.Id)
                         && m.Source.Id.Equals(source.Id)
                         && m.Source.Type.Equals(source.Type)
                         && m.Target.Id.Equals(target.Id)
                         && m.Target.Type.Equals(target.Type)
                         && m.Level.Equals(level));
            }
            else
            {
                mapping = mappings.FirstOrDefault(
                    m => m.Parent == null
                         && m.Source.Id.Equals(source.Id)
                         && m.Source.Type.Equals(source.Type)
                         && m.Target.Id.Equals(target.Id)
                         && m.Target.Type.Equals(target.Type)
                         && m.Level.Equals(level));
            }

            if (mapping == null)
            {
                if (rule != null && rule.Id == 0 && rule.RegEx != null)
                {
                    rule = mappingManager.CreateTransformationRule(rule.RegEx, rule.Mask);
                }

                mapping = mappingManager.CreateMapping(source, target, level, rule, parent);
            }
            else
            {
                if (rule != null)
                {
                    rule = mappingManager.UpdateTransformationRule(rule.Id, rule.RegEx, rule.Mask);

                    mapping.TransformationRule = rule;
                    mappingManager.UpdateMapping(mapping);
                }
            }


            return mapping;

        }

        #endregion

        #region delete

        public static bool UpdateSimpleMappings(long sourceId, long targetId, List<SimpleMappingModel> newListOfSimpleMappings, Mapping parent, MappingManager mappingManager)
        {

            List<Mapping> mappingsInDatabase = mappingManager.MappingRepo.Get()
                    .Where(m => m.Parent != null && m.Parent.Id.Equals(parent.Id)).ToList();

            List<long> deleteMappings = new List<long>();

            //delete all mappings that are not in the list
            foreach (var mapping in mappingsInDatabase)
            {
                bool exist = false;

                foreach (var newMapping in newListOfSimpleMappings)
                {
                    if (mapping.Source.ElementId.Equals(newMapping.Source.ElementId) &&
                    mapping.Target.ElementId.Equals(newMapping.Target.ElementId))
                    {
                        exist = true;
                    }
                }

                if (!exist)
                    deleteMappings.Add(mapping.Id);

            }

            foreach (var id in deleteMappings)
            {
                DeleteMapping(id, mappingManager, false);
            }


            //Create simple mappings
            //all mappings with the same  source or target should

            List<LinkElementModel> createdLinkELementModels = new List<LinkElementModel>();

            foreach (var sm in newListOfSimpleMappings)
            {

                LinkElement simpleMappingSource = null;
                LinkElement simpleMappingTarget = null;

                //if its not a new parent mapping or its in the list, please select existing
                simpleMappingSource = MappingHelper.CreateIfNotExistLinkElement(sm.Source, sourceId, mappingManager);


                //if its not a new parent mapping or its in the list, please select existing
                ////if (ExistLinkElementModel(sm.Target, createdLinkELementModels))

                simpleMappingTarget = MappingHelper.CreateIfNotExistLinkElement(sm.Target, targetId, mappingManager);


                //if (sm.TransformationRule. != null)
                //    simpleMappingTarget = mappingManager.UpdateLinkElement(simpleMappingTarget.Id);


                TransformationRule transformationRule = new TransformationRule(sm.TransformationRule.Id, sm.TransformationRule.RegEx, sm.TransformationRule.Mask);


                Mapping simplemapping = MappingHelper.CreateIfNotExistMapping(simpleMappingSource, simpleMappingTarget, 2, null, parent, mappingManager);

                if (transformationRule != null)
                {
                    transformationRule = mappingManager.UpdateTransformationRule(transformationRule.Id, transformationRule.RegEx, transformationRule.Mask);

                    simplemapping.TransformationRule = transformationRule;
                    mappingManager.UpdateMapping(simplemapping);
                }

                sm.Id = simplemapping.Id;

            }

            return false;
        }

        public static bool DeleteMapping(long id, MappingManager mappingManager, bool recursive = true)
        {
            Mapping mapping = mappingManager.GetMappings(id);

            if (recursive)
            {
                IEnumerable<Mapping> childMappings = mappingManager.GetChildMapping(id);

                foreach (var cm in childMappings)
                {
                    mappingManager.DeleteMapping(cm.Id);
                }
            }
            mappingManager.DeleteMapping(mapping.Id);


            return true;

        }


        #endregion

        #region helper

        public static bool ExistLinkElementModel(LinkElementModel leModel, List<LinkElementModel> leModels)
        {

            if (leModels.Any(le => le.ElementId.Equals(leModel.ElementId) &&
                le.Type.Equals(leModel.Type) &&
                le.Complexity.Equals(leModel.Complexity)))
            {
                return true;
            }

            return false;
        }

        public static bool ExistLinkElement(LinkElementModel leModel)
        {
            MappingManager mappingManager = new MappingManager();

            try
            {
                if (mappingManager.LinkElementRepo.Get()
                    .Any(le => le.ElementId.Equals(leModel.ElementId) &&
                    le.Type.Equals(leModel.Type) &&
                    le.Complexity.Equals(leModel.Complexity)))
                {
                    return true;
                }

                return false;
            }
            finally
            {
                mappingManager.Dispose();
            }
        }

        public static bool IsSimpleElement(long id)
        {
            MappingManager mappingManager = new MappingManager();


            try
            {
                LinkElement le = mappingManager.GetLinkElement(id);

                if (le.Complexity == LinkElementComplexity.Simple) return true;

                return false;
            }
            finally
            {
                mappingManager.Dispose();
            }
        }


        public static long GetId(long elementId, LinkElementType type, MappingManager mappingManager)
        {

            long linkElementId = 0;

            LinkElement linkElement =
                mappingManager.LinkElementRepo.Get()
                    .FirstOrDefault(le => le.ElementId.Equals(elementId) && le.Type.Equals(type));

            if (linkElement != null)
            {
                linkElementId = linkElement.Id;
            }

            return linkElementId;
        }

        //public static string GetMask(long elementId, LinkElementType type)
        //{
        //    MappingManager mappingManager = new MappingManager();

        //    string mask = "";

        //    LinkElement linkElement =
        //        mappingManager.LinkElementRepo.Get()
        //            .FirstOrDefault(le => le.ElementId.Equals(elementId) && le.Type.Equals(type));

        //    if (linkElement != null)
        //    {
        //        mask = linkElement.Mask;
        //    }

        //    return mask;
        //}

        ///// <summary>
        ///// if parent is complex send back al children ids
        ///// else send back the parent id
        ///// </summary>
        ///// <returns></returns>
        //public static List<long> GetChildrenIds(LinkElement parent)
        //{

        //    List<long> childrenIds = new List<long>();

        //    if (parent.Complexity.Equals(LinkElementComplexity.Simple))
        //        childrenIds.Add(parent.Id);

        //    else
        //    {
        //        MappingManager mappingManager = new MappingManager();
        //        childrenIds = mappingManager.LinkElementRepo.Get()
        //                   .Where(le => le.Parent != null && le.Parent.Id.Equals(parent.Id))
        //                   .Select(le => le.Id)
        //                   .ToList();
        //    }

        //    return childrenIds;
        //}
    }
    #endregion

}
