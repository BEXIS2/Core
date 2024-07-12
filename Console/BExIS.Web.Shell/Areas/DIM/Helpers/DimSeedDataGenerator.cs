using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Entities.Publications;
using BExIS.Dim.Helpers;
using BExIS.Dim.Services;
using BExIS.Dim.Services.Mappings;
using BExIS.Dlm.Entities.Meanings;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Meanings;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Modules.Dim.UI.Helper;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Vaiona.Persistence.Api;
using Vaiona.Web.Mvc.Modularity;
using static BExIS.Modules.Dim.UI.Controllers.ConceptOutController;

namespace BExIS.Modules.Dim.UI.Helpers
{
    public class DimSeedDataGenerator : IModuleSeedDataGenerator
    {
        public void Dispose()
        {
            // nothing to do for now...
        }

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
                if (!operationManager.Exists("dim", "Publish", "*")) operationManager.Create("DIM", "Publish", "*");
                if (!operationManager.Exists("dim", "mappingOut", "*")) operationManager.Create("DIM", "Publish", "*");
                if (!operationManager.Exists("dim", "conceptOut", "*")) operationManager.Create("DIM", "Publish", "*");
                if (!operationManager.Exists("dim", "linkElementTypeOut", "*")) operationManager.Create("DIM", "Publish", "*");

                #endregion public available

                #endregion SECURITY

                #region EXPORT

                submissionManager.Load();

                createMetadataStructureRepoMaps();

                #endregion EXPORT

                #region MAPPING

                createMappings();

                createDataCiteMappingConcept();

                createGBIFDWCMappingConcept();

                createFunctionConcept();

                #endregion MAPPING

                #region meanings for GBIFDWC

                createMeaningsForGBIFDWC();

                createReleationships();

                #endregion meanings for GBIFDWC
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

        private void createMeaningsForGBIFDWC()
        {
            using (var meaningManager = new MeaningManager())
            {
                ExternalLink prefix = new ExternalLink();
                ExternalLink releation = new ExternalLink();

                // prefix
                if (!meaningManager.getPrefixes().Any(p => p.Name.Equals("dwc")))
                {
                    prefix.Name = "dwc";
                    prefix.URI = "http://rs.tdwg.org/dwc/terms/";
                    prefix.Type = ExternalLinkType.prefix;
                    prefix.Prefix = null;
                    prefix.prefixCategory = null;

                    prefix = meaningManager.addExternalLink(prefix);
                }
                else
                {
                    prefix = meaningManager.getPrefixes().FirstOrDefault(p => p.Name.Equals("dwc"));
                }

                // releation (hasDwcTerm)
                if (!meaningManager.getExternalLinks().Any(p => p.Name.Equals("hasDwcTerm")))
                {
                    releation.Name = "hasDwcTerm";
                    releation.URI = "na";
                    releation.Type = ExternalLinkType.relationship;
                    releation.Prefix = null;
                    releation.prefixCategory = null;

                    releation = meaningManager.addExternalLink(releation);
                }
                else
                {
                    releation = meaningManager.getPrefixes().FirstOrDefault(p => p.Name.Equals("hasDwcTerm"));
                }

                // links & meanings
                List<string> links = new List<string>();
                links.Add("occurrenceID");
                links.Add("basisOfRecord");
                links.Add("scientificName");
                links.Add("eventDate");
                links.Add("countryCode");
                links.Add("taxonRank");
                links.Add("kingdom");
                links.Add("decimalLatitude");
                links.Add("decimalLongitude");
                links.Add("geodeticDatum");
                links.Add("coordinateUncertaintyInMeters");
                links.Add("individualCount");
                links.Add("organismQuantity");
                links.Add("organismQuantityType");
                links.Add("informationWithheld");
                links.Add("dataGeneralizations");
                links.Add("eventTime");
                links.Add("country");
                links.Add("eventID");
                links.Add("eventDate");
                links.Add("samplingProtocol");
                links.Add("samplingSizeUnit");
                links.Add("samplingSizeValue");
                links.Add("parentEventID");
                links.Add("samplingEffort");
                links.Add("locationID");
                links.Add("footprintWKT");
                links.Add("occurrenceStatus");

                foreach (var l in links)
                {
                    ExternalLink link = new ExternalLink();
                    if (!meaningManager.getExternalLinks().Any(p => p.Name.Equals(l)))
                    {
                        link.Name = l;
                        link.URI = l;
                        link.Type = ExternalLinkType.vocabulary;
                        link.Prefix = prefix;
                        link.prefixCategory = null;

                        link = meaningManager.addExternalLink(link);

                        if (link.Id > 0)
                        {
                            // create meaning
                            var linkList = new List<ExternalLink>();
                            linkList.Add(link);

                            Meaning meaning = new Meaning();
                            if (meaningManager.getMeanings().Any(m => m.Name.Equals(l)))
                            {
                                meaning = meaningManager.getMeanings().FirstOrDefault(m => m.Name.Equals(l));
                            }

                            meaning.Name = l;
                            meaning.Selectable = true;
                            meaning.Approved = true;
                            meaning.ExternalLinks.Add(new MeaningEntry(releation, linkList));

                            meaningManager.addMeaning(meaning);
                        }
                    }
                }
            }
        }

        private void createReleationships()
        {
            using (var meaningManager = new MeaningManager())
            {
                ExternalLink prefix = new ExternalLink();
                ExternalLink releation = new ExternalLink();

                // prefix
                string n = "i-adopt";
                if (!meaningManager.getPrefixes().Any(p => p.Name.Equals(n)))
                {
                    prefix.Name = n;
                    prefix.URI = "https://i-adopt.github.io/#/";
                    prefix.Type = ExternalLinkType.prefix;
                    prefix.Prefix = null;
                    prefix.prefixCategory = null;

                    prefix = meaningManager.addExternalLink(prefix);
                }
                else
                {
                    prefix = meaningManager.getPrefixes().FirstOrDefault(p => p.Name.Equals(n));
                }

                List<string> releationshiptypes = new List<string>();
                releationshiptypes.Add("hasContextObject");
                releationshiptypes.Add("hasMatrix");
                releationshiptypes.Add("hasObjectOfInterest");
                releationshiptypes.Add("hasProperty");

                foreach (var c in releationshiptypes)
                {
                    if (!meaningManager.getExternalLinks().Any(p => p.Name.Equals(c)))
                    {
                        releation = new ExternalLink();
                        releation.Name = c;
                        releation.URI = c;
                        releation.Type = ExternalLinkType.relationship;
                        releation.Prefix = prefix;
                        releation.prefixCategory = null;

                        releation = meaningManager.addExternalLink(releation);
                    }
                }
            }
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

        private void createDataCiteMappingConcept()
        {
            using (var conceptManager = new ConceptManager())
            {
                MappingConcept mappingConcept;
                try
                {
                    mappingConcept = conceptManager.FindByName("datacite");

                }
                catch (Exception e)
                {
                    mappingConcept = conceptManager.CreateMappingConcept("datacite", "The concept for DIO management at DataCite.", "https://schema.datacite.org/meta/kernel-4.5/", "");
                }

                List<MappingKey> mappingKeys;
                try
                {
                    mappingKeys = conceptManager.MappingKeyRepo.Query(k => k.Concept.Id.Equals(mappingConcept.Id)).ToList();
                }
                catch(Exception e)
                {
                    mappingKeys = new List<MappingKey>();
                }

                #region mandatory

                // identifier(s) (id: 1)

                // type
                if (!mappingKeys.Any(k => k.XPath.Equals("data/type")))
                    conceptManager.CreateMappingKey("Type", "", "", false, false, "data/type", mappingConcept);

                // event
                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/event")))
                    conceptManager.CreateMappingKey("Event", "", "", false, false, "data/attributes/event", mappingConcept);

                // creator(s) (id: 2)
                MappingKey creators = null;
                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/creators")))
                    creators = conceptManager.CreateMappingKey("Creators", "", "https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/creator/#id1", false, true, "data/attributes/creators", mappingConcept);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/creators/name")))
                    conceptManager.CreateMappingKey("Name", "", "", false, false, "data/attributes/creators/name", mappingConcept, creators);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/creators/givenName")))
                    conceptManager.CreateMappingKey("GivenName", "", "", true, false, "data/attributes/creators/givenName", mappingConcept, creators);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/creators/familyName")))
                    conceptManager.CreateMappingKey("FamilyName", "", "", true, false, "data/attributes/creators/familyName", mappingConcept, creators);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/creators/nameType")))
                    conceptManager.CreateMappingKey("NameType", "", "", true, false, "data/attributes/creators/nameType", mappingConcept, creators);

                // title(s) (id: 3)
                MappingKey titles = null;
                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/titles")))
                    titles = conceptManager.CreateMappingKey("Titles", "", "https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/title/#id1", false, true, "data/attributes/titles", mappingConcept);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/titles/title")))
                    conceptManager.CreateMappingKey("Title", "", "", false, false, "data/attributes/titles/title", mappingConcept, titles);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/titles/lang")))
                    conceptManager.CreateMappingKey("Lang", "", "", true, false, "data/attributes/titles/lang", mappingConcept, titles);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/titles/titleType")))
                    conceptManager.CreateMappingKey("TitleType", "", "", true, false, "data/attributes/titles/titleType", mappingConcept, titles);

                // publisher (id: 4)
                MappingKey publisher = null;
                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/publisher")))
                    publisher = conceptManager.CreateMappingKey("Publisher", "", "https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/publisher/#id1", false, true, "data/attributes/publisher", mappingConcept);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/publisher/name")))
                    conceptManager.CreateMappingKey("Name", "", "", false, false, "data/attributes/publisher/name", mappingConcept, publisher);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/publisher/publisherIdentifier")))
                    conceptManager.CreateMappingKey("PublisherIdentifier", "", "", true, false, "data/attributes/publisher/publisherIdentifier", mappingConcept, publisher);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/publisher/publisherIdentifierScheme")))
                    conceptManager.CreateMappingKey("PublisherIdentifierScheme", "", "", false, false, "data/attributes/publisher/publisherIdentifierScheme", mappingConcept, publisher);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/publisher/schemeUri")))
                    conceptManager.CreateMappingKey("SchemeUri", "", "", true, false, "data/attributes/publisher/schemeUri", mappingConcept, publisher);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/publisher/lang")))
                    conceptManager.CreateMappingKey("Language", "", "", true, false, "data/attributes/publisher/lang", mappingConcept, publisher);

                // publicationYear (id: 5)
                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/publicationYear")))
                    conceptManager.CreateMappingKey("PublicationYear", "", "https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/publicationyear/#id1", false, false, "data/attributes/publicationYear", mappingConcept);

                // resourceType (id: 10)
                MappingKey types = null;
                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/types")))
                    types = conceptManager.CreateMappingKey("Types", "", "", false, true, "data/attributes/types", mappingConcept);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/types/resourceTypeGeneral")))
                    conceptManager.CreateMappingKey("ResourceTypeGeneral", "", "https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/resourcetype/#a-resourcetypegeneral", false, false, "data/attributes/types/resourceTypeGeneral", mappingConcept, types);


                #endregion

                #region recommended and optional

                // subject(s) (id: 6)
                MappingKey subjects = null;
                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/subjects")))
                    subjects = conceptManager.CreateMappingKey("Subjects", "", "www.google.de", true, true, "data/attributes/subjects", mappingConcept);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/subjects/subject")))
                    conceptManager.CreateMappingKey("Subject", "", "", false, false, "data/attributes/subjects/subject", mappingConcept, subjects);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/subjects/subjectScheme")))
                    conceptManager.CreateMappingKey("SubjectScheme", "", "", true, false, "data/attributes/subjects/subjectScheme", mappingConcept, subjects);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/subjects/schemeUri")))
                    conceptManager.CreateMappingKey("SchemeUri", "", "", true, false, "data/attributes/subjects/schemeUri", mappingConcept, subjects);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/subjects/valueUri")))
                    conceptManager.CreateMappingKey("ValueUri", "", "", true, false, "data/attributes/subjects/valueUri", mappingConcept, subjects);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/subjects/lang")))
                    conceptManager.CreateMappingKey("Lang", "", "", true, false, "data/attributes/subjects/lang", mappingConcept, subjects);

                // contributor(s) (id: 7)
                MappingKey contributors = null;
                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/contributors")))
                    contributors = conceptManager.CreateMappingKey("Contributors", "", "https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/contributor/#id1", true, true, "data/attributes/contributors", mappingConcept);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/contributors/name")))
                    conceptManager.CreateMappingKey("Name", "", "", false, false, "data/attributes/contributors/name", mappingConcept, contributors);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/contributors/givenName")))
                    conceptManager.CreateMappingKey("GivenName", "", "", true, false, "data/attributes/contributors/givenName", mappingConcept, contributors);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/contributors/familyName")))
                    conceptManager.CreateMappingKey("FamilyName", "", "", true, false, "data/attributes/contributors/familyName", mappingConcept, contributors);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/contributors/nameType")))
                    conceptManager.CreateMappingKey("NameType", "", "", true, false, "data/attributes/contributors/nameType", mappingConcept, contributors);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/contributors/contributorType")))
                    conceptManager.CreateMappingKey("ContributorType", "", "", true, false, "data/attributes/contributors/contributorType", mappingConcept, contributors);

                // date(s) (id: 8)
                MappingKey dates = null;
                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/dates")))
                    dates = conceptManager.CreateMappingKey("Dates", "", "https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/date/#id1", true, true, "data/attributes/dates", mappingConcept);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/dates/date")))
                    conceptManager.CreateMappingKey("Date", "", "", false, false, "data/attributes/dates/date", mappingConcept, dates);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/dates/dateType")))
                    conceptManager.CreateMappingKey("DateType", "", "", false, false, "data/attributes/dates/dateType", mappingConcept, dates);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/dates/dateInformation")))
                    conceptManager.CreateMappingKey("DateInformation", "", "", true, false, "data/attributes/dates/dateInformation", mappingConcept, dates);


                // language (id: 9)
                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/language")))
                    conceptManager.CreateMappingKey("Language", "", "https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/language/#id1", true, false, "data/attributes/language", mappingConcept);

                // alternate identifiers (id: 11)
                MappingKey alternateIdentifiers = null;
                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/alternateIdentifiers")))
                    alternateIdentifiers = conceptManager.CreateMappingKey("AlternateIdentifiers", "", "https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/alternateidentifier/#id1", true, true, "data/attributes/alternateIdentifiers", mappingConcept);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/alternateIdentifiers/alternateIdentifierType")))
                    conceptManager.CreateMappingKey("AlternateIdentifierType", "", "", false, false, "data/attributes/alternateIdentifiers/alternateIdentifierType", mappingConcept, alternateIdentifiers);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/alternateIdentifiers/alternateIdentifier")))
                    conceptManager.CreateMappingKey("AlternateIdentifier", "", "", false, false, "data/attributes/alternateIdentifiers/alternateIdentifier", mappingConcept, alternateIdentifiers);

                // related identifiers (id: 12)
                MappingKey relatedIdentifiers = null;
                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/relatedIdentifiers")))
                    relatedIdentifiers = conceptManager.CreateMappingKey("RelatedIdentifiers", "", "https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/relatedidentifier/#id1", true, true, "data/attributes/relatedIdentifiers", mappingConcept);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/relatedIdentifiers/relatedIdentifier")))
                    conceptManager.CreateMappingKey("RelatedIdentifier", "", "", false, false, "data/attributes/relatedIdentifiers/relatedIdentifier", mappingConcept, relatedIdentifiers);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/relatedIdentifiers/relatedIdentifierType")))
                    conceptManager.CreateMappingKey("RelatedIdentifierType", "", "", false, false, "data/attributes/relatedIdentifiers/relatedIdentifierType", mappingConcept, relatedIdentifiers);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/relatedIdentifiers/relationType")))
                    conceptManager.CreateMappingKey("RelationType", "", "", false, false, "data/attributes/relatedIdentifiers/relationType", mappingConcept, relatedIdentifiers);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/relatedIdentifiers/relatedMetadataScheme")))
                    conceptManager.CreateMappingKey("RelatedMetadataScheme", "", "", true, false, "data/attributes/relatedIdentifiers/relatedMetadataScheme", mappingConcept, relatedIdentifiers);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/relatedIdentifiers/schemeURI")))
                    conceptManager.CreateMappingKey("SchemeURI", "", "", true, false, "data/attributes/relatedIdentifiers/schemeURI", mappingConcept, relatedIdentifiers);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/relatedIdentifiers/schemeType")))
                    conceptManager.CreateMappingKey("SchemeType", "", "", true, false, "data/attributes/relatedIdentifiers/schemeType", mappingConcept, relatedIdentifiers);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/relatedIdentifiers/resourceTypeGeneral")))
                    conceptManager.CreateMappingKey("ResourceTypeGeneral", "", "", true, false, "data/attributes/relatedIdentifiers/resourceTypeGeneral", mappingConcept, relatedIdentifiers);

                // size(s) (id: 13)
                MappingKey sizes = null;
                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/sizes")))
                    sizes = conceptManager.CreateMappingKey("Sizes", "", "https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/size/", true, false, "data/attributes/sizes", mappingConcept);

                // format(s) (id: 14)
                MappingKey formats = null;
                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/formats")))
                    formats = conceptManager.CreateMappingKey("Formats", "", "https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/format/", true, false, "data/attributes/formats", mappingConcept);

                // right(s) (id: 16)
                MappingKey rightsList = null;
                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/rightsList")))
                    rightsList = conceptManager.CreateMappingKey("RightsList", "", "https://datacite-metadata-schema.readthedocs.io/en/4.5/properties/rights/#id1", true, true, "data/attributes/rightsList", mappingConcept);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/rightsList/rights")))
                    conceptManager.CreateMappingKey("Rights", "", "", false, false, "data/attributes/rightsList/rights", mappingConcept, rightsList);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/rightsList/rightsUri")))
                    conceptManager.CreateMappingKey("RightsUri", "", "", true, false, "data/attributes/rightsList/rightsUri", mappingConcept, rightsList);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/rightsList/schemeUri")))
                    conceptManager.CreateMappingKey("SchemeUri", "", "", true, false, "data/attributes/rightsList/schemeUri", mappingConcept, rightsList);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/rightsList/rightsIdentifier")))
                    conceptManager.CreateMappingKey("RightsIdentifier", "", "", true, false, "data/attributes/rightsList/rightsIdentifier", mappingConcept, rightsList);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/rightsList/rightsIdentifierScheme")))
                    conceptManager.CreateMappingKey("RightsIdentifierScheme", "", "", true, false, "data/attributes/rightsList/rightsIdentifierScheme", mappingConcept, rightsList);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/rightsList/lang")))
                    conceptManager.CreateMappingKey("Lang", "", "", true, false, "data/attributes/rightsList/lang", mappingConcept, rightsList);

                // description(s) (id: 17)
                MappingKey descriptions = null;
                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/descriptions")))
                    descriptions = conceptManager.CreateMappingKey("Descriptions", "", "", true, true, "data/attributes/descriptions", mappingConcept);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/descriptions/lang")))
                    conceptManager.CreateMappingKey("Lang", "", "", true, false, "data/attributes/descriptions/lang", mappingConcept, descriptions);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/descriptions/description")))
                    conceptManager.CreateMappingKey("Description", "", "", false, false, "data/attributes/descriptions/description", mappingConcept, descriptions);

                if (!mappingKeys.Any(k => k.XPath.Equals("data/attributes/descriptions/descriptionType")))
                    conceptManager.CreateMappingKey("DescriptionType", "", "", false, false, "data/attributes/descriptions/descriptionType", mappingConcept, descriptions);

                #endregion
            }
        }

        private void createFunctionConcept()
        {
            using (var conceptManager = new ConceptManager())
            {
                // concept
                // check if concept exist
                var concept = conceptManager.MappingConceptRepository.Query(c => c.Name.Equals("Functions")).FirstOrDefault();

                var keys = new List<MappingKey>();

                if (concept == null) //if not create
                    concept = conceptManager.CreateMappingConcept("Functions", "...", "", "");
                else // if exist load available keys
                {
                    keys = conceptManager.MappingKeyRepo.Query(k => k.Concept.Id.Equals(concept.Id)).ToList();
                }

                // type
                if (!keys.Any(k => k.XPath.Equals("test/function/a")))
                    conceptManager.CreateMappingKey("Function A", "", "", false, false, "test/function/a", concept);

                if (!keys.Any(k => k.XPath.Equals("test/function/b")))
                    conceptManager.CreateMappingKey("Function B", "", "", false, false, "test/function/b", concept);

                if (!keys.Any(k => k.XPath.Equals("test/function/c")))
                    conceptManager.CreateMappingKey("Function C", "", "", false, false, "test/function/c", concept);
            }
        }

        private void createGBIFDWCMappingConcept()
        {
            using (var conceptManager = new ConceptManager())
            {
                var concept = conceptManager.MappingConceptRepository.Query(c => c.Name.Equals("GBIF")).FirstOrDefault();

                var keys = new List<MappingKey>();

                if (concept == null) //if not create
                    concept = conceptManager.CreateMappingConcept("GBIF", "The concept is needed to create a darwin core archive for GBIF.", "https://ipt.gbif.org/manual/en/ipt/latest/dwca-guide", @"Modules\DIM\concepts\gbif\eml.xsd");
                else // if exist load available keys
                {
                    keys = conceptManager.MappingKeyRepo.Query(k => k.Concept.Id.Equals(concept.Id)).ToList();
                }

                //alternateIdentifier
                if (!keys.Any(k => k.Name.Equals("alternateIdentifier")))
                    conceptManager.CreateMappingKey(
                        "alternateIdentifier",
                        "It is a Universally Unique Identifier (UUID) for the EML document and not for the dataset. This term is optional. A list of different identifiers can be supplied. E.g., 619a4b95-1a82-4006-be6a-7dbe3c9b33c5.",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-resource.html#alternateIdentifier",
                        false,
                        false,
                        "eml/dataset/alternateIdentifier",
                        concept);

                //title
                if (!keys.Any(k => k.Name.Equals("title")))
                    conceptManager.CreateMappingKey(
                        "title",
                        "A description of the resource that is being documented that is long enough to differentiate it from other similar resources. Multiple titles may be provided, particularly " +
                        "when trying to express the title in more than one language (use the \"xml: lang\" attribute to indicate the language if not English/en). E.g. Vernal pool amphibian density data, Isla Vista, 1990-1996.",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-resource.html#title",
                        false,
                        false,
                        "eml/dataset/title",
                        concept);

                //creator
                MappingKey creator = null;
                if (!keys.Any(k => k.Name.Equals("creator")))
                    creator = conceptManager.CreateMappingKey(
                        "creator",
                        "The resource creator is the person or organization responsible for creating the resource itself. See section “People and Organizations” for more details.",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-resource.html#creator",
                        false,
                        true,
                        "eml/dataset/creator",
                        concept);

                // creator/givenName
                if (!keys.Any(k => k.XPath.Equals("eml/dataset/creator/individualName/givenName")))
                    conceptManager.CreateMappingKey(
                        "givenName",
                        "Subfield of individualName field. The given name field can be used for the first name of the individual associated with the resource, or for any other names that are not intended to be alphabetized (as appropriate). E.g., Jonny",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-party.html#givenName",
                        true,
                        false,
                        "eml/dataset/creator/individualName/givenName",
                        concept, creator);

                // creator/surName
                if (!keys.Any(k => k.XPath.Equals("eml/dataset/creator/individualName/surName")))
                    conceptManager.CreateMappingKey(
                        "surName",
                        "Subfield of individualName field. The surname field is used for the last name of the individual associated with the resource. This is typically the family name of an individual, for example, the name by which s/he is referred to in citations. E.g. Carson",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-party.html#surName",
                        false,
                        false,
                        "eml/dataset/creator/individualName/surName",
                        concept, creator);

                //metadataProvider
                MappingKey metadataProvider = null;
                if (!keys.Any(k => k.Name.Equals("metadataProvider")))
                    metadataProvider = conceptManager.CreateMappingKey(
                        "metadataProvider",
                        "The metadataProvider is the person or organization responsible for providing documentation for the resource. See section “People and Organizations” for more details",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-resource.html#metadataProvider",
                        false,
                        true,
                        "eml/dataset/metadataProvider",
                        concept);

                // metadataProvider/givenName
                if (!keys.Any(k => k.XPath.Equals("eml/dataset/metadataProvider/individualName/givenName")))
                    conceptManager.CreateMappingKey(
                        "givenName",
                        "Subfield of individualName field. The given name field can be used for the first name of the individual associated with the resource, or for any other names that are not intended to be alphabetized (as appropriate). E.g., Jonny",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-party.html#givenName",
                        true,
                        false,
                        "eml/dataset/metadataProvider/individualName/givenName",
                        concept, metadataProvider);

                // metadataProvider/surName
                if (!keys.Any(k => k.XPath.Equals("eml/dataset/metadataProvider/individualName/surName")))
                    conceptManager.CreateMappingKey(
                        "surName",
                        "Subfield of individualName field. The surname field is used for the last name of the individual associated with the resource. This is typically the family name of an individual, for example, the name by which s/he is referred to in citations. E.g. Carson",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-party.html#surName",
                        false,
                        false,
                        "eml/dataset/metadataProvider/individualName/surName",
                        concept, metadataProvider);

                //pubDate
                if (!keys.Any(k => k.Name.Equals("pubDate")))
                    conceptManager.CreateMappingKey(
                        "pubDate",
                        "The date that the resource was published. The format should be represented as: CCYY, which represents a 4 digit year, or as CCYY-MM-DD, which denotes the full year, month, and day. Note that month and day are optional components. Formats must conform to ISO 8601. E.g. 2010-09-20.",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-resource.html#pubDate",
                        false,
                        false,
                        "eml/dataset/pubDate",
                        concept);

                //language
                if (!keys.Any(k => k.Name.Equals("language")))
                    conceptManager.CreateMappingKey(
                        "language",
                        "The language in which the resource (not the metadata document) is written. This can be a well-known language name, or one of the ISO language codes to be more precise. GBIF recommendation is to use the ISO language code (https://api.gbif.org/v1/enumeration/language). E.g., English.",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-resource.html#language",
                        true,
                        false,
                        "eml/dataset/language",
                        concept);

                //abstract
                if (!keys.Any(k => k.Name.Equals("abstract")))
                    conceptManager.CreateMappingKey(
                        "abstract",
                        "A brief overview of the resource that is being documented.",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-resource.html#abstract",
                        false,
                        false,
                        "eml/dataset/abstract/para",
                        concept);

                //intellectualRights
                if (!keys.Any(k => k.Name.Equals("intellectualRights")))
                    conceptManager.CreateMappingKey(
                        "intellectualRights",
                        "A rights management statement for the resource, or reference a service providing such information. Rights information encompasses Intellectual Property Rights (IPR), Copyright, and various Property Rights. In the case of a data set, rights might include requirements for use, requirements for attribution, or other requirements the owner would like to impose. " +
                        "E.g., © 2001 Regents of the University of California Santa Barbara. Free for use by all individuals provided that the owners are acknowledged in any use or publication.",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-resource.html#intellectualRights",
                        false,
                        false,
                        "eml/dataset/intellectualRights/para/ulink/citetitle",
                        concept);

                //keyword
                if (!keys.Any(k => k.Name.Equals("keyword")))
                    conceptManager.CreateMappingKey(
                        "keyword",
                        "This field names a keyword or key phrase that concisely describes the resource or is related to the resource. Each keyword field should contain one and only one keyword (i.e., keywords should not be separated by commas or other delimiters)." +
                        "Example(s): biodiversity",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-resource.html#keyword",
                        true,
                        false,
                        "eml/dataset/keywordSet/keyword",
                        concept);

                //geographicCoverage
                MappingKey geographicCoverage = null;
                if (!keys.Any(k => k.Name.Equals("geographicCoverage")))
                    geographicCoverage = conceptManager.CreateMappingKey(
                        "geographicCoverage",
                        "A container for spatial information about a resource; allows a bounding box for the overall coverage (in lat long), and also allows description of arbitrary polygons with exclusions.",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#geographicCoverage",
                        true,
                        true,
                        "eml/dataset/coverage/geographicCoverage",
                        concept);

                //geographicCoverage/geographicDescription
                if (!keys.Any(k => k.XPath.Equals("eml/dataset/coverage/geographicCoverage/geographicDescription")))
                    conceptManager.CreateMappingKey(
                        "geographicDescription",
                        "A short text description of a dataset’s geographic areal domain.A text description is especially important to provide a geographic setting when the extent of the dataset cannot be well described by the \"boundingCoordinates\".E.g., \"Manistee River watershed\", \"extent of 7 1/2 minute quads containing any property belonging to Yellowstone National Park\"",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#geographicDescription",
                        false,
                        false,
                        "eml/dataset/coverage/geographicCoverage/geographicDescription",
                        concept, geographicCoverage);

                //geographicCoverage/boundingCoordinates/westBoundingCoordinate
                if (!keys.Any(k => k.XPath.Equals("eml/dataset/coverage/geographicCoverage/boundingCoordinates/westBoundingCoordinate")))
                    conceptManager.CreateMappingKey(
                        "westBoundingCoordinate",
                        "Subfield of boundingCoordinates field covering the W margin of a bounding box. The longitude in decimal degrees of the western-most point of the bounding box that is being described. E.g., -18.25, +25, 45.24755.",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#westBoundingCoordinate",
                        false,
                        false,
                        "eml/dataset/coverage/geographicCoverage/boundingCoordinates/westBoundingCoordinate",
                        concept, geographicCoverage);

                //geographicCoverage/boundingCoordinates/eastBoundingCoordinate
                if (!keys.Any(k => k.XPath.Equals("eml/dataset/coverage/geographicCoverage/boundingCoordinates/eastBoundingCoordinate")))
                    conceptManager.CreateMappingKey(
                        "eastBoundingCoordinate",
                        "Subfield of boundingCoordinates field covering the E margin of a bounding box. The longitude in decimal degrees of the eastern-most point of the bounding box that is being described. E.g., -18.25, +25, 45.24755.",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#eastBoundingCoordinate",
                        false,
                        false,
                        "eml/dataset/coverage/geographicCoverage/boundingCoordinates/eastBoundingCoordinate",
                        concept, geographicCoverage);

                //geographicCoverage/boundingCoordinates/northBoundingCoordinate
                if (!keys.Any(k => k.XPath.Equals("eml/dataset/coverage/geographicCoverage/boundingCoordinates/northBoundingCoordinate")))
                    conceptManager.CreateMappingKey(
                        "northBoundingCoordinate",
                        "Subfield of boundingCoordinates field covering the N margin of a bounding box. The longitude in decimal degrees of the northern-most point of the bounding box that is being described. E.g., -18.25, +25, 65.24755.",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#northBoundingCoordinate",
                        false,
                        false,
                        "eml/dataset/coverage/geographicCoverage/boundingCoordinates/northBoundingCoordinate",
                        concept, geographicCoverage);

                //geographicCoverage/boundingCoordinates/southBoundingCoordinate
                if (!keys.Any(k => k.XPath.Equals("eml/dataset/coverage/geographicCoverage/boundingCoordinates/southBoundingCoordinate")))
                    conceptManager.CreateMappingKey(
                        "southBoundingCoordinate",
                        "Subfield of boundingCoordinates field covering the S margin of a bounding box. The longitude in decimal degrees of the southern-most point of the bounding box that is being described. E.g., -118.25, +25, 84.24755.",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#southBoundingCoordinate",
                        false,
                        false,
                        "eml/dataset/coverage/geographicCoverage/boundingCoordinates/southBoundingCoordinate",
                        concept, geographicCoverage);

                //taxonomicCoverage
                MappingKey taxonomicCoverage = null;
                if (!keys.Any(k => k.Name.Equals("taxonomicCoverage")))
                    taxonomicCoverage = conceptManager.CreateMappingKey(
                        "taxonomicCoverage",
                        "A container for taxonomic information about a resource. It includes a list of species names (or higher level ranks) from one or more classification systems. Please note the taxonomic classifications should not be nested, just listed one after the other.",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#TaxonomicCoverage",
                        true,
                        true,
                        "eml/dataset/coverage/taxonomicCoverage",
                        concept);

                if (!keys.Any(k => k.XPath.Equals("eml/dataset/coverage/taxonomicCoverage/generalTaxonomicCoverage")))
                    conceptManager.CreateMappingKey(
                        "generalTaxonomicCoverage",
                        "Taxonomic Coverage is a container for taxonomic information about a resource. It includes a list of species names (or higher level ranks) from one or more classification systems. A description of the range of taxa addressed in the data set or collection. Use a simple comma separated list of taxa. " +
                        "E.g., \"All vascular plants were identified to family or species, mosses and lichens were identified as moss or lichen.\"",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#generalTaxonomicCoverage",
                        true,
                        false,
                        "eml/dataset/coverage/taxonomicCoverage/generalTaxonomicCoverage",
                        concept, taxonomicCoverage);

                if (!keys.Any(k => k.XPath.Equals("eml/dataset/coverage/taxonomicCoverage/taxonomicClassification/taxonRankName")))
                    conceptManager.CreateMappingKey(
                        "taxonRankName",
                        "The name of the taxonomic rank for which the Taxon rank value is provided. E.g., phylum, class, genus, species.",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#taxonRankName",
                        true,
                        false,
                        "eml/dataset/coverage/taxonomicCoverage/taxonomicClassification/taxonRankName",
                        concept, taxonomicCoverage);

                if (!keys.Any(k => k.XPath.Equals("eml/dataset/coverage/taxonomicCoverage/taxonomicClassification/taxonRankValue")))
                    conceptManager.CreateMappingKey(
                        "taxonRankValue",
                        "The name representing the taxonomic rank of the taxon being described. E.g. Acer would be an example of a genus rank value, and rubrum would be an example of a species rank value, together indicating the common name of red maple. It is recommended to start with Kingdom and include ranks down to the most detailed level possible.",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#taxonRankName",
                        false,
                        false,
                        "eml/dataset/coverage/taxonomicCoverage/taxonomicClassification/taxonRankValue",
                        concept, taxonomicCoverage);

                if (!keys.Any(k => k.XPath.Equals("eml/dataset/coverage/taxonomicCoverage/taxonomicClassification/commonName")))
                    conceptManager.CreateMappingKey(
                        "commonName",
                        "Applicable common names; these common names may be general descriptions of a group of organisms if appropriate. E.g., invertebrates, waterfowl.",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-coverage.html#commonName",
                        true,
                        false,
                        "eml/dataset/coverage/taxonomicCoverage/taxonomicClassification/commonName",
                        concept, taxonomicCoverage);

                //contact
                MappingKey contact = null;
                if (!keys.Any(k => k.Name.Equals("contact")))
                    contact = conceptManager.CreateMappingKey(
                        "contact",
                        "The contact field contains contact information for this dataset. This is the person or institution to contact with questions about the use, interpretation of a data set. See section “People and Organizations” for more details.",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-resource.html#contact",
                        false,
                        true,
                        "eml/dataset/contact",
                        concept);

                // contact/givenName
                if (!keys.Any(k => k.XPath.Equals("eml/dataset/contact/individualName/givenName")))
                    conceptManager.CreateMappingKey(
                        "givenName",
                        "Subfield of individualName field. The given name field can be used for the first name of the individual associated with the resource, or for any other names that are not intended to be alphabetized (as appropriate). E.g., Jonny",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-party.html#givenName",
                        true,
                        false,
                        "eml/dataset/contact/individualName/givenName",
                        concept, contact);

                // contact/surName
                if (!keys.Any(k => k.XPath.Equals("eml/dataset/contact/individualName/surName")))
                    conceptManager.CreateMappingKey(
                        "surName",
                        "Subfield of individualName field. The surname field is used for the last name of the individual associated with the resource. This is typically the family name of an individual, for example, the name by which s/he is referred to in citations. E.g. Carson",
                        "https://sbclter.msi.ucsb.edu/external/InformationManagement/EML_211_schema/docs/eml-2.1.1/eml-party.html#surName",
                        true,
                        false,
                        "eml/dataset/contact/individualName/surName",
                        concept, contact);
            }
        }

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

        #region createSystemKeyMappings

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

        #endregion createSystemKeyMappings

        #region createPartyTypeMappings

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
                        PartyRelationshipType partyRelationshipType = partyReleationships.FirstOrDefault(p => p.Title.Equals(ModuleManager.GetModuleSettings("bam").GetValueByKey("OwnerPartyRelationshipType")));
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
                        PartyRelationshipType partyRelationshipType = partyReleationships.FirstOrDefault(p => p.Title.Equals(ModuleManager.GetModuleSettings("bam").GetValueByKey("OwnerPartyRelationshipType")));
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

        #endregion createPartyTypeMappings

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
    }
}