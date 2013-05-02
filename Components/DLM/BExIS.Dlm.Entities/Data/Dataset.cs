using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Core.Data;
using System.Xml.Linq;
using System.Xml;
using BExIS.Dlm.Entities.DataStructure;

namespace BExIS.Dlm.Entities.Data
{
    [AutomaticMaterializationInfo("ExtendedPropertyValues", typeof(List<ExtendedPropertyValue>), "XmlExtendedPropertyValues", typeof(XmlNode))]
    public class Dataset : BusinessEntity, IBusinessVersionedEntity
    {        
        #region Attributes

        /// Mapping metadata and extended propertiees as component, makes them fields of Dataset table! this is good for performance (no need for joins) and 
        /// also helps to reduce the complixity of content versioning, as there are less associations.
        public virtual string Title { get; set; }
        public virtual string Description { get; set; }
        public virtual XmlDocument Metadata { get; set; } 
        public virtual XmlDocument XmlExtendedPropertyValues { get; set; } 

        #endregion

        #region Associations

        public virtual DataStructure.DataStructure DataStructure { get; set; }
        public virtual ICollection<DataTuple> Tuples { get; set; } // if (dataStructure is UnStructuredDataStructure) no tuple can be added to this property
        // Dont Map for persistence
        public virtual IList<ExtendedPropertyValue> ExtendedPropertyValues { get; set; }

        /// <summary>
        /// At least one for unstructured data
        /// </summary>
        public virtual ICollection<ContentDescriptor> ContentDescriptors { get; set; }

        public virtual ICollection<DataView> Views { get; set; }
        // Dataset Versions
        #endregion

        #region Methods

        public Dataset()
            : this(new StructuredDataStructure())
        {
        }

        public Dataset(DataStructure.DataStructure dataStructure)
        {
            ExtendedPropertyValues = new List<ExtendedPropertyValue>();
            Tuples = new List<DataTuple>();
            ContentDescriptors = new List<ContentDescriptor>();
            Metadata = null; // new XmlElement();// Metadata.Metadata();
            //XmlExtendedPropertyValues = new XmlDocument();
            this.DataStructure = dataStructure;
            if (dataStructure is UnStructuredDataStructure)
                Tuples = null;
        }

        public override void Validate()
        {            
            // check whether type of data structure is unstructured? if yes, at least one ContentDescriptor must be provided
        }

        public virtual void AddTuples(params DataTuple[] tuples)
        {
            if (this.DataStructure is UnStructuredDataStructure)
                throw (new Exception("Unstructured datasets are not allowed to have data tuples!"));
            foreach (var tuple in tuples)
            {
                if (!Tuples.Contains(tuple))
                {
                    if (tuple.OrderNo <= 0)
                        tuple.OrderNo = Tuples.Count() + 1;
                    tuple.Dataset = this;
                    Tuples.Add(tuple);
                }                
            }
        }

        public override void Materialize()
        {
            base.Materialize();
            this.Tuples.ToList().ForEach(p => p.Materialize());
        }

        public override void Dematerialize()
        {
            base.Dematerialize();
            //this.Tuples.ForEach(p => p.Dematerialize());
            // for test
            foreach (var item in this.Tuples)
            {
                item.Dematerialize();
            }

        }

        #endregion
    }

    //public class DataSetVersion : Dataset
    //{
    //}
}
