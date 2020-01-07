using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using System;
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

        public static XElement GetElement(this XElement menuTree, string id)
        {
            return menuTree.Elements("Export").FirstOrDefault(i => i.Attribute("id").Value == id);
        }

        public static MvcHtmlString LaunchBar(this HtmlHelper htmlHelper)
        {
            StringBuilder sb = new StringBuilder();
            var lunchBarRoot = ModuleManager.ExportTree.GetElement("lunchbarRoot");

            foreach (var launchBarItem in lunchBarRoot.Elements())
            {
                if (launchBarItem.HasElements)
                {
                    sb.Append($"<li class='dropdown'><a href='#' class='dropdown-toggle' data-toggle='dropdown' role='button' aria-haspopup='true' aria-expanded='false'>{launchBarItem.Attribute("title").Value}<span class='caret'></span></a><ul class='dropdown-menu'>");

                    foreach (var child in launchBarItem.Elements())
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
                    if (!string.IsNullOrWhiteSpace(launchBarItem.Attribute("area").Value))
                        sb.Append(@"/").Append(launchBarItem.Attribute("area").Value);

                    if (!string.IsNullOrWhiteSpace(launchBarItem.Attribute("controller").Value))
                        sb.Append(@"/").Append(launchBarItem.Attribute("controller").Value);

                    if (!string.IsNullOrWhiteSpace(launchBarItem.Attribute("action").Value))
                        sb.Append(@"/").Append(launchBarItem.Attribute("action").Value);

                    sb.Append("'>").Append(launchBarItem.Attribute("title").Value).Append("</a></li>");
                }
            }

            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString MenuBar(this HtmlHelper htmlHelper, string userName)
        {
            StringBuilder sb = new StringBuilder();
            var menuBarRoot = ModuleManager.ExportTree.GetElement("menubarRoot");

            foreach (var menuBarItem in menuBarRoot.Elements())
            {
                if (menuBarItem.HasElements)
                {
                    StringBuilder menuItemSb = new StringBuilder();

                    menuItemSb.Append($"<li class='dropdown'><a href='#' class='dropdown-toggle' data-toggle='dropdown' role='button' aria-haspopup='true' aria-expanded='false'>{menuBarItem.Attribute("title").Value}<span class='caret'></span></a><ul class='dropdown-menu'>");

                    bool childAccess = false;

                    foreach (var child in menuBarItem.Elements())
                    {
                        if (hasOperationRigths(child, userName))
                        {
                            childAccess = true;

                            menuItemSb.Append($"<li><a href='");
                            if (!string.IsNullOrWhiteSpace(child.Attribute("area").Value))
                                menuItemSb.Append(@"/").Append(child.Attribute("area").Value);

                            if (!string.IsNullOrWhiteSpace(child.Attribute("controller").Value))
                                menuItemSb.Append(@"/").Append(child.Attribute("controller").Value);

                            if (!string.IsNullOrWhiteSpace(child.Attribute("action").Value))
                                menuItemSb.Append(@"/").Append(child.Attribute("action").Value);

                            menuItemSb.Append("'>").Append(child.Attribute("title").Value).Append("</a></li>");
                        }
                    }

                    menuItemSb.Append($"</ul></li>");

                    if (childAccess) sb.Append(menuItemSb.ToString());
                }
                else
                {
                    if (hasOperationRigths(menuBarItem, userName))
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
            }

            return new MvcHtmlString(sb.ToString());
        }

        [Obsolete("", true)]
        public static MvcHtmlString AccountBar(this HtmlHelper htmlHelper)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"<li class='dropdown'><a href='#' class='dropdown-toggle' data-toggle='dropdown' role='button' aria-haspopup='true' aria-expanded='false'>Account<span class='caret'></span></a><ul class='dropdown-menu'>");

            sb.Append($"<li><a href=''></a>Details</li>");
            sb.Append($"<li><a href=''></a>Profile</li>");
            sb.Append($"<li><a href=''></a>Sign Out</li>");

            sb.Append($"</ul></li>");

            return new MvcHtmlString(sb.ToString());
        }

        public static MvcHtmlString Settings(this HtmlHelper htmlHelper, string userName)
        {
            StringBuilder sb = new StringBuilder();
            var settingsRoot = ModuleManager.ExportTree.GetElement("settingsRoot");

            var children = settingsRoot.Elements()
                .OrderBy(x => x.Attribute("area").Value).ThenBy(x => x.Attribute("order").Value);

            var currentArea = "";

            bool childAccess = false;

            foreach (var child in children)
            {
                if (hasOperationRigths(child, userName))
                {
                    childAccess = true;

                    var area = child.Attribute("area").Value;

                    if (currentArea != "" && area != currentArea)
                    {
                        sb.Append($"<li role=\"separator\" class=\"divider\"></li>");
                    }

                    currentArea = area;

                    sb.Append($"<li><a href='");
                    if (!string.IsNullOrWhiteSpace(area))
                        sb.Append(@"/").Append(area);

                    if (!string.IsNullOrWhiteSpace(child.Attribute("controller").Value))
                        sb.Append(@"/").Append(child.Attribute("controller").Value);

                    if (!string.IsNullOrWhiteSpace(child.Attribute("action").Value))
                        sb.Append(@"/").Append(child.Attribute("action").Value);

                    sb.Append("'>").Append(child.Attribute("title").Value).Append("</a></li>");
                }
            }

            if (childAccess)
                return new MvcHtmlString($"<li class='dropdown'><a href='#' class='dropdown-toggle' data-toggle='dropdown' role='button' aria-haspopup='true' aria-expanded='false'><i class='fa fa-cog'></i></a><ul class='dropdown-menu seetings-menu'>" + sb.ToString() + $"</ul></li>");
            else
                return new MvcHtmlString("");
        }

        private static bool hasOperationRigths(XElement operation, string userName)
        {
            FeaturePermissionManager featurePermissionManager = new FeaturePermissionManager();
            OperationManager operationManager = new OperationManager();

            try
            {
                //get parameters for the function to check
                string name = userName;
                string area = operation.Attribute("area").Value.ToLower();
                string controller = operation.Attribute("controller").Value.ToLower();
                //currently the action are not check, so we use a wildcard
                string action = "*";//operation.Attribute("action").Value.ToLower();

                //// check if the operation is public
                //var op = operationManager.Operations.Where(x => x.Module.ToUpperInvariant() == area.ToUpperInvariant() && x.Controller.ToUpperInvariant() == controller.ToUpperInvariant() && x.Action.ToUpperInvariant() == action.ToUpperInvariant()).FirstOrDefault();
                //var feature = op?.Feature;
                //if (feature == null) return true;

                ////or user has rights
                //if (string.IsNullOrEmpty(userName)) return false;
                return featurePermissionManager.HasAccess<User>(name, area, controller, action);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                featurePermissionManager.Dispose();
                operationManager.Dispose();
            }
        }
    }
}
