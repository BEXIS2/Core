using System.Collections.Generic;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.DataStructure
{
    public class Dimension : BaseEntity
    {
        #region Attributes

        public virtual string Name { get; set; }

        /// <summary>
        /// The specification in terms of LxMyTzPmEn. LMTPE are fundamental dimensions, xyzmn are +/- integer showing the power of their lefthand side dimension
        /// L(+1)M(+1,-1)T(-2) for acceleration, L0M0T+1 for time
        /// </summary>
        public virtual string Specification { get; set; }

        public virtual string Description { get; set; }

        #endregion Attributes

        #region Associations

        public virtual ICollection<Unit> Units { get; set; }

        #endregion Associations

        #region Mathods

        //overide equals and hashcode to use the spec/ axes
        public Dimension()
        {
            Units = new List<Unit>();
        }

        #endregion Mathods
    }
}