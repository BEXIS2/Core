namespace BExIS.Web.Shell.Areas.DIM.Models.Mapping
{
    public class MappingModel
    {
        public long Id { get; set; }
        public LinkElementModel Source { get; set; }
        public LinkElementModel Target { get; set; }

        public SimpleMappingModel SimpleMapping { get; set; }
    }


    public class SimpleMappingModel : MappingModel
    {
        public TransformationRuleModel TransformationRule { get; set; }
    }
}