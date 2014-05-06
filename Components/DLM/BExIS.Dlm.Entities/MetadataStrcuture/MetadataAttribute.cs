using BExIS.Dlm.Entities.DataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Dlm.Entities.MetadataStructure
{
    public class MetadataAttribute : DataContainer
    {
        #region Attributes
        // all the required attributes are inherited!
        #endregion

        #region Associations

        public virtual ICollection<MetadataAttributeUsage> UsedIn { get; set; }

        #endregion

        #region Mathods

        public MetadataAttribute()
        {
            UsedIn = new List<MetadataAttributeUsage>();
        }

        #endregion

    }
}
