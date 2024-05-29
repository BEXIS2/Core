using BExIS.Dim.Entities.Mappings;
using BExIS.Dim.Helpers.Mappings;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.Dim.UI.Helper
{
    public class ReleationshipHelper
    {
        //toDo put this function to DIM
        public void SetRelationships(long datasetid, long metadataStructureId, XmlDocument metadata)
        {
            using (PartyManager partyManager = new PartyManager())
            using (PartyTypeManager partyTypeManager = new PartyTypeManager())
            using (PartyRelationshipTypeManager partyRelationshipTypeManager = new PartyRelationshipTypeManager())
            {
                try
                {
                    using (var uow = this.GetUnitOfWork())
                    {

                        //check if mappings exist between system/relationships and the metadatastructure/attr
                        // get all party mapped nodes
                        IEnumerable<XElement> complexElements = XmlUtility.GetXElementsByAttribute("partyid", XmlUtility.ToXDocument(metadata));



                        // get releaionship type id for owner
                        var releationships = uow.GetReadOnlyRepository<PartyRelationshipType>().Get().Where(
                            p => p.AssociatedPairs.Any(
                                ap => ap.SourcePartyType.Title.ToLower().Equals("dataset") || ap.TargetPartyType.Title.ToLower().Equals("dataset")
                                ));

                        foreach (XElement item in complexElements)
                        {
                            if (item.HasAttributes)
                            {
                                long sourceId = Convert.ToInt64(item.Attribute("id").Value);
                                string type = item.Attribute("type").Value;
                                long partyid = Convert.ToInt64(item.Attribute("partyid").Value);

                                LinkElementType sourceType = LinkElementType.MetadataNestedAttributeUsage;
                                if (type.Equals("MetadataPackageUsage")) sourceType = LinkElementType.MetadataPackageUsage;

                                foreach (var releationship in releationships)
                                {
                                    // when mapping in both directions are exist
                                    if (MappingUtils.ExistMappings(sourceId, sourceType, releationship.Id, LinkElementType.PartyRelationshipType) &&
                                        MappingUtils.ExistMappings(releationship.Id, LinkElementType.PartyRelationshipType, sourceId, sourceType))
                                    {
                                        // create releationship

                                        // create a Party for the dataset
                                        var customAttributes = new Dictionary<String, String>();
                                        customAttributes.Add("Name", datasetid.ToString());
                                        customAttributes.Add("Id", datasetid.ToString());

                                        var datasetParty = partyManager.Create(partyTypeManager.PartyTypeRepository.Get(cc => cc.Title == "Dataset").First(), "[description]", null, null, customAttributes);
                                        var person = partyManager.GetParty(partyid);


                                        var partyTpePair = releationship.PartyRelationships.FirstOrDefault().PartyTypePair;

                                        if (partyTpePair != null && person != null && datasetParty != null)
                                        {
                                            partyManager.AddPartyRelationship(
                                                datasetParty.Id,
                                                person.Id,
                                                "Owner Releationship",
                                                "",
                                                partyTpePair.Id

                                                );
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }
    }
}