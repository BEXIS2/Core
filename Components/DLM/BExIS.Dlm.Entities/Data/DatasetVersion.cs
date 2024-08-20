using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Vaiona.Entities.Common;

/// <summary>
///
/// </summary>
namespace BExIS.Dlm.Entities.Data
{
    /// <summary>
    ///
    /// </summary>
    public enum DatasetVersionStatus
    {
        Old = 0             // a previous version that is checked in and not the latest version anymore
        , CheckedOut = 1    // dataset is checked out for edit. this marker indicates the working copy version
        , CheckedIn = 2     // the checked in and latest version. upon checking in the working copy, the current checked in version changes to Old.
    }

    [AutomaticMaterializationInfo("ExtendedPropertyValues", typeof(List<ExtendedPropertyValue>), "XmlExtendedPropertyValues", typeof(XmlNode))]
    public class DatasetVersion : BusinessEntity, IBusinessVersionedEntity
    {
        #region Attributes

        // Mapping metadata and extended properties as component, makes them fields of Dataset table! this is good for performance (no need for joins) and
        // also helps to reduce the complexity of content versioning, as there are less associations.
        public virtual string Title { get; set; }

        public virtual string Description { get; set; } // Description of the dataset not the changes made in this version.

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string ChangeDescription { get; set; } // the information about the change itself, the reason, etc

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual DateTime Timestamp { get; set; } // to find latest version, ordering the versions, etc.

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual XmlDocument Metadata { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual XmlDocument XmlExtendedPropertyValues { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual DatasetVersionStatus Status { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string VersionType { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string VersionName { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string VersionDescription { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual bool PublicAccess { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual DateTime PublicAccessDate { get; set; }

        #endregion Attributes

        #region Associations

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual Dataset Dataset { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual Tag Tag { get; set; }

        /// <summary>
        /// There is a need for the tuples to be managed smarter inside any version, in order to:
        /// 1: reduce the amount of tuple duplication
        /// 2: be able to find out changes among versions e.g., deleted tuples, added tuples, changed ones
        /// 3: be able to find out what had changed in a specific version
        /// for these reasons, its better to not have the tuples loaded by the data access layer at first.
        /// PriliminaryTuples are those added or edited by this version. they may change over time
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual ICollection<DataTuple> PriliminaryTuples { get; set; }

        /// <summary>
        /// At least one for unstructured data
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual ICollection<ContentDescriptor> ContentDescriptors { get; set; }

        /// <summary>
        /// EffectiveTuples are those that show what is this version. They are calculated by running a set of queries over different tables, and unifying the results
        /// </summary>
        //[XmlIgnore]
        //public virtual ICollection<DataTuple> EffectiveTuples_del { get; set; }

        //public virtual ICollection<DataTuple> Tuples { get; set; } // if (dataStructure is UnStructuredDataStructure) no tuple can be added to this property
        // Don't Map for persistence

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual IList<ExtendedPropertyValue> ExtendedPropertyValues { get; set; }

        #endregion Associations

        #region Methods

        ///it is here for the automatic object creation by the persistence layer. Programmers SHOULD use the other constructor that takes a Dataset as a parameter

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param></param>
        public DatasetVersion()
        {
            ExtendedPropertyValues = new List<ExtendedPropertyValue>();
            ContentDescriptors = new List<ContentDescriptor>();
            PriliminaryTuples = new List<DataTuple>();
            Timestamp = DateTime.UtcNow;
            //XmlExtendedPropertyValues = new XmlDocument();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataset"></param>
        public DatasetVersion(Dataset dataset) : this()
        {
            this.Dataset = dataset;
            //if (this.Dataset.DataStructure is UnStructuredDataStructure)
            //{
            //}
        }

        //public override void Validate()
        //{
        //    // check whether type of data structure is unstructured? if yes, at least one ContentDescriptor must be provided
        //}

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param></param>
        public override void Materialize(bool includeChildren = true)
        {
            base.Materialize();
            if (includeChildren)
            {
                if (this.PriliminaryTuples != null)
                    this.PriliminaryTuples.ToList().ForEach(p => p.Materialize());
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param></param>
        public override void Dematerialize(bool includeChildren = true)
        {
            base.Dematerialize();
            if (includeChildren)
            {
                if (this.PriliminaryTuples != null)
                    this.PriliminaryTuples.ToList().ForEach(p => p.Dematerialize());
            }
        }

        #endregion Methods
    }
}