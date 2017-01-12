using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Security.Entities.Authentication;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Authentication;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;

namespace BExIS.Web.Shell.Areas.BAM.Helpers
{
    public class Helper
    {
        public static int CountRelations(long sourcePartyId, long targetPartyId, PartyRelationshipType partyRelationshipType)
        {
            PartyManager partyManager = new PartyManager();
            var cnt = partyManager.RepoPartyRelationships.Query(item => (item.PartyRelationshipType != null && item.PartyRelationshipType.Id == partyRelationshipType.Id)
                                      && (item.FirstParty != null && item.FirstParty.Id == sourcePartyId)
                                       && (item.SecondParty != null && item.SecondParty.Id == targetPartyId)).Count();
            return cnt;
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
                    var partyType = partyTypeManager.Repo.Get(item => item.Title == title).FirstOrDefault();
                    //If there is not such a party type
                    if (partyType == null)
                    {
                        partyType = partyTypeManager.Create(title, "Imported from partyTypes.xml", null);
                        partyTypeManager.AddStatusType(partyType, "Create", "", 0);
                        foreach (XmlNode customAttrNode in partyTypeNode.ChildNodes)
                        {
                            var customAttrType = customAttrNode.Attributes["type"] == null ? "String" : customAttrNode.Attributes["type"].Value;
                            var description = customAttrNode.Attributes["description"] == null ? "" : customAttrNode.Attributes["description"].Value;
                            var validValues = customAttrNode.Attributes["validValues"] == null ? "" : customAttrNode.Attributes["validValues"].Value;
                            var isValueOptional = customAttrNode.Attributes["isValueOptional"] == null ? true : Convert.ToBoolean(customAttrNode.Attributes["isValueOptional"].Value);
                            var partyCustomAttribute = partyTypeManager.CreatePartyCustomAttribute(partyType, customAttrType, customAttrNode.Attributes["Name"].Value, description, validValues, isValueOptional);
                        }
                        // xmlDoc.Save(filePath);
                    }
                    else
                    {
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
                                var customAttribute = partyTypeManager.CreatePartyCustomAttribute(partyType, customAttrType, customAttrName, customAttrDescription, customAttrValidValues, customAttrIsValueOptional);
                            }
                            // xmlDoc.Save(filePath);
                        }
                    }

                }
            var partyRelationshipTypesNodeList = xmlDoc.SelectNodes("//PartyRelationshipTypes");
            if (partyRelationshipTypesNodeList.Count > 0)

                foreach (XmlNode partyRelationshipTypesNode in partyRelationshipTypesNodeList[0].ChildNodes)
                {
                    var partyRelationshipTypeManager = new PartyRelationshipTypeManager();
                    var title = partyRelationshipTypesNode.Attributes["Name"].Value;
                    var description = partyRelationshipTypesNode.Attributes["Description"].Value;

                    var indicatesHierarchy = partyRelationshipTypesNode.Attributes["IndicatesHierarchy"] == null ? false : Convert.ToBoolean(partyRelationshipTypesNode.Attributes["IndicatesHierarchy"].Value);

                    var maxCardinality = partyRelationshipTypesNode.Attributes["MaxCardinality"] == null ? 0 : int.Parse(partyRelationshipTypesNode.Attributes["MaxCardinality"].Value);

                    var minCardinality = partyRelationshipTypesNode.Attributes["MinCardinality"] == null ? 0 : int.Parse(partyRelationshipTypesNode.Attributes["MinCardinality"].Value);

                    //Import party type pairs
                    var partyTypesPairNodeList = partyRelationshipTypesNode.SelectNodes("//PartyTypesPairs");
                    var partyTypePairs = new List<PartyTypePair>();
                    foreach (XmlNode partyTypesPairNode in partyTypesPairNodeList[0].ChildNodes)
                    {
                        var alowedSourceTitle = partyTypesPairNode.Attributes["AlowedSource"].Value;
                        var alowedTargetTitle = partyTypesPairNode.Attributes["AlowedTarget"].Value;
                        var alowedSource = partyTypeManager.Repo.Get(item => item.Title == alowedSourceTitle).FirstOrDefault();
                        if (alowedSource == null)
                            throw new Exception("Error in importing party relationship types ! \r\n " + alowedSourceTitle + " is not a party type!!");
                        var alowedTarget = partyTypeManager.Repo.Get(item => item.Title == alowedTargetTitle).FirstOrDefault();
                        if (alowedTarget == null)
                            throw new Exception("Error in importing party relationship types ! \r\n " + alowedTargetTitle + " is not a party type!!");

                        var typePairTitle = partyTypesPairNode.Attributes["Title"].Value;

                        var typePairTDescription = partyTypesPairNode.Attributes["Description"].Value;
                        partyTypePairs.Add(new PartyTypePair()
                        {
                            AlowedSource = alowedSource,
                            AlowedTarget = alowedTarget,
                            Description = typePairTDescription,
                            Title = typePairTitle
                        });
                    }

                    var partyRelationshipType = partyRelationshipTypeManager.Repo.Get(item => item.Title == title).FirstOrDefault();
                    //If there is not such a party relationship type 
                    //It is mandatory to create at least one party type pair when we are creating a party type relation
                    //
                    if (partyRelationshipType == null)
                        partyRelationshipType = partyRelationshipTypeManager.Create(title, description, indicatesHierarchy, maxCardinality, minCardinality, partyTypePairs.First().AlowedSource, partyTypePairs.First().AlowedTarget,
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
                partyRelationshipTypeManager.UpdatePartyTypePair(entity.Id, partyTypePair.Title, partyTypePair.AlowedSource, partyTypePair.AlowedTarget, partyTypePair.Description, entity.PartyRelationshipType);
            else
                partyRelationshipTypeManager.AddPartyTypePair(partyTypePair.Title, partyTypePair.AlowedSource, partyTypePair.AlowedTarget, partyTypePair.Description, partyRelationshipType);
        }
    }
    //Check duplicate and add edit 
    }