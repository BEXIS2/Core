using System.Web.Routing;

namespace Vaiona.Web.Mvc.Models
{
    public class ActionModel
    {
        public string ContentKey { get; set; }
        public string Type { get; set; }
        public bool IsEnabled { get; set; }

        public string AreaName { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public RouteValueDictionary Parameters { get; set; }

        public string ViewName { get; set; }
        public object ViewData { get; set; }

        public ActionModel()
        {
            Parameters = new RouteValueDictionary();
        }
    }
}