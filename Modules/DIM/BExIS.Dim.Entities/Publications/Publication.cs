using BExIS.Dlm.Entities.Data;
using System;
using Vaiona.Entities.Common;


namespace BExIS.Dim.Entities.Publications
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class Publication : BaseEntity, IBusinessVersionedEntity
    {
        #region Attributes

        public virtual long ResearchObjectId { get; set; }

        public virtual long RepositoryObjectId { get; set; }

        public virtual string Name { get; set; }

        public virtual DateTime Timestamp { get; set; }

        public virtual PublicationStatus Status { get; set; }

        public virtual string FilePath { get; set; }

        public virtual string ExternalLink { get; set; }

        #endregion

        #region Associations        
     
        public virtual DatasetVersion DatasetVersion { get; set; }
      
        public virtual Broker Broker { get; set; }
    
        public virtual Repository Repository { get; set; }
    
        public virtual string Response { get; set; }

        #endregion 

        #region Methods

        #endregion
    }

    public enum PublicationStatus
    {
        Open = 0,
        Accepted = 1,
        Rejected = 2
    }
}
