using BExIS.Dlm.Entities.Party;
using System.Collections.Generic;

namespace BExIS.Modules.Bam.UI.Models
{
    public class ReadPartyModel
    {
        public long Id { get; set; }

        public long PartyTypeId { get; set; }
        public Dictionary<string, string> PartyCustomAttributes { get; set; }

        public ReadPartyModel()
        {
            PartyCustomAttributes = new Dictionary<string, string>();
        }

        public static ReadPartyModel Convert(Party party)
        {
            var model = new ReadPartyModel
            {
                Id = party.Id,
                PartyTypeId = party.PartyType.Id
            };

            foreach (var item in party.CustomAttributeValues)
            {
                model.PartyCustomAttributes.Add(item.CustomAttribute.Name, item.Value);
            }

            return model;
        }
    }
}