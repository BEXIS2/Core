using System.Web.Mvc;
using System.Xml.Linq;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Utils.WebHelpers
{

    public static class HtmlNavigationExtensions
    {
        public static XElement ExportTree(this HtmlHelper htmlHelper)
        {
            return ModuleManager.ExportTree;
        }
    }
}
