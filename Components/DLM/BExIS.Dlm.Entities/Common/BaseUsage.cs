using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Common
{
    public class BaseUsage: BaseEntity
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

    }
}
