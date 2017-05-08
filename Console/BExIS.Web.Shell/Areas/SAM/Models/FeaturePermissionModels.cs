using BExIS.Security.Entities.Objects;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Modules.Sam.UI.Models
{
    public class FeatureTreeViewItemModel
    {
        public List<FeatureTreeViewItemModel> Children { get; set; }
        public string Description { get; set; }
        public string FeatureName { get; set; }
        public long Id { get; set; }
        public bool IsPublic { get; set; }

        public static FeatureTreeViewItemModel Convert(Feature feature)
        {
            return new FeatureTreeViewItemModel()
            {
                Id = feature.Id,
                FeatureName = feature.Name,
                Description = feature.Description,

                IsPublic = feature.IsPublic,

                Children = feature.Children.Select(Convert).ToList()
            };
        }
    }
}