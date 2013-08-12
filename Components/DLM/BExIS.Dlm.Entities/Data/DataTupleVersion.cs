using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using BExIS.Core.Data;

namespace BExIS.Dlm.Entities.Data
{
    public class DataTupleVersion : DataTuple
    {
        // datasetVersion.Id+Tuple.Id are the PK together

        public virtual DatasetVersion ActingDatasetVersion { get; set; }
        public virtual DataTuple OriginalTuple { get; set; }

        public virtual ICollection<DataTupleVersion> History { get { throw new Exception("DataTuple Version has no history, use DataTuple entity instead");} }

        //public virtual DatasetVersion DatasetVersion { get; set; }
        //public virtual int OrderNo { get; set; } //indicates the order of the associated tuple in the version
        //public virtual TupleAction TupleAction { get; set; }

        //public virtual XmlDocument XmlVariableValues { get; set; }
        //public virtual XmlDocument XmlAmendments { get; set; } 

        ///// <summary>
        ///// to make it easy to find latest version, especially when someone wants to navigate from the tuple to the version(s).
        ///// A tuple may be linked to more than on version i.e., in case of UnChanged
        ///// </summary>
        //public virtual DateTime Timestamp { get; set; }
    }
}
