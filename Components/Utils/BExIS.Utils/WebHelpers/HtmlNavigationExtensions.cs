using System.Linq;
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
            var menuBar = "";
            var menuBarRoot = ModuleManager.ExportTree.GetElement("menubarRoot");

            foreach (var menuBarItem in menuBarRoot.Elements())
            {
                if (menuBarItem.HasElements)
                {
                    menuBar += $"<li class='dropdown'><a href='#' class='dropdown-toggle' data-toggle='dropdown' role='button' aria-haspopup='true' aria-expanded='false'>{menuBarItem.Attribute("title").Value}<span class='caret'></span></a><ul class='dropdown-menu'>";

                    foreach (var child in menuBarItem.Elements())
                    {
                        menuBar += $"<li><a href='/{child.Attribute("area").Value}/{child.Attribute("controller").Value}/{child.Attribute("action").Value}'>{child.Attribute("title").Value}</a></li>";
                    }

                    menuBar += $"</ul></li>";
                }
                else
                {
                    menuBar += $"<li><a href='/{menuBarItem.Attribute("area").Value}/{menuBarItem.Attribute("controller").Value}/{menuBarItem.Attribute("action").Value}'>{menuBarItem.Attribute("title").Value}</a></li>";
                }
            }

            return new MvcHtmlString(menuBar);
        }

        public static MvcHtmlString LunchBar(this HtmlHelper htmlHelper)
        {
            return new MvcHtmlString("");
        }

        public static MvcHtmlString Settings(this HtmlHelper htmlHelper)
        {
            var settings = "";
            var settingsRoot = ModuleManager.ExportTree.GetElement("settingsRoot");

            foreach (var child in settingsRoot.Elements())
            {
                settings += $"<li><a href='/{child.Attribute("area").Value}/{child.Attribute("controller").Value}/{child.Attribute("action").Value}'>{child.Attribute("title").Value}</a></li>";
            }

            return new MvcHtmlString($"<li class='dropdown'><a href='#' class='dropdown-toggle' data-toggle='dropdown' role='button' aria-haspopup='true' aria-expanded='false'><i class='fa fa-cog'></i></a><ul class='dropdown-menu'>" + settings + $"</ul></li>");
        }

        public static XElement GetElement(this XElement menuTree, string id)
        {
            return menuTree.Elements("Export").FirstOrDefault(i => i.Attribute("id").Value == id);
        }
    }
}
