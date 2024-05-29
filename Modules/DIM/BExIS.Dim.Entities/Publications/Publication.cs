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

        //ToDO Add DOI to the class
        /// <summary>
        /// DOI of a object from a Repository
        /// </summary>
        public virtual string Doi { get; set; }
        /// <summary>
        /// Id of a object from a Broker
        /// </summary>
        public virtual long ResearchObjectId { get; set; }

        /// <summary>
        /// Id of a object from a DataRepo
        /// </summary>
        public virtual long RepositoryObjectId { get; set; }

        /// <summary>
        /// Name of the Publication
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// NameTimestamp of the Publication
        /// </summary>
        public virtual DateTime Timestamp { get; set; }

        /// <summary>
        /// Status of the Publication
        /// </summary>
        public virtual string Status { get; set; }

        /// <summary>
        /// Local Filepath to server, starts @ datapath
        /// </summary>
        public virtual string FilePath { get; set; }

        /// <summary>
        /// Link to external Source
        /// </summary>
        public virtual string ExternalLink { get; set; }

        #endregion

        #region Associations        

        /// <summary>
        /// inverse map
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual DatasetVersion DatasetVersion { get; set; }


        /// <summary>
        /// inverse map
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual Broker Broker { get; set; }

        /// <summary>
        /// inverse map
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual Repository Repository { get; set; }

        /// <summary>
        /// inverse map
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual string Response { get; set; }

        #endregion 

        #region Methods

        #endregion
    }
}
