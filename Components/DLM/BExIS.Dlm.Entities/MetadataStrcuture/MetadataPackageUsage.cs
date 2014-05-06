using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.MetadataStructure
{
    public class MetadataPackageUsage: BaseEntity
    {
        #region Attributes

        public virtual int MinCardinality { get; set; } // Min cardinality 0 is interpreted as optional usage, Min can not be negative. not supported in NH
        public virtual int MaxCardinality { get; set; }
        public virtual string Label { get; set; }
        public virtual string Description { get; set; }

        #endregion

        #region Associations

        // its possible for a structure to be connected to a single package more than once, probably in different roles
        public virtual BExIS.Dlm.Entities.MetadataStructure.MetadataStructure   MetadataStructure   { get; set; } // intentionally used the full name. otherwise is not shown in the class diagram.
        public virtual MetadataPackage                                          MetadataPackage     { get; set; }

        #endregion

        #region Mathods

        #endregion

    }
}
