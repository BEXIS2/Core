using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Vaiona.Entities.Common;

/// <summary>
///
/// </summary>        
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

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public abstract class Constraint: BaseEntity
    {
        protected string defaultMessageTemplate;
        protected string defaultNegatedMessageTemplate;

        #region Attributes

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual ConstraintProviderSource Provider { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual string ConstraintSelectionPredicate { get; set; } // only for external providers

        /// <summary>
        /// the culture the constraint applies to. i.e., a Regex to match a taxon name in German may differ from its equivalent in English, ...
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string CultureId { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual string Description { get; set; }

        /// <summary>
        /// determines whether the constraint should be evaluated or the negate of it. e.g., the input should be evaluated against the range or outside of the range.
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual bool Negated { get; set; }

        /// <summary>
        /// provides a scope information so that validators can be grouped, categorized, or distinguished for a specific purpose. Like fast validation, detailed validation, etc.
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual string Context { get; set; }

        /// <summary>
        /// The message to be conveyed to the user in case of rule break.
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual string MessageTemplate { get; set; }

        /// <summary>
        /// The message to be conveyed to the user in case of negated rule break.
        /// </summary>
        /// <remarks> maybe </remarks>
        /// <seealso cref=""/>        
        public virtual string NegatedMessageTemplate { get; set; }
        
        #endregion

        #region Associations

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual DataContainer DataContainer { get; set; }
        
        #endregion

        #region Mathods
        /// <summary>
        /// to be implemented by concrete classes. it checks whether the input data satisfies the constraint
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="data">the data to be evaluated</param>
        /// <param name="auxilialy">in most cases not used, but in comparison</param>     
        public abstract bool IsSatisfied(object data, object auxilialy = null);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public abstract string ErrorMessage { get; }
        #endregion

    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    [AutomaticMaterializationInfo("Items", typeof(List<DomainItem>), "XmlDomainItems", typeof(XmlDocument))]
    public class DomainConstraint : Constraint
    {
        #region Attributes

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual XmlDocument XmlDomainItems { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual List<DomainItem> Items { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public override string ErrorMessage
        {
            get
            {
                if (Negated)
                {
                    return (string.Format(
                        (!string.IsNullOrWhiteSpace(NegatedMessageTemplate) ? NegatedMessageTemplate : defaultNegatedMessageTemplate),
                        string.Join(",", Items.Select(p=>p.Key)) ));
                }
                else
                {
                    return (string.Format(
                        (!string.IsNullOrWhiteSpace(MessageTemplate) ? MessageTemplate : defaultMessageTemplate),
                        string.Join(",", Items.Select(p => p.Key)) ));
                }
            }
        }

        #endregion

        #region Associations

        #endregion

        #region Mathods

        /// <summary>
        /// set the default message and the negated message
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        public DomainConstraint()
        {      
            defaultMessageTemplate = "Provided value is not a domain item. The value should be one of these items: {0}.";
            defaultNegatedMessageTemplate = "Provided value is a domain item, but the constraint is negated. The value should not be one of these items: {0}.";
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="data"></param>
        /// <param name="auxilialy"></param>
        public override bool IsSatisfied(object data, object auxilialy = null)
        {
            this.Materialize(); // test it
            // Domain items are stored as string, so instead of converting them to the containers data type, it is easier and faster to convert the input data to string
            // it computes the XOR between the positive clause of the constraint and the "Negated" Boolean,
            // meaning if Negated is true the function returns false for inputs that are in the domain
            return (Negated ^ (Items.Where(p => p.Key.Equals(data.ToString(), StringComparison.InvariantCultureIgnoreCase)).Count() > 0)); 
        }

        #endregion

       
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class DomainItem
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public string Key { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public string Value { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class PatternConstraint : Constraint
    {
        #region Attributes

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual string MatchingPhrase { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual bool CaseSensitive { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public override string ErrorMessage
        {
            get
            {
                if (Negated)
                {
                    return (string.Format(
                        (!string.IsNullOrWhiteSpace(NegatedMessageTemplate) ? NegatedMessageTemplate : defaultNegatedMessageTemplate),
                        MatchingPhrase, (CaseSensitive? "case insensitive": "case sensitive") ));
                }
                else
                {
                    return (string.Format(
                        (!string.IsNullOrWhiteSpace(MessageTemplate) ? MessageTemplate : defaultMessageTemplate),
                         MatchingPhrase, (CaseSensitive ? "case insensitive" : "case sensitive") ));
                }
            }
        }

        #endregion

        #region Associations

        #endregion

        #region Mathods

        /// <summary>
        /// set the default message and the negated message
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        public PatternConstraint()
        {
            defaultMessageTemplate = "Provided value does not match the pattern. The value should match {0} {1}.";
            defaultNegatedMessageTemplate = "Provided value matches the pattern, but the constraint is negated. The value should not match {0} {1}.";
        }

        /// <summary>
        /// checks whether the input parameter matches the MatchingPhrase
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="data">is the input data in STRING format</param>
        /// <param name="auxilialy"></param>
        public override bool IsSatisfied(object data, object auxilialy = null)
        {
            if(!CaseSensitive)
                return (Negated ^ (Regex.IsMatch(data.ToString(), MatchingPhrase, RegexOptions.IgnoreCase)));
            else
                return (Negated ^ (Regex.IsMatch(data.ToString(), MatchingPhrase)));
        }

        #endregion
}

    /// <summary>
    /// checks whether the value is inside/ outside of the specified range, not/including the boundaries.
    /// The range constraint usually applies to numeric data types, but in case of string it can be used as string length check. The data type
    /// of the provided value can be determined by the DataContainer.DataType....
    /// </summary>
    /// <remarks></remarks>   
    public class RangeConstraint : Constraint
    {
        #region Attributes

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual double   Lowerbound { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual double   Upperbound { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual bool     LowerboundIncluded { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual bool     UpperboundIncluded { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public override string ErrorMessage
        {
            get
            {
                if (Negated)
                {
                    return (string.Format(
                        (!string.IsNullOrWhiteSpace(NegatedMessageTemplate) ? NegatedMessageTemplate : defaultNegatedMessageTemplate),
                        Lowerbound, (LowerboundIncluded ? "less than or equal to" : "less than"), Upperbound, (UpperboundIncluded ? "greater than or equal to" : "greater than")));
                }
                else
                {
                    return (string.Format(
                        (!string.IsNullOrWhiteSpace(MessageTemplate) ? MessageTemplate : defaultMessageTemplate),
                        Lowerbound, (LowerboundIncluded ? "inclusive" : "exclusive"), Upperbound, (UpperboundIncluded ? "inclusive" : "exclusive")));
                }
            }
        }
        #endregion

        #region Associations

        #endregion

        #region Mathods

        /// <summary>
        /// set the default message and the negated message
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param></param>       
        public RangeConstraint()
        {
            defaultMessageTemplate = "Provided value is out of range. The value should be between {0} {1} and {2} {3}.";
            defaultNegatedMessageTemplate = "Provided value is in range, but the constraint is negated. The value should be {1} {0} or {3} {2}.";
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="data"></param>
        /// <param name="auxilialy"></param>
        public override bool IsSatisfied(object data, object auxilialy = null)
        {
            // the data type is defined by the associated data attribute
            // use dynamic link library, Flee or DLR to convert the Body to an executable code, pass data to it and return the result
            double d = Convert.ToDouble(data);
            if (LowerboundIncluded == true && d < Lowerbound || (LowerboundIncluded == false && d <= Lowerbound)) //out of lower bound
                return (Negated ^ false);

            if (UpperboundIncluded == true && d > Upperbound || (UpperboundIncluded == false && d >= Upperbound)) //out of upper bound
                return (Negated ^ false);
            return (Negated ^ true);
        }        

        #endregion
    }
  
    /// <summary>
    /// compares the input value to a reference/ target one using a specified comparison operator. 
    /// The reference object can be an object or the value of a variable/ parameter in the same tuple
    /// </summary>
    /// <remarks></remarks>  
    public class CompareConstraint : Constraint
    {
        #region Attributes
        
        /// <summary>
        /// The comparison operator, it is always a binary operator capable of comparing its left side to the right and return true or false.
        /// valid operators depend on the data type, so the UI should show proper operators based on the data attribute's data type during the constraint definition.
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual ComparisonOperator Operator { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual ComparisonTargetType TargetType { get; set; }

        /// <summary>
        /// the target is used in the right side of the comparison operator
        /// If the target type is:
        /// Value: the target is the string representation of the value that should be compared to the input. the evaluation method should convert it to the container's data type before use.
        /// Parameter: the target shows the Id of the parameter in the current type that its value should be used in the comparison
        /// Variable: the target shows the Id of the variable in the current type that its value should be used in the comparison
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual string Target { get; set; }

        /// <summary>
        /// indicate whether the offset value is absolute or percentage
        /// in some cases the right hand side of the operator should be compared to the left having an offset. for example the project end date should be at least 100 days after its start date.
        /// Or the value of variable v1 can not be greater than 17% of the values of variable v2.
        /// Offset type can be absolute or ratio. Absolute offsets are added to the right hand side and ratios are multiplied to the right hand side before the comparison to take place
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual ComparisonOffsetType OffsetType { get; set; }

        /// <summary>
        /// The value of the offset to be taken into account in the comparison according to <seealso cref="ComparisonOffsetTye"/>
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual double OffsetValue { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public override string ErrorMessage
        {
            get
            {
                if (Negated)
                {
                    return (string.Format(
                        (!string.IsNullOrWhiteSpace(NegatedMessageTemplate) ? NegatedMessageTemplate : defaultNegatedMessageTemplate),
                        "put parameters here"));
                }
                else
                {
                    return (string.Format(
                        (!string.IsNullOrWhiteSpace(MessageTemplate) ? MessageTemplate : defaultMessageTemplate),
                        "put parameters here"));
                }
            }
        }

        #endregion

        #region Associations

        #endregion

        #region Mathods

        /// <summary>
        /// set the default message and the negated message
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param></param>       
        public CompareConstraint()
        {
            defaultMessageTemplate = "replace with a proper message {0}.";
            defaultNegatedMessageTemplate = "replace with a proper message {0}.";
        }


        /// <summary>
        /// problem: the constraint needs the target value to compare it, if it is a variable or parameter the constraint has no clue / responsibility to access its value 
        /// solution: the caller must use the TargetType and Target attributes to access proper variable/ parameter and obtain their values then pass the value as auxiliary data.
        /// the function just compares data to auxiliary according to the operator
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="data"></param>
        /// <param name="auxilialy"></param>
        public override bool IsSatisfied(object data, object auxilialy = null)
        {
            /// <summary>
            /// the data type is defined by the associated data attribute
            /// use dynamic link library, Flee or DLR to convert the Body to an executable code, pass data to it and return the result
            /// </summary>
            /// <remarks></remarks>        
            switch (Operator)
            {
                case ComparisonOperator.Equals:
                    {
                        //check for other type, like string
                        if (DataContainer.DataType.SystemType.Equals(System.TypeCode.String.ToString(), StringComparison.CurrentCultureIgnoreCase))
                        {
                            return Negated ^ (((string)data).Equals((string)auxilialy));
                        }
                        double d = Convert.ToDouble(data);
                        double aux = Convert.ToDouble(auxilialy);
                        if (OffsetType == ComparisonOffsetType.Absolute)
                            return Negated ^ (d.Equals(aux + OffsetValue));
                        if (OffsetType == ComparisonOffsetType.Ratio)
                            return Negated ^ (d.Equals(aux * OffsetValue));
                        break;
                    }
                case ComparisonOperator.NotEquals:
                    {
                        if (DataContainer.DataType.SystemType.Equals(System.TypeCode.String.ToString(), StringComparison.CurrentCultureIgnoreCase))
                        {
                            return Negated ^ (!((string)data).Equals((string)auxilialy));
                        }
                        double d = Convert.ToDouble(data);
                        double aux = Convert.ToDouble(auxilialy);
                        if (OffsetType == ComparisonOffsetType.Absolute)
                            return Negated ^ (!d.Equals(aux + OffsetValue));
                        if (OffsetType == ComparisonOffsetType.Ratio)
                            return Negated ^ (!d.Equals(aux * OffsetValue));
                        break;
                    }
                case ComparisonOperator.GreaerThan:
                    {
                        if (DataContainer.DataType.SystemType.Equals(System.TypeCode.String.ToString(), StringComparison.CurrentCultureIgnoreCase))
                        {
                            return Negated ^ (((string)data).Length > ((string)auxilialy).Length);
                        }
                        double d = Convert.ToDouble(data);
                        double aux = Convert.ToDouble(auxilialy);
                        if (OffsetType == ComparisonOffsetType.Absolute)
                            return Negated ^ (d > (aux + OffsetValue));
                        if (OffsetType == ComparisonOffsetType.Ratio)
                            return Negated ^ (d > (aux * OffsetValue));
                        break;
                    }
                case ComparisonOperator.GreaterThanOrEqual:
                    {
                        if (DataContainer.DataType.SystemType.Equals(System.TypeCode.String.ToString(), StringComparison.CurrentCultureIgnoreCase))
                        {
                            return Negated ^ (((string)data).Length >= ((string)auxilialy).Length);
                        }
                        double d = Convert.ToDouble(data);
                        double aux = Convert.ToDouble(auxilialy);
                        if (OffsetType == ComparisonOffsetType.Absolute)
                            return Negated ^ (d >= (aux + OffsetValue));
                        if (OffsetType == ComparisonOffsetType.Ratio)
                            return Negated ^ (d >= (aux * OffsetValue));
                        break;
                    }
                case ComparisonOperator.LessThan:
                    {
                        if (DataContainer.DataType.SystemType.Equals(System.TypeCode.String.ToString(), StringComparison.CurrentCultureIgnoreCase))
                        {
                            return Negated ^ (((string)data).Length < ((string)auxilialy).Length);
                        }
                        double d = Convert.ToDouble(data);
                        double aux = Convert.ToDouble(auxilialy);
                        if (OffsetType == ComparisonOffsetType.Absolute)
                            return Negated ^ (d < (aux + OffsetValue));
                        if (OffsetType == ComparisonOffsetType.Ratio)
                            return Negated ^ (d < (aux * OffsetValue));
                        break;
                    }
                case ComparisonOperator.LessThanOrEqual:
                    {                        
                        if (DataContainer.DataType.SystemType.Equals(System.TypeCode.String.ToString(), StringComparison.CurrentCultureIgnoreCase))
                        {
                            return Negated ^ (((string)data).Length <= ((string)auxilialy).Length);
                        }
                        double d = Convert.ToDouble(data);
                        double aux = Convert.ToDouble(auxilialy);
                        if (OffsetType == ComparisonOffsetType.Absolute)
                            return Negated ^ (d <= (aux + OffsetValue));
                        if (OffsetType == ComparisonOffsetType.Ratio)
                            return Negated ^ (d <= (aux * OffsetValue));
                        break;
                    }
                default:
                    break;
            }
            return false;
        }

        #endregion

    }

    /// <summary>
    /// 
    /// </summary>
    public enum ComparisonOperator
    {
        Equals, NotEquals, GreaerThan, GreaterThanOrEqual, LessThan, LessThanOrEqual, 
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ComparisonTargetType
    {
        Value, Parameter, Variable,
    }

    /// <summary>
    /// 
    /// </summary>
    public enum ComparisonOffsetType
    {
        Absolute, Ratio,
    }
}
