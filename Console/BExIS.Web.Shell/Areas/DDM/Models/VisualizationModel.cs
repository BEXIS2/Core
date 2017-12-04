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

        public string[] axisX { get; set; }
        public int[] axisY { get; set; }

        public Dictionary<string, int> values {get; set;} 

    }
}