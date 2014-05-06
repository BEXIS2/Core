using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.MetadataStructure
{
    public class MetadataPackage: BusinessEntity
    {
        #region Attributes

        public virtual string   Name        { get; set; }
        public virtual string   Description { get; set; }
        public virtual bool     IsEnabled     { get; set; }
        
        #endregion

        #region Associations

        public virtual ICollection<MetadataPackageUsage>    UsedIn                  { get; set; }
        public virtual ICollection<MetadataAttributeUsage>  MetadataAttributeUsages { get; set; } // needs to preserve the order
        
        #endregion

        #region Mathods

        public MetadataPackage()
        {
            UsedIn = new List<MetadataPackageUsage>();
            MetadataAttributeUsages = new List<MetadataAttributeUsage>();
        }

        #endregion

    }
}
