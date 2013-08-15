using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.DataStructure
{
    public class ConversionMethod: BaseEntity
    {
        #region Attributes

        public virtual string Description { get; set; }
        public virtual string Formula { get; set; } // 100*x => Target = 100*Source

        #endregion

        #region Associations

        public virtual Unit Source{ get; set; }
        public virtual Unit Target { get; set; }
        
        #endregion

        #region Mathods

        #endregion

    }
}
