using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dlm.Entities.Party;
using BExIS.Web.Shell.Areas.BAM.Controllers;

namespace BExIS.Web.Shell.Areas.SAM.Models
{
    public class CreatePartyModel
    {
        public List<PartyCustomAttributeModel> PartyCustomAttributes { get; set; }
        public long PartyTypeId { get; set; }

        public CreatePartyModel()
        {
            PartyCustomAttributes = new List<PartyCustomAttributeModel>();
        }
    }

    public class PartyCustomAttributeModel
    {
        public virtual string DataType { get; set; }
        public virtual string Description { get; set; }
        public virtual int DisplayOrder { get; set; }
        public long Id { get; set; }
        public virtual bool IsValueOptional { get; set; }
        public virtual string Name { get; set; }
        public virtual string ValidValues { get; set; }
        public string Value { get; set; }

        public static PartyCustomAttributeModel Convert(PartyCustomAttribute attribute)
        {
            return new PartyCustomAttributeModel()
            {
                DataType = attribute.DataType,
                Description = attribute.Description,
                DisplayOrder = attribute.DisplayOrder,
                Id = attribute.Id,
                IsValueOptional = attribute.IsValueOptional,
                Name = attribute.Name,
                ValidValues = attribute.ValidValues
            };
        }
    }
}