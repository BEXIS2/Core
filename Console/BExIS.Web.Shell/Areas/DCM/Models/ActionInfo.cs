using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class ActionInfo
    {
        public string AreaName { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public Dictionary<String,object> Parameters { get; set; }

        public ActionInfo()
        {
            AreaName = "";
            ControllerName = "";
            ActionName = "";
            Parameters = new Dictionary<String, object>();
        }
    }
}