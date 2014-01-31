
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.MetadataStructure
{
    public class MetadataAttributeUsage: BaseEntity
    {
        #region Attributes

        public virtual int MinCardinality { get; set; }
        public virtual int MaxCardinality { get; set; }
        public virtual string Label { get; set; }

        #endregion

        #region Associations

        public virtual MetadataPackage      MetadataPackage     { get; set; }
        public virtual MetadataAttribute    MetadataAttribute   { get; set; }

        #endregion

        #region Mathods

        #endregion

    }
}
