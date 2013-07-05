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

        // Its not needed, make the database more complex, Javad/ 03.07.13
        //public virtual ICollection<Constraint> Constraints { get; set; }

        //// not to map
        //public IList<DomainValueConstraint> DomainValues
        //{
        //    get
        //    {
        //        return (this.Constraints.Distinct()
        //                    .Where(p => p.GetType().Equals(typeof(DomainValueConstraint)))
        //                    .ToList() as IList<DomainValueConstraint>
        //                );
        //    }
        //}

        //// not to map
        //public IList<ValidatorConstraint> Validators
        //{
        //    get
        //    {
        //        return (this.Constraints.Distinct()
        //                    .Where(p => p.GetType().Equals(typeof(ValidatorConstraint)))
        //                    .ToList() as IList<ValidatorConstraint>
        //                );
        //    }
        //}

        #endregion

        #region Mathods

        public ExtendedProperty()
        {
            //Constraints = new List<Constraint>();
        }
        
        #endregion
    }
}
