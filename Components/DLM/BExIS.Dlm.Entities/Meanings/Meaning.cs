using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Meanings;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Meanings
{
    public class Meaning : BaseEntity , IDisposable
    {
        private bool disposedValue;

        public Meaning() { }

        public Meaning(Meaning meaning)
        {
            this.Name = meaning.Name;
            this.ShortName = meaning.ShortName;
            this.Description = meaning.Description;
            this.approved = meaning.approved;
            this.selectable = meaning.selectable;
            this.variable = meaning.variable;
            this.externalLink = meaning.externalLink;
            this.related_meaning = meaning.related_meaning;
        }

        public Meaning(String name, String shortName, String description, Selectable selectable, Approved approved, IEnumerable<ExternalLink> externalLink, IList<Variable> variable, IList<Meaning> meaning)
        {
            this.Name = name;
            this.ShortName = shortName;
            this.Description = description;
            this.selectable = selectable;
            this.approved = approved;
            this.variable = variable;
            this.externalLink = externalLink;
            this.related_meaning = meaning;
        }

        [Required(ErrorMessage = "Must not be Empty")]
        public virtual String Name { get; set; }
        [Required(ErrorMessage = "Must not be Empty")]
        public virtual String ShortName { get; set; }
        [Required(ErrorMessage = "Must not be Empty")]
        public virtual String Description { get; set; }
        [Required(ErrorMessage = "Must not be Empty")]
        public virtual Selectable selectable { get; set; }
        [Required(ErrorMessage = "Must not be Empty")]
        public virtual Approved approved { get; set; }

        [Required(ErrorMessage = "Must not be Empty")]
        public virtual IEnumerable<ExternalLink> externalLink { get; set; }

        [Required(ErrorMessage = "Must not be Empty")]
        public virtual IEnumerable<Variable> variable { get; set; }

        public virtual IEnumerable<Meaning> related_meaning { get; set; }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~Meaning()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public enum Approved
    {
        yes = 1
        , No = 2
    }
    public enum Selectable
    {
        yes = 1
        , No = 2
    }
}
