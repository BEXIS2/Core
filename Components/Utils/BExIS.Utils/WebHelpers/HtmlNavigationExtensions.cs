using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Windows;
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
                            sb.Append(@"/").Append(child.Attribute("area").Value.ToLower());

                        if (!string.IsNullOrWhiteSpace(child.Attribute("controller").Value))
                            sb.Append(@"/").Append(child.Attribute("controller").Value.ToLower());

                        if (!string.IsNullOrWhiteSpace(child.Attribute("action").Value))
                            sb.Append(@"/").Append(child.Attribute("action").Value.ToLower());

                        if (child.Attribute("argument") != null &&  !string.IsNullOrWhiteSpace(child.Attribute("argument").Value))
                            sb.Append(@"/").Append(child.Attribute("argument").Value.ToLower());

                        sb.Append("' target=\"_blank\" >").Append(child.Attribute("title").Value).Append("</a></li>");
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

                    if (!string.IsNullOrWhiteSpace(launchBarItem.Attribute("argument").Value))
                        sb.Append(@"/").Append(launchBarItem.Attribute("argument").Value.ToLower());

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
                                menuItemSb.Append(@"/").Append(child.Attribute("area").Value.ToLower());

                            if (!string.IsNullOrWhiteSpace(child.Attribute("controller").Value))
                                menuItemSb.Append(@"/").Append(child.Attribute("controller").Value.ToLower());

                            if (!string.IsNullOrWhiteSpace(child.Attribute("action").Value))
                                menuItemSb.Append(@"/").Append(child.Attribute("action").Value.ToLower());

                            if (child.Attribute("argument") != null && !string.IsNullOrWhiteSpace(child.Attribute("argument").Value))
                                sb.Append(@"/").Append(child.Attribute("argument").Value.ToLower());

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
                            sb.Append(@"/").Append(menuBarItem.Attribute("area").Value.ToLower());

                        if (!string.IsNullOrWhiteSpace(menuBarItem.Attribute("controller").Value))
                            sb.Append(@"/").Append(menuBarItem.Attribute("controller").Value.ToLower());

                        if (!string.IsNullOrWhiteSpace(menuBarItem.Attribute("action").Value))
                            sb.Append(@"/").Append(menuBarItem.Attribute("action").Value.ToLower());

                        if (menuBarItem.Attribute("argument") != null && !string.IsNullOrWhiteSpace(menuBarItem.Attribute("argument").Value))
                            sb.Append(@"/").Append(menuBarItem.Attribute("argument").Value.ToLower());

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
                        sb.Append(@"/").Append(child.Attribute("controller").Value.ToLower());

                    if (!string.IsNullOrWhiteSpace(child.Attribute("action").Value))
                        sb.Append(@"/").Append(child.Attribute("action").Value.ToLower());

                    if (child.Attribute("argument") != null && !string.IsNullOrWhiteSpace(child.Attribute("argument").Value))
                        sb.Append(@"/").Append(child.Attribute("argument").Value.ToLower());

                    sb.Append("'>").Append(child.Attribute("title").Value).Append("</a></li>");
                }
            }

            if (childAccess)
                return new MvcHtmlString($"<li class='dropdown'><a href='#' class='dropdown-toggle' data-toggle='dropdown' role='button' aria-haspopup='true' aria-expanded='false'><i class='fa fa-cog'></i></a><ul class='dropdown-menu seetings-menu' style='max-height: 90vh;overflow-y: scroll;'>" + sb.ToString() + $"</ul></li>");
            else
                return new MvcHtmlString("");
        }

        private static bool hasOperationRigths(XElement operation, string userName)
        {
            //get parameters for the function to check
            string name = userName;

            // if operation come from the shell the area is empty
            // operations are resgistered with area as shell
            // set area as shell if its empty
            string area = "";
            if (string.IsNullOrEmpty(operation.Attribute("area").Value)) area = "shell";
            else area = operation.Attribute("area").Value.ToLower();

            string controller = operation.Attribute("controller").Value.ToLower();

            string identifier = name + "_" + area + "_" + controller;

            // check if rights already stored in the session
            if (System.Web.HttpContext.Current.Session["menu_permission"] != null && ((Dictionary<string, bool>)System.Web.HttpContext.Current.Session["menu_permission"]).ContainsKey(identifier))
            {
                return (bool)((Dictionary<string, bool>)System.Web.HttpContext.Current.Session["menu_permission"])[identifier];
            }

            FeaturePermissionManager featurePermissionManager = new FeaturePermissionManager();
            OperationManager operationManager = new OperationManager();

            try
            {
                //currently the action are not check, so we use a wildcard
                string action = "*";//operation.Attribute("action").Value.ToLower();

                //// check if the operation is public
                //var op = operationManager.Operations.Where(x => x.Module.ToUpperInvariant() == area.ToUpperInvariant() && x.Controller.ToUpperInvariant() == controller.ToUpperInvariant() && x.Action.ToUpperInvariant() == action.ToUpperInvariant()).FirstOrDefault();
                //var feature = op?.Feature;
                //if (feature == null) return true;

                ////or user has rights
                //if (string.IsNullOrEmpty(userName)) return false;
                bool permission = featurePermissionManager.HasAccessAsync<User>(name, area, controller, action).Result;

                System.Web.HttpContext.Current.Session[identifier] = permission;

                // check if dictionary for menu permissions exists and create it if not
                if (System.Web.HttpContext.Current.Session["menu_permission"] == null)
                {
                    System.Web.HttpContext.Current.Session["menu_permission"] = new Dictionary<string, bool>();
                }

                ((Dictionary<string, bool>)System.Web.HttpContext.Current.Session["menu_permission"]).Add(identifier, permission); // add menu right for the currently logged in user to session

                return permission;
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