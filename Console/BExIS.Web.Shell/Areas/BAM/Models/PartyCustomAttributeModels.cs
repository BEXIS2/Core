using BExIS.Dlm.Entities.Party;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Bam.UI.Models
{
    public class ReadPartyCustomAttributeModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }

        public static ReadPartyCustomAttributeModel Convert(PartyCustomAttribute customAttribute)
        {
            return new ReadPartyCustomAttributeModel
            {
                Id= customAttribute.Id,
                Name = customAttribute.Name,
                DisplayName = customAttribute.DisplayName,
                Description = customAttribute.Description,
            };
        }
    }


}