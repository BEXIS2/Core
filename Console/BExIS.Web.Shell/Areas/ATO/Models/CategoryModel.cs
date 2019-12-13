using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Ato.UI.Models
{
    public class CategoryModel
    {
        public string Name;
        public string Description;
        public List<BasicModel> BasicModels;
        public List<string> Links;

        public CategoryModel()
        {
            Name = string.Empty;
            Description = string.Empty;

            BasicModels = new List<BasicModel>();
        }
    }
}