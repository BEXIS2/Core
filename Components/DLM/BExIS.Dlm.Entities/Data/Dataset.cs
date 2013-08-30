using System;
using System.Collections.Generic;
using BExIS.Dlm.Entities.Administration;
using BExIS.Dlm.Entities.DataStructure;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Data
{
    public enum DatasetStatus
    {
            CheckedOut  = 1 // dataset version is newly created or checked out for edit. during this time no other user is able to change the dataset
        ,   CheckedIn   = 2 // the version has committed the changes. The whole version and its tuples are freezed then.
        ,   Deleted     = 3 // dataset was committed and then logically deleted. physical delete in not managed here because it deletes the should versions, histories. but a Purge function can be available for db admins
    }

    /// <summary>
    /// The dataset points to many versions, but those versions do not have the effective complete tuples because of the design of the database. In order to have a complete version of 
    /// the dataset, the DatasetManager.GetDatasetLatestVersion or GetDatasetVersion should be called.
    /// </summary>
    public class Dataset : BusinessEntity
    {        
        #region Attributes
        public virtual DatasetStatus Status { get; set; }
        public virtual DateTime LastCheckIOTimestamp { get; set; } // the time of last check-in or checkout
        public virtual string CheckOutUser { get; set; }

        #endregion

        #region Associations

        public virtual DataStructure.DataStructure DataStructure { get; set; }
        public virtual ICollection<DatasetVersion> Versions { get; set; }
        public virtual ResearchPlan ResearchPlan { get; set; } // it can be null, but check how hibernate deals with nullables
        public virtual ICollection<DataView> Views { get; set; }
        #endregion

        #region Methods

        public Dataset()
            : this(new StructuredDataStructure())
        {
            
        }

        public Dataset(DataStructure.DataStructure dataStructure)
        {
            Versions = new List<DatasetVersion>(); 
            Status = DatasetStatus.CheckedIn;
            //Metadata = null; // new XmlElement();// Metadata.Metadata();
            //XmlExtendedPropertyValues = new XmlDocument();
            this.DataStructure = dataStructure;
        }

        //public override void Validate()
        //{            
        //    // check whether type of data structure is unstructured? if yes, at least one ContentDescriptor must be provided
        //}

        #endregion
    }
}
