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
            MissingValues = new List<MissingValue>();
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
        /// This is the unit directly associated with the variable. At the variable creation time, it is possible to attach the
        /// variable to the unit of its associated data container or to another unit that its dimension is equal to the dimension of the
        /// unit of the data container.
        /// </summary>
        /// <remarks>If the data attribute's unit changes, the validity of the variable's unit should be evaluated again.</remarks>
        /// <remarks>If the variable's unit is going to be changed, the compatibility to the data container's unit's dimension should be preserved.</remarks>
        public virtual Unit Unit { get; set; } // 0..1

        public virtual ICollection<MissingValue> MissingValues { get; set; } // 0..1
    }
}