using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Vaiona.Entities.Common;

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

        #region Associations

        public virtual DataContainer DataContainer { get; set; }
        
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
        public virtual string Kind { get; set; } // Range, Regex, Domain, Comparer

        /// <summary>
        /// If Range -> x..y | If domain -> x, y, z | If comparer -> x so that x is the variable Id in the same data structure that the input is compared with
        /// </summary>
        public virtual string Body { get; set; } // maybe it is possible to write another property of type Func<string, bool> (or Expression) on top of this, to change the string to an executable equivalent
        public virtual string CultureId { get; set; } // the culture validator applies to. i.e., a Regex to match a taxon name in German may differ from its equivalent in English, ...
        public virtual string Description { get; set; } // maybe: promote to Constraint
        public virtual bool Negated { get; set; } // determines whether the body should be evaluated or the negate of it. e.g., the input should be evaluated against the range or outside of the range.
        
        /// <summary>
        /// if it is string -> Range validator checks the length of the input, otherwise the value
        /// </summary>
        public virtual System.TypeCode DataType { get; set; }

        /// <summary>
        /// It is an xml node with variable (preferably on) elements. the content of the element(s) depends on the Kind of the validator
        /// Range: L: Is lower bound inclusive | U: is upper bound inclusive
        /// Comparer: 
        ///     OP: the comparison operator like >, <, ==, . depeneds on the data type, so the UI should show proper operators based on the chosen data type during the validator creation. 
        ///     TargetType: Var (Variable | Parameter), Value
        ///     Offset: The value that is considered (added/ multiplied) in the right side of the operator during the evaluation. case: the project finish date should be at least 1000 Ticks after its start time
        ///     or the amount of water in the v1 variable can not exceed 17% of the v2.
        ///     OffestType: indicate whether the offset value is absolute or percentage
        /// </summary>
        public virtual XmlNode Opntions { get; set; }

        /// <summary>
        /// provides a scope information so that validators can be grouped, categorized, or distinguished for a specific purpose. Like fast validation, detailed validation, etc.
        /// </summary>
        public virtual string Context { get; set; }

        /// <summary>
        /// The message to be conveyed to the user in case of rule break.
        /// </summary>
        public virtual string Message { get; set; }

        #endregion

        #region Associations

        #endregion

        #region Mathods

        public virtual bool Evaulate(object data)
        {
            // use dynamic link library, Flee or DLR to cover the Body to an executable code, pass data to it and return the result
            return false;
        }

        #endregion

    }
}
