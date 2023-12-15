using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Meanings;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Meanings
{
    public class Meaning : BaseEntity
    {
 

        public Meaning() {
            this.ExternalLinks = new List<MeaningEntry>();
            this.Related_meaning = new List<Meaning>();
        }

        public Meaning(Meaning meaning)
        {
            this.Name = meaning.Name;
            this.ShortName = meaning.ShortName;
            this.Description = meaning.Description;
            this.Selectable = meaning.Selectable;
            this.Approved = meaning.Approved;
            this.ExternalLinks = meaning.ExternalLinks;
            this.Related_meaning = meaning.Related_meaning;
        }

        public Meaning(String name, String shortName, String description, bool Selectable, bool approved, IList<MeaningEntry> externalLink, IList<Meaning> meaning)
        {
            this.Name = name;
            this.ShortName = shortName;
            this.Description = description;
            this.Selectable = Selectable;
            this.Approved = approved;
            this.ExternalLinks = externalLink;
            this.Related_meaning = meaning;
        }

        [Required(ErrorMessage = "Must not be Empty"),Key]
        public virtual String Name { get; set; }
        [Required(ErrorMessage = "Must not be Empty")]
        public virtual String ShortName { get; set; }
        [Required(ErrorMessage = "Must not be Empty")]
        public virtual String Description { get; set; }
        [Required(ErrorMessage = "Must not be Empty")]
        public virtual Boolean Selectable { get; set; }
        [Required(ErrorMessage = "Must not be Empty")]
        public virtual Boolean Approved { get; set; }
        public virtual IList<MeaningEntry> ExternalLinks { get; set; }
        public virtual IList<Meaning> Related_meaning { get; set; }

    }

}
