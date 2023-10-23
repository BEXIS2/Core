using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.Meanings;
using System.Collections.Generic;

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
        public virtual Variable Variable { get; set; }
    }

    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>
    public abstract class Variable : BaseUsage
    {

        /// <summary>
        /// 
        /// </summary>
        public virtual DataType DataType { get; set; }

        /// <summary>
        /// This is the unit directly associated with the variable. At the variable creation time, it is possible to attach the
        /// variable to the unit of its associated data container or to another unit that its dimension is equal to the dimension of the
        /// unit of the data container.
        /// </summary>
        /// <remarks>If the data attribute's unit changes, the validity of the variable's unit should be evaluated again.</remarks>
        /// <remarks>If the variable's unit is going to be changed, the compatibility to the data container's unit's dimension should be preserved.</remarks>
        public virtual Unit Unit { get; set; } // 0..1


        public virtual ICollection<Constraint> VariableConstraints { get; set; }

        public virtual ICollection<MissingValue> MissingValues { get; set; } // 0..1
        public virtual ICollection<Meaning> Meanings { get; set; } // 0..1

        public virtual int DisplayPatternId { get; set; }
    }

    public class VariableInstance : Variable
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual StructuredDataStructure DataStructure { get; set; }

        public virtual VariableTemplate VariableTemplate { get; set; }        

        public virtual int VarTemplate { get; set; }

        public virtual bool IsKey { get; set; }

        public VariableInstance()
        {
            MinCardinality = 0; // to make the parameter optional by default
            MaxCardinality = 1; // this must always remain 1
            MissingValues = new List<MissingValue>();
            DisplayPatternId = -1;
        }

    }

    public class VariableTemplate : Variable
    {
        public virtual bool Approved { get; set; }


        public VariableTemplate()
        {
            MinCardinality = 0; // to make the parameter optional by default
            MaxCardinality = 1; // this must always remain 1

        }
    }
}