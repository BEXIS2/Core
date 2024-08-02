using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Party
{
    public class PartyCustomAttributeValue : BaseEntity
    {
        public PartyCustomAttributeValue()
        {
            CustomAttribute = new PartyCustomAttribute();
        }

        #region Attributes

        public virtual string Value { get; set; }

        #endregion Attributes

        #region Associations

        public virtual PartyCustomAttribute CustomAttribute { get; set; }
        public virtual Party Party { get; set; }

        #endregion Associations
    }
}