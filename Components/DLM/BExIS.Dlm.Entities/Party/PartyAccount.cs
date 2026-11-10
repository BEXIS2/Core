using BEXIS2.DB.Entities.Common;

namespace BEXIS2.Dlm.Entities.Party
{
    public class PartyUser : BaseEntity
    {
        public virtual long PartyId { get; set; }

        public virtual long UserId { get; set; }

        public virtual Party Party { get; set; }
    }
}