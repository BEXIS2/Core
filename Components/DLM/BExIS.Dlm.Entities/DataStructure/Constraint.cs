using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        /// <summary>
        /// the culture the constraint applies to. i.e., a Regex to match a taxon name in German may differ from its equivalent in English, ...
        /// </summary>
        public virtual string CultureId { get; set; } 
       
        public virtual string Description { get; set; }

        /// <summary>
        /// determines whether the constraint should be evaluated or the negate of it. e.g., the input should be evaluated against the range or outside of the range.
        /// </summary>
        public virtual bool Negated { get; set; }

        /// <summary>
        /// provides a scope information so that validators can be grouped, categorized, or distinguished for a specific purpose. Like fast validation, detailed validation, etc.
        /// </summary>
        public virtual string Context { get; set; }

        /// <summary>
        /// The message to be conveyed to the user in case of rule break.
        /// </summary>
        public virtual string Message { get; set; }

        /// <summary>
        /// The message to be conveyed to the user in case of negated rule break.
        /// </summary>
        // maybe 
        public virtual string NegatedMessage { get; set; }
        
        #endregion

        #region Associations

        public virtual DataContainer DataContainer { get; set; }
        
        #endregion

        #region Mathods

        public abstract bool IsSatisfied(object data)
        {
            return (true);
        }

        #endregion

    }

    [AutomaticMaterializationInfo("Items", typeof(List<DomainItem>), "XmlDomainItems", typeof(XmlDocument))]
    public class DomainConstraint : Constraint
    {
        #region Attributes

        public virtual XmlDocument XmlDomainItems { get; set; }
        public List<DomainItem> Items { get; set; }
        #endregion

        #region Associations

        #endregion

        #region Mathods

        public DomainConstraint()
        {
            // set the default message and the negated message
        }

        public virtual bool IsSatisfied(object data)
        {
            this.Materialize(); // test it
            // Domain items are stored as string, so instead of converting them to the containers data type, it is easier and faster to convert the input data to string
            // it computes the XOR between the positive clause of the constraint and the "Negated" Boolean,
            // meaning if Negated is true the function returns false for inputs that are in the domain
            return (Negated ^ (Items.Where(p => p.Key.Equals(data.ToString())).Count() > 0)); 
        }

        #endregion

        public class DomainItem
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
    }

    public class PatternConstraint : Constraint
    {
        #region Attributes

        public virtual string MatchingPhrase { get; set; }

        #endregion

        #region Associations

        #endregion

        #region Mathods

        public PatternConstraint()
        {
            // set the default message and the negated message
        }
        /// <summary>
        /// checks whether the input parameter matches the MatchingPhrase
        /// </summary>
        /// <param name="data">is the input data in STRING format</param>
        /// <returns></returns>
        public virtual bool IsSatisfied(object data)
        {
            return (Negated ^ (Regex.IsMatch(data.ToString(), MatchingPhrase, RegexOptions.IgnoreCase)));
        }

        #endregion
}

    /// <summary>
    /// checks whether the value is inside/ outside of the specified range, not/including the boundaries.
    /// The range constraint usually applies to numeric data types, but in case of string it can be used as string length check. The data type
    /// of the provided value can be determined by the DataContainer.DataType....
    /// </summary>
    public class RangeConstraint : Constraint
    {
        #region Attributes

        public float Lowerbound { get; set; }
        public float Upperbound { get; set; }
        public bool LowerboundIncluded { get; set; }
        public bool UpperboundIncluded { get; set; }


        #endregion

        #region Associations

        #endregion

        #region Mathods

        public RangeConstraint()
        {
            // set the default message and the negated message
        }

        public virtual bool IsSatisfied(object data)
        {
            // the data type is defined by the associated data attribute
            // use dynamic link library, Flee or DLR to convert the Body to an executable code, pass data to it and return the result
            return false;
        }

        #endregion
    }
  
    /// <summary>
    /// compares the input value to a reference/ target one using a specified comparison operator. 
    /// The reference object can be an object or the value of a variable/ parameter in the same tuple
    /// </summary>
    public class ComparerConstraint : Constraint
    {
        #region Attributes
        
        /// <summary>
        /// The comparison operator, it is always a binary operator capable of comparing its left side to the right and return true or false.
        /// valid operators depend on the data type, so the UI should show proper operators based on the data attribute's data type during the constraint definition.
        /// </summary>
        public virtual ComparisonOperator Operator { get; set; }

        public ComparisonTargetType TargetType { get; set; }

        /// <summary>
        /// the target is used in the right side of the comparison operator
        /// If the target type is:
        /// Value: the target is the string representation of the value that should be compared to the input. the evaluation method should convert it to the container's data type before use.
        /// Parameter: the target shows the Id of the parameter in the current type that its value should be used in the comparison
        /// Variable: the target shows the Id of the variable in the current type that its value should be used in the comparison
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// in some cases the right hand side of the operator should be compared to the left having an offset. for example the project end date should be at least 100 days after its start date.
        /// Or the value of variable v1 can not be greater than 17% of the values of variable v2.
        /// Offset type can be absolute or ratio. Absolute offsets are added to the right hand side and ratios are multiplied to the right hand side before the comparison to take place
        /// </summary>
        public ComparisonOffsetType OffsetType { get; set; }

        /// <summary>
        /// The value of the offset to be taken into account in the comparison according to <seealso cref="ComparisonOffsetTye"/>
        /// </summary>
        public float Offset { get; set; }

        

        /// <summary>        
        /// Comparer: 
        ///     OP: the comparison operator like >, <, ==, . depeneds on the data type, so the UI should show proper operators based on the data attribute''s data type during the validator creation. 
        ///     TargetType: Var (Variable | Parameter), Value
        ///     Offset: The value that is considered (added/ multiplied) in the right side of the operator during the evaluation. case: the project finish date should be at least 1000 Ticks after its start time
        ///     or the amount of water in the v1 variable can not exceed 17% of the v2.
        ///     OffestType: indicate whether the offset value is absolute or percentage
        /// </summary>


        #endregion

        #region Associations

        #endregion

        #region Mathods

        public ComparerConstraint()
        {
            // set the default message and the negated message
        }

        public virtual bool IsSatisfied(object data)
        {
            // the data type is defined by the associated data attribute
            // use dynamic link library, Flee or DLR to convert the Body to an executable code, pass data to it and return the result
            return false;
        }

        #endregion

    }

    public enum ComparisonOperator
    {
        Equals, GreaerThan, GreaterThanOrEqual, LessThan, LessThanOrEqual, 
    }

    public enum ComparisonTargetType
    {
        Value, Parameter, Variable,
    }

    public enum ComparisonOffsetType
    {
        Absolute, Ratio,
    }
}
