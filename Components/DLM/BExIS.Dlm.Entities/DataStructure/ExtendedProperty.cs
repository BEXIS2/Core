using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Core.Data;

namespace BExIS.Dlm.Entities.DataStructure
{
    public class ExtendedProperty : BaseEntity
    {
        #region Attributes

        public virtual string Name { get; set; }
        public virtual string Description { get; set; }
        
        #endregion

        #region Associations

        public virtual DataContainer DataContainer { get; set; }
        public virtual ICollection<Constraint> Constraints { get; set; }

        #endregion

        #region Mathods

        public ExtendedProperty()
        {
            Constraints = new List<Constraint>();
        }
        
        #endregion
    }
}
