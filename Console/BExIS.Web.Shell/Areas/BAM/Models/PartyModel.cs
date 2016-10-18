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
        public List<PartyType> PartyTypeList { get; set; }
        public Party Party { get; set; }
        public List<Error> Errors { get; set; }
        public PartyModel()
        {
            Party = new Party();
            PartyTypeList = new List<PartyType>();
            Errors = new List<Error>();
        }
    }
}