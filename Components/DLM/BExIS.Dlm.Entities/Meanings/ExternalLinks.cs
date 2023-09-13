using System;
using System.ComponentModel.DataAnnotations;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Meanings
{
    public class ExternalLink : BaseEntity, IDisposable
    {
        private bool disposedValue;

        [RegularExpression(@"^http://.*", ErrorMessage = "URI must start with http://")]
        public virtual string URI { get; set; }
        [Required(ErrorMessage = "Must not be Empty")]
        public virtual string Name { get; set; }
        [Required(ErrorMessage = "Must not be Empty")]
        public virtual string Type { get; set; }

        public ExternalLink(string uRI, string label, string type)
        {
            URI = uRI ?? throw new ArgumentNullException(nameof(uRI));
            this.Name = label ?? throw new ArgumentNullException(nameof(label));
            this.Type = type ?? throw new ArgumentNullException(nameof(type));

        }

        public ExternalLink(ExternalLink ExternalLink)
        {
            this.URI = ExternalLink.URI;
            this.Name = ExternalLink.Name;
            this.Type = ExternalLink.Type;
        }

        public ExternalLink() { }

        



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
}
