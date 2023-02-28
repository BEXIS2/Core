using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Entities.Publication;
using BExIS.Dim.Helpers;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Modules.Dim.UI.Helper;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Vaiona.Persistence.Api;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dim.UI.Helpers
{
    public class DimSeedDataGenerator : IModuleSeedDataGenerator
    {
        public void GenerateSeedData()
        {
            SubmissionManager submissionManager = new SubmissionManager();
            FeatureManager featureManager = new FeatureManager();
            FeaturePermissionManager featurePermissionManager = new FeaturePermissionManager();
            OperationManager operationManager = new OperationManager();

            try
            {
                #region SECURITY

                //workflows = größere sachen, vielen operation
                //operations = einzelne actions

                //1.controller -> 1.Operation

                Feature DataDissemination =
                    featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Data Dissemination"));
                if (DataDissemination == null)
                    DataDissemination = featureManager.Create("Data Dissemination", "Data Dissemination");

                Feature Mapping = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Mapping"));
                if (Mapping == null) Mapping = featureManager.Create("Mapping", "Mapping", DataDissemination);

                Feature Submission = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Submission"));
                if (Submission == null) Submission = featureManager.Create("Submission", "Submission", DataDissemination);

                Feature API = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("API") && f.Parent.Equals(DataDissemination));
                if (API == null) API = featureManager.Create("API", "API", DataDissemination);

                //set api public
                featurePermissionManager.Create(null, API.Id, Security.Entities.Authorization.PermissionType.Grant);

                Operation operation = null;

                #region Help Workflow

                if (!operationManager.Exists("dim", "help", "*")) operationManager.Create("DIM", "Help", "*");

                #endregion Help Workflow

                #region Admin Workflow

                if (!operationManager.Exists("dim", "admin", "*")) operationManager.Create("DIM", "Admin", "*", DataDissemination);
                if (!operationManager.Exists("dim", "datacitedoi", "*")) operationManager.Create("DIM", "DataCiteDoi", "*", DataDissemination);
                if (!operationManager.Exists("dim", "mapping", "*")) operationManager.Create("DIM", "Mapping", "*", Mapping);

                #endregion Admin Workflow

                #region Mapping Workflow

                //ToDo add security after Refactoring DIM mapping workflow

                //workflow = new Workflow();
                //workflow.Name = "Mapping";
                //workflowManager.Create(workflow);

                //operation = operationManager.Create("Dim", "Admin", "*", null, workflow);
                //workflow.Operations.Add(operation);

                //Mapping.Workflows.Add(workflow);

                #endregion Mapping Workflow

                #region Submission Workflow

                //ToDo add security after Refactoring DIM Submission workflow

                //workflow = new Workflow();
                //workflow.Name = "Submission";
                //workflowManager.Create(workflow);

                //operation = operationManager.Create("Dim", "Admin", "*", null, workflow);
                //workflow.Operations.Add(operation);

                //Submission.Workflows.Add(workflow);

                #endregion Submission Workflow

                #region Export

                if (!operationManager.Exists("dim", "export", "*")) operationManager.Create("DIM", "export", "*");

                #endregion Export

                #region API

                if (!operationManager.Exists("api", "MetadataOut", "*")) operationManager.Create("API", "MetadataOut", "*", API);
                if (!operationManager.Exists("api", "MetadataStructureOut", "*")) operationManager.Create("API", "MetadataStructureOut", "*", API);
                if (!operationManager.Exists("api", "DataOut", "*")) operationManager.Create("api", "DataOut", "*", API);
                if (!operationManager.Exists("api", "DatasetOut", "*")) operationManager.Create("api", "DatasetOut", "*", API);
                if (!operationManager.Exists("api", "DataStatisticOut", "*")) operationManager.Create("api", "DataStatisticOut", "*", API);
                if (!operationManager.Exists("api", "DataQualityOut", "*")) operationManager.Create("api", "DataQualityOut", "*", API);
                if (!operationManager.Exists("api", "AttachmentOut", "*")) operationManager.Create("api", "AttachmentOut", "*", API);

                if (!operationManager.Exists("api", "Metadata", "*")) operationManager.Create("API", "Metadata", "*", API);
                if (!operationManager.Exists("api", "Data", "*")) operationManager.Create("api", "Data", "*", API);
                if (!operationManager.Exists("api", "Dataset", "*")) operationManager.Create("api", "Dataset", "*", API);
                if (!operationManager.Exists("api", "Attachment", "*")) operationManager.Create("api", "Attachment", "*", API);

                #endregion API

                #region public available

                if (!operationManager.Exists("dim", "submission", "*")) operationManager.Create("DIM", "Submission", "*");

                #endregion public available

                #endregion SECURITY

                #region EXPORT

                submissionManager.Load();

                createMetadataStructureRepoMaps();

                #endregion EXPORT

                #region MAPPING

                createMappings();

                createDOIMappingConcept();

                #endregion MAPPING
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                featureManager.Dispose();
                operationManager.Dispose();
                featurePermissionManager.Dispose();
            }

            //ImportPartyTypes();
        }

        private void createDOIMappingConcept()
        {
            using (var conceptManager = new ConceptManager())
            {
                // concept
                var concept = conceptManager.CreateMappingConcept("DOI", "The concept is needed to create a DIO via DataCite.", "https://schema.datacite.org/meta/kernel-4.4/");

                // keys
                var titles = conceptManager.CreateMappingKey("Titles", "", "", false, true,"data/titles/", concept);
                var title = conceptManager.CreateMappingKey("Title", "", "", false, false,"data/titles/title", concept, titles);
                var lang = conceptManager.CreateMappingKey("Lang", "", "", false, false,"data/titles/lang", concept, titles);


                var creator = conceptManager.CreateMappingKey("Creators", "", "www.google.de",false,true, "data/creators", concept);
                var firstname = conceptManager.CreateMappingKey("Firstname", "", "", false, false, "data/creators/firstname", concept,creator);
                var lastname = conceptManager.CreateMappingKey("Lastname", "", "", false, false, "data/creators/lastname", concept, creator);
            }
        }

        private void createMetadataStructureRepoMaps()
        {
            using (PublicationManager publicationManager = new PublicationManager())
            using (MetadataStructureManager metadataStructureManager = new MetadataStructureManager())
            {
                //set MetadataStructureToRepository for gbif and pensoft
                long metadataStrutcureId = 0;
                long repositoryId = 0;

                //get id of metadatstructure
                string metadatStrutcureName = "gbif";
                if (metadataStructureManager.Repo.Get().Any(m => m.Name.ToLower().Equals(metadatStrutcureName)))
                {
                    MetadataStructure metadataStructure =
                        metadataStructureManager.Repo.Get()
                            .FirstOrDefault(m => m.Name.ToLower().Equals(metadatStrutcureName));
                    if (metadataStructure != null)
                    {
                        metadataStrutcureId = metadataStructure.Id;
                    }
                }

                //get id of metadatstructure
                string repoName = "pensoft";
                if (publicationManager.RepositoryRepo.Get().Any(m => m.Name.ToLower().Equals(repoName)))
                {
                    Repository repository =
                        publicationManager.RepositoryRepo.Get().FirstOrDefault(m => m.Name.ToLower().Equals(repoName));
                    if (repository != null)
                    {
                        repositoryId = repository.Id;
                    }
                }

                if (metadataStrutcureId > 0 && repositoryId > 0)
                {
                    publicationManager.CreateMetadataStructureToRepository(metadataStrutcureId, repositoryId);
                }
            }
        }


        private void createMappings()
        {
            try
            {
                createSystemKeyMappings();
                createPartyTypeMappings();
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        #region createSystemKeyMappings

        private void createSystemKeyMappings()
        {
            object tmp = "";
            List<MetadataStructure> metadataStructures =
                tmp.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get().ToList();

            using (MappingManager mappingManager = new MappingManager())
            {
                XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);

                //#region ABCD BASIC
                if (metadataStructures.Any(m => m.Name.ToLower().Equals("basic abcd") || m.Name.ToLower().Equals("full abcd")))
                {
                    MetadataStructure metadataStructure =
                        metadataStructures.FirstOrDefault(m => m.Name.ToLower().Equals("basic abcd"));

                    XDocument metadataRef = xmlMetadataWriter.CreateMetadataXml(metadataStructure.Id);

                    //create root mapping
                    LinkElement abcdRoot = createLinkELementIfNotExist(mappingManager, metadataStructure.Id, metadataStructure.Name, LinkElementType.MetadataStructure, LinkElementComplexity.None);

                    //create system mapping
                    LinkElement system = createLinkELementIfNotExist(mappingManager, 0, "System", LinkElementType.System, LinkElementComplexity.None);

                    #region mapping ABCD BASIC to System Keys

                    Debug.WriteLine("abcd to root");
                    Mapping rootTo = MappingHelper.CreateIfNotExistMapping(abcdRoot, system, 0, null, null, mappingManager);
                    Debug.WriteLine("root to abcd");
                    Mapping rootFrom = MappingHelper.CreateIfNotExistMapping(system, abcdRoot, 0, null, null, mappingManager);
                    Debug.WriteLine("Title");

                    if (Exist("Title", LinkElementType.MetadataNestedAttributeUsage))
                    {
                        createToKeyMapping("Title", LinkElementType.MetadataNestedAttributeUsage, "Title", LinkElementType.MetadataNestedAttributeUsage, Key.Title, rootTo, metadataRef, mappingManager);
                        createFromKeyMapping("Title", LinkElementType.MetadataNestedAttributeUsage, "Title", LinkElementType.MetadataNestedAttributeUsage, Key.Title, rootFrom, metadataRef, mappingManager);
                    }

                    if (Exist("Details", LinkElementType.MetadataNestedAttributeUsage) &&
                        Exist("MetadataDescriptionRepr", LinkElementType.ComplexMetadataAttribute))
                    {
                        Debug.WriteLine("Details");
                        createToKeyMapping("Details", LinkElementType.MetadataNestedAttributeUsage, "MetadataDescriptionRepr", LinkElementType.ComplexMetadataAttribute, Key.Description, rootTo, metadataRef, mappingManager);
                        createFromKeyMapping("Details", LinkElementType.MetadataNestedAttributeUsage, "MetadataDescriptionRepr", LinkElementType.ComplexMetadataAttribute, Key.Description, rootFrom, metadataRef, mappingManager);
                    }

                    if (Exist("FullName", LinkElementType.MetadataNestedAttributeUsage) &&
                        Exist("PersonName", LinkElementType.ComplexMetadataAttribute))
                    {
                        Debug.WriteLine("FullName");
                        createToKeyMapping("FullName", LinkElementType.MetadataNestedAttributeUsage, "PersonName", LinkElementType.ComplexMetadataAttribute, Key.Author, rootTo, metadataRef, mappingManager);
                        createFromKeyMapping("FullName", LinkElementType.MetadataNestedAttributeUsage, "PersonName", LinkElementType.ComplexMetadataAttribute, Key.Author, rootFrom, metadataRef, mappingManager);
                    }

                    if (Exist("Text", LinkElementType.MetadataNestedAttributeUsage) &&
                        Exist("License", LinkElementType.MetadataNestedAttributeUsage))
                    {
                        Debug.WriteLine("Text");
                        createToKeyMapping("Text", LinkElementType.MetadataNestedAttributeUsage, "License", LinkElementType.MetadataNestedAttributeUsage, Key.License, rootTo, metadataRef, mappingManager);
                        createFromKeyMapping("Text", LinkElementType.MetadataNestedAttributeUsage, "License", LinkElementType.MetadataNestedAttributeUsage, Key.License, rootFrom, metadataRef, mappingManager);
                    }

                    #endregion mapping ABCD BASIC to System Keys
                }

                //#endregion

                #region mapping GBIF to System Keys

                if (metadataStructures.Any(m => m.Name.ToLower().Equals("gbif")))
                {
                    MetadataStructure metadataStructure =
                        metadataStructures.FirstOrDefault(m => m.Name.ToLower().Equals("gbif"));

                    XDocument metadataRef = xmlMetadataWriter.CreateMetadataXml(metadataStructure.Id);

                    //create root mapping
                    LinkElement gbifRoot = createLinkELementIfNotExist(mappingManager, metadataStructure.Id, metadataStructure.Name, LinkElementType.MetadataStructure, LinkElementComplexity.None);

                    //create system mapping
                    LinkElement system = createLinkELementIfNotExist(mappingManager, 0, "System", LinkElementType.System, LinkElementComplexity.None);

                    #region mapping GBIF to System Keys

                    Mapping rootTo = mappingManager.CreateMapping(gbifRoot, system, 0, null, null);
                    Mapping rootFrom = mappingManager.CreateMapping(system, gbifRoot, 0, null, null);

                    if (Exist("title", LinkElementType.MetadataNestedAttributeUsage) &&
                        Exist("Basic", LinkElementType.MetadataPackageUsage))
                    {
                        createToKeyMapping("title", LinkElementType.MetadataNestedAttributeUsage, "Basic", LinkElementType.MetadataPackageUsage, Key.Title, rootTo, metadataRef, mappingManager);
                        createFromKeyMapping("title", LinkElementType.MetadataNestedAttributeUsage, "Basic", LinkElementType.MetadataPackageUsage, Key.Title, rootFrom, metadataRef, mappingManager);
                    }

                    if (Exist("para", LinkElementType.MetadataNestedAttributeUsage) &&
                        Exist("abstract", LinkElementType.MetadataPackageUsage))
                    {
                        createToKeyMapping("para", LinkElementType.MetadataNestedAttributeUsage, "abstract", LinkElementType.MetadataPackageUsage, Key.Description, rootTo, metadataRef, mappingManager);
                        createFromKeyMapping("para", LinkElementType.MetadataNestedAttributeUsage, "abstract", LinkElementType.MetadataPackageUsage, Key.Description, rootFrom, metadataRef, mappingManager);
                    }

                    if (Exist("givenName", LinkElementType.MetadataNestedAttributeUsage) &&
                        Exist("individualName", LinkElementType.MetadataAttributeUsage))
                    {
                        createToKeyMapping("givenName", LinkElementType.MetadataNestedAttributeUsage, "Metadata/creator/creatorType/individualName", LinkElementType.MetadataAttributeUsage, Key.Author, rootTo, metadataRef, mappingManager, mappingManager.CreateTransformationRule("", "givenName[0] surName[0]"));
                        createToKeyMapping("givenName", LinkElementType.MetadataNestedAttributeUsage, "Metadata/creator/creatorType/individualName", LinkElementType.MetadataAttributeUsage, Key.Author, rootFrom, metadataRef, mappingManager, mappingManager.CreateTransformationRule(@"\w+", "Author[0]"));
                    }

                    if (Exist("surName", LinkElementType.MetadataNestedAttributeUsage) &&
                        Exist("individualName", LinkElementType.MetadataAttributeUsage))
                    {
                        createToKeyMapping("surName", LinkElementType.MetadataNestedAttributeUsage, "Metadata/creator/creatorType/individualName", LinkElementType.MetadataAttributeUsage, Key.Author, rootTo, metadataRef, mappingManager, mappingManager.CreateTransformationRule("", "givenName[0] surName[0]"));
                        createToKeyMapping("surName", LinkElementType.MetadataNestedAttributeUsage, "Metadata/creator/creatorType/individualName", LinkElementType.MetadataAttributeUsage, Key.Author, rootFrom, metadataRef, mappingManager, mappingManager.CreateTransformationRule(@"\w+", "Author[1]"));
                    }

                    if (Exist("title", LinkElementType.MetadataNestedAttributeUsage) &&
                        Exist("project", LinkElementType.MetadataPackageUsage))
                    {
                        createToKeyMapping("title", LinkElementType.MetadataNestedAttributeUsage, "project", LinkElementType.MetadataPackageUsage, Key.ProjectTitle, rootTo, metadataRef, mappingManager);
                        createFromKeyMapping("title", LinkElementType.MetadataNestedAttributeUsage, "project", LinkElementType.MetadataPackageUsage, Key.ProjectTitle, rootFrom, metadataRef, mappingManager);
                    }

                    #endregion mapping GBIF to System Keys
                }

                #endregion mapping GBIF to System Keys
            }
        }

        /// <summary>
        /// Map a node from xml to system key
        /// </summary>
        /// <param name="simpleNodeName">name or xpath</param>
        /// <param name="simpleType"></param>
        /// <param name="complexNodeName">name or xpath</param>
        /// <param name="complexType"></param>
        /// <param name="key"></param>
        /// <param name="root"></param>
        /// <param name="metadataRef"></param>
        /// <param name="mappingManager"></param>
        private void createToKeyMapping(
            string simpleNodeName, LinkElementType simpleType,
            string complexNodeName, LinkElementType complexType,
            Key key,
            Mapping root,
            XDocument metadataRef,
            MappingManager mappingManager, TransformationRule transformationRule = null)
        {
            if (transformationRule == null) transformationRule = new TransformationRule();

            LinkElement le = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(key),
                    key.ToString(), LinkElementType.Key, LinkElementComplexity.Simple);

            if (simpleNodeName.Equals(complexNodeName))
            {
                List<XElement> elements = getXElements(simpleNodeName, metadataRef);

                foreach (XElement xElement in elements)
                {
                    string sId = xElement.Attribute("id").Value;
                    string name = xElement.Attribute("name").Value;
                    LinkElement tmp = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(sId), name,
                        simpleType, LinkElementComplexity.Simple);

                    Mapping tmpMapping = MappingHelper.CreateIfNotExistMapping(tmp, le, 1, new TransformationRule(), root, mappingManager);
                    MappingHelper.CreateIfNotExistMapping(tmp, le, 2, transformationRule, tmpMapping, mappingManager);
                }
            }
            else
            {
                IEnumerable<XElement> complexElements = getXElements(complexNodeName, metadataRef);

                foreach (var complex in complexElements)
                {
                    string sIdComplex = complex.Attribute("id").Value;
                    string nameComplex = complex.Attribute("name").Value;
                    LinkElement tmpComplexElement = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(sIdComplex), nameComplex,
                        complexType, LinkElementComplexity.Complex);

                    Mapping complexMapping = MappingHelper.CreateIfNotExistMapping(tmpComplexElement, le, 1, new TransformationRule(), root, mappingManager);

                    IEnumerable<XElement> simpleElements = XmlUtility.GetAllChildren(complex).Where(s => s.Name.LocalName.Equals(simpleNodeName));

                    foreach (XElement xElement in simpleElements)
                    {
                        string sId = xElement.Attribute("id").Value;
                        string name = xElement.Attribute("name").Value;
                        LinkElement tmp = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(sId), name,
                            simpleType, LinkElementComplexity.Simple);

                        MappingHelper.CreateIfNotExistMapping(tmp, le, 2, transformationRule, complexMapping, mappingManager);
                    }
                }
            }
        }

        /// <summary>
        /// Map a system key to xml node
        /// </summary>
        /// <param name="simpleNodeName"></param>
        /// <param name="simpleType"></param>
        /// <param name="complexNodeName"></param>
        /// <param name="complexType"></param>
        /// <param name="key"></param>
        /// <param name="root"></param>
        /// <param name="metadataRef"></param>
        /// <param name="mappingManager"></param>
        private void createFromKeyMapping(
            string simpleNodeName, LinkElementType simpleType,
            string complexNodeName, LinkElementType complexType,
            Key key,
            Mapping root,
            XDocument metadataRef,
            MappingManager mappingManager)
        {
            LinkElement le = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(key),
                    key.ToString(), LinkElementType.Key, LinkElementComplexity.Simple);

            if (simpleNodeName.Equals(complexNodeName))
            {
                IEnumerable<XElement> elements = getXElements(simpleNodeName, metadataRef);

                foreach (XElement xElement in elements)
                {
                    string sId = xElement.Attribute("id").Value;
                    string name = xElement.Attribute("name").Value;
                    LinkElement tmp = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(sId), name,
                        simpleType, LinkElementComplexity.Simple);

                    Mapping tmpMapping = MappingHelper.CreateIfNotExistMapping(le, tmp, 1, null, root, mappingManager);
                    MappingHelper.CreateIfNotExistMapping(le, tmp, 2, null, tmpMapping, mappingManager);
                }
            }
            else
            {
                IEnumerable<XElement> complexElements = getXElements(complexNodeName, metadataRef);

                foreach (var complex in complexElements)
                {
                    string sIdComplex = complex.Attribute("id").Value;
                    string nameComplex = complex.Attribute("name").Value;
                    LinkElement tmpComplexElement = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(sIdComplex), nameComplex,
                        complexType, LinkElementComplexity.Complex);

                    Mapping complexMapping = MappingHelper.CreateIfNotExistMapping(le, tmpComplexElement, 1, null, root, mappingManager);

                    IEnumerable<XElement> simpleElements = XmlUtility.GetAllChildren(complex).Where(s => s.Name.LocalName.Equals(simpleNodeName));

                    foreach (XElement xElement in simpleElements)
                    {
                        string sId = xElement.Attribute("id").Value;
                        string name = xElement.Attribute("name").Value;
                        LinkElement tmp = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(sId), name,
                            simpleType, LinkElementComplexity.Simple);

                        MappingHelper.CreateIfNotExistMapping(le, tmp, 2, null, complexMapping, mappingManager);
                    }
                }
            }
        }

        #endregion createSystemKeyMappings

        #region createPartyTypeMappings

        private void createPartyTypeMappings()
        {
            object tmp = "";
            List<MetadataStructure> metadataStructures =
                tmp.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get().ToList();
            List<PartyType> partyTypes =
                tmp.GetUnitOfWork().GetReadOnlyRepository<PartyType>().Get().ToList();
            List<PartyCustomAttribute> partyCustomAttrs =
                tmp.GetUnitOfWork().GetReadOnlyRepository<PartyCustomAttribute>().Get().ToList();

            List<PartyRelationshipType> partyReleationships =
                tmp.GetUnitOfWork().GetReadOnlyRepository<PartyRelationshipType>().Get().ToList();

            MappingManager mappingManager = new MappingManager();
            XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);

            try
            {
                #region ABCD BASIC

                if (metadataStructures.Any(m => m.Name.ToLower().Equals("basic abcd")))
                {
                    MetadataStructure metadataStructure =
                        metadataStructures.FirstOrDefault(m => m.Name.ToLower().Equals("basic abcd"));

                    XDocument metadataRef = xmlMetadataWriter.CreateMetadataXml(metadataStructure.Id);

                    //create root mapping
                    LinkElement abcdRoot = createLinkELementIfNotExist(mappingManager, metadataStructure.Id,
                        metadataStructure.Name, LinkElementType.MetadataStructure, LinkElementComplexity.None);

                    //create system mapping
                    LinkElement system = createLinkELementIfNotExist(mappingManager, 0, "System", LinkElementType.System,
                        LinkElementComplexity.None);

                    #region mapping ABCD BASIC to System Keys

                    Mapping rootTo = MappingHelper.CreateIfNotExistMapping(abcdRoot, system, 0, null, null, mappingManager);
                    Mapping rootFrom = MappingHelper.CreateIfNotExistMapping(system, abcdRoot, 0, null, null, mappingManager);

                    // create mapping for paryttypes

                    #region person

                    if (partyTypes.Any(p => p.Title.Equals("Person")))
                    {
                        PartyType partyType = partyTypes.FirstOrDefault(p => p.Title.Equals("Person"));
                        //FirstName
                        string complexAttrName = "MicroAgentP";

                        if (partyCustomAttrs.Any(
                            pAttr => pAttr.Name.Equals("FirstName") && pAttr.PartyType.Id.Equals(partyType.Id)))
                        {
                            PartyCustomAttribute partyCustomAttribute = partyCustomAttrs.FirstOrDefault(
                                pAttr => pAttr.Name.Equals("FirstName") && pAttr.PartyType.Id.Equals(partyType.Id));

                            createToPartyTypeMapping(
                                "Name", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootTo, metadataRef,
                                mappingManager,
                                new TransformationRule(@"\w+", "Name[0]"));

                            createFromPartyTypeMapping(
                                "Name", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootFrom, metadataRef,
                                mappingManager,
                                new TransformationRule(@"\w+", "FirstName[0] LastName[0]"));
                        }

                        if (partyCustomAttrs.Any(
                            pAttr => pAttr.Name.Equals("LastName") && pAttr.PartyType.Id.Equals(partyType.Id)))
                        {
                            PartyCustomAttribute partyCustomAttribute = partyCustomAttrs.FirstOrDefault(
                                pAttr => pAttr.Name.Equals("LastName") && pAttr.PartyType.Id.Equals(partyType.Id));

                            createToPartyTypeMapping(
                                "Name", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootTo, metadataRef,
                                mappingManager,
                                new TransformationRule(@"\w+", "Name[1]"));

                            createFromPartyTypeMapping(
                                "Name", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootFrom, metadataRef,
                                mappingManager,
                                new TransformationRule(@"\w+", "FirstName[0] LastName[0]"));
                        }

                        if (partyCustomAttrs.Any(
                            pAttr => pAttr.Name.Equals("Phone") && pAttr.PartyType.Id.Equals(partyType.Id)))
                        {
                            PartyCustomAttribute partyCustomAttribute = partyCustomAttrs.FirstOrDefault(
                                pAttr => pAttr.Name.Equals("Phone") && pAttr.PartyType.Id.Equals(partyType.Id));

                            createToPartyTypeMapping(
                                "Phone", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootTo, metadataRef,
                                mappingManager,
                                new TransformationRule());

                            createFromPartyTypeMapping(
                                "Phone", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootFrom, metadataRef,
                                mappingManager,
                                new TransformationRule());
                        }

                        if (partyCustomAttrs.Any(
                            pAttr => pAttr.Name.Equals("EMail") && pAttr.PartyType.Id.Equals(partyType.Id)))
                        {
                            PartyCustomAttribute partyCustomAttribute = partyCustomAttrs.FirstOrDefault(
                                pAttr => pAttr.Name.Equals("EMail") && pAttr.PartyType.Id.Equals(partyType.Id));

                            createToPartyTypeMapping(
                                "Email", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootTo, metadataRef,
                                mappingManager,
                                new TransformationRule());

                            createFromPartyTypeMapping(
                                "Email", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootFrom, metadataRef,
                                mappingManager,
                                new TransformationRule());
                        }

                        #region adress

                        if (partyCustomAttrs.Any(
                            pAttr => pAttr.Name.Equals("Street") && pAttr.PartyType.Id.Equals(partyType.Id)))
                        {
                            PartyCustomAttribute partyCustomAttribute = partyCustomAttrs.FirstOrDefault(
                                pAttr => pAttr.Name.Equals("Street") && pAttr.PartyType.Id.Equals(partyType.Id));

                            createToPartyTypeMapping(
                                "Address", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootTo, metadataRef,
                                mappingManager,
                                new TransformationRule(@"\w+", "Address[0]"));

                            createFromPartyTypeMapping(
                                "Address", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootFrom, metadataRef,
                                mappingManager,
                                new TransformationRule());
                        }

                        if (partyCustomAttrs.Any(
                            pAttr => pAttr.Name.Equals("City") && pAttr.PartyType.Id.Equals(partyType.Id)))
                        {
                            PartyCustomAttribute partyCustomAttribute = partyCustomAttrs.FirstOrDefault(
                                pAttr => pAttr.Name.Equals("City") && pAttr.PartyType.Id.Equals(partyType.Id));

                            createToPartyTypeMapping(
                                "Address", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootTo, metadataRef,
                                mappingManager,
                                new TransformationRule(@"\w+", "Address[1]"));

                            createFromPartyTypeMapping(
                                "Address", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootFrom, metadataRef,
                                mappingManager,
                                new TransformationRule());
                        }

                        if (partyCustomAttrs.Any(
                            pAttr => pAttr.Name.Equals("ZipCode") && pAttr.PartyType.Id.Equals(partyType.Id)))
                        {
                            PartyCustomAttribute partyCustomAttribute = partyCustomAttrs.FirstOrDefault(
                                pAttr => pAttr.Name.Equals("ZipCode") && pAttr.PartyType.Id.Equals(partyType.Id));

                            createToPartyTypeMapping(
                                "Address", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootTo, metadataRef,
                                mappingManager,
                                new TransformationRule(@"\w+", "Address[2]"));

                            createFromPartyTypeMapping(
                                "Address", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootFrom, metadataRef,
                                mappingManager,
                                new TransformationRule());
                        }

                        if (partyCustomAttrs.Any(
                            pAttr => pAttr.Name.Equals("Country") && pAttr.PartyType.Id.Equals(partyType.Id)))
                        {
                            PartyCustomAttribute partyCustomAttribute = partyCustomAttrs.FirstOrDefault(
                                pAttr => pAttr.Name.Equals("Country") && pAttr.PartyType.Id.Equals(partyType.Id));

                            createToPartyTypeMapping(
                                "Address", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootTo, metadataRef,
                                mappingManager,
                                new TransformationRule(@"\w+", "Address[3]"));

                            createFromPartyTypeMapping(
                                "Address", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootFrom, metadataRef,
                                mappingManager,
                                new TransformationRule());
                        }

                        #endregion adress

                        #region owner releationship

                        string personUsage = "Person";

                        if (partyCustomAttrs.Any(
                            pAttr => pAttr.Name.Equals("FirstName") && pAttr.PartyType.Id.Equals(partyType.Id)))
                        {
                            PartyCustomAttribute partyCustomAttribute = partyCustomAttrs.FirstOrDefault(
                                pAttr => pAttr.Name.Equals("FirstName") && pAttr.PartyType.Id.Equals(partyType.Id));

                            createToPartyTypeMapping(
                                "FullName", LinkElementType.MetadataNestedAttributeUsage,
                                personUsage, LinkElementType.MetadataNestedAttributeUsage,
                                partyCustomAttribute, partyType, rootTo, metadataRef,
                                mappingManager,
                                new TransformationRule(@"\w+", "Name[0]"));

                            createFromPartyTypeMapping(
                                "FullName", LinkElementType.MetadataNestedAttributeUsage,
                                personUsage, LinkElementType.MetadataNestedAttributeUsage,
                                partyCustomAttribute, partyType, rootFrom, metadataRef,
                                mappingManager,
                                new TransformationRule(@"\w+", "FirstName[0] LastName[0]"));
                        }

                        if (partyCustomAttrs.Any(
                            pAttr => pAttr.Name.Equals("LastName") && pAttr.PartyType.Id.Equals(partyType.Id)))
                        {
                            PartyCustomAttribute partyCustomAttribute = partyCustomAttrs.FirstOrDefault(
                                pAttr => pAttr.Name.Equals("LastName") && pAttr.PartyType.Id.Equals(partyType.Id));

                            createToPartyTypeMapping(
                                "FullName", LinkElementType.MetadataNestedAttributeUsage,
                                personUsage, LinkElementType.MetadataNestedAttributeUsage,
                                partyCustomAttribute, partyType, rootTo, metadataRef,
                                mappingManager,
                                new TransformationRule(@"\w+", "Name[1]"));

                            createFromPartyTypeMapping(
                                "FullName", LinkElementType.MetadataNestedAttributeUsage,
                                personUsage, LinkElementType.MetadataNestedAttributeUsage,
                                partyCustomAttribute, partyType, rootFrom, metadataRef,
                                mappingManager,
                                new TransformationRule(@"\w+", "FirstName[0] LastName[0]"));
                        }

                        // add releationship type mapping

                        PartyRelationshipType partyRelationshipType = partyReleationships.FirstOrDefault(p => p.Title.Equals(ConfigurationManager.AppSettings["OwnerPartyRelationshipType"]));
                        if (partyRelationshipType != null)
                        {
                            createToPartyReleationMapping(
                                "FullName", LinkElementType.MetadataNestedAttributeUsage,
                                personUsage, LinkElementType.MetadataNestedAttributeUsage,
                                partyRelationshipType, rootTo, metadataRef,
                                mappingManager,
                                new TransformationRule(@"\w+", "FullName[0]"));

                            createFromPartyReleationMapping(
                                "FullName", LinkElementType.MetadataNestedAttributeUsage,
                                personUsage, LinkElementType.MetadataNestedAttributeUsage,
                                partyRelationshipType, rootFrom, metadataRef,
                                mappingManager,
                                new TransformationRule(@"\w+", "FirstName[0] LastName[0]"));
                        }

                        #endregion owner releationship
                    }

                    #endregion person

                    #region Organisation

                    if (partyTypes.Any(p => p.Title.Equals("Organization")))
                    {
                        PartyType partyType = partyTypes.FirstOrDefault(p => p.Title.Equals("Organization"));
                        //FirstName
                        string complexAttrName = "Metadata/Metadata/MetadataType/Owners/OwnersType/Owner/Contact/Organisation/Organisation/Name/Label/Representation/RepresentationType";

                        if (partyCustomAttrs.Any(
                            pAttr => pAttr.Name.Equals("Name") && pAttr.PartyType.Id.Equals(partyType.Id)))
                        {
                            PartyCustomAttribute partyCustomAttribute = partyCustomAttrs.FirstOrDefault(
                                pAttr => pAttr.Name.Equals("Name") && pAttr.PartyType.Id.Equals(partyType.Id));

                            createToPartyTypeMapping(
                                "Text", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootTo, metadataRef,
                                mappingManager,
                                new TransformationRule());

                            createFromPartyTypeMapping(
                                "Text", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootFrom, metadataRef,
                                mappingManager,
                                new TransformationRule());
                        }
                    }

                    #endregion Organisation

                    #endregion mapping ABCD BASIC to System Keys
                }

                #endregion ABCD BASIC

                #region GBIF

                if (metadataStructures.Any(m => m.Name.ToLower().Equals("gbif")))
                {
                    MetadataStructure metadataStructure =
                        metadataStructures.FirstOrDefault(m => m.Name.ToLower().Equals("gbif"));

                    XDocument metadataRef = xmlMetadataWriter.CreateMetadataXml(metadataStructure.Id);

                    //create root mapping
                    LinkElement gbifRoot = createLinkELementIfNotExist(mappingManager, metadataStructure.Id,
                        metadataStructure.Name, LinkElementType.MetadataStructure, LinkElementComplexity.None);

                    //create system mapping
                    LinkElement system = createLinkELementIfNotExist(mappingManager, 0, "System", LinkElementType.System,
                        LinkElementComplexity.None);

                    #region mapping GFBIO to System Keys

                    Mapping rootTo = MappingHelper.CreateIfNotExistMapping(gbifRoot, system, 0, null, null, mappingManager);
                    Mapping rootFrom = MappingHelper.CreateIfNotExistMapping(system, gbifRoot, 0, null, null, mappingManager);

                    // create mapping for paryttypes

                    #region person

                    if (partyTypes.Any(p => p.Title.Equals("Person")))
                    {
                        PartyType partyType = partyTypes.FirstOrDefault(p => p.Title.Equals("Person"));
                        //FirstName

                        string complexAttrName = "individualNameType";

                        if (partyCustomAttrs.Any(
                            pAttr => pAttr.Name.Equals("FirstName") && pAttr.PartyType.Id.Equals(partyType.Id)))
                        {
                            PartyCustomAttribute partyCustomAttribute = partyCustomAttrs.FirstOrDefault(
                                pAttr => pAttr.Name.Equals("FirstName") && pAttr.PartyType.Id.Equals(partyType.Id));

                            createToPartyTypeMapping(
                                "givenName", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootTo, metadataRef,
                                mappingManager,
                                new TransformationRule());

                            createFromPartyTypeMapping(
                                "givenName", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootFrom, metadataRef,
                                mappingManager,
                                new TransformationRule());
                        }

                        if (partyCustomAttrs.Any(
                            pAttr => pAttr.Name.Equals("LastName") && pAttr.PartyType.Id.Equals(partyType.Id)))
                        {
                            PartyCustomAttribute partyCustomAttribute = partyCustomAttrs.FirstOrDefault(
                                pAttr => pAttr.Name.Equals("LastName") && pAttr.PartyType.Id.Equals(partyType.Id));

                            createToPartyTypeMapping(
                                "surName", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootTo, metadataRef,
                                mappingManager,
                                new TransformationRule());

                            createFromPartyTypeMapping(
                                "surName", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.ComplexMetadataAttribute,
                                partyCustomAttribute, partyType, rootFrom, metadataRef,
                                mappingManager,
                                new TransformationRule());
                        }

                        #region owner relationship

                        //Metadata/creator/creatorType/individualName
                        PartyRelationshipType partyRelationshipType = partyReleationships.FirstOrDefault(p => p.Title.Equals(ConfigurationManager.AppSettings["OwnerPartyRelationshipType"]));
                        if (partyRelationshipType != null)
                        {
                            createToPartyReleationMapping(
                                "givenName", LinkElementType.MetadataNestedAttributeUsage,
                                "Metadata/creator/creatorType/individualName", LinkElementType.MetadataAttributeUsage,
                                partyRelationshipType, rootTo, metadataRef,
                                mappingManager,
                                new TransformationRule());

                            createFromPartyReleationMapping(
                                "givenName", LinkElementType.MetadataNestedAttributeUsage,
                                "Metadata/creator/creatorType/individualName", LinkElementType.MetadataAttributeUsage,
                                partyRelationshipType, rootFrom, metadataRef,
                                mappingManager,
                                new TransformationRule());
                        }

                        #endregion owner relationship
                    }

                    #endregion person

                    #region Project

                    if (partyTypes.Any(p => p.Title.Equals("Project")))
                    {
                        PartyType partyType = partyTypes.FirstOrDefault(p => p.Title.Equals("Project"));
                        //FirstName
                        string complexAttrName = "project";

                        if (partyCustomAttrs.Any(
                            pAttr => pAttr.Name.Equals("Name") && pAttr.PartyType.Id.Equals(partyType.Id)))
                        {
                            PartyCustomAttribute partyCustomAttribute = partyCustomAttrs.FirstOrDefault(
                                pAttr => pAttr.Name.Equals("Name") && pAttr.PartyType.Id.Equals(partyType.Id));

                            createToPartyTypeMapping(
                                "title", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.MetadataPackageUsage,
                                partyCustomAttribute, partyType, rootTo, metadataRef,
                                mappingManager,
                                new TransformationRule());

                            createFromPartyTypeMapping(
                                "title", LinkElementType.MetadataNestedAttributeUsage,
                                complexAttrName, LinkElementType.MetadataPackageUsage,
                                partyCustomAttribute, partyType, rootFrom, metadataRef,
                                mappingManager,
                                new TransformationRule());
                        }
                    }

                    #endregion Project

                    #endregion mapping GFBIO to System Keys
                }

                #endregion GBIF
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                mappingManager.Dispose();
            }
        }

        private void createToPartyTypeMapping(
            string simpleNodeName, LinkElementType simpleType,
            string complexNodeName, LinkElementType complexType,
            PartyCustomAttribute partyCustomAttr,
            PartyType partyType,
            Mapping root,
            XDocument metadataRef,
            MappingManager mappingManager, TransformationRule transformationRule = null)
        {
            if (transformationRule == null) transformationRule = new TransformationRule();

            LinkElement le = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(partyType.Id),
                    partyType.Title, LinkElementType.PartyType, LinkElementComplexity.Complex);

            XElement complex = getXElements(complexNodeName, metadataRef).FirstOrDefault();

            string sIdComplex = complex.Attribute("id").Value;
            string nameComplex = complex.Attribute("name").Value;
            LinkElement tmpComplexElement = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(sIdComplex), nameComplex,
                complexType, LinkElementComplexity.Complex);

            Mapping complexMapping = MappingHelper.CreateIfNotExistMapping(tmpComplexElement, le, 1, new TransformationRule(), root, mappingManager);

            IEnumerable<XElement> simpleElements = XmlUtility.GetAllChildren(complex).Where(s => s.Name.LocalName.Equals(simpleNodeName));

            LinkElement simpleLe = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(partyCustomAttr.Id),
            partyCustomAttr.Name, LinkElementType.PartyCustomType, LinkElementComplexity.Simple);

            foreach (XElement xElement in simpleElements)
            {
                string sId = xElement.Attribute("id").Value;
                string name = xElement.Attribute("name").Value;
                LinkElement tmp = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(sId), name,
                    simpleType, LinkElementComplexity.Simple);

                MappingHelper.CreateIfNotExistMapping(tmp, simpleLe, 2, transformationRule, complexMapping, mappingManager);
            }
        }

        private void createFromPartyTypeMapping(
            string simpleNodeName, LinkElementType simpleType,
            string complexNodeName, LinkElementType complexType,
            PartyCustomAttribute partyCustomAttr,
            PartyType partyType,
            Mapping root,
            XDocument metadataRef,
            MappingManager mappingManager, TransformationRule transformationRule = null)
        {
            if (transformationRule == null) transformationRule = new TransformationRule();

            LinkElement le = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(partyType.Id),
                    partyType.Title, LinkElementType.PartyType, LinkElementComplexity.Complex);

            XElement complex = getXElements(complexNodeName, metadataRef).FirstOrDefault();

            string sIdComplex = complex.Attribute("id").Value;
            string nameComplex = complex.Attribute("name").Value;
            LinkElement tmpComplexElement = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(sIdComplex), nameComplex,
                complexType, LinkElementComplexity.Complex);

            Mapping complexMapping = MappingHelper.CreateIfNotExistMapping(le, tmpComplexElement, 1, new TransformationRule(), root, mappingManager);

            IEnumerable<XElement> simpleElements = XmlUtility.GetAllChildren(complex).Where(s => s.Name.LocalName.Equals(simpleNodeName));

            LinkElement simpleLe = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(partyCustomAttr.Id),
            partyCustomAttr.Name, LinkElementType.PartyCustomType, LinkElementComplexity.Simple);

            foreach (XElement xElement in simpleElements)
            {
                string sId = xElement.Attribute("id").Value;
                string name = xElement.Attribute("name").Value;
                LinkElement tmp = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(sId), name,
                    simpleType, LinkElementComplexity.Simple);

                MappingHelper.CreateIfNotExistMapping(simpleLe, tmp, 2, transformationRule, complexMapping, mappingManager);
            }
        }

        private void createToPartyReleationMapping(
            string simpleNodeName, LinkElementType simpleType,
            string complexNodeName, LinkElementType complexType,
            PartyRelationshipType partyReleationType,
            Mapping root,
            XDocument metadataRef,
            MappingManager mappingManager, TransformationRule transformationRule = null)
        {
            if (transformationRule == null) transformationRule = new TransformationRule();

            LinkElement le = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(partyReleationType.Id),
                    partyReleationType.Title, LinkElementType.PartyRelationshipType, LinkElementComplexity.Simple);

            XElement complex = getXElements(complexNodeName, metadataRef).FirstOrDefault();

            string sIdComplex = complex.Attribute("id").Value;
            string nameComplex = complex.Attribute("name").Value;
            LinkElement tmpComplexElement = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(sIdComplex), nameComplex,
                complexType, LinkElementComplexity.Complex);

            Mapping complexMapping = MappingHelper.CreateIfNotExistMapping(tmpComplexElement, le, 1, new TransformationRule(), root, mappingManager);

            IEnumerable<XElement> simpleElements = XmlUtility.GetAllChildren(complex).Where(s => s.Name.LocalName.Equals(simpleNodeName));

            LinkElement simpleLe = le;

            foreach (XElement xElement in simpleElements)
            {
                string sId = xElement.Attribute("id").Value;
                string name = xElement.Attribute("name").Value;
                LinkElement tmp = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(sId), name,
                    simpleType, LinkElementComplexity.Simple);

                MappingHelper.CreateIfNotExistMapping(tmp, simpleLe, 2, transformationRule, complexMapping, mappingManager);
            }
        }

        private void createFromPartyReleationMapping(
            string simpleNodeName, LinkElementType simpleType,
            string complexNodeName, LinkElementType complexType,
            PartyRelationshipType partyReleationType,
            Mapping root,
            XDocument metadataRef,
            MappingManager mappingManager, TransformationRule transformationRule = null)
        {
            //create ruleif not exist
            if (transformationRule == null) transformationRule = new TransformationRule();

            //create complex elements if not exits
            LinkElement le = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(partyReleationType.Id),
                    partyReleationType.Title, LinkElementType.PartyRelationshipType, LinkElementComplexity.Simple);

            XElement complex = getXElements(complexNodeName, metadataRef).FirstOrDefault();

            string sIdComplex = complex.Attribute("id").Value;
            string nameComplex = complex.Attribute("name").Value;
            LinkElement tmpComplexElement = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(sIdComplex), nameComplex,
                complexType, LinkElementComplexity.Complex);

            //map complex
            Mapping complexMapping = MappingHelper.CreateIfNotExistMapping(le, tmpComplexElement, 1, new TransformationRule(), root, mappingManager);

            IEnumerable<XElement> simpleElements = XmlUtility.GetAllChildren(complex).Where(s => s.Name.LocalName.Equals(simpleNodeName));

            LinkElement simpleLe = le;

            foreach (XElement xElement in simpleElements)
            {
                string sId = xElement.Attribute("id").Value;
                string name = xElement.Attribute("name").Value;
                LinkElement tmp = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(sId), name,
                    simpleType, LinkElementComplexity.Simple);

                MappingHelper.CreateIfNotExistMapping(simpleLe, tmp, 2, transformationRule, complexMapping, mappingManager);
            }
        }

        #endregion createPartyTypeMappings

        private LinkElement createLinkELementIfNotExist(
            MappingManager mappingManager,
            long id,
            string name,
            LinkElementType type,
            LinkElementComplexity complexity)
        {
            LinkElement element = mappingManager.GetLinkElement(id, name, type);

            if (element == null)
            {
                element = mappingManager.CreateLinkElement(
                    id,
                    type,
                    complexity,
                    name,
                    ""
                    );
            }

            return element;
        }

        private static List<XElement> getXElements(string nodename, XDocument metadataRef)
        {
            if (!nodename.Contains("/"))
            {
                return XmlUtility.GetXElementByNodeName(nodename, metadataRef).ToList();
            }
            else
            {
                List<XElement> tmp = new List<XElement>();
                tmp.Add(metadataRef.XPathSelectElement(nodename));
                return tmp;
            }
        }

        private static void ImportPartyTypes()
        {
            //PartyTypeManager partyTypeManager = new PartyTypeManager();
            //var filePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("BAM"), "partyTypes.xml");
            //XDocument xDoc = XDocument.Load(filePath);
            //XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.Load(xDoc.CreateReader());
            //var partyTypesNodeList = xmlDoc.SelectNodes("//PartyTypes");
            //if (partyTypesNodeList.Count > 0)
            //    foreach (XmlNode partyTypeNode in partyTypesNodeList[0].ChildNodes)
            //    {
            //        var title = partyTypeNode.Attributes["Name"].Value;
            //        //If there is not such a party type
            //        if (partyTypeManager.Repo.Get(item => item.Title == title).Count == 0)
            //        {
            //            //
            //            var partyType = partyTypeManager.Create(title, "Imported from partyTypes.xml", null);
            //            partyTypeManager.AddStatusType(partyType, "Create", "", 0);
            //            foreach (XmlNode customAttrNode in partyTypeNode.ChildNodes)
            //            {
            //                var customAttrType = customAttrNode.Attributes["type"] == null ? "String" : customAttrNode.Attributes["type"].Value;
            //                var description = customAttrNode.Attributes["description"] == null ? "" : customAttrNode.Attributes["description"].Value;
            //                var validValues = customAttrNode.Attributes["validValues"] == null ? "" : customAttrNode.Attributes["validValues"].Value;
            //                var isValueOptional = customAttrNode.Attributes["isValueOptional"] == null ? true : Convert.ToBoolean(customAttrNode.Attributes["isValueOptional"].Value);
            //                partyTypeManager.CreatePartyCustomAttribute(partyType, customAttrType, customAttrNode.Attributes["Name"].Value, description, validValues, isValueOptional);
            //            }
            //        }
            //        //edit add other custom attr

            //    }
        }

        private bool Exist(string name, LinkElementType type)
        {
            using (var uow = this.GetUnitOfWork())
            {
                if (type == LinkElementType.MetadataAttributeUsage)
                {
                    if (uow.GetReadOnlyRepository<MetadataAttributeUsage>().Get().Any(m => m.Label.ToLower().Equals(name.ToLower()))) return true;
                }

                if (type == LinkElementType.MetadataNestedAttributeUsage)
                {
                    if (uow.GetReadOnlyRepository<MetadataNestedAttributeUsage>().Get().Any(m => m.Label.ToLower().Equals(name.ToLower()))) return true;
                }

                if (type == LinkElementType.MetadataPackageUsage)
                {
                    if (uow.GetReadOnlyRepository<MetadataPackageUsage>().Get().Any(m => m.Label.ToLower().Equals(name.ToLower()))) return true;
                }

                if (type == LinkElementType.SimpleMetadataAttribute)
                {
                    if (uow.GetReadOnlyRepository<MetadataSimpleAttribute>().Get().Any(m => m.Name.ToLower().Equals(name.ToLower()))) return true;
                }

                if (type == LinkElementType.ComplexMetadataAttribute)
                {
                    if (uow.GetReadOnlyRepository<MetadataCompoundAttribute>().Get().Any(m => m.Name.ToLower().Equals(name.ToLower()))) return true;
                }
            }

            return false;
        }

        public void Dispose()
        {
            // nothing to do for now...
        }
    }
}