using Vaiona.Entities.Common;

namespace BExIS.Dim.Entities.Mapping
{
    public class TransformationRule : BaseEntity, IBusinessVersionedEntity
    {
        public virtual string RegEx { get; set; }

        public TransformationRule()
        {
            RegEx = "";
        }

        public TransformationRule(string regex)
        {
            RegEx = regex;
        }



        public TransformationRule(long id, string regex)
        {
            Id = id;
            RegEx = regex;
        }
    }
}
