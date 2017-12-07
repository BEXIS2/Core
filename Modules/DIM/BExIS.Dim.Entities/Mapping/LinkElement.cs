using Vaiona.Entities.Common;

namespace BExIS.Dim.Entities.Mapping
{
    public class LinkElement : BaseEntity, IBusinessVersionedEntity
    {
        #region Attributes

        public virtual long ElementId { get; set; }

        /// <summary>
        /// Name of the Publication
        /// </summary>
        public virtual string Name { get; set; }
        public virtual string XPath { get; set; }
        public virtual bool IsSequence { get; set; }
        public virtual LinkElementType Type { get; set; }
        public virtual LinkElementComplexity Complexity { get; set; }
        //public virtual string Mask { get; set; }


        #endregion

        #region Associations    

        //public virtual LinkElement Parent { get; set; }

        //public virtual List<LinkElement> Children { get; set; }

        #endregion
    }
}
