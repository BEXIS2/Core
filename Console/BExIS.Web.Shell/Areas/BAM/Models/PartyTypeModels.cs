using BExIS.Dlm.Entities.Party;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Bam.UI.Models
{
    public class ReadPartyTypeModel
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public string DisplayName { get; set; }
        public bool SystemType { get; set; }

        public static ReadPartyTypeModel Convert(PartyType partyType)
        {
            return new ReadPartyTypeModel
            {
                Id = partyType.Id,
                Description = partyType.Description,
                Title = partyType.Title,
                DisplayName = partyType.DisplayName,
                SystemType = partyType.SystemType
            };
        }
    }
}