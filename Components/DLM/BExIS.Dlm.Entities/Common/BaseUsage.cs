using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Common
{
    public class BaseUsage : BaseEntity
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks> Min cardinality 0 is interpreted as optional usage, Min can not be negative. not supported in NH </remarks>
        /// <seealso cref=""/>
        public virtual int MinCardinality { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual int MaxCardinality { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string Label { get; set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string Description { get; set; }

        /// <summary>
        /// The actual data type of the value is defined by the DataAttribute.DataType.
        /// So developer should take care and convert this value to proper type before assigning it to parameter values
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string DefaultValue { get; set; }

        /// <summary>
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string FixedValue { get; set; }

        /// <summary>
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual string MissingValue { get; set; }

        /// <summary>
        /// Indicates whether the value of the variable/parameter is optional.
        /// </summary>
        /// <remarks>Setting this propery affects the minimum cardinality.</remarks>
        /// <seealso cref=""/>
        public virtual bool IsValueOptional
        {
            get { return MinCardinality < 1; } //if MinCardinality cardinality is zero (less than 1), the parameter value is optional
            set { MinCardinality = value ? 0 : 1; } // if value is optional, set the min cardinality to zero
        }

        /// <summary>
        /// Indicates the order of the usage in its context, e.g., variable order in a data structure.
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        public virtual int OrderNo { get; set; }
    }
}