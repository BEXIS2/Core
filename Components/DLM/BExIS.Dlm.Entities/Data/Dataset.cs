using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.DataStructure;
using System;
using System.Collections.Generic;
using Vaiona.Entities.Common;

/// <summary>
///
/// </summary>
namespace BExIS.Dlm.Entities.Data
{
    /// <summary>
    /// This is a list fo currently used entity types in BExIS. It is used to identify the type of the entity in a generic way.
    /// </summary>
    public enum EntityType
    {
        Dataset = 1,
        Publication = 2,
        Sample = 3,
        Extension = 4,
    }

    /// <summary>
    ///
    /// </summary>
    public enum DatasetStatus
    {
        CheckedOut = 1 // dataset version is newly created or checked out for edit. during this time no other user is able to change the dataset
        , CheckedIn = 2 // the version has committed the changes. The whole version and its tuples are freezed then.
        , Deleted = 3 // dataset was committed and then logically deleted. physical delete in not managed here because it deletes the should versions, histories. but a Purge function can be available for db admins
    }

    /// <summary>
    ///
    /// </summary>
    public enum DatasetStateInfo
    {
        NotValid = 1, // Metadata is not valid
        Valid = 2  // Metadata is valid
    }

    /// <summary>
    /// The dataset points to many versions, but those versions do not have the effective complete tuples because of the design of the database. In order to have a complete version of
    /// the dataset, the DatasetManager.GetDatasetLatestVersion or GetDatasetVersion should be called.
    /// </summary>
    /// <remarks></remarks>
    public class Dataset : BusinessEntity
    {
        #region Attributes

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual DatasetStatus Status { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual DateTime LastCheckIOTimestamp { get; set; } // the time of last check-in or checkout

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string CheckOutUser { get; set; }

        #endregion Attributes

        #region Associations

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual DataStructure.DataStructure DataStructure { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual MetadataStructure.MetadataStructure MetadataStructure { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual ICollection<DatasetVersion> Versions { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual ResearchPlan ResearchPlan { get; set; } // it can be null, but check how hibernate deals with nullables

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual EntityTemplate EntityTemplate { get; set; } // it can be null, but check how hibernate deals with nullables

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual ICollection<DatasetView> Views { get; set; }

        #endregion Associations

        #region Methods

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param></param>
        public Dataset()
            : this(new EntityTemplate(), new StructuredDataStructure())
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataStructure"></param>
        public Dataset(EntityTemplate entityTemplate, DataStructure.DataStructure dataStructure)
        {
            if (entityTemplate == null)
                throw new ArgumentNullException("Dataset can not be constructed without a entity template.");

            if (dataStructure != null)
            {
                this.DataStructure = dataStructure;
                dataStructure.Datasets.Add(this);
            }

            Versions = new List<DatasetVersion>();
            Status = DatasetStatus.CheckedIn;
        }

        //public override void Validate()
        //{
        //    // check whether type of data structure is unstructured? if yes, at least one ContentDescriptor must be provided
        //}

        #endregion Methods
    }
}