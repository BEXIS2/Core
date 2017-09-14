using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Bam.UI.Helpers
{
    public class Helper
    {
        public static int CountRelations(long sourcePartyId, PartyRelationshipType partyRelationshipType)
        {
            PartyManager partyManager = new PartyManager();
            var cnt = partyManager.RepoPartyRelationships.Query(item => (item.PartyRelationshipType != null && item.PartyRelationshipType.Id == partyRelationshipType.Id)
                                      && (item.FirstParty != null && (item.FirstParty.Id == sourcePartyId) || (item.SecondParty.Id == sourcePartyId))
                                       && (item.EndDate >= DateTime.Now)).Count();
            return cnt;
        }

        public static string GetDisplayName(PartyRelationshipType partyRelatinshipType)
        {
            return (string.IsNullOrWhiteSpace(partyRelatinshipType.DisplayName) ? partyRelatinshipType.Title : partyRelatinshipType.DisplayName);
        }

        public static String ValidateRelationships(IEnumerable<PartyRelationshipType> requiredPartyRelationTypes, long partyId)
        {
            var partyManager = new PartyManager();
            var partyRelations = partyManager.RepoPartyRelationships.Get(cc => cc.FirstParty.Id == partyId);
            String messages = "";
            foreach (var requiredPartyRelationType in requiredPartyRelationTypes)
            {
                if (partyRelations.Where(cc => cc.PartyRelationshipType.Id == requiredPartyRelationType.Id).Count() < requiredPartyRelationType.MinCardinality)
                    messages += (String.Format("<br/>{0} relationship type '{1}'.", requiredPartyRelationType.MinCardinality, requiredPartyRelationType.DisplayName));
            }
            if (!string.IsNullOrEmpty(messages))
                messages = "these relationship types are required : " + messages;
            return messages;
        }
        public static String ValidateRelationships(long partyId)
        {
            var partyManager = new PartyManager();
            var party = partyManager.Repo.Get(partyId);
            var requiredPartyRelationTypes = new PartyRelationshipTypeManager().GetAllPartyRelationshipTypes(party.PartyType.Id).Where(cc => cc.MinCardinality > 0);
            var partyRelations = partyManager.RepoPartyRelationships.Get(cc => cc.FirstParty.Id == party.Id);
            String messages = "";
            foreach (var requiredPartyRelationType in requiredPartyRelationTypes)
            {
                if (partyRelations.Where(cc => cc.PartyRelationshipType.Id == requiredPartyRelationType.Id).Count() < requiredPartyRelationType.MinCardinality)
                    messages += (String.Format("<li>{0} more relationship type of '{1}'.</li>", requiredPartyRelationType.MinCardinality - partyRelations.Where(cc => cc.PartyRelationshipType.Id == requiredPartyRelationType.Id).Count(), requiredPartyRelationType.DisplayName));
            }
            if (!string.IsNullOrEmpty(messages))
                messages = "These relationship types are required : <ul>" + messages + "</ul>";
            return messages;
        }
    }

    public class BAMSeedDataGenerator
    {
        public static void GenerateSeedData()
        {
            createSecuritySeedData();
            ImportPartyTypes();
        }
        private static void createSecuritySeedData()
        {
            // Javad:
            // 1) all the create operations should check for existence of the record
            // 2) failure on creating any record should rollback the whole seed data generation. It is one transaction.
            // 3) failues should throw an exception with enough information to pin point the root cause
            // 4) only seed data related to the functions of this modules should be genereated here.
            // BUG: seed data creation is not working because of the changes that were done in the entities and services.
            // TODO: reimplement the seed data creation method.

            //#region Security

            //// Tasks
            var operationManager = new OperationManager();
            var featureManager = new FeatureManager();

            var root = featureManager.FindRoots().FirstOrDefault();

            var bamFeature = featureManager.Create("Business administration module (BAM)", "", root);

            var partyFeature = featureManager.Create("Party", "", bamFeature);
            var partyOperation = operationManager.Create("BAM", "Party", "*", partyFeature);
            var partyServiceOperation = operationManager.Create("BAM", "PartyService", "*");
        }

        /// <summary>
        /// Update rules:
        /// Comparison for update is by the title of elements: title of elements are not editable
        /// if title of an element is changed because remove is forbiden here ,  it adds it as a new element and the old one will remain there
        /// 
        /// </summary>
        private static void ImportPartyTypes()
        {
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            var filePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("BAM"), "partyTypes.xml");
            XDocument xDoc = XDocument.Load(filePath);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xDoc.CreateReader());
            var partyTypesNodeList = xmlDoc.SelectNodes("//PartyTypes");
            if (partyTypesNodeList.Count > 0)
                foreach (XmlNode partyTypeNode in partyTypesNodeList[0].ChildNodes)
                {
                    //Convert xmAttributeCollection to list to skipt the case sensitive and null problems
                    var attributes = new List<XmlAttribute>();
                    foreach (XmlAttribute att in partyTypeNode.Attributes)
                    {
                        attributes.Add(att);
                    }
                    var title = GetAttributeValue(attributes, "Name", true);
                    var displayName = GetAttributeValue(attributes, "DisplayName", false);
                    var partyType = partyTypeManager.Repo.Get(item => item.Title == title).FirstOrDefault();
                    //If there is not such a party type
                    if (partyType == null)
                    {
                        var partyStatusTypes = new List<PartyStatusType>();
                        partyStatusTypes.Add(new PartyStatusType() { Name = "Created", Description = "" });
                        partyType = partyTypeManager.Create(title, "Imported from partyTypes.xml", displayName, partyStatusTypes);
                        var customAttrs = new List<PartyCustomAttribute>();
                        foreach (XmlNode customAttrNode in partyTypeNode.ChildNodes)
                        {
                            var customAttrNodeAttributes = new List<XmlAttribute>();
                            foreach (XmlAttribute att in customAttrNode.Attributes)
                            {
                                customAttrNodeAttributes.Add(att);
                            }
                            PartyCustomAttribute partyCustomAttr = ParsePartyCustomAttribute(customAttrNodeAttributes);

                            customAttrs.Add(new PartyCustomAttribute()
                            {
                                DataType = partyCustomAttr.DataType,
                                Description = partyCustomAttr.Description,
                                IsMain = partyCustomAttr.IsMain,
                                IsUnique = partyCustomAttr.IsUnique,
                                IsValueOptional = partyCustomAttr.IsValueOptional,
                                Name = partyCustomAttr.Name,
                                PartyType = partyType,
                                ValidValues = partyCustomAttr.ValidValues,
                                DisplayName=partyCustomAttr.DisplayName
                            });
                        }
                        if (!customAttrs.Any(c => c.IsMain))
                            customAttrs[0].IsMain = true;
                        foreach (var customAttr in customAttrs)
                            partyTypeManager.CreatePartyCustomAttribute(customAttr);
                    }
                    else
                    {
                        var customAttrs = new List<PartyCustomAttribute>();
                        foreach (XmlNode customAttrNode in partyTypeNode.ChildNodes)
                        {
                            var attributesList = new List<XmlAttribute>();
                            foreach (XmlAttribute att in customAttrNode.Attributes)
                            {
                                attributesList.Add(att);
                            }
                          
                            var customAttrName =GetAttributeValue(attributesList,"Name", true);
                            //create new custom attribute if there is not such a name
                            if (!partyType.CustomAttributes.Any(item => item.Name == customAttrName))
                            {
                                var customAttrNodeAttributes = new List<XmlAttribute>();
                                foreach (XmlAttribute att in customAttrNode.Attributes)
                                {
                                    customAttrNodeAttributes.Add(att);
                                }

                                PartyCustomAttribute partyCustomAttr = ParsePartyCustomAttribute(customAttrNodeAttributes);
                                customAttrs.Add(new PartyCustomAttribute()
                                {
                                    DataType = partyCustomAttr.DataType,
                                    Description = partyCustomAttr.Description,
                                    IsMain = partyCustomAttr.IsMain,
                                    IsUnique = partyCustomAttr.IsUnique,
                                    IsValueOptional = partyCustomAttr.IsValueOptional,
                                    Name = customAttrName,
                                    PartyType = partyType,
                                    ValidValues = partyCustomAttr.ValidValues,
                                    DisplayName=partyCustomAttr.DisplayName
                                });
                            }
                        }
                        if (!customAttrs.Any(c => c.IsMain) && !partyType.CustomAttributes.Any(c => c.IsMain))
                            throw new Exception("There is no main field. Each party type needs at least one main field.");
                        foreach (var customAttr in customAttrs)
                            partyTypeManager.CreatePartyCustomAttribute(customAttr);
                    }

                }
            var partyRelationshipTypesNodeList = xmlDoc.SelectNodes("//PartyRelationshipTypes");
            if (partyRelationshipTypesNodeList.Count > 0)

                foreach (XmlNode partyRelationshipTypesNode in partyRelationshipTypesNodeList[0].ChildNodes)
                {
                    var customAttrNodeAttributes = new List<XmlAttribute>();
                    foreach (XmlAttribute att in partyRelationshipTypesNode.Attributes)
                    {
                        customAttrNodeAttributes.Add(att);
                    }
                    var partyRelationshipTypeManager = new PartyRelationshipTypeManager();
                    var title = GetAttributeValue(customAttrNodeAttributes, "Name", true);
                    var displayName = GetAttributeValue(customAttrNodeAttributes, "DisplayName", false);
                    var description = GetAttributeValue(customAttrNodeAttributes, "Description", false);
                    var indicatesHierarchy = GetAttributeValue(customAttrNodeAttributes, "IndicatesHierarchy", true);// false;
                    var maxCardinality = GetAttributeValue(customAttrNodeAttributes, "MaxCardinality", true); // -1 
                    var minCardinality = GetAttributeValue(customAttrNodeAttributes, "MinCardinality", true); // 0 

                    //Import party type pairs
                    var partyTypePairs = new List<PartyTypePair>();
                    foreach (XmlNode partyTypesPairNode in partyRelationshipTypesNode.ChildNodes[0].ChildNodes)
                    {
                        var partyTypesPairNodeAttributes = new List<XmlAttribute>();
                        foreach (XmlAttribute att in partyTypesPairNode.Attributes)
                        {
                            partyTypesPairNodeAttributes.Add(att);
                        }
                        var allowedSourceTitle = GetAttributeValue(partyTypesPairNodeAttributes, "AllowedSource", true);
                        var allowedTargetTitle = GetAttributeValue(partyTypesPairNodeAttributes, "AllowedTarget", true);
                        var allowedSource = partyTypeManager.Repo.Get(item => item.Title.ToLower() == allowedSourceTitle.ToLower()).FirstOrDefault();
                        if (allowedSource == null)
                            throw new Exception("Error in importing party relationship types ! \r\n " + allowedSourceTitle + " is not a party type!!");
                        var allowedTarget = partyTypeManager.Repo.Get(item => item.Title.ToLower() == allowedTargetTitle.ToLower()).FirstOrDefault();
                        if (allowedTarget == null)
                            throw new Exception("Error in importing party relationship types ! \r\n " + allowedTargetTitle + " is not a party type!!");

                        var typePairTitle = GetAttributeValue(partyTypesPairNodeAttributes, "Title", true);
                        var typePairDescription = GetAttributeValue(partyTypesPairNodeAttributes, "Description", false);
                        var typePairDefault = GetAttributeValue(partyTypesPairNodeAttributes, "Default", true);

                        partyTypePairs.Add(new PartyTypePair()
                        {
                            AllowedSource = allowedSource,
                            AllowedTarget = allowedTarget,
                            Description = typePairDescription,
                            Title = typePairTitle,
                            PartyRelationShipTypeDefault = typePairDefault == null ? true : Convert.ToBoolean(typePairDefault)

                        });
                    }

                    var partyRelationshipType = partyRelationshipTypeManager.Repo.Get(item => item.Title == title).FirstOrDefault();
                    //If there is not such a party relationship type 
                    //It is mandatory to create at least one party type pair when we are creating a party type relation
                    //
                    if (partyRelationshipType == null)
                        partyRelationshipType = partyRelationshipTypeManager.Create(title, displayName, description, (indicatesHierarchy == null ? false : Convert.ToBoolean(indicatesHierarchy)), maxCardinality == null ? -1 : int.Parse(maxCardinality), minCardinality == null ? 0 : int.Parse(minCardinality), partyTypePairs.First().PartyRelationShipTypeDefault, partyTypePairs.First().AllowedSource, partyTypePairs.First().AllowedTarget,
        partyTypePairs.First().Title, partyTypePairs.First().Description);
                    else
                    {
                        partyRelationshipType = partyRelationshipTypeManager.Update(partyRelationshipType.Id, title, description, (indicatesHierarchy == null ? false : Convert.ToBoolean(indicatesHierarchy)), maxCardinality == null ? -1 : int.Parse(maxCardinality), minCardinality == null ? 0 : int.Parse(minCardinality));
                        UpdateOrCreatePartyTypePair(partyTypePairs.First(), partyRelationshipType, partyRelationshipTypeManager);
                    }
                    //If there are more than one partyTypepair exist
                    //if (partyTypePairs.Count() > 1)
                    foreach (var partyTypePair in partyTypePairs.Where(item => item != partyTypePairs.First()))
                        UpdateOrCreatePartyTypePair(partyTypePair, partyRelationshipType, partyRelationshipTypeManager);// 
                }

        }

        private static PartyCustomAttribute ParsePartyCustomAttribute(List<XmlAttribute> attributes)
        {
            var customAttrType = GetAttributeValue(attributes, "type", false);
            var description = GetAttributeValue(attributes, "description", false);
            var validValues = GetAttributeValue(attributes, "validValues", false);
            var isValueOptional = GetAttributeValue(attributes, "isValueOptional", true);
            var isUnique = GetAttributeValue(attributes, "isUnique", true);
            var isMain = GetAttributeValue(attributes, "isMain", true);
            var displayName = GetAttributeValue(attributes, "DisplayName", false);

            return new PartyCustomAttribute()
            {
                DataType = customAttrType,
                Description = description,
                IsMain = isMain == null ? false : Convert.ToBoolean(isMain),
                IsUnique = isUnique == null ? false : Convert.ToBoolean(isUnique),
                IsValueOptional = isValueOptional == null ? true : Convert.ToBoolean(isValueOptional),
                Name = GetAttributeValue(attributes, "Name", true),
                ValidValues = validValues,
                DisplayName=displayName
            };
        }

        private static string GetAttributeValue(IEnumerable<XmlAttribute> attributes, string name, bool returnNull)
        {
            var attr = attributes
                 .FirstOrDefault(xa =>
                     string.Equals(xa.Name,
                                   name,
                                   StringComparison.InvariantCultureIgnoreCase));
            if (attr != null)
                return attr.Value;
            else if (returnNull && attr == null)
                return null;
            else
                return "";

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partyTypePair"></param>
        /// <param name="partyRelationshipType"></param>
        /// <param name="partyRelationshipTypeManager"></param>
        private static void UpdateOrCreatePartyTypePair(PartyTypePair partyTypePair, PartyRelationshipType partyRelationshipType, PartyRelationshipTypeManager partyRelationshipTypeManager)
        {
            var entity = partyRelationshipTypeManager.RepoPartyTypePair.Get(item => item.Title == partyTypePair.Title && item.PartyRelationshipType.Id == partyRelationshipType.Id).FirstOrDefault();
            if (entity != null)
                partyRelationshipTypeManager.UpdatePartyTypePair(entity.Id, partyTypePair.Title, partyTypePair.AllowedSource, partyTypePair.AllowedTarget, partyTypePair.Description, partyTypePair.PartyRelationShipTypeDefault, entity.PartyRelationshipType);
            else
                partyRelationshipTypeManager.AddPartyTypePair(partyTypePair.Title, partyTypePair.AllowedSource, partyTypePair.AllowedTarget, partyTypePair.Description, partyTypePair.PartyRelationShipTypeDefault, partyRelationshipType);
        }
    }
    //Check duplicate and add edit 
}