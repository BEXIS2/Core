using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.DataStructure
{
    public class DataType : BaseEntity
    {
        #region Attributes

        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        public virtual string SystemType { get; set; } // System.Int32, etc

        #endregion

        #region Associations

        public virtual ICollection<DataContainer> DataContainers { get; set; }
        public virtual ICollection<Unit> ApplicableUnits { get; set; }

        #endregion

        #region Mathods

        public DataType()
        {
            DataContainers = new List<DataContainer>();
            ApplicableUnits = new List<Unit>();
        }
        #endregion

    }
}
