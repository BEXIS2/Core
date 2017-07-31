using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
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
    }

    public class BAMSeedDataGenerator
    {
        public static void GenerateSeedData()
        {
            ImportPartyTypes();
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
                    var title = partyTypeNode.Attributes["Name"].Value;
                    var displayName = partyTypeNode.Attributes["DisplayName"] == null ? "" : partyTypeNode.Attributes["DisplayName"].Value;
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
                            var customAttrType = customAttrNode.Attributes["type"] == null ? "String" : customAttrNode.Attributes["type"].Value;
                            var description = customAttrNode.Attributes["description"] == null ? "" : customAttrNode.Attributes["description"].Value;
                            var validValues = customAttrNode.Attributes["validValues"] == null ? "" : customAttrNode.Attributes["validValues"].Value;
                            var isValueOptional = customAttrNode.Attributes["isValueOptional"] == null ? true : Convert.ToBoolean(customAttrNode.Attributes["isValueOptional"].Value);
                            var isUnique = customAttrNode.Attributes["isUnique"] == null ? false : Convert.ToBoolean(customAttrNode.Attributes["isUnique"].Value);
                            var isMain = customAttrNode.Attributes["isMain"] == null ? false : Convert.ToBoolean(customAttrNode.Attributes["isMain"].Value);
                            customAttrs.Add(new PartyCustomAttribute()
                            {
                                DataType = customAttrType,
                                Description = description,
                                IsMain = isMain,
                                IsUnique = isUnique,
                                IsValueOptional = isValueOptional,
                                Name = customAttrNode.Attributes["Name"].Value,
                                PartyType = partyType,
                                ValidValues = validValues
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
                            var customAttrName = customAttrNode.Attributes["Name"].Value;
                            //create new custom attribute if there is not such a name
                            if (!partyType.CustomAttributes.Any(item => item.Name == customAttrName))
                            {
                                var customAttrType = customAttrNode.Attributes["type"] == null ? "String" : customAttrNode.Attributes["type"].Value;
                                var customAttrDescription = customAttrNode.Attributes["description"] == null ? "" : customAttrNode.Attributes["description"].Value;
                                var customAttrValidValues = customAttrNode.Attributes["validValues"] == null ? "" : customAttrNode.Attributes["validValues"].Value;
                                var customAttrIsValueOptional = customAttrNode.Attributes["isValueOptional"] == null ? true : Convert.ToBoolean(customAttrNode.Attributes["isValueOptional"].Value);
                                var customAttrIsUnique = customAttrNode.Attributes["IsUnique"] == null ? false : Convert.ToBoolean(customAttrNode.Attributes["IsUnique"].Value);
                                var customAttrIsMain = customAttrNode.Attributes["isMain"] == null ? false : Convert.ToBoolean(customAttrNode.Attributes["isMain"].Value);
                                customAttrs.Add(new PartyCustomAttribute()
                                {
                                    DataType = customAttrType,
                                    Description = customAttrDescription,
                                    IsMain = customAttrIsMain,
                                    IsUnique = customAttrIsUnique,
                                    IsValueOptional = customAttrIsValueOptional,
                                    Name = customAttrName,
                                    PartyType = partyType,
                                    ValidValues = customAttrValidValues
                                });
                            }
                        }
                        if (!customAttrs.Any(c => c.IsMain))
                            throw new Exception("There is no main field. Each party type needs at least one main field.");
                        foreach (var customAttr in customAttrs)
                            partyTypeManager.CreatePartyCustomAttribute(customAttr);
                    }

                }
            var partyRelationshipTypesNodeList = xmlDoc.SelectNodes("//PartyRelationshipTypes");
            if (partyRelationshipTypesNodeList.Count > 0)

                foreach (XmlNode partyRelationshipTypesNode in partyRelationshipTypesNodeList[0].ChildNodes)
                {
                    var partyRelationshipTypeManager = new PartyRelationshipTypeManager();
                    var title = partyRelationshipTypesNode.Attributes["Name"].Value;
                    var displayName = partyRelationshipTypesNode.Attributes["DisplayName"] == null ? "" : partyRelationshipTypesNode.Attributes["DisplayName"].Value;
                    var description = partyRelationshipTypesNode.Attributes["Description"] == null ? "" : partyRelationshipTypesNode.Attributes["Description"].Value;
                    var indicatesHierarchy = partyRelationshipTypesNode.Attributes["IndicatesHierarchy"] == null ? false : Convert.ToBoolean(partyRelationshipTypesNode.Attributes["IndicatesHierarchy"].Value);
                    var maxCardinality = partyRelationshipTypesNode.Attributes["MaxCardinality"] == null ? -1 : int.Parse(partyRelationshipTypesNode.Attributes["MaxCardinality"].Value);
                    var minCardinality = partyRelationshipTypesNode.Attributes["MinCardinality"] == null ? 0 : int.Parse(partyRelationshipTypesNode.Attributes["MinCardinality"].Value);

                    //Import party type pairs
                    var partyTypePairs = new List<PartyTypePair>();
                    foreach (XmlNode partyTypesPairNode in partyRelationshipTypesNode.ChildNodes[0].ChildNodes)
                    {
                        var allowedSourceTitle = partyTypesPairNode.Attributes["AllowedSource"].Value;
                        var allowedTargetTitle = partyTypesPairNode.Attributes["AllowedTarget"].Value;
                        var allowedSource = partyTypeManager.Repo.Get(item => item.Title == allowedSourceTitle).FirstOrDefault();
                        if (allowedSource == null)
                            throw new Exception("Error in importing party relationship types ! \r\n " + allowedSourceTitle + " is not a party type!!");
                        var allowedTarget = partyTypeManager.Repo.Get(item => item.Title == allowedTargetTitle).FirstOrDefault();
                        if (allowedTarget == null)
                            throw new Exception("Error in importing party relationship types ! \r\n " + allowedTargetTitle + " is not a party type!!");

                        var typePairTitle = partyTypesPairNode.Attributes["Title"].Value;
                        var typePairDescription = partyTypesPairNode.Attributes["Description"] == null ? "" : partyTypesPairNode.Attributes["Description"].Value;
                        var typePairDefault = partyTypesPairNode.Attributes["Default"] == null ? false : Convert.ToBoolean(partyTypesPairNode.Attributes["Default"].Value);

                        partyTypePairs.Add(new PartyTypePair()
                        {
                            AllowedSource = allowedSource,
                            AllowedTarget = allowedTarget,
                            Description = typePairDescription,
                            Title = typePairTitle,
                            PartyRelationShipTypeDefault = typePairDefault

                        });
                    }

                    var partyRelationshipType = partyRelationshipTypeManager.Repo.Get(item => item.Title == title).FirstOrDefault();
                    //If there is not such a party relationship type 
                    //It is mandatory to create at least one party type pair when we are creating a party type relation
                    //
                    if (partyRelationshipType == null)
                        partyRelationshipType = partyRelationshipTypeManager.Create(title, displayName, description, indicatesHierarchy, maxCardinality, minCardinality, partyTypePairs.First().PartyRelationShipTypeDefault, partyTypePairs.First().AllowedSource, partyTypePairs.First().AllowedTarget,
        partyTypePairs.First().Title, partyTypePairs.First().Description);
                    else
                    {
                        partyRelationshipType = partyRelationshipTypeManager.Update(partyRelationshipType.Id, title, description, indicatesHierarchy, maxCardinality, minCardinality);
                        UpdateOrCreatePartyTypePair(partyTypePairs.First(), partyRelationshipType, partyRelationshipTypeManager);
                    }
                    //If there are more than one partyTypepair exist
                    //if (partyTypePairs.Count() > 1)
                    foreach (var partyTypePair in partyTypePairs.Where(item => item != partyTypePairs.First()))
                        UpdateOrCreatePartyTypePair(partyTypePair, partyRelationshipType, partyRelationshipTypeManager);// 
                }

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