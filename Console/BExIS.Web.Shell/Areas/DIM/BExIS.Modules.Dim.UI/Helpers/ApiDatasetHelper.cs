using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Dim.UI.Models.Api;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Utils.Data.Helpers;
using BExIS.Xml.Helpers;
using NameParser;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BExIS.Modules.Dim.UI.Helpers
{
    public class ApiDatasetHelper
    {
        public ApiDatasetModel GetContent(DatasetVersion datasetVersion, long id, int versionNumber, long metadataStructureId, long dataStructureId)
        {
            ApiDatasetModel datasetModel = new ApiDatasetModel()
            {
                Id = id,
                Version = versionNumber,
                VersionId = datasetVersion.Id,
                VersionDate = datasetVersion.Timestamp.ToString(new CultureInfo("en-US")),
                Title = datasetVersion.Title,
                Description = datasetVersion.Description,
                DataStructureId = dataStructureId,
                MetadataStructureId = metadataStructureId
            };

            Dictionary<string, List<XObject>> objects = new Dictionary<string, List<XObject>>();

            // add additional Information / mapped system keys
            foreach (Key k in Enum.GetValues(typeof(Key)))
            {
                var tmp = MappingUtils.GetValuesFromMetadata(Convert.ToInt64(k), LinkElementType.Key,
               metadataStructureId, XmlUtility.ToXDocument(datasetVersion.Metadata));

                if (tmp != null)
                {
                    string value = string.Join(",", tmp.Distinct());
                    if (!string.IsNullOrEmpty(value))
                    {
                        datasetModel.AdditionalInformations.Add(k.ToString(), value);
                    }
                    // collect all results for each system key
                    objects.Add(k.ToString(), MappingUtils.GetXObjectFromMetadata(Convert.ToInt64(k), LinkElementType.Key,
                                datasetVersion.Dataset.MetadataStructure.Id, XmlUtility.ToXDocument(datasetVersion.Metadata)));
                }
            }

            // get isMain parts for all found parties if exists (e.g. first and last name)
            datasetModel.Parties = getPartyIsMainAttributesForParties(objects);

            // set up person key list
            var personKeyList = new List<string>();
            // setup dic with values of the persons
            var personKeyDic = new Dictionary<string, List<XObject>>();

            // add keys here
            personKeyList.Add(Key.Author.ToString());

            foreach (var key in personKeyList)
            {
                // check if key exists in alle mapped elements
                if (objects.ContainsKey(key))
                    personKeyDic.Add(key, objects[key]);
            }

            datasetModel.Names = getSplitedNames(personKeyDic);

            var publicAndDate = getPublicAndDate(id);
            // check is public
            datasetModel.IsPublic = publicAndDate.Item1;

            // check for publication date
            datasetModel.PublicationDate = publicAndDate.Item2.ToString(new CultureInfo("en-US"));

            // get links
            EntityReferenceHelper entityReferenceHelper = new EntityReferenceHelper();
            datasetModel.Links.From = entityReferenceHelper.GetSourceReferences(id, datasetVersion.Dataset.EntityTemplate.EntityType.Id, versionNumber);
            datasetModel.Links.To = entityReferenceHelper.GetTargetReferences(id, datasetVersion.Dataset.EntityTemplate.EntityType.Id, versionNumber);


            return datasetModel;
        }

        // Search if the XML element contains a partyid and return it
        private long getPartyId(XElement e)
        {
            if (e.Attributes().Any(x => x.Name.LocalName.Equals("partyid")))
            {
                return Convert.ToInt64(e.Attribute("partyid").Value);
            }
            if (e.Parent != null) return getPartyId(e.Parent);

            return 0;
        }

        private Dictionary<string, Dictionary<string, string>> getPartyIsMainAttributesForParties(Dictionary<string, List<XObject>> elements)
        {
            Dictionary<string, Dictionary<string, string>> dict = new Dictionary<string, Dictionary<string, string>>();
            using (PartyManager partyManager = new PartyManager())
            {
                foreach (var key in elements)
                {
                    foreach (XObject o in key.Value)
                    {
                        if (o is XElement)
                        {
                            string value = "";

                            XElement element = (XElement)o;
                            value = element.Value;

                            long partyid = getPartyId(element);
                            Dictionary<string, string> dict2 = new Dictionary<string, string>();
              

                            // id direct
                            if (partyid > 0)
                            {
                                var party = partyManager.GetParty(partyid);

                                if (party != null)
                                {
                                    var attrs = party.CustomAttributeValues.Where(a => a.CustomAttribute.IsMain == true).ToArray();

                                    foreach (var attr in attrs)
                                    {
                                        dict2.Add(attr.CustomAttribute.Name, attr.Value);
                                    }
                                }
                                if (!dict.ContainsKey(value))
                                {
                                    dict.Add(value, dict2);
                                }
                            }
                        }
                    }
                }
            }

            return dict;
        }

        private Dictionary<string, Dictionary<string, string>> getSplitedNames(Dictionary<string, List<XObject>> elements)
        {
            Dictionary<string, Dictionary<string, string>> dict = new Dictionary<string, Dictionary<string, string>>();
            using (PartyManager partyManager = new PartyManager())
            {
                foreach (var key in elements)
                {
                    foreach (XObject o in key.Value)
                    {
                        if (o is XElement)
                        {
                            XElement element = (XElement)o;

                            long partyid = getPartyId(element);
                            Dictionary<string, string> dict2 = new Dictionary<string, string>();
                            // id direct
                            Console.WriteLine(partyid);
                            if (partyid > 0) { }
                            else
                            {
                                var person = new HumanName(element.Value);
                                var GivenName = (person.Middle.Length > 0) ? $"{person.First} {person.Middle}" : $"{person.First}";
                                var FamilyName = person.Last;
                                dict2.Add("GivenName", GivenName);
                                dict2.Add("FamilyName", FamilyName);

                                if (!dict.ContainsKey(element.Value))
                                {
                                    dict.Add(element.Value, dict2);
                                }
                            }
                        }
                    }
                }
            }
            return dict;
        }

        // @TODO: move to dataset manager?
        private static Tuple<bool, DateTime> getPublicAndDate(long id)
        {
            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            using (EntityManager entityManager = new EntityManager())
            {
                long? entityTypeId = entityManager.FindByName(typeof(Dataset).Name)?.Id;
                entityTypeId = entityTypeId.HasValue ? entityTypeId.Value : -1;
                return entityPermissionManager.GetPublicAndDate(entityTypeId.Value, id);
            }
        }

    }
}
