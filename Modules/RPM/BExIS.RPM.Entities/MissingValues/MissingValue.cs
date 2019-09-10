using BExIS.Dlm.Entities.DataStructure;
using Vaiona.Entities.Common;

namespace BExIS.Rpm.Entities.MissingValues
{
    public class missingValue : BaseEntity, IBusinessVersionedEntity
    {
        #region Attributes

        public virtual string DisplayName { get; set; }
        public virtual string Placeholder { get; set; }
        public virtual string Description { get; set; }
        public virtual long VariableId { get; set; }

        #endregion Attributes

        #region Associations

        #endregion Associations
    }
}