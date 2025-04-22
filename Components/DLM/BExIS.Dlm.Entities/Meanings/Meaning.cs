using BExIS.Dlm.Entities.DataStructure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Meanings
{
    public class Meaning : BaseEntity
    {
        public virtual ICollection<MeaningEntry> ExternalLinks { get; set; }
        public virtual ICollection<Meaning> Related_meaning { get; set; }
        public virtual ICollection<Constraint> Constraints { get; set; }

        public Meaning()
        {
            this.ExternalLinks = new List<MeaningEntry>();
            this.Related_meaning = new List<Meaning>();
            this.Constraints = new List<Constraint>();
        }

        [Required(ErrorMessage = "Must not be Empty"), Key]
        public virtual String Name { get; set; }

        [Required(ErrorMessage = "Must not be Empty")]
        public virtual String ShortName { get; set; }

        [Required(ErrorMessage = "Must not be Empty")]
        public virtual String Description { get; set; }

        [Required(ErrorMessage = "Must not be Empty")]
        public virtual Boolean Selectable { get; set; }

        [Required(ErrorMessage = "Must not be Empty")]
        public virtual Boolean Approved { get; set; }
    }
}