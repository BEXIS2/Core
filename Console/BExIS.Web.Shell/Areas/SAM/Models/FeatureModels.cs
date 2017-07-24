using BExIS.Security.Entities.Objects;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using BExIS.Modules.Sam.UI.Controllers;

namespace BExIS.Modules.Sam.UI.Models
{
    public class FeatureModel
    {
        public List<FeatureModel> Children { get; set; }

        [Display(Name = "Description")]
        [Required]
        [StringLength(250, ErrorMessage = "The description must be {2} - {1} characters long.", MinimumLength = 16)]
        public string Description { get; set; }

        [Display(Name = "Feature Name")]
        [RegularExpression("^([A-Za-z]+)$", ErrorMessage = "The role name must consist only of letters.")]
        //[Remote("ValidateFeatureName", "Features", AdditionalFields = "Id")]
        [Required]
        [StringLength(50, ErrorMessage = "The feature name must be {2} - {1} characters long.", MinimumLength = 3)]
        public string FeatureName { get; set; }

        [Display(Name = "Feature ID")]
        [Editable(false)]
        [Required]
        public long Id { get; set; }

        public static FeatureModel Convert(Feature feature)
        {
            return new FeatureModel()
            {
                Id = feature.Id,
                FeatureName = feature.Name,
                Description = feature.Description,
                Children = feature.Children.Select(c => FeatureModel.Convert(c)).ToList<FeatureModel>()
            };
        }
    }

    public class FeatureTreeViewModel
    {
        public List<FeatureTreeViewModel> Children { get; set; }
        public string Description { get; set; }
        public string FeatureName { get; set; }
        public long Id { get; set; }
        public bool IsFeatureInEveryoneGroup { get; set; }

        public static FeatureTreeViewModel Convert(Feature feature, IsFeatureInEveryoneGroupDelegate isFeatureInEveryoneGroupDelegate)
        {
            return new FeatureTreeViewModel()
            {
                Id = feature.Id,
                FeatureName = feature.Name,
                Description = feature.Description,

                IsFeatureInEveryoneGroup = isFeatureInEveryoneGroupDelegate(feature.Id),

                Children = feature.Children.Select(c => FeatureTreeViewModel.Convert(c, isFeatureInEveryoneGroupDelegate)).ToList<FeatureTreeViewModel>()
            };
        }
    }
}