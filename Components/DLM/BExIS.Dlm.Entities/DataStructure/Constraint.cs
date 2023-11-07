using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.DataStructure
{

    /// <summary>
    /// The base class for all concrete constraint types. It provides various attributes needed for all the constraint sub types. Among them the cultureId is a means 
    /// to define different constraints on a single data attribute based on the culture.
    /// </summary>
    public abstract class Constraint : BaseEntity
    {
        protected string defaultMessageTemplate;
        protected string defaultNegatedMessageTemplate;

        #region Attributes

        /// <summary>
        /// Indicates whether the constraint is defined internally or should be populated by accessing an external provider.
        /// </summary>
        public virtual ConstraintProviderSource Provider { get; set; }

        /// <summary>
        /// If the constraint is defined externally, the provider is supposed to have all the information needed to access the external source in order to obtain the required constraint attributes.
        /// </summary>
        public virtual string ConstraintSelectionPredicate { get; set; } // only for external providers

        /// <summary>
        /// the culture the constraint applies to. i.e., a Regex to match a taxon name in German may differ from its equivalent in English, ...
        /// </summary>
        public virtual string CultureId { get; set; }

        /// <summary>
        /// A free form name of the constraint
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// A free form description of the constraint
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// determines whether the constraint or its negation should be evaluated. e.g., the input should be evaluated for being in a range or outside of it.
        /// </summary>
        public virtual bool Negated { get; set; }

        /// <summary>
        /// provides a piece of  information for grouping, categorizing, or distinguishing for purposes. Like fast validation, detailed validation, validation via the UI or API, etc.
        /// </summary>
        public virtual string Context { get; set; }

        /// <summary>
        /// The message to be conveyed to the user in case of the constraint break.
        /// </summary>
        public virtual string MessageTemplate { get; set; }

        /// <summary>
        /// The message to be conveyed to the user in case of the negated constraint break.
        /// </summary>
        public virtual string NegatedMessageTemplate { get; set; }

        #endregion

        #region Associations

        /// <summary>
        /// The <see cref="DataContainer"/> the constraint applies on.
        /// </summary>
        public virtual DataContainer DataContainer { get; set; }

        public virtual ICollection<Variable> VariableConstraints { get; set; }

        #endregion

        #region Methods
        /// <summary>
        /// The method checks whether the input <paramref name="data"/> satisfies the constraint. To be implemented by concrete sub-classes.
        /// </summary>
        /// <param name="data">the data to be evaluated</param>
        /// <param name="auxiliary">An optional data item needed by some of the constraints. In most cases not used, but in comparison</param>     
        public abstract bool IsSatisfied(object data, object auxiliary = null);

        /// <summary>
        /// The actual error message to be returned to the caller. It is generated based on the <
        /// </summary>
        public abstract string ErrorMessage { get; }

        /// <summary>
        /// The formal human readable translation of the constraint.
        /// </summary>
        public abstract string FormalDescription { get; }
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
                        string.Join(",", Items.Select(p => p.Key))));
                }
                else
                {
                    return (string.Format(
                        (!string.IsNullOrWhiteSpace(MessageTemplate) ? MessageTemplate : defaultMessageTemplate),
                        string.Join(",", Items.Select(p => p.Key))));
                }
            }
        }

        public override string FormalDescription
        {
            get
            {
                if (Negated)
                {
                    return (string.Format("The value must not be any of these items: {0}.", string.Join(",", Items.Select(p => p.Key))));
                }
                else
                {
                    return (string.Format("The value must be one of these items: {0}.", string.Join(",", Items.Select(p => p.Key))));
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
            Items = new List<DomainItem>();
        }

        public DomainConstraint(ConstraintProviderSource provider, string constraintSelectionPredicate, string cultureId
            , string description, bool negated, string context, string messageTemplate, string negatedMessageTemplate, List<DomainItem> items) : this()
        {
            Contract.Requires(items != null);
            Contract.Requires(items.Count > 0);

            Provider = provider;
            ConstraintSelectionPredicate = constraintSelectionPredicate;
            CultureId = cultureId;
            Description = description;
            Negated = negated;
            Context = !string.IsNullOrEmpty(context) ? context : "Default";
            MessageTemplate = messageTemplate;
            NegatedMessageTemplate = negatedMessageTemplate;
            Items = items;

            this.Dematerialize();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="data"></param>
        /// <param name="auxiliary"></param>
        public override bool IsSatisfied(object data, object auxiliary = null)
        {
            if (Items == null || !Items.Any()) this.Materialize(); // test it

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
                        MatchingPhrase, (CaseSensitive ? "case sensitive" : "case insensitive")));
                }
                else
                {
                    return (string.Format(
                        (!string.IsNullOrWhiteSpace(MessageTemplate) ? MessageTemplate : defaultMessageTemplate),
                         MatchingPhrase, (CaseSensitive ? "case sensitive" : "case insensitive")));
                }
            }
        }

        public override string FormalDescription
        {
            get
            {
                if (Negated)
                {
                    return (string.Format("The value must not match this pattern: '{0}'. The matching is {1}.", MatchingPhrase, (CaseSensitive ? "case sensitive" : "case insensitive")));
                }
                else
                {
                    return (string.Format("The value must match this pattern: '{0}'. The matching is {1}.", MatchingPhrase, (CaseSensitive ? "case sensitive" : "case insensitive")));
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

        public PatternConstraint(ConstraintProviderSource provider, string constraintSelectionPredicate, string cultureId
            , string description, bool negated, string context, string messageTemplate, string negatedMessageTemplate, string matchingPhrase, bool caseSensitive)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(matchingPhrase));

            Provider = provider;
            ConstraintSelectionPredicate = constraintSelectionPredicate;
            CultureId = cultureId;
            Description = description;
            Negated = negated;
            Context = !string.IsNullOrEmpty(context) ? context : "Default";
            MessageTemplate = messageTemplate;
            NegatedMessageTemplate = negatedMessageTemplate;
            MatchingPhrase = matchingPhrase;
            CaseSensitive = caseSensitive;
        }

        /// <summary>
        /// checks whether the input parameter matches the MatchingPhrase
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="data">is the input data in STRING format</param>
        /// <param name="auxiliary"></param>
        public override bool IsSatisfied(object data, object auxiliary = null)
        {
            if (!CaseSensitive)
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
        public virtual double Lowerbound { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual double Upperbound { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual bool LowerboundIncluded { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual bool UpperboundIncluded { get; set; }

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
                        Lowerbound, (!LowerboundIncluded ? "less than or equal to" : "less than"), Upperbound, (!UpperboundIncluded ? "greater than or equal to" : "greater than")));
                }
                else
                {
                    return (string.Format(
                        (!string.IsNullOrWhiteSpace(MessageTemplate) ? MessageTemplate : defaultMessageTemplate),
                        Lowerbound, (LowerboundIncluded ? "inclusive" : "exclusive"), Upperbound, (UpperboundIncluded ? "inclusive" : "exclusive")));
                }
            }
        }

        public override string FormalDescription
        {
            get
            {
                if (Negated)
                {
                    return (string.Format("The value must be {0} {1} or {2} {3}.", (!LowerboundIncluded ? "less than or equal to" : "less than"), Lowerbound // if boundaries are not included in the constarint, they are part of the negated
                                                                                 , (!UpperboundIncluded ? "greater than or equal to" : "greater than"), Upperbound
                           ));
                }
                else
                {
                    return (string.Format("The value must be between {0} ({1}) and {2} ({3}).", Lowerbound, (LowerboundIncluded ? "including" : "excluding")
                                                                                          , Upperbound, (UpperboundIncluded ? "including" : "excluding")
                           ));
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

        public RangeConstraint(ConstraintProviderSource provider, string constraintSelectionPredicate, string cultureId
            , string description, bool negated, string context, string messageTemplate, string negatedMessageTemplate, double lowerbound, bool lowerboundIncluded
            , double upperbound, bool upperboundIncluded)
        {
            Contract.Requires(lowerbound <= upperbound);

            Provider = provider;
            ConstraintSelectionPredicate = constraintSelectionPredicate;
            CultureId = cultureId;
            Description = description;
            Negated = negated;
            Context = !string.IsNullOrEmpty(context) ? context : "Default";
            MessageTemplate = messageTemplate;
            NegatedMessageTemplate = negatedMessageTemplate;
            Lowerbound = lowerbound;
            LowerboundIncluded = lowerboundIncluded;
            Upperbound = upperbound;
            UpperboundIncluded = upperboundIncluded;
        }


        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="data"></param>
        /// <param name="auxiliary"></param>
        public override bool IsSatisfied(object data, object auxiliary = null)
        {
            // the data type is defined by the associated data attribute
            // use dynamic link library, Flee or DLR to convert the Body to an executable code, pass data to it and return the result
            double d = 0.0;
            if (data is string)
                d = ((string)data).Length;
            else
            {
                try
                {
                    d = Convert.ToDouble(data);
                }
                catch
                {
                    return (Negated ^ false);
                }
            }
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
    public class ComparisonConstraint : Constraint
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

        public override string FormalDescription
        {
            get
            {
                String comparer = "", target = "", offestType = "";
                switch (TargetType)
                {
                    case ComparisonTargetType.Value:
                        target = Target;
                        break;
                    case ComparisonTargetType.Parameter:
                        target = "The value of parameter with Id: " + Target;
                        break;
                    case ComparisonTargetType.Variable:
                        target = "The value of variable with Id: " + Target;
                        break;
                    default:
                        break;
                }
                switch (OffsetType)
                {
                    case ComparisonOffsetType.Absolute:
                        offestType = "plus";
                        break;
                    case ComparisonOffsetType.Ratio:
                        offestType = "multiply";
                        break;
                    default:
                        break;
                }
                if (Negated)
                {
                    switch (Operator)
                    {
                        case ComparisonOperator.Equals:
                            comparer = "not equal to";
                            break;
                        case ComparisonOperator.NotEquals:
                            comparer = "equal to";
                            break;
                        case ComparisonOperator.GreaerThan:
                            comparer = "less than or equal to";
                            break;
                        case ComparisonOperator.GreaterThanOrEqual:
                            comparer = "less than";
                            break;
                        case ComparisonOperator.LessThan:
                            comparer = "greater than or equal to";
                            break;
                        case ComparisonOperator.LessThanOrEqual:
                            comparer = "greater than";
                            break;
                        default:
                            break;
                    }
                    return (string.Format("The value must be {0} {1} {2} {3}", comparer, target, offestType, OffsetValue));
                }
                else
                {
                    switch (Operator)
                    {
                        case ComparisonOperator.Equals:
                            comparer = "equal to";
                            break;
                        case ComparisonOperator.NotEquals:
                            comparer = "not equal to";
                            break;
                        case ComparisonOperator.GreaerThan:
                            comparer = "greater than";
                            break;
                        case ComparisonOperator.GreaterThanOrEqual:
                            comparer = "greater than or equal to";
                            break;
                        case ComparisonOperator.LessThan:
                            comparer = "less than";
                            break;
                        case ComparisonOperator.LessThanOrEqual:
                            comparer = "less than or equal to";
                            break;
                        default:
                            break;
                    }
                    return (string.Format("The value must be {0} {1} {2} {3}", comparer, target, offestType, OffsetValue)); // the value must be greater than the target value plus the offest
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
        public ComparisonConstraint()
        {
            defaultMessageTemplate = "replace with a proper message {0}.";
            defaultNegatedMessageTemplate = "replace with a proper message {0}.";
        }

        public ComparisonConstraint(ConstraintProviderSource provider, string constraintSelectionPredicate, string cultureId
            , string description, bool negated, string context, string messageTemplate, string negatedMessageTemplate, ComparisonOperator comparisonOperator, ComparisonTargetType targetType
            , string target, ComparisonOffsetType offsetType, double offset)
        {
            //Contract.Requires();

            Provider = provider;
            ConstraintSelectionPredicate = constraintSelectionPredicate;
            CultureId = cultureId;
            Description = description;
            Negated = negated;
            Context = context ?? "Default";
            MessageTemplate = messageTemplate;
            NegatedMessageTemplate = negatedMessageTemplate;
            Operator = comparisonOperator;
            TargetType = targetType;
            Target = target;
            OffsetType = offsetType;
            OffsetValue = offset;
        }


        /// <summary>
        /// problem: the constraint needs the target value to compare it, if it is a variable or parameter the constraint has no clue / responsibility to access its value 
        /// solution: the caller must use the TargetType and Target properties to access proper variable/ parameter and obtain their values then pass the value as auxiliary data.
        /// the function just compares data to auxiliary according to the operator
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="data"></param>
        /// <param name="auxiliary"></param>
        public override bool IsSatisfied(object data, object auxiliary = null)
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
                            return Negated ^ (((string)data).Equals((string)auxiliary));
                        }
                        double d = Convert.ToDouble(data);
                        double aux = Convert.ToDouble(auxiliary);
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
                            return Negated ^ (!((string)data).Equals((string)auxiliary));
                        }
                        double d = Convert.ToDouble(data);
                        double aux = Convert.ToDouble(auxiliary);
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
                            return Negated ^ (((string)data).Length > ((string)auxiliary).Length);
                        }
                        double d = Convert.ToDouble(data);
                        double aux = Convert.ToDouble(auxiliary);
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
                            return Negated ^ (((string)data).Length >= ((string)auxiliary).Length);
                        }
                        double d = Convert.ToDouble(data);
                        double aux = Convert.ToDouble(auxiliary);
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
                            return Negated ^ (((string)data).Length < ((string)auxiliary).Length);
                        }
                        double d = Convert.ToDouble(data);
                        double aux = Convert.ToDouble(auxiliary);
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
                            return Negated ^ (((string)data).Length <= ((string)auxiliary).Length);
                        }
                        double d = Convert.ToDouble(data);
                        double aux = Convert.ToDouble(auxiliary);
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
    /// Constraints can obtain the definition of their function from their internal attributes or from an external source.
    /// In case of internal, the definition of the constraint is inside the constraint, but If it is external, the constraint is defined somewhere else like a database, and model just contains its identifier and access method. 
    /// This is a good option for integration of variable values with external systems. or variables that their values changes over time
    /// </summary>
    public enum ConstraintProviderSource
    {
        Internal, External
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
