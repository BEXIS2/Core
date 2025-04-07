using System;
using System.ComponentModel.DataAnnotations;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Meanings
{
    public class PrefixCategory : BaseEntity
    {
        //private bool disposedValue;

        [Required(ErrorMessage = "Must not be Empty"), Key]
        public virtual String Name { get; set; }

        [Required(ErrorMessage = "Must not be Empty")]
        public virtual String Description { get; set; }

        public PrefixCategory(string Name, string Description)
        {
            this.Name = Name ?? throw new ArgumentNullException(nameof(Name));
            this.Description = Description ?? throw new ArgumentNullException(nameof(Description));
        }

        public PrefixCategory(PrefixCategory ExternalLink)
        {
            this.Name = ExternalLink.Name;
            this.Description = ExternalLink.Description;
        }

        public PrefixCategory()
        { }

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!disposedValue)
        //    {
        //        if (disposing)
        //        {
        //            // TODO: dispose managed state (managed objects)
        //        }

        //        // TODO: free unmanaged resources (unmanaged objects) and override finalizer
        //        // TODO: set large fields to null
        //        disposedValue = true;
        //    }
        //}

        //public void Dispose()
        //{
        //    // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //    Dispose(disposing: true);
        //    GC.SuppressFinalize(this);
        //}
    }
}