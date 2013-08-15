using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.DataStructure
{
    public class Methodology : BaseEntity
    {
        #region Attributes

        public virtual string AppliedStandards { get; set; }
        public virtual string Procedure { get; set; }
        public virtual string Tolerance { get; set; }
        public virtual string Tools { get; set; }

        #endregion

        #region Associations

        public virtual ICollection<DataContainer> DataContainers { get; set; }

        #endregion

        #region Mathods
        public Methodology()
        {
            DataContainers = new List<DataContainer>();
        }

        #endregion
    }
}
