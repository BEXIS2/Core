using Vaiona.Entities.Common;

namespace BExIS.Dim.Entities.Mappings
{
    public class TransformationRule : BaseEntity, IBusinessVersionedEntity
    {
        public virtual string RegEx { get; set; }
        public virtual string Mask { get; set; }
        public virtual string DefaultValue { get; set; }

        public TransformationRule()
        {
            RegEx = "";
            Mask = "";
            DefaultValue = "";
        }

        public TransformationRule(string regex, string mask, string defaultValue="")
        {
            RegEx = regex;
            Mask = mask;
            DefaultValue = defaultValue;
        }



        public TransformationRule(long id, string regex, string mask, string defaultValue = "")
        {
            Id = id;
            RegEx = regex;
            Mask = mask;
            DefaultValue = defaultValue;
        }
    }
}
