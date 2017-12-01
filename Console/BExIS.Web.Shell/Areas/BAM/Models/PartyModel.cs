using BExIS.Dlm.Entities.Party;
using BExIS.IO.Transform.Validation.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Bam.UI.Models
{
    public class PartyModel
    {
        public PartyModel()
        {
            PartyType = new PartyType();
            PartyTypeList = new List<PartyType>();
            Errors = new List<Error>();
        }
        public long Id { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public PartyType PartyType { get; set; }
        public String Name { get; set; }
        public List<Error> Errors { get; set; }

        public bool ViewMode { get; set; }
        [Display(Name = "Metadata Structure")]
        [Required(ErrorMessage = "Please select a party type.")]
        public List<PartyType> PartyTypeList { get; set; }
        public List<PartyRelationship> PartyRelationships
        {
            get
            {
                if (Id != 0)
                    return new Dlm.Services.Party.PartyManager().PartyRelationshipRepository.Get
                        (item => item.FirstParty.Id == Id || item.SecondParty.Id == Id).ToList();
                else
                    return null;
            }
        }
    }

    //public class PartyRelationshipTypeModel : Dlm.Entities.Party.PartyRelationshipType
    //{

    //    public int CurrentCardinality
    //    {
    //        get;
    //    }
    //    public PartyRelationshipTypeModel(int partyId)
    //    {

    //    }
    //}
}