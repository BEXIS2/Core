using Vaiona.Entities.Common;

namespace BExIS.Dim.Entities.Mapping
{
    public class TransformationRule : BaseEntity, IBusinessVersionedEntity
    {
        public virtual string RegEx { get; set; }
        public virtual string Mask { get; set; }

        public TransformationRule()
        {
            RegEx = "";
            Mask = "";
        }

        public TransformationRule(string regex, string mask)
        {
            RegEx = regex;
            Mask = mask;
        }



        public TransformationRule(long id, string regex, string mask)
        {
            Id = id;
            RegEx = regex;
            Mask = mask;
        }
    }
}
