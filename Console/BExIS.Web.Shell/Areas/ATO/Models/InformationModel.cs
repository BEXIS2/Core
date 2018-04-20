using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Ato.UI.Models
{
    public class InformationModel
    {
        public List<CategoryModel> CategoryModels { get; set; }

        public InformationModel()
        {
            CategoryModels = new List<CategoryModel>();
        }
    }
}