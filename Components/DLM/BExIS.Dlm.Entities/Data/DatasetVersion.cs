using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Core.Data;
using System.Xml.Linq;
using System.Xml;
using BExIS.Dlm.Entities.DataStructure;
using System.Xml.Serialization;

namespace BExIS.Dlm.Entities.Data
{

    [AutomaticMaterializationInfo("ExtendedPropertyValues", typeof(List<ExtendedPropertyValue>), "XmlExtendedPropertyValues", typeof(XmlNode))]
    public class DatasetVersion : BusinessEntity, IBusinessVersionedEntity
    {
        #region Attributes

        /// Mapping metadata and extended properties as component, makes them fields of Dataset table! this is good for performance (no need for joins) and 
        /// also helps to reduce the complexity of content versioning, as there are less associations.
        //public virtual string Title { get; set; }
        //public virtual string Description { get; set; } // Description of the dataset not the changes made in this version.
        public virtual string ChangeDescription { get; set; } // the information about the change itself, the reason, etc
        public virtual DateTime Timestamp { get; set; } // to find latest version, ordering the versions, etc.
        public virtual XmlDocument Metadata { get; set; }
        public virtual XmlDocument XmlExtendedPropertyValues { get; set; }


        #endregion

        #region Associations

        public virtual Dataset Dataset { get; set; }

        /// <summary>
        /// There is a need for the tuples to be managed smarter inside any version, in order to:
        /// 1: reduce the amount of tuple duplication
        /// 2: be able to find out changes among versions e.g., deleted tupes, added tuples, changed ones
        /// 3: be able to find out what had changed in a specific version
        /// for these reasons, its better to not have the tuples loaded by the data access layer at first.       
        /// PriliminaryTuples are those added or edited by this version. they may change over time
        /// </summary>
        public virtual ICollection<DataTuple> PriliminaryTuples { get; set; }

        /// <summary>
        /// At least one for unstructured data
        /// </summary>
        public virtual ICollection<ContentDescriptor> ContentDescriptors { get; set; }

        /// <summary>
        /// EffectiveTuples are those that show what is this version. They are caclulated by runnung a set of queries over different tables, and unifying the results
        /// </summary>
        [XmlIgnore]
        public virtual ICollection<DataTuple> EffectiveTuples { get; set; }

        //public virtual ICollection<DataTuple> Tuples { get; set; } // if (dataStructure is UnStructuredDataStructure) no tuple can be added to this property
        // Don't Map for persistence
        public virtual IList<ExtendedPropertyValue> ExtendedPropertyValues { get; set; }

        #endregion

        #region Methods

        ///it is here for the automatic object creation by the persistence layer. Programmers SHOULD use the other constructor that takes a Dataset as a parameter
        public DatasetVersion()
        {
            ExtendedPropertyValues = new List<ExtendedPropertyValue>();
            ContentDescriptors = new List<ContentDescriptor>();
            //XmlExtendedPropertyValues = new XmlDocument();
        }

        public DatasetVersion(Dataset   dataset): this()
        {
            this.Dataset = dataset;
            //if (this.Dataset.DataStructure is UnStructuredDataStructure)
            //{
            //}
        }

        public override void Validate()
        {            
            // check whether type of data structure is unstructured? if yes, at least one ContentDescriptor must be provided
        }
       
        public override void Materialize()
        {
            base.Materialize();
            this.PriliminaryTuples.ToList().ForEach(p => p.Materialize());
        }

        public override void Dematerialize()
        {
            base.Dematerialize();
            this.PriliminaryTuples.ToList().ForEach(p => p.Dematerialize());            
        }

        #endregion
    }

   
}
