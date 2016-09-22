using BExIS.Dlm.Entities.Party;
using BExIS.IO.Transform.Validation.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.BAM.Models
{
    public class PartyModel
    {
        [Display(Name = "Metadata Structure")]
        [Required(ErrorMessage = "Please select a party type.")]
       // public long SelectedpartyTypeId { get; set; }
        public List<PartyType> PartyTypeList { get; set; }
        public Party Party { get; set; }
        //public List<PartyCustomAttribute> PartyCustomAttributeList { get; set; }
        //public List<PartyCustomAttributeValue> PartyCustomAttributeValueList { get; set; }
        public List<Error> Errors { get; set; }
        public PartyModel()
        {
            //SelectedpartyTypeId = 0;
            Party = new Party();
            PartyTypeList = new List<PartyType>();
            //PartyCustomAttributeList = new List<Dlm.Entities.Party.PartyCustomAttribute>();
            //PartyCustomAttributeValueList = new List<PartyCustomAttributeValue>();
            Errors = new List<Error>();
            Party.StartDate = DateTime.Now;
            //It should be changed
            Party.EndDate = DateTime.Now.AddMonths(1);
        }
    }
}