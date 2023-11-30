using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Meanings
{
    public class MeaningEntry : BaseEntity, IDisposable
    {
        private bool disposedValue;
        public virtual ExternalLink MappingRelation { get; set; }
        public virtual IList<ExternalLink> MappedLinks { get; set; }

        public MeaningEntry(){
            this.MappingRelation = new ExternalLink();
            this.MappedLinks = new List<ExternalLink>();
        }
        public MeaningEntry(MeaningEntry meaning)
        {
            this.MappingRelation = meaning.MappingRelation;
            this.MappedLinks = meaning.MappedLinks;
        }

        public MeaningEntry( ExternalLink mapping_relation, IList<ExternalLink> MappedLinks)
        {
            this.MappingRelation = mapping_relation;
            this.MappedLinks = MappedLinks;
        }

        protected void Dispose(bool disposing)
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
