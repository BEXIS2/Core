using BExIS.Dlm.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.MetadataStructure          
{
    public class MetadataStructure: BusinessEntity
    {
        #region Attributes

        public virtual string Name          { get; set; }
        public virtual string Description   { get; set; }

        public virtual string XsdFileName   { get; set; }
        public virtual string XslFileName   { get; set; }

        #endregion

        #region Associations

        public virtual MetadataStructure Parent { get; set; }
        public virtual ICollection<MetadataStructure> Children { get; set; }
        public virtual ICollection<MetadataPackageUsage> MetadataPackageUsages { get; set; } // needs to preserve the order
        public virtual ICollection<Dataset> Datasets { get; set; }

        #endregion

        #region Mathods

        public MetadataStructure()
        {
            Children = new List<MetadataStructure>();
            MetadataPackageUsages = new List<MetadataPackageUsage>();
            Datasets = new List<Dataset>();
        }
        #endregion

    }
}
