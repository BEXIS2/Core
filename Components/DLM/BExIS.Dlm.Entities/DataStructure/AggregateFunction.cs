using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Core.Data;

namespace BExIS.Dlm.Entities.DataStructure
{
    /// <summary>
    /// This entity lets define various processing functions that can be applied on variables. for example, temperature can be processed by Min, Max, Average, etc.
    /// </summary>
    public class AggregateFunction: BaseEntity
    {
        #region Attributes

        public virtual string Name { get; set; }
        public virtual string Description { get; set; }

        #endregion

        #region Associations

        public virtual ICollection<DataContainer> DataContainers { get; set; }

        #endregion

        #region Mathods

        public AggregateFunction()
        {
            DataContainers = new List<DataContainer>();
        }


        #endregion

    }
}
