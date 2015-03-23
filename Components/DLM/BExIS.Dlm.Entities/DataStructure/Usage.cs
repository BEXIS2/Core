using BExIS.Dlm.Entities.Common;
using System.Collections.Generic;
using Vaiona.Entities.Common;

/// <summary>
///
/// </summary>        
namespace BExIS.Dlm.Entities.DataStructure
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class Parameter : BaseUsage
    {
        public Parameter()
        {
            MinCardinality = 0; // to make the parameter optional by default
            MaxCardinality = 1; // this must always remain 1
        }
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual DataAttribute DataAttribute { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual Variable Variable { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual bool IsValueOptional 
        {
            get { return MinCardinality < 1; } //if MinCardinality cardinality is zero (less than 1), the parameter value is optional
            set { MinCardinality = value ? 0 : 1; } // if value is optional, set the min cardinality to zero
        }

        /// <summary>
        /// The actual data type of the value is defined by the DataAttribute.DataType.
        /// So developer should take care and convert this value to proper type before assigning it to parameter values
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual string DefaultValue { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual string MissingValue { get; set; } 

    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class Variable : BaseUsage
    {
        public Variable()
        {
            MinCardinality = 0; // to make the parameter optional by default
            MaxCardinality = 1; // this must always remain 1
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual DataAttribute DataAttribute { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual StructuredDataStructure DataStructure { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks> DataAttribute is the controller of this association </remarks>
        /// <seealso cref=""/>        
        public virtual ICollection<Parameter> Parameters { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual bool IsValueOptional 
        {
            get { return MinCardinality < 1; } //if MinCardinality cardinality is zero (less than 1), the variable value is optional
            set { MinCardinality = (value ? 0 : 1); } // if value is optional, set the min cardinality to zero
        }

        /// <summary>
        /// The actual data type of the value is defined by the DataAttribute.DataType.
        /// So developer should take care and convert this value to proper type before assigning it to variable values
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual string DefaultValue { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public virtual string MissingValue { get; set; } 

    }
}
