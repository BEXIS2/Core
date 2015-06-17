using BExIS.IO.Transform.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Web.Shell.Areas.RPM.Models
{
    public class ShowHelpModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        
        public ShowHelpModel()
        {
            Title = "";
            Description = "";
        }
    }
}
