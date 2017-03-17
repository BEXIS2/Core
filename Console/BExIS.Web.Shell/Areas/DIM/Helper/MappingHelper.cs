using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Dim.UI.Models.Mapping;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BExIS.Modules.Dim.UI.Helper
{
    public class MappingHelper
    {
        #region load rootList

        public static List<LinkElementRootListItem> LoadSelectionList()
        {
            List<LinkElementRootListItem> tmp = new List<LinkElementRootListItem>();

            //load System
            LinkElementRootListItem li = new LinkElementRootListItem(0, "System", LinkElementType.System);


            tmp.Add(li);

            //load Metadata Strutcure
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
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

        #endregion


        #region load Model from metadataStructure

        public static LinkElementRootModel LoadFromMetadataStructure(long id, LinkElementPostion rootModelType)
        {

            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            MetadataStructure metadataStructure = metadataStructureManager.Repo.Get(id);

            LinkElementRootModel model = new LinkElementRootModel(LinkElementType.MetadataStructure, id, metadataStructure.Name, rootModelType);

            if (metadataStructure != null)
            {

                MappingManager mappingManager = new MappingManager();
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
                    LinkElementComplexity.Complex, "",
                    metadataStructure.Description);


                foreach (var pUsage in metadataStructure.MetadataPackageUsages)
                {
                    addUsageAsLinkElement(pUsage, "Metadata", model, LEModel);
                }

                model = CreateLinkElementContainerModels(model);
            }
            return model;
        }

        public static void addUsageAsLinkElement(BaseUsage usage, string parentXpath, LinkElementRootModel rootModel, LinkElementModel parent)
        {
            int min = usage.MinCardinality;
            string childName = "";



            MappingManager mappingManager = new MappingManager();


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

            }


            // add usage
            xPath = parentXpath + "/" + usageName.Replace(" ", string.Empty) + "/" + typeName;

            long linkElementId = 0;
            string mask = "";

            LinkElement linkElement =
                mappingManager.LinkElementRepo.Get()
                    .FirstOrDefault(le => le.ElementId.Equals(usage.Id) && le.Type.Equals(type));

            if (linkElement != null)
            {
                linkElementId = linkElement.Id;
                mask = linkElement.Mask;
            }


            LinkElementModel LEModel = new LinkElementModel(
                linkElementId,
                usage.Id,
                type, usage.Label, xPath, rootModel.Position, complexity, mask, usage.Description);
            rootModel.LinkElements.Add(LEModel);

            LEModel.Parent = parent;

            //add type
            if (addTypeAsLinkElement)
            {
                linkElementId = 0;

                linkElement =
                    mappingManager.LinkElementRepo.Get()
                        .FirstOrDefault(le => le.ElementId.Equals(typeId) && le.Type.Equals(LinkElementType.ComplexMetadataAttribute));

                if (linkElement != null)
                {
                    linkElementId = linkElement.Id;
                    mask = linkElement.Mask;
                }

                LEModel = new LinkElementModel(
                    linkElementId,
                    typeId,
                    LinkElementType.ComplexMetadataAttribute,
                    typeName,
                    "",
                    rootModel.Position,
                    complexity,
                    mask,
                    typeDescription);

                LEModel.Parent = parent;


                if (!rootModel.LinkElements.Any(le => le.ElementId.Equals(typeId) &&
                                                      le.Type.Equals(LinkElementType.ComplexMetadataAttribute)))
                {
                    rootModel.LinkElements.Add(LEModel);
                }
            }


            //Debug.WriteLine("1: " + LEModel.Name + " " + LEModel.Type);

            //check childrens
            // this line calls to the DCM module. JAVAD.
            //List<BaseUsage> childrenUsages = UsageHelper.GetChildren(usage);
            // please remove the following line, after you fixed the call to DCM
            List<BaseUsage> childrenUsages = new List<BaseUsage>();
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

        public static LinkElementRootModel LoadfromSystem(LinkElementPostion rootModelType)
        {
            LinkElementRootModel model = new LinkElementRootModel(LinkElementType.System, 0, "System", rootModelType);

            MappingManager mappingManager = new MappingManager();
            LinkElement SystemRoot = mappingManager.GetLinkElement(0,
                    LinkElementType.System);

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

            //get all parties - complex
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            IEnumerable<PartyType> partyTypes = partyTypeManager.Repo.Get();

            foreach (var pt in partyTypes)
            {
                LinkElementModel ptModel = createLinkElementModelType(pt, model, LEParent);
                model.LinkElements.Add(ptModel);
                //get all partyCustomTypeAttr -> simple
                model.LinkElements.AddRange(createLinkElementModelPartyCustomType(pt, model, ptModel));
            }

            //get all keys -> simple
            foreach (Key value in Key.GetValues(typeof(Key)))
            {
                long linkElementId = GetId((int)value, LinkElementType.Key);
                string mask = GetMask((int)value, LinkElementType.Key);

                LinkElementModel LEModel = new LinkElementModel(
                        linkElementId,
                        (int)value,
                        LinkElementType.Key, value.ToString(), "", model.Position, LinkElementComplexity.Simple, mask, "");

                LEModel.Parent = LEParent;

                model.LinkElements.Add(LEModel);

            }

            //create container
            model = CreateLinkElementContainerModels(model);

            return model;
        }

        private static LinkElementModel createLinkElementModelType(
            PartyType partyType,
            LinkElementRootModel rootModel,
            LinkElementModel parent)
        {
            MappingManager mappingManager = new MappingManager();

            long linkElementId = GetId(partyType.Id, LinkElementType.PartyType);
            string mask = GetMask(partyType.Id, LinkElementType.PartyType);

            LinkElementModel LEModel = new LinkElementModel(
                        linkElementId,
                        partyType.Id,
                        LinkElementType.PartyType, partyType.Title, "", rootModel.Position, LinkElementComplexity.Complex, mask, partyType.Description);

            LEModel.Parent = parent;

            return LEModel;
        }

        private static List<LinkElementModel> createLinkElementModelPartyCustomType(PartyType partyType, LinkElementRootModel rootModel, LinkElementModel parent)
        {
            List<LinkElementModel> tmp = new List<LinkElementModel>();
            MappingManager mappingManager = new MappingManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();


            foreach (var partyCustomType in partyType.CustomAttributes)
            {
                long linkElementId = GetId(partyCustomType.Id, LinkElementType.PartyCustomType);
                string mask = GetMask(partyCustomType.Id, LinkElementType.PartyCustomType);

                LinkElementModel LEModel = new LinkElementModel(
                            linkElementId,
                            partyCustomType.Id,
                            LinkElementType.PartyCustomType, partyCustomType.Name, partyType.Title + "/" + partyCustomType.Name, rootModel.Position, LinkElementComplexity.Simple, mask, partyCustomType.Description);
                LEModel.Parent = parent;

                tmp.Add(LEModel);
            }


            return tmp;
        }


        #endregion

        #region loadMapping

        public static List<ComplexMappingModel> LoadMappings(
            long sourceElementId, LinkElementType sourceType,
            List<long> sourceLinkElements,
            long targetElementId, LinkElementType targetType,
            List<long> targetLinkElements)
        {

            List<ComplexMappingModel> tmp = new List<ComplexMappingModel>();
            MappingManager mappingManager = new MappingManager();

            List<LinkElement> existingSourceLinkElements =
                mappingManager.LinkElementRepo.Get().Where(le => sourceLinkElements.Contains(le.Id)).ToList();
            List<LinkElement> existingTargetLinkElements =
                mappingManager.LinkElementRepo.Get().Where(le => targetLinkElements.Contains(le.Id)).ToList();

            List<Mapping> mappings = mappingManager.MappingRepo.Get().Where(
                m => sourceLinkElements.Contains(m.Source.Id) &&
                     targetLinkElements.Contains(m.Target.Id)
                     && m.Level.Equals(1)).ToList();

            foreach (var mapping in mappings)
            {
                LinkElement source = existingSourceLinkElements.FirstOrDefault(le => le.Id.Equals(mapping.Source.Id));
                LinkElement target = existingTargetLinkElements.FirstOrDefault(le => le.Id.Equals(mapping.Target.Id));

                ComplexMappingModel model = CreateComplexMappingModel(mapping, source, target);

                if (source != null && target != null)
                {
                    //load childrens ids
                    List<long> sourceChildrenIds = GetChildrenIds(source);

                    List<long> targetChildrenIds = GetChildrenIds(target);

                    List<Mapping> childMappings = mappingManager.MappingRepo.Get().Where(
                        m => sourceChildrenIds.Contains(m.Source.Id) &&
                             targetChildrenIds.Contains(m.Target.Id) && m.Level.Equals(2)).ToList();

                    foreach (var cm in childMappings)
                    {
                        //ToDO Add transformation rule
                        model.SimpleMappings.Add(CreateSimpleMappingModel(cm, cm.Source, cm.Target));
                    }
                }


                tmp.Add(model);
            }




            return tmp;
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
                        model.Children = getChildrenFromMetadataNestedUsage(model);
                        break;
                    }
            }

            return model;
        }

        private static List<LinkElementModel> getChildrenFromPartyType(LinkElementModel model)
        {
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            IEnumerable<PartyCustomAttribute> ptAttr = partyTypeManager.RepoPartyCustomAttribute.Get().Where(p => p.PartyType.Id.Equals(model.ElementId));

            foreach (var attr in ptAttr)
            {
                model.Children.Add(
                    new LinkElementModel(
                        0,
                        attr.Id,
                        LinkElementType.PartyCustomType, attr.Name, "", model.Position, LinkElementComplexity.Simple, "", attr.Description)
                    );
            }

            return model.Children;
        }

        private static List<LinkElementModel> getChildrenFromComplexMetadataAttribute(LinkElementModel model)
        {
            MetadataAttributeManager metadataAttributeManager = new MetadataAttributeManager();
            MetadataCompoundAttribute mca = metadataAttributeManager.MetadataCompoundAttributeRepo.Get(model.ElementId);

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


                model.Children.Add(
                        new LinkElementModel(
                            0,
                            attr.Id,
                            type, attr.Label, "", model.Position, complexity, "", attr.Description)
                        );
            }

            return model.Children;
        }

        private static List<LinkElementModel> getChildrenFromMetadataNestedUsage(LinkElementModel model)
        {
            //MetadataStructureManager msm = new MetadataStructureManager();
            //Metadata

            //MetadataAttributeManager metadataAttributeManager = new MetadataAttributeManager();
            //MetadataCompoundAttribute mca = metadataAttributeManager .M.Get(model.ElementId);

            //foreach (var attr in mca.MetadataNestedAttributeUsages)
            //{
            //    model.Children.Add(
            //        new LinkElementModel(
            //            0,
            //            attr.Id,
            //            LinkElementType.PartyCustomType, attr.Label, "", model.Position, attr.Description)
            //        );
            //}

            return model.Children;
        }

        #region create

        public static ComplexMappingModel CreateComplexMappingModel(Mapping mapping, LinkElement source, LinkElement target)
        {
            LinkElementModel sourceModel = CreateLinkElementModel(source, LinkElementPostion.Source);
            LinkElementModel targetModel = CreateLinkElementModel(target, LinkElementPostion.Target);

            sourceModel = LoadChildren(sourceModel);
            targetModel = LoadChildren(targetModel);

            return new ComplexMappingModel()
            {
                Id = mapping.Id,
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
            }
            else
            {
                transformationRuleModel = new TransformationRuleModel();
            }
            //ToDo Load Rules

            return new SimpleMappingModel()
            {
                Id = mapping.Id,
                Source = sourceModel,
                Target = targetModel,
                TransformationRule = transformationRuleModel
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
                Complexity = le.Complexity,
                Mask = le.Mask

            };

        }

        public static LinkElement CreateLinkElement(LinkElementModel model)
        {
            MappingManager mappingManager = new MappingManager();


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
                        model.XPath,
                        model.Mask
                        );
        }

        public static LinkElement CreateLinkElement(LinkElementModel model, long parentId)
        {
            MappingManager mappingManager = new MappingManager();

            return mappingManager.CreateLinkElement(
                        model.ElementId,
                        model.Type,
                        model.Complexity,
                        model.Name,
                        model.XPath,
                        model.Mask,
                        false,
                        parentId
                        );
        }

        public static LinkElement CreateIfNotExistLinkElement(LinkElementModel leModel)
        {
            MappingManager mappingManager = new MappingManager();


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

        public static LinkElement CreateIfNotExistLinkElement(LinkElementModel leModel, long parentId)
        {
            MappingManager mappingManager = new MappingManager();


            if (ExistLinkElement(leModel))
            {
                return mappingManager.LinkElementRepo.Get()
                    .FirstOrDefault(le => le.ElementId.Equals(leModel.ElementId) && le.Type.Equals(leModel.Type));
            }
            else
            {
                return CreateLinkElement(leModel, parentId);
            }

        }

        public static Mapping CreateIfNotExistMapping(LinkElement source, LinkElement target, long level, TransformationRule rule)
        {
            MappingManager mappingManager = new MappingManager();

            Mapping mapping = mappingManager.MappingRepo.Get().FirstOrDefault(
                m => m.Source.Id.Equals(source.Id) &&
                     m.Target.Id.Equals(target.Id) &&
                     m.Level.Equals(level));
            if (mapping == null)
            {
                if (rule != null && rule.Id == 0 && rule.RegEx != null)
                {
                    rule = mappingManager.CreateTransformationRule(rule.RegEx);
                }

                mapping = mappingManager.CreateMapping(source, target, level, rule);
            }
            else
            {
                if (rule != null)
                {
                    rule = mappingManager.UpdateTransformationRule(rule.Id, rule.RegEx);

                    mapping.TransformationRule = rule;
                    mappingManager.UpdateMapping(mapping);
                }
            }


            return mapping;
        }

        #endregion

        #region delete

        public static bool UpdateSimpleMappings(long sourceId, long targetId, List<SimpleMappingModel> newListOfSimpleMappings)
        {

            MappingManager mappingManager = new MappingManager();

            List<Mapping> mappingsInDatabase = mappingManager.MappingRepo.Get()
                    .Where(m => m.Source.Parent != null &&
                    m.Source.Parent.Id.Equals(sourceId) &&
                    m.Target.Parent != null &&
                    m.Target.Parent.Id.Equals(targetId)).ToList();

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
                DeleteMapping(id, false);
            }


            //Create simple mappings
            foreach (var sm in newListOfSimpleMappings)
            {
                LinkElement simpleMappingSource = MappingHelper.CreateIfNotExistLinkElement(sm.Source, sourceId);
                LinkElement simpleMappingTarget = MappingHelper.CreateIfNotExistLinkElement(sm.Target, targetId);

                if (sm.Target.Mask != null)
                    simpleMappingTarget = mappingManager.UpdateLinkElement(simpleMappingTarget.Id, sm.Target.Mask);


                TransformationRule transformationRule = new TransformationRule(sm.TransformationRule.Id, sm.TransformationRule.RegEx);


                Mapping simplemapping = MappingHelper.CreateIfNotExistMapping(simpleMappingSource, simpleMappingTarget, 2, null);

                if (transformationRule != null)
                {
                    transformationRule = mappingManager.UpdateTransformationRule(transformationRule.Id, transformationRule.RegEx);

                    simplemapping.TransformationRule = transformationRule;
                    mappingManager.UpdateMapping(simplemapping);
                }

                sm.Id = simplemapping.Id;

            }

            return false;
        }


        public static bool DeleteMapping(long id, bool recursive = true)
        {

            MappingManager mappingManager = new MappingManager();
            Mapping mapping = mappingManager.GetMappings(id);

            if (recursive)
            {
                List<long> sourceChildIds =
                    mappingManager.LinkElementRepo.Get()
                        .Where(le => le.Parent != null && le.Parent.Id.Equals(mapping.Source.Id))
                        .Select(le => le.Id).ToList();

                List<long> targetChildIds =
                    mappingManager.LinkElementRepo.Get()
                        .Where(le => le.Parent != null && le.Parent.Id.Equals(mapping.Target.Id))
                        .Select(le => le.Id).ToList();

                List<Mapping> childMappings = mappingManager.MappingRepo.Get()
                    .Where(m => sourceChildIds.Contains(m.Source.Id)
                                && targetChildIds.Contains(m.Target.Id)).ToList();

                foreach (var cm in childMappings)
                {
                    mappingManager.DeleteMapping(cm);
                }
            }
            mappingManager.DeleteMapping(mapping);


            return true;
        }


        #endregion

        #region helper

        public static bool ExistLinkElement(LinkElementModel leModel)
        {
            MappingManager mappingManager = new MappingManager();

            if (mappingManager.LinkElementRepo.Get()
                .Any(le => le.ElementId.Equals(leModel.ElementId) && le.Type.Equals(leModel.Type)))
            {
                return true;
            }

            return false;
        }

        public static bool IsSimpleElement(long id)
        {
            MappingManager mappingManager = new MappingManager();
            LinkElement le = mappingManager.GetLinkElement(id);

            if (le.Complexity == LinkElementComplexity.Simple) return true;

            return false;
        }


        public static long GetId(long elementId, LinkElementType type)
        {
            MappingManager mappingManager = new MappingManager();

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

        public static string GetMask(long elementId, LinkElementType type)
        {
            MappingManager mappingManager = new MappingManager();

            string mask = "";

            LinkElement linkElement =
                mappingManager.LinkElementRepo.Get()
                    .FirstOrDefault(le => le.ElementId.Equals(elementId) && le.Type.Equals(type));

            if (linkElement != null)
            {
                mask = linkElement.Mask;
            }

            return mask;
        }

        /// <summary>
        /// if parent is complex send back al children ids
        /// else send back the parent id
        /// </summary>
        /// <returns></returns>
        public static List<long> GetChildrenIds(LinkElement parent)
        {

            List<long> childrenIds = new List<long>();

            if (parent.Complexity.Equals(LinkElementComplexity.Simple))
                childrenIds.Add(parent.Id);

            else
            {
                MappingManager mappingManager = new MappingManager();
                childrenIds = mappingManager.LinkElementRepo.Get()
                           .Where(le => le.Parent != null && le.Parent.Id.Equals(parent.Id))
                           .Select(le => le.Id)
                           .ToList();
            }

            return childrenIds;
        }
    }
    #endregion

}
