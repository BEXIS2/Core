using System.Linq;
using System.Text;
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

        public static MvcHtmlString MenuBar(this HtmlHelper htmlHelper)
        {
            StringBuilder sb = new StringBuilder();
            var menuBarRoot = ModuleManager.ExportTree.GetElement("menubarRoot");

            foreach (var menuBarItem in menuBarRoot.Elements())
            {
                if (menuBarItem.HasElements)
                {
                    sb.Append($"<li class='dropdown'><a href='#' class='dropdown-toggle' data-toggle='dropdown' role='button' aria-haspopup='true' aria-expanded='false'>{menuBarItem.Attribute("title").Value}<span class='caret'></span></a><ul class='dropdown-menu'>");

                    foreach (var child in menuBarItem.Elements())
                    {
                        sb.Append($"<li><a href='");
                        if (!string.IsNullOrWhiteSpace(child.Attribute("area").Value))
                            sb.Append(@"/").Append(child.Attribute("area").Value);

                        if (!string.IsNullOrWhiteSpace(child.Attribute("controller").Value))
                            sb.Append(@"/").Append(child.Attribute("controller").Value);

                        if (!string.IsNullOrWhiteSpace(child.Attribute("action").Value))
                            sb.Append(@"/").Append(child.Attribute("action").Value);

                        sb.Append("'>").Append(child.Attribute("title").Value).Append("</a></li>");
                    }

                    sb.Append($"</ul></li>");
                }
                else
                {
                    sb.Append($"<li><a href='");
                    if (!string.IsNullOrWhiteSpace(menuBarItem.Attribute("area").Value))
                        sb.Append(@"/").Append(menuBarItem.Attribute("area").Value);

                    if (!string.IsNullOrWhiteSpace(menuBarItem.Attribute("controller").Value))
                        sb.Append(@"/").Append(menuBarItem.Attribute("controller").Value);

                    if (!string.IsNullOrWhiteSpace(menuBarItem.Attribute("action").Value))
                        sb.Append(@"/").Append(menuBarItem.Attribute("action").Value);

                    sb.Append("'>").Append(menuBarItem.Attribute("title").Value).Append("</a></li>");
                }
            }

            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString LaunchBar(this HtmlHelper htmlHelper)
        {
            return new MvcHtmlString("");
        }

        public static MvcHtmlString Settings(this HtmlHelper htmlHelper)
        {
            StringBuilder sb = new StringBuilder();
            var settingsRoot = ModuleManager.ExportTree.GetElement("settingsRoot");

            foreach (var child in settingsRoot.Elements())
            {
                sb.Append($"<li><a href='");
                if (!string.IsNullOrWhiteSpace(child.Attribute("area").Value))
                    sb.Append(@"/").Append(child.Attribute("area").Value);

                if (!string.IsNullOrWhiteSpace(child.Attribute("controller").Value))
                    sb.Append(@"/").Append(child.Attribute("controller").Value);

                if (!string.IsNullOrWhiteSpace(child.Attribute("action").Value))
                    sb.Append(@"/").Append(child.Attribute("action").Value);

                sb.Append("'>").Append(child.Attribute("title").Value).Append("</a></li>");

            }

            return new MvcHtmlString($"<li class='dropdown'><a href='#' class='dropdown-toggle' data-toggle='dropdown' role='button' aria-haspopup='true' aria-expanded='false'><i class='fa fa-cog'></i></a><ul class='dropdown-menu'>" + sb.ToString() + $"</ul></li>");
        }

        public static XElement GetElement(this XElement menuTree, string id)
        {
            return menuTree.Elements("Export").FirstOrDefault(i => i.Attribute("id").Value == id);
        }
    }
}
