using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Party
{
    public class PartyCustomGridColumns : BaseEntity
    {
        public PartyCustomGridColumns()
        {
            CustomAttribute = new PartyCustomAttribute();
            TypePair = new PartyTypePair();
        }

        #region Associations

        //public virtual PartyType PartyType { get; set; }
        public virtual PartyCustomAttribute CustomAttribute { get; set; }

        public virtual PartyTypePair TypePair { get; set; }
        public virtual long? UserId { get; set; }
        public virtual bool Enable { get; set; }

        #endregion Associations
    }
}