using BExIS.Dlm.Entities.Party;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Modules.Bam.UI.Models
{
    public class PartyCustomAttributeAndValuesModel
    {
        private Party _party = new Party();

        public PartyCustomAttributeAndValuesModel(Party party)
        {
            _party = party;
        }

        public List<PartyCustomAttribute> PartyCustomAttribute
        {
            get
            {
                if (_party.Id != 0)
                    return _party.PartyType.CustomAttributes.ToList();
                else
                    return null;
            }
        }

        public List<PartyCustomAttributeValue> PartyCustomAttributeValues
        {
            get
            {
                if (_party.Id != 0)
                    return _party.CustomAttributeValues.ToList();
                else
                    return null;
            }
        }
    }
}