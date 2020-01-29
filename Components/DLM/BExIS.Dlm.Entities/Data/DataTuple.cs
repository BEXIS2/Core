using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Vaiona.Entities.Common;
using Vaiona.Logging.Aspects;
using Vaiona.Logging;
using System.IO;
using System.Xml.Serialization;
using System.Text;
using Vaiona.Persistence.Api;
using BExIS.Dlm.Entities.DataStructure;

/// <summary>
///
/// </summary>
namespace BExIS.Dlm.Entities.Data
{
    /// <summary>
    /// Its to overcome an inheritance issue with NH: when TupleVersion is derived from DataTuple all queries on DataTuple return versions too.
    /// </summary>
    /// <remarks></remarks>
    [AutomaticMaterializationInfo("VariableValues", typeof(List<VariableValue>), "XmlVariableValues", typeof(XmlDocument))]
    [AutomaticMaterializationInfo("Amendments", typeof(List<Amendment>), "XmlAmendments", typeof(XmlDocument))]
    public abstract class AbstractTuple : BaseEntity, IBusinessVersionedEntity
    {
        #region Attributes

        /// <summary>
        /// indicates the order of the associated tuple in the version
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual int OrderNo { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual TupleAction TupleAction { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual DateTime Timestamp { get; set; }

        /// <summary>
        /// Every variablevalue has a value, result time and also sampling time (if apply)
        /// In addition the xml node contains all associated parameter values
        /// every variablevalue knows about its Obtaining method which is one of the items in the DataAttribute ValueType
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual XmlDocument XmlVariableValues { get; set; }

        public virtual XmlDocument XmlVariableValues2 { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual XmlDocument XmlAmendments { get; set; }

        public virtual String JsonVariableValues { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string Values { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public abstract DataTupleType TupleType { get; }

        #endregion Attributes

        #region Associations

        /// <summary>
        /// inverse map
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual DatasetVersion DatasetVersion { get; set; }

        /// <summary>
        /// Map from and to XmlVariableValues. Do not map to persistence data directly
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual IList<VariableValue> VariableValues { get; set; }


        public virtual IList<VariableValue> VariableValues2 { get; set; }

        /// <summary>
        /// Do not map to persistence data directly. Materialize after load
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual IList<Amendment> Amendments { get; set; }

        #endregion Associations
    }

    /// <summary>
    /// In order to show what had happened to each tuple, a record of the action applied to them is maintained in the version they belong to.
    /// </summary>
    public enum TupleAction
    {
        Created = 1   // the tuple is created explicitly in this version
        , Edited = 2   // the tuple was from the previous version, but edited here and is attached to this version a new instance keeping the original tuple ID
        , Deleted = 3   // the tuple from the previous version is deleted, and this version is just pointing to that tuple in the previous one to keep track of deleted tuples. it is possible to omit this action
        , Untouched = 4   // the tuple is part of this version without any change. in this case the new version points to the previous one by an "Untouched"  action to prevent duplicating the tuple.
    }

    /// <summary>
    ///
    /// </summary>
    public enum DataTupleType
    {
        Original,
        History
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public class DataTuple : AbstractTuple
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public override DataTupleType TupleType
        {
            get { return DataTupleType.Original; }
        }

        #region Associations

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        //public virtual ICollection<DataTupleVersion> History { get; set; }

        #endregion Associations

        #region Methods

        // No need to override these functions, the base one performs the task for normal cases
        // If you have a very special case or the performance of the generic one is not good, then override the methods
        //public override void Dematerialize()
        //{
        //    XmlVariableValues = (XmlDocument)transformer.ExportTo(VariableValues, "VariableValues", 1);
        //    XmlAmendments = (XmlDocument)transformer.ExportTo(Amendments, "Amendments", 1);
        //}

        //public override void Materialize()
        //{
        //    VariableValues = transformer.ImportFrom<List<VariableValue>>(XmlVariableValues, null);
        //    Amendments = transformer.ImportFrom<List<Amendment>>(XmlAmendments, null);
        //}

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        public DataTuple()
        {
            //XmlVariableValues = new XmlDocument();
            //XmlAmendments = new XmlDocument();
            VariableValues = new List<VariableValue>();
            Amendments = new List<Amendment>();
            //History = new List<DataTupleVersion>();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>
        public override void Materialize(bool includeChildren = true)
        {
            base.Materialize();
            VariableValues.ToList().ForEach(p => p.Tuple = this);
        }

        public virtual void Materialize2(bool includeChildren = true)
        {

            using (XmlReader reader = XmlReader.Create((new StringReader(XmlVariableValues2.InnerXml))))
            {
                XmlSerializer xsSubmit = new XmlSerializer(typeof(List<VariableValue>));
                this.VariableValues2 = (List<VariableValue>)xsSubmit.Deserialize(reader);

                using (var uow = this.GetUnitOfWork())
                {
                    foreach (var item in VariableValues2)
                    {
                        Variable variable = uow.GetReadOnlyRepository<Variable>().Get(item.VariableId);
                        item.Variable = variable;
                        item.DataAttribute = variable.DataAttribute;
                    }
                }

            }
        }

        public virtual void Dematerialize2(bool includeChildren = true)
        {
            XmlSerializer xsSubmit = new XmlSerializer(typeof(List<VariableValue>));

            using (var sww = new StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sww))
                {
                    xsSubmit.Serialize(writer, this.VariableValues);

                    var x = new XmlDocument();
                    x.LoadXml(sww.ToString());

                    XmlVariableValues2 = x;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>  
        //public override void Dematerialize(bool includeChildren = true)
        //{


        //}

        #endregion
    }
}