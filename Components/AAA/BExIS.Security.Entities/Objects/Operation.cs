using BEXIS2.DB.Entities.Common;

namespace BExIS.Security.Entities.Objects
{
    public class Operation : BaseEntity
    {
        public virtual string Action { get; set; }
        public virtual string Controller { get; set; }
        public virtual Feature Feature { get; set; }
        public virtual string Module { get; set; }
    }
}