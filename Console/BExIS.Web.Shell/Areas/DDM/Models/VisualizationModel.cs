using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Modules.Sam.UI.Models;

namespace BExIS.Modules.Ddm.UI.Models
{
    public class VisualizationModel
    {
        public string title { get; set; }  // Title of a diagram
        public string type { get; set; }  // Type of a diagram

        public IEnumerable<EntityTreeViewItemModel> entities { get; set; }

        // Only for LNdW
        public string yAxis { get; set; }
        public Dictionary<string, int> values {get; set;} 

    }
}