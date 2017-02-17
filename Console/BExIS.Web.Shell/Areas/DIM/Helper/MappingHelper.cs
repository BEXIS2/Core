using BExIS.Dcm.CreateDatasetWizard;
using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dlm.Services.Party;
using BExIS.Web.Shell.Areas.DIM.Models.Mapping;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace BExIS.Web.Shell.Areas.DIM.Helper
{
    public class MappingHelper
    {
        #region load Model from metadataStructure

        public static LinkElementRootModel LoadFromMetadataStructure(long id, LinkElementPostion rootModelType)
        {

            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            MetadataStructure metadataStructure = metadataStructureManager.Repo.Get(id);

            LinkElementRootModel model = new LinkElementRootModel(LinkElementType.MetadataStructure, id, metadataStructure.Name, rootModelType);

            if (metadataStructure != null)
            {
                foreach (var pUsage in metadataStructure.MetadataPackageUsages)
                {
                    addUsageAsLinkElement(pUsage, "Metadata", model);
                }

                model = CreateLinkElementContainerModels(model);
            }
            return model;
        }

        public static void addUsageAsLinkElement(BaseUsage usage, string parentXpath, LinkElementRootModel rootModel)
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
            LinkElement linkElement =
                mappingManager.LinkElementRepo.Get()
                    .FirstOrDefault(le => le.ElementId.Equals(usage.Id) && le.Type.Equals(type));

            if (linkElement != null)
            {
                linkElementId = linkElement.Id;
            }


            LinkElementModel LEModel = new LinkElementModel(
                linkElementId,
                usage.Id,
                type, usage.Label, xPath, rootModel.Position, complexity, usage.Description);
            rootModel.LinkElements.Add(LEModel);

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
                }

                LEModel = new LinkElementModel(
                    linkElementId,
                    typeId,
                    LinkElementType.ComplexMetadataAttribute,
                    typeName,
                    "",
                    rootModel.Position,
                    complexity,
                    typeDescription);

                if (!rootModel.LinkElements.Any(le => le.ElementId.Equals(typeId) &&
                                                      le.Type.Equals(LinkElementType.ComplexMetadataAttribute)))
                {
                    rootModel.LinkElements.Add(LEModel);
                }
            }


            Debug.WriteLine("1: " + LEModel.Name + " " + LEModel.Type);

            //check childrens
            List<BaseUsage> childrenUsages = UsageHelper.GetChildren(usage);
            if (childrenUsages.Count > 0)
            {
                foreach (BaseUsage childUsage in childrenUsages)
                {
                    addUsageAsLinkElement(childUsage, xPath, rootModel);
                }

                //AddChildrens
                //addLinkElementsFromChildrens(usage, xPath, rootModel);
            }
        }

        public static void addLinkElementsFromChildrens(BaseUsage usage, string parentXpath, LinkElementRootModel rootModel)
        {

            List<BaseUsage> childrenUsages = UsageHelper.GetChildren(usage);
            MappingManager mappingManager = new MappingManager();

            if (childrenUsages.Count > 0)
            {
                foreach (BaseUsage childUsage in childrenUsages)
                {
                    //
                    string xPath = parentXpath + "/" + childUsage.Label.Replace(" ", string.Empty);

                    //bool complex = false;

                    //string attrName = "";
                    //long id = 0;

                    //attrName = UsageHelper.GetNameOfType(childUsage);
                    //id = UsageHelper.GetIdOfType(childUsage);

                    //LinkElementModel LEModel;

                    //if (!UsageHelper.IsSimple(childUsage))
                    //{
                    //    long linkElementId = 0;
                    //    LinkElement linkElement =
                    //        mappingManager.LinkElementRepo.Get()
                    //            .FirstOrDefault(le => le.ElementId.Equals(id) && le.Type.Equals(LinkElementType.ComplexMetadatAttribute));

                    //    if (linkElement != null)
                    //    {
                    //        linkElementId = linkElement.Id;
                    //    }

                    //    //Create Link Element complex
                    //    LEModel = new LinkElementModel(
                    //        linkElementId,
                    //        id,
                    //        LinkElementType.ComplexMetadatAttribute, attrName, "", rootModel.Position, childUsage.Description);

                    //    if (!rootModel.LinkElements.Any(m => m.Name.Equals(attrName)))
                    //        rootModel.LinkElements.Add(LEModel);


                    //    Debug.WriteLine("1: " + LEModel.Name + " " + LEModel.Type);

                    //}
                    //else
                    //{
                    //    // create simple element and usage
                    //    LEModel = new LinkElementModel(
                    //        0,
                    //        id,
                    //        LinkElementModelType.SimpleMetadataAttribute, attrName, xPath, rootModel.Position, childUsage.Description);

                    //}



                    addUsageAsLinkElement(childUsage, xPath, rootModel);

                }
            }




        }

        #endregion

        #region Load Model From System

        public static LinkElementRootModel LoadfromSystem(LinkElementPostion rootModelType)
        {
            LinkElementRootModel model = new LinkElementRootModel(LinkElementType.System, 0, "System", rootModelType);

            //get all parties - complex
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            IEnumerable<PartyType> partyTypes = partyTypeManager.Repo.Get();

            foreach (var pt in partyTypes)
            {
                model.LinkElements.Add(createLinkElementModelType(pt, model));
            }


            //get all keys -> simple


            foreach (Key value in Key.GetValues(typeof(Key)))
            {
                LinkElementModel LEModel = new LinkElementModel(
                        0,
                        0,
                        LinkElementType.Key, value.ToString(), "", model.Position, LinkElementComplexity.Simple, "");

                model.LinkElements.Add(LEModel);

            }

            //create container
            model = CreateLinkElementContainerModels(model);

            return model;
        }

        private static LinkElementModel createLinkElementModelType(PartyType partyType, LinkElementRootModel rootModel)
        {
            MappingManager mappingManager = new MappingManager();

            long linkElementId = 0;

            LinkElement linkElement =
                mappingManager.LinkElementRepo.Get()
                    .FirstOrDefault(le => le.ElementId.Equals(partyType.Id) && le.Type.Equals(LinkElementType.PartyType));

            if (linkElement != null)
            {
                linkElementId = linkElement.Id;
            }

            LinkElementModel LEModel = new LinkElementModel(
                        linkElementId,
                        partyType.Id,
                        LinkElementType.PartyType, partyType.Title, "", rootModel.Position, LinkElementComplexity.Complex, partyType.Description);

            return LEModel;
        }

        #endregion

        #region loadMapping

        public static List<MappingModel> LoadMappings(
            long sourceElementId, LinkElementType sourceType,
            List<long> sourceLinkElements,
            long targetElementId, LinkElementType targetType,
            List<long> targetLinkElements)
        {

            List<MappingModel> tmp = new List<MappingModel>();
            MappingManager mappingManager = new MappingManager();

            List<LinkElement> existingSourceLinkElements =
                mappingManager.LinkElementRepo.Get().Where(le => sourceLinkElements.Contains(le.Id)).ToList();
            List<LinkElement> existingTargetLinkElements =
                mappingManager.LinkElementRepo.Get().Where(le => targetLinkElements.Contains(le.Id)).ToList();

            List<Mapping> mappings = mappingManager.MappingRepo.Get().Where(
                m => sourceLinkElements.Contains(m.Source.Id) &&
                     targetLinkElements.Contains(m.Target.Id)).ToList();

            foreach (var mapping in mappings)
            {
                LinkElement source = existingSourceLinkElements.FirstOrDefault(le => le.Id.Equals(mapping.Source.Id));
                LinkElement target = existingTargetLinkElements.FirstOrDefault(le => le.Id.Equals(mapping.Target.Id));



                tmp.Add(CreateMappingModel(mapping, source, target));

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
                        LinkElementType.PartyCustomType, attr.Name, "", model.Position, LinkElementComplexity.Simple, attr.Description)
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
                complexity = attr.Member.Self is MetadataSimpleAttribute
                    ? LinkElementComplexity.Simple
                    : LinkElementComplexity.Complex;

                model.Children.Add(
                        new LinkElementModel(
                            0,
                            attr.Id,
                            LinkElementType.PartyCustomType, attr.Label, "", model.Position, complexity, attr.Description)
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

        public static MappingModel CreateMappingModel(Mapping mapping, LinkElement source, LinkElement target)
        {
            LinkElementModel sourceModel = CreateLinkElementModel(source, LinkElementPostion.Source);
            LinkElementModel targetModel = CreateLinkElementModel(target, LinkElementPostion.Target);

            sourceModel = LoadChildren(sourceModel);
            targetModel = LoadChildren(targetModel);

            return new MappingModel()
            {
                Id = mapping.Id,
                Source = sourceModel,
                Target = targetModel
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
                Type = le.Type
            };

        }

        public static LinkElement CreateLinkElement(LinkElementModel model)
        {
            MappingManager mappingManager = new MappingManager();

            return mappingManager.CreateLinkElement(
                        model.ElementId,
                        model.Type,
                        model.Name,
                        model.XPath
                        );
        }

        public static LinkElement CreateLinkElement(LinkElementModel model, long parentId)
        {
            MappingManager mappingManager = new MappingManager();

            return mappingManager.CreateLinkElement(
                        model.ElementId,
                        model.Type,
                        model.Name,
                        model.XPath,
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

        public static Mapping CreateIfNotExistMapping(LinkElement source, LinkElement target, TransformationRule rule)
        {
            MappingManager mappingManager = new MappingManager();

            Mapping mapping = mappingManager.MappingRepo.Get().FirstOrDefault(
                m => m.Source.Id.Equals(source.Id) &&
                     m.Target.Id.Equals(target.Id));
            if (mapping == null)
                mapping = mappingManager.CreateMapping(source, target, null);

            return mapping;
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

        #endregion

    }
}