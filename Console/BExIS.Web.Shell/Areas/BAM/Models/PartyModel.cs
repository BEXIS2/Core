using BExIS.Dlm.Entities.Party;
using BExIS.IO.Transform.Validation.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BExIS.Modules.Bam.UI.Models
{
    public class PartyModel
    {
        public PartyModel()
        {
            PartyType = new PartyType();
            PartyTypeList = new List<PartyType>();
            PartyRelationships = new List<PartyRelationshipModel>();
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

        public List<PartyRelationshipModel> PartyRelationships { get; set; }
    }

    public class PartyRelationshipModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public string SourceName { get; set; }
        public string TargetName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Type { get; set; }
    }
}