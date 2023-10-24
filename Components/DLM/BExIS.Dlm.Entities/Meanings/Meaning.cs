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

        public Meaning() {
            ExternalLink = new List<ExternalLink>();
        
        }

        public Meaning(Meaning meaning)
        {
            this.Name = meaning.Name;
            this.ShortName = meaning.ShortName;
            this.Description = meaning.Description;
            this.Selectable = meaning.Selectable;
            this.Variables = meaning.Variables;
            this.ExternalLink = meaning.ExternalLink;
            this.Related_meaning = meaning.Related_meaning;
        }

        public Meaning(String name, String shortName, String description, Selectable Selectable, Approved approved, IEnumerable<ExternalLink> ExternalLink, IList<Variable> Variable, IList<Meaning> meaning)
        {
            this.Name = name;
            this.ShortName = shortName;
            this.Description = description;
            this.Selectable = Selectable;
            this.Approved = approved;
            this.Variables = Variable;
            this.ExternalLink = ExternalLink;
            this.Related_meaning = meaning;
        }

        [Required(ErrorMessage = "Must not be Empty")]
        public virtual String Name { get; set; }
        [Required(ErrorMessage = "Must not be Empty")]
        public virtual String ShortName { get; set; }
        [Required(ErrorMessage = "Must not be Empty")]
        public virtual String Description { get; set; }
        [Required(ErrorMessage = "Must not be Empty")]
        public virtual Selectable Selectable { get; set; }
        [Required(ErrorMessage = "Must not be Empty")]
        public virtual Approved Approved { get; set; }

        [Required(ErrorMessage = "Must not be Empty")]
        public virtual IEnumerable<ExternalLink> ExternalLink { get; set; }

        [Required(ErrorMessage = "Must not be Empty")]
        public virtual IEnumerable<Variable> Variables { get; set; }

        public virtual IEnumerable<Meaning> Related_meaning { get; set; }

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
