using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Logging;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Bam.UI.Helpers
{
    public class BAMSeedDataGenerator : IDisposable
    {
        public void GenerateSeedData()
        {
            createSecuritySeedData();
            ImportPartyTypes();
            // AddSystemRelationshipsSamples();
        }

        private void AddSystemRelationshipsSamples()
        {
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            //Example for adding system parties
            var customAttrs = new Dictionary<string, string>();
            customAttrs.Add("Name", "test dataset");
            Helper.CreateParty(DateTime.MinValue, DateTime.MaxValue, "", partyTypeManager.PartyTypeRepository.Get(cc => cc.Title == "Dataset").First().Id, customAttrs);
            customAttrs = new Dictionary<string, string>();
            customAttrs.Add("Name", "test resource");
            Helper.CreateParty(DateTime.MinValue, DateTime.MaxValue, "", partyTypeManager.PartyTypeRepository.Get(cc => cc.Title == "Resource").First().Id, customAttrs);
        }

        private bool createSecuritySeedData()
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
            if (featureManager.Exists("Business administration module (BAM)", root))
                return false;
            var bamFeature = featureManager.Create("Business administration module (BAM)", "", root);
            if (featureManager.Exists("Party", bamFeature))
                return true;
            var partyFeature = featureManager.Create("Party", "", bamFeature);

            var partyOperation = operationManager.Create("BAM", "Party", "*", partyFeature);
            var partyServiceOperation = operationManager.Create("BAM", "PartyService", "*");
            var partyHelp = operationManager.Create("BAM", "Help", "*");
            return true;
        }

        /// <summary>
        /// Update rules:
        /// Comparison for update is by the title of elements: title of elements are not editable
        /// if title of an element is changed because remove is forbiden here ,  it adds it as a new element and the old one will remain there
        ///
        /// </summary>
        private void ImportPartyTypes()
        {
            PartyTypeManager partyTypeManager = null;
            PartyManager partyManager = null;
            PartyRelationshipTypeManager partyRelationshipTypeManager = null;
            try
            {
                partyTypeManager = new PartyTypeManager();
                partyManager = new PartyManager();
                partyRelationshipTypeManager = new PartyRelationshipTypeManager();
                var filePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("BAM"), "partyTypes.xml");
                XDocument xDoc = XDocument.Load(filePath);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xDoc.CreateReader());
                var partyTypesNodeList = xmlDoc.SelectNodes("//PartyTypes");

                var deleteAbleAttr = new List<PartyCustomAttribute>();
                var deleteAbleAttrValues = new List<PartyCustomAttributeValue>();

                if (partyTypesNodeList.Count > 0)
                    foreach (XmlNode partyTypeNode in partyTypesNodeList[0].ChildNodes)
                    {
                        if (!(partyTypeNode is XmlElement)) continue;

                        //Convert xmAttributeCollection to list to skipt the case sensitive and null problems
                        var attributes = new List<XmlAttribute>();
                        foreach (XmlAttribute att in partyTypeNode.Attributes)
                        {
                            attributes.Add(att);
                        }
                        var title = GetAttributeValue(attributes, "Name", true);
                        var displayName = GetAttributeValue(attributes, "DisplayName", false);
                        var systemType = GetAttributeValue(attributes, "SystemType", true);
                        var partyType = partyTypeManager.PartyTypeRepository.Get(item => item.Title == title).FirstOrDefault();
                        //If there is not such a party type
                        if (partyType == null)
                        {
                            var partyStatusTypes = new List<PartyStatusType>();
                            partyStatusTypes.Add(new PartyStatusType() { Name = "Created", Description = "" });
                            partyType = partyTypeManager.Create(title, "Imported from partyTypes.xml", displayName, partyStatusTypes, (systemType == null ? false : Convert.ToBoolean(systemType)));
                            var customAttrs = new List<PartyCustomAttribute>();
                            foreach (XmlNode customAttrNode in partyTypeNode.ChildNodes)
                            {
                                if (!(customAttrNode is XmlElement)) continue;

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
                                    DisplayName = partyCustomAttr.DisplayName,
                                    Condition = partyCustomAttr.Condition
                                });
                            }

                            if (!customAttrs.Any(c => c.IsMain))
                                customAttrs[0].IsMain = true;

                            foreach (var customAttr in customAttrs)
                                partyTypeManager.CreatePartyCustomAttribute(customAttr);
                        }
                        else //partytype exist
                        {
                            var newCustomAttrs = new List<PartyCustomAttribute>();
                            var existingCustomAttrs = new List<PartyCustomAttribute>();

                            foreach (XmlNode customAttrNode in partyTypeNode.ChildNodes)
                            {
                                if (!(customAttrNode is XmlElement)) continue;

                                var attributesList = new List<XmlAttribute>();
                                foreach (XmlAttribute att in customAttrNode.Attributes)
                                {
                                    attributesList.Add(att);
                                }

                                var customAttrName = GetAttributeValue(attributesList, "Name", true);
                                //create new custom attribute if there is not such a name
                                if (!partyType.CustomAttributes.Any(item => item.Name == customAttrName))
                                {
                                    var customAttrNodeAttributes = new List<XmlAttribute>();
                                    foreach (XmlAttribute att in customAttrNode.Attributes)
                                    {
                                        customAttrNodeAttributes.Add(att);
                                    }

                                    PartyCustomAttribute partyCustomAttr = ParsePartyCustomAttribute(customAttrNodeAttributes);
                                    newCustomAttrs.Add(new PartyCustomAttribute()
                                    {
                                        DataType = partyCustomAttr.DataType,
                                        Description = partyCustomAttr.Description,
                                        IsMain = partyCustomAttr.IsMain,
                                        IsUnique = partyCustomAttr.IsUnique,
                                        IsValueOptional = partyCustomAttr.IsValueOptional,
                                        Name = customAttrName,
                                        PartyType = partyType,
                                        ValidValues = partyCustomAttr.ValidValues,
                                        DisplayName = partyCustomAttr.DisplayName,
                                        Condition = partyCustomAttr.Condition
                                    });
                                }
                                else //update if exist
                                {
                                    //add to existingCustomAttr list
                                    var existingAttr = partyType.CustomAttributes.Where(item => item.Name == customAttrName).FirstOrDefault();
                                    if (existingAttr != null) existingCustomAttrs.Add(existingAttr);
                                }
                            }// end foreach customAttrNode

                            if (!newCustomAttrs.Any(c => c.IsMain) && !partyType.CustomAttributes.Any(c => c.IsMain))
                                throw new Exception("There is no main field. Each party type needs at least one main field.");

                            // create all custom Attr´s
                            foreach (var customAttr in newCustomAttrs)
                                partyTypeManager.CreatePartyCustomAttribute(customAttr);

                            // Delete all attrs that are no longer in the partytype.xml
                            newCustomAttrs.AddRange(existingCustomAttrs);
                            var currentListOfAttr = partyType.CustomAttributes;

                            foreach (var attr in currentListOfAttr)
                            {
                                if (!newCustomAttrs.Any(a => a.Id.Equals(attr.Id)))
                                {
                                    deleteAbleAttr.Add(attr);
                                    //select all value that are created based on the attr
                                    // the values need to delete befor the attr itself
                                    deleteAbleAttrValues.AddRange(
                                        partyManager.PartyCustomAttributeValueRepository.Query()
                                        .Where(v => v.CustomAttribute.Id.Equals(attr.Id)));
                                }
                            }
                        }
                    }
                var partyRelationshipTypesNodeList = xmlDoc.SelectNodes("//PartyRelationshipTypes");
                if (partyRelationshipTypesNodeList.Count > 0)

                    foreach (XmlNode partyRelationshipTypesNode in partyRelationshipTypesNodeList[0].ChildNodes)
                    {
                        if (!(partyRelationshipTypesNode is XmlElement)) continue;

                        var customAttrNodeAttributes = new List<XmlAttribute>();
                        foreach (XmlAttribute att in partyRelationshipTypesNode.Attributes)
                        {
                            customAttrNodeAttributes.Add(att);
                        }

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
                            var allowedSourceTitle = GetAttributeValue(partyTypesPairNodeAttributes, "SourceType", true);
                            var allowedTargetTitle = GetAttributeValue(partyTypesPairNodeAttributes, "TargetType", true);
                            var allowedSource = partyTypeManager.PartyTypeRepository.Get(item => item.Title.ToLower() == allowedSourceTitle.ToLower()).FirstOrDefault();
                            if (allowedSource == null)
                                throw new Exception("Error in importing party relationship types ! \r\n " + allowedSourceTitle + " is not a party type!!");
                            var allowedTarget = partyTypeManager.PartyTypeRepository.Get(item => item.Title.ToLower() == allowedTargetTitle.ToLower()).FirstOrDefault();
                            if (allowedTarget == null)
                                throw new Exception("Error in importing party relationship types ! \r\n " + allowedTargetTitle + " is not a party type!!");

                            var typePairTitle = GetAttributeValue(partyTypesPairNodeAttributes, "Title", true);
                            var typePairDescription = GetAttributeValue(partyTypesPairNodeAttributes, "Description", false);
                            var typePairDefault = GetAttributeValue(partyTypesPairNodeAttributes, "Default", true);
                            var conditionSource = GetAttributeValue(partyTypesPairNodeAttributes, "conditionSource", false);
                            var conditionTarget = GetAttributeValue(partyTypesPairNodeAttributes, "conditionTarget", false);
                            var permissionsTemplate = GetAttributeValue(partyTypesPairNodeAttributes, "permissionsTemplate", false);
                            partyTypePairs.Add(new PartyTypePair()
                            {
                                SourcePartyType = allowedSource,
                                TargetPartyType = allowedTarget,
                                Description = typePairDescription,
                                Title = typePairTitle,
                                PartyRelationShipTypeDefault = typePairDefault == null ? true : Convert.ToBoolean(typePairDefault),
                                ConditionSource = conditionSource,
                                ConditionTarget = conditionTarget,
                                PermissionTemplate = Helper.GetPermissionValue(permissionsTemplate)
                            });
                        }

                        var partyRelationshipType = partyRelationshipTypeManager.PartyRelationshipTypeRepository.Get(item => item.Title == title).FirstOrDefault();
                        //If there is not such a party relationship type
                        //It is mandatory to create at least one party type pair when we are creating a party type relation
                        //
                        if (partyRelationshipType == null)
                            partyRelationshipType = partyRelationshipTypeManager.Create(title, displayName, description, (indicatesHierarchy == null ? false : Convert.ToBoolean(indicatesHierarchy)), maxCardinality == null ? -1 : int.Parse(maxCardinality), minCardinality == null ? 0 : int.Parse(minCardinality), partyTypePairs.First().PartyRelationShipTypeDefault, partyTypePairs.First().SourcePartyType, partyTypePairs.First().TargetPartyType,
            partyTypePairs.First().Title, partyTypePairs.First().Description, partyTypePairs.First().ConditionSource, partyTypePairs.First().ConditionTarget, partyTypePairs.First().PermissionTemplate);
                        else
                        {
                            partyRelationshipType = partyRelationshipTypeManager.Update(partyRelationshipType.Id, title, "", description, (indicatesHierarchy == null ? false : Convert.ToBoolean(indicatesHierarchy)), maxCardinality == null ? -1 : int.Parse(maxCardinality), minCardinality == null ? 0 : int.Parse(minCardinality));
                            UpdateOrCreatePartyTypePair(partyTypePairs.First(), partyRelationshipType, partyRelationshipTypeManager);
                        }
                        //If there are more than one partyTypepair exist
                        //if (partyTypePairs.Count() > 1)
                        foreach (var partyTypePair in partyTypePairs.Where(item => item != partyTypePairs.First()))
                            UpdateOrCreatePartyTypePair(partyTypePair, partyRelationshipType, partyRelationshipTypeManager);//
                    }
                //Add all the custom Attribute names ao custom grid column of default user
                foreach (var partyType in partyTypeManager.PartyTypeRepository.Get(cc => !cc.SystemType))
                {
                    foreach (var partyCustomAttr in partyType.CustomAttributes)
                        partyManager.UpdateOrAddPartyGridCustomColumn(partyType, partyCustomAttr, null);
                    var partyRelationshipTypePairs = partyRelationshipTypeManager.PartyTypePairRepository.Get(cc => cc.SourcePartyType.Id == partyType.Id && !cc.TargetPartyType.SystemType);
                    foreach (var partyTypePair in partyRelationshipTypePairs)
                        partyManager.UpdateOrAddPartyGridCustomColumn(partyType, null, partyTypePair);
                }

                if (deleteAbleAttr.Any())
                {
                    //delete all existing PartyCustomAttrValues
                    deleteAbleAttrValues.ForEach(a => partyManager.RemovePartyCustomAttributeValue(a));

                    // Delete all GridColumns of the CustomAttribute
                    var listOfIds = deleteAbleAttr.Select(d => d.Id);
                    var gridColumns = partyManager.PartyCustomGridColumnsRepository.Get();
                    var listOfGridColumns = gridColumns.Where(c => c.CustomAttribute != null && listOfIds.Contains(c.CustomAttribute.Id)).ToList();

                    listOfGridColumns.ForEach(c => partyManager.RemovePartyGridCustomColumn(c.Id));

                    // add CustomAttribute Grid Columns
                    deleteAbleAttr.ForEach(a => partyTypeManager.DeletePartyCustomAttribute(a));
                }
            }
            catch (Exception ex)
            {
                LoggerFactory.LogCustom("SeedData Failed: " + ex.Message);
                throw ex;
            }
            finally
            {
                partyManager?.Dispose();
                partyTypeManager?.Dispose();
                partyRelationshipTypeManager?.Dispose();
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
            var condition = GetAttributeValue(attributes, "Condition", false);
            return new PartyCustomAttribute()
            {
                DataType = customAttrType,
                Description = description,
                IsMain = isMain == null ? false : Convert.ToBoolean(isMain),
                IsUnique = isUnique == null ? false : Convert.ToBoolean(isUnique),
                IsValueOptional = isValueOptional == null ? true : Convert.ToBoolean(isValueOptional),
                Name = GetAttributeValue(attributes, "Name", true),
                ValidValues = validValues,
                DisplayName = displayName,
                Condition = condition
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
            var entity = partyRelationshipTypeManager.PartyTypePairRepository.Get(item => item.Title == partyTypePair.Title && item.PartyRelationshipType.Id == partyRelationshipType.Id).FirstOrDefault();
            if (entity != null)
                partyRelationshipTypeManager.UpdatePartyTypePair(entity.Id, partyTypePair.Title, partyTypePair.SourcePartyType, partyTypePair.TargetPartyType, partyTypePair.Description, "", "", partyTypePair.PartyRelationShipTypeDefault, entity.PartyRelationshipType, entity.PermissionTemplate);
            else
                partyRelationshipTypeManager.AddPartyTypePair(partyTypePair.Title, partyTypePair.SourcePartyType, partyTypePair.TargetPartyType, partyTypePair.Description, partyTypePair.PartyRelationShipTypeDefault, partyRelationshipType, partyTypePair.ConditionSource, partyTypePair.ConditionTarget, partyTypePair.PermissionTemplate);
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}