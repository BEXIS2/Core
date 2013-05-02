using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace BExIS.Core.UI
{
    public static class ControllerExtensions
    {
        public static PartialViewResult PartialViewThemed(this Controller controller, string viewName)
        {
            // get the proper view based on the current theme
            return (null); // retun controller.PartialView using reflection
        }
    }
}
