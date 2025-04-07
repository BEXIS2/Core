using System.Collections.Generic;

namespace BExIS.Modules.Dim.UI.Models.Mapping
{
    public abstract class MappingModel
    {
        public long Id { get; set; }
        public long ParentId { get; set; }

        public LinkElementModel Source { get; set; }
        public LinkElementModel Target { get; set; }
    }

    public class ComplexMappingModel : MappingModel
    {
        public List<SimpleMappingModel> SimpleMappings { get; set; }

        public ComplexMappingModel()
        {
            SimpleMappings = new List<SimpleMappingModel>();
        }
    }

    public class SimpleMappingModel : MappingModel
    {
        public TransformationRuleModel TransformationRule { get; set; }

        public SimpleMappingModel()
        {
            TransformationRule = new TransformationRuleModel();
        }
    }
}