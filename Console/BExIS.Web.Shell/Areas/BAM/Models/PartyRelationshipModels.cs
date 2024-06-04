using BExIS.Dlm.Entities.Party;

namespace BExIS.Modules.Bam.UI.Models
{
    public class ReadPartyRelationshipModel
    {
        public long Id { get; set; }

        public static ReadPartyRelationshipModel Convert(PartyRelationship partyRelationship)
        {
            var model = new ReadPartyRelationshipModel
            {
                Id = partyRelationship.Id
            };

            return model;
        }
    }
}