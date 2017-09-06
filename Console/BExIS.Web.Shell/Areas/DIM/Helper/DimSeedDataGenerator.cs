
using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Entities.Publication;
using BExIS.Dim.Helpers;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace BExIS.Modules.Dim.UI.Helpers
{
    public class DimSeedDataGenerator
    {
        public static void GenerateSeedData()
        {


            #region SECURITY

            //workflows = größere sachen, vielen operation
            //operations = einzelne actions

            //1.controller -> 1.Operation


            FeatureManager featureManager = new FeatureManager();

            Feature DataDissemination =
                featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Data Dissemination"));
            if (DataDissemination == null)
                DataDissemination = featureManager.Create("Data Dissemination", "Data Dissemination");

            Feature Mapping = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Mapping"));
            if (Mapping == null) Mapping = featureManager.Create("Mapping", "Mapping", DataDissemination);

            Feature Submission = featureManager.FeatureRepository.Get().FirstOrDefault(f => f.Name.Equals("Submission"));
            if (Submission == null) Submission = featureManager.Create("Submission", "Submission", DataDissemination);

            OperationManager operationManager = new OperationManager();


            #region Help Workflow

            operationManager.Create("DIM", "Help", "*", DataDissemination);

            #endregion

            #region Admin Workflow

            operationManager.Create("Dim", "Admin", "*", DataDissemination);

            operationManager.Create("Dim", "Submission", "*", Submission);
            operationManager.Create("Dim", "Mapping", "*", Mapping);


            #endregion

            #region Mapping Workflow

            //ToDo add security after Refactoring DIM mapping workflow


            //workflow = new Workflow();
            //workflow.Name = "Mapping";
            //workflowManager.Create(workflow);

            //operation = operationManager.Create("Dim", "Admin", "*", null, workflow);
            //workflow.Operations.Add(operation);

            //Mapping.Workflows.Add(workflow);

            #endregion

            #region Submission Workflow

            //ToDo add security after Refactoring DIM Submission workflow

            //workflow = new Workflow();
            //workflow.Name = "Submission";
            //workflowManager.Create(workflow);

            //operation = operationManager.Create("Dim", "Admin", "*", null, workflow);
            //workflow.Operations.Add(operation);

            //Submission.Workflows.Add(workflow);

            #endregion

            #endregion

            #region EXPORT

            SubmissionManager submissionManager = new SubmissionManager();
            submissionManager.Load();

            createMetadataStructureRepoMaps();


            #endregion

            #region MAPPING

            createMappings();

            #endregion

            //ImportPartyTypes();


        }

        private static void createMetadataStructureRepoMaps()
        {
            PublicationManager publicationManager = new PublicationManager();

            //set MetadataStructureToRepository for gbif and pensoft
            long metadataStrutcureId = 0;
            long repositoryId = 0;

            //get id of metadatstructure
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
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

        private static void createMappings()
        {
            createSystemKeyMappings();
        }

        private static void createSystemKeyMappings()
        {
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            MappingManager mappingManager = new MappingManager();
            XmlMetadataWriter xmlMetadataWriter = new XmlMetadataWriter(XmlNodeMode.xPath);

            #region ABCD BASIC
            if (metadataStructureManager.Repo.Query().Any(m => m.Name.ToLower().Equals("basic abcd")))
            {
                MetadataStructure metadataStructure =
                    metadataStructureManager.Repo.Query().FirstOrDefault(m => m.Name.ToLower().Equals("basic abcd"));

                XDocument metadataRef = xmlMetadataWriter.CreateMetadataXml(metadataStructure.Id);


                //create root mapping
                LinkElement abcdRoot = createLinkELementIfNotExist(mappingManager, metadataStructure.Id, metadataStructure.Name, LinkElementType.MetadataStructure, LinkElementComplexity.Complex);

                //create system mapping
                LinkElement system = createLinkELementIfNotExist(mappingManager, 0, "System", LinkElementType.System, LinkElementComplexity.Complex);

                #region mapping ABCD BASIC to System Keys


                Mapping root = mappingManager.CreateMapping(abcdRoot, system, 0, null, null);

                //get all title (title/titleType)
                IEnumerable<XElement> elements = XmlUtility.GetXElementByNodeName("Title", metadataRef);

                LinkElement title = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(Key.Title), Key.Title.ToString(), LinkElementType.Key, LinkElementComplexity.Simple);

                foreach (XElement xElement in elements)
                {
                    string sId = xElement.Attribute("id").Value;
                    string name = xElement.Attribute("name").Value;
                    LinkElement tmp = createLinkELementIfNotExist(mappingManager, Convert.ToInt64(sId), name, LinkElementType.MetadataNestedAttributeUsage, LinkElementComplexity.Simple);

                    Mapping tmpMapping = mappingManager.CreateMapping(tmp, title, 1, null, root);
                    mappingManager.CreateMapping(tmp, title, 2, null, tmpMapping);

                }


                #endregion
            }

            #endregion


            #region mapping GBIF to System Keys


            #endregion
        }

        private static LinkElement createLinkELementIfNotExist(
            MappingManager mappingManager,
            long id,
            string name,
            LinkElementType type,
            LinkElementComplexity complexity)
        {
            LinkElement element = mappingManager.GetLinkElement(id, type);

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
    }
}