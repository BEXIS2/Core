using BExIS.Dlm.Entities.Party;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Modules.Bam.UI.Models
{
    public class ReadPartyRelationshipTypeModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DisplayName { get; set; }

        public List<ReadPartyRelationshipTypePairModel> PartyRelationshipTypePairs { get; set; }

        public ReadPartyRelationshipTypeModel()
        {
            PartyRelationshipTypePairs = new List<ReadPartyRelationshipTypePairModel>();
        }

        public static ReadPartyRelationshipTypeModel Convert(PartyRelationshipType partyRelationshipType)
        {
            var model = new ReadPartyRelationshipTypeModel
            {
                Id = partyRelationshipType.Id,
                Title = partyRelationshipType.Title,
                Description = partyRelationshipType.Description,
                DisplayName = partyRelationshipType.DisplayName,
                PartyRelationshipTypePairs = partyRelationshipType.AssociatedPairs.Select(p => ReadPartyRelationshipTypePairModel.Convert(p)).ToList()
            };

            return model;
        }
    }
}