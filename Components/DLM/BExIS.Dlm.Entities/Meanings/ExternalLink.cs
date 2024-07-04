using System;
using System.ComponentModel.DataAnnotations;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Meanings
{
    public class ExternalLink : BaseEntity
    {
        //private bool disposedValue;

        [Required(ErrorMessage = "Must not be Empty"), Key, Url]
        public virtual string URI { get; set; }

        [Required(ErrorMessage = "Must not be Empty")]
        public virtual string Name { get; set; }

        [Required(ErrorMessage = "Must not be Empty"), Key]
        public virtual ExternalLinkType Type { get; set; }

        public virtual ExternalLink Prefix { get; set; }
        public virtual PrefixCategory prefixCategory { get; set; }

        public ExternalLink(string uRI, string label, ExternalLinkType type, ExternalLink Prefix, PrefixCategory prefixCategory)
        {
            this.URI = uRI ?? throw new ArgumentNullException(nameof(uRI));
            this.Name = label ?? throw new ArgumentNullException(nameof(label));
            this.Type = type;
            this.Prefix = Prefix;
            this.prefixCategory = prefixCategory;
        }

        public ExternalLink(ExternalLink ExternalLink)
        {
            this.URI = ExternalLink.URI;
            this.Name = ExternalLink.Name;
            this.Type = ExternalLink.Type;
            this.Prefix = ExternalLink.Prefix;
            this.prefixCategory = ExternalLink.prefixCategory;
            this.Id = ExternalLink.Id;
        }

        public ExternalLink()
        {
            this.prefixCategory = new PrefixCategory();
        }

        //protected  void Dispose(bool disposing)
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

    public enum ExternalLinkType
    {
        prefix = 1,
        link = 2,

        [Display(Name = "entity - class")]
        entity = 3,

        [Display(Name = "characteristics - property")]
        characteristics = 4,

        [Display(Name = "vocabulary - dictionary")]
        vocabulary = 5,

        [Display(Name = "relationship - connection")]
        relationship = 6
    }
}