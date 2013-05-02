using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Core.Data;

namespace BExIS.Dlm.Entities.DataStructure
{

    /// <summary>
    /// Internal or external.
    /// In case of internal, the definition of the constraint is inside the model by means of validator, default or domain values.
    /// If it is external, the constraint is defined somewhere else like a database. and model just contains its identifier and access method. 
    /// This is good option for integration of variable values with external systems. or variables that their values changes over time
    /// </summary>
    public enum ConstraintProviderSource
    {
        Internal, External
    }

    public abstract class Constraint: BaseEntity
    {
        #region Attributes

        public virtual ConstraintProviderSource Provider { get; set; }
        public virtual string ConstraintSelectionPredicate { get; set; } // only for external providers

        #endregion

        //public virtual DataContainer DataContainer { get; set; }
        
        #region Associations

        #endregion

        #region Mathods

        #endregion

    }

    public class DomainValueConstraint : Constraint
    {
        #region Attributes
        
        /// <summary>
        /// The actual data type of the value is defined by the DataContainer.DataType.
        /// So developer should take care and convert this value to proper type before assigning it to variable values
        /// </summary>
        public virtual string DomainValue { get; set; } 
        public virtual string Description { get; set; }

        #endregion

        #region Associations

        #endregion

        #region Mathods

        #endregion
    }

    public class DefaultValueConstraint : Constraint
    {
        #region Attributes

        /// <summary>
        /// The actual data type of the value is defined by the DataContainer.DataType.
        /// So developer should take care and convert this value to proper type before assigning it to variable values
        /// Maybe object datatype is better. check if NH supports this and how? is it queriable?
        /// </summary>
        public virtual string DefaultValue { get; set; } 
        public virtual string MissingValue { get; set; } 

        #endregion

        #region Associations

        #endregion

        #region Mathods

        #endregion

    }

    public class ValidatorConstraint : Constraint
    {
        #region Attributes

        public virtual string Body { get; set; } // maybe it is possible to wiite another property of type Func<string, bool> (or Expression) on top of this, to change the string to an executable equivalent

        #endregion

        #region Associations

        #endregion

        #region Mathods

        #endregion

    }
}
