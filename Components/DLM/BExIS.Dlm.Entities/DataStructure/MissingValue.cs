using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.DataStructure
{
    public class MissingValue : BaseEntity, IBusinessVersionedEntity
    {
        #region Attributes

        public virtual string DisplayName { get; set; }
        public virtual string Placeholder { get; set; }
        public virtual string Description { get; set; }

        #endregion Attributes
    }
}