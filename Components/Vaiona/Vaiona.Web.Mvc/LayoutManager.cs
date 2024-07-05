using Microsoft.Web.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Models;

namespace Vaiona.Web.Mvc
{
    public static class LayoutManager
    {
        public static MvcHtmlString RenderAuto(this HtmlHelper helper, string contentKey, List<object> data = null, bool fallback = true)
        {
            try
            {
                string output = string.Empty;
                List<ActionModel> acModels = getContentProviderInfo(contentKey, data);
                foreach (var acModel in acModels.Where(p => p.IsEnabled == true))
                {
                    if (acModel.Type.Equals("Action", StringComparison.InvariantCultureIgnoreCase))
                    {
                        output += RenderContent(helper, acModel).ToString(); // a separator like <br/> may be needed!
                    }
                    else if (acModel.Type.Equals("View", StringComparison.InvariantCultureIgnoreCase))
                    {
                        output += RenderThemedContent(helper, acModel, fallback).ToString(); // here too
                    }
                }
                return (new MvcHtmlString(output));
            }
            catch
            {
                MvcHtmlString htmlError = new MvcHtmlString("");
                if (AppConfiguration.ThrowErrorWhenParialContentNotFound)
                {
                    htmlError = new MvcHtmlString(string.Format("No content for key '{0}' was found.", contentKey));
                }
                return (htmlError);
            }
        }

        public static string GetLayoutName(this HtmlHelper helper)
        {
            string layoutName = "~/Views/Shared/_Layout.cshtml"; // built-in layout
            var request = helper.ViewContext.HttpContext.Request;
            var session = helper.ViewContext.HttpContext.Session;
            if (AppConfiguration.IsPostBack(request) && !string.IsNullOrWhiteSpace(request["Theme"]))
            {
                Themes.CurrentTheme = request["Theme"];
                session["CurrentTheme"] = Themes.CurrentTheme;
            }
            else if (session["CurrentTheme"] != null)
            {
                Themes.CurrentTheme = session["CurrentTheme"] as string;
            }
            string layoutFileName = string.Concat(AppConfiguration.ActiveLayoutName, ".cshtml");
            layoutFileName = Themes.GetResourcePath("Layouts", layoutFileName); // Auto fallback to DefaultTheme
            if (!string.IsNullOrWhiteSpace(layoutFileName))
            {
                layoutName = layoutFileName;
            }
            return layoutName;
        }

        private static MvcHtmlString RenderThemedContent(this HtmlHelper helper, ActionModel actionModel, bool fallbackToNonThemedVersion = true)
        {
            string path = Themes.GetResourcePath("Partials", string.Concat(actionModel.ViewName, ".cshtml"));
            if (!File.Exists(HttpContext.Current.Server.MapPath(path)) && fallbackToNonThemedVersion)
                return (helper.Partial(actionModel.ViewName, actionModel.ViewData));
            return (helper.Partial(path, actionModel.ViewData));
            //return(((WebPageBase)helper.ViewContext.View).RenderPage(Themes.GetResourcePath("Partials", string.Concat(partialViewName, ".cshtml")), data));
        }

        // If a parameter is in the overridingParameters list, its value will override the one defined in the content map
        private static MvcHtmlString RenderContent(this HtmlHelper helper, ActionModel actionModel)
        {
            // the IsEnabled should be checked by the caller
            return (helper.Action(actionModel.ActionName, actionModel.ControllerName, actionModel.Parameters)); // maybe RenderAction is better
        }

        private static XElement getMappingItem(string contentKey)
        {
            XElement contentMap = getLayoutMap();
            XElement mapItem = (from item in contentMap.Elements("MapItem")
                                where item.Attribute("ContentKey").Value.Equals(contentKey, StringComparison.InvariantCultureIgnoreCase)
                                //&& item.Attribute("Type").Value.Equals(type, StringComparison.InvariantCultureIgnoreCase)
                                select item)
                               .FirstOrDefault(); // maybe there more than one item using a key! i.e. to put more content in one place

            if (mapItem == null)
            {
                throw new Exception(string.Format("Error: No Mapping item matched the provided key: {0}", contentKey));
                //return (new MvcHtmlString("Error"));//   return an error message
            }
            return (mapItem);
        }

        private static List<ActionModel> getContentProviderInfo(string contentKey, List<object> parametersList)
        {
            XElement mapItem = getMappingItem(contentKey);
            List<XElement> contentProviders = mapItem.Element("ContentProviders").Elements("ContentProvider").ToList();
            List<ActionModel> actionModels = new List<ActionModel>();
            for (int i = 0; i < contentProviders.Count(); i++)
            {
                XElement contentProvider = contentProviders[i];
                object parameters = null;
                if (parametersList != null && parametersList[i] != null)
                {
                    try
                    {
                        parameters = parametersList[i];
                    }
                    catch { }
                }
                ActionModel actionModel = new ActionModel()
                {
                    Type = contentProvider.Attribute("Type").Value,
                    ContentKey = contentKey,
                    IsEnabled = true,
                };
                try
                {
                    actionModel.IsEnabled = bool.Parse(contentProvider.Attribute("Enabled").Value);
                }
                catch { }

                actionModels.Add(actionModel);
                if (actionModel.Type.Equals("Action", StringComparison.InvariantCultureIgnoreCase))
                {
                    actionModel.AreaName = contentProvider.Attribute("Area").Value;
                    actionModel.ControllerName = contentProvider.Attribute("Controller").Value;
                    actionModel.ActionName = contentProvider.Attribute("Action").Value;

                    Dictionary<string, object> overridingParameters = null;// new Dictionary<string, object>();
                    if (parameters != null)
                        overridingParameters = (Dictionary<string, object>)(parameters);
                    actionModel.Parameters["area"] = ""; // for default routes
                    if (!string.IsNullOrWhiteSpace(actionModel.AreaName))
                        actionModel.Parameters["area"] = actionModel.AreaName;

                    foreach (var par in contentProvider
                                        .Element("Parameters")
                                        .Elements("Parameter")
                                        .Where(p => !string.IsNullOrWhiteSpace(p.Attribute("Name").Value))
                                        )
                    {
                        string parName = par.Attribute("Name").Value;
                        if (overridingParameters != null && overridingParameters.ContainsKey(parName))
                        {
                            actionModel.Parameters[parName] = overridingParameters[parName];
                        }
                        else
                        {
                            object parVal = par.Attribute("Value").Value;
                            actionModel.Parameters[parName] = parVal;
                        }
                    }
                }
                else if (actionModel.Type.Equals("View", StringComparison.InvariantCultureIgnoreCase))
                {
                    actionModel.ViewName = contentProvider.Attribute("View").Value;
                    if (parameters != null)
                    {
                        actionModel.ViewData = parameters;
                    }
                    else
                    {
                        var par = contentProvider
                                        .Element("Parameters")
                                        .Elements("Parameter")
                                        .Where(p => !string.IsNullOrWhiteSpace(p.Attribute("Name").Value))
                                        .FirstOrDefault(); // Views can have just one object as the view model
                        actionModel.ViewData = par == null ? null : par.Attribute("Value").Value;
                    }
                }
            }

            return (actionModels);
        }

        private static ObjectCache cache = MemoryCache.Default;

        private static XElement getLayoutMap()
        {
            XElement contentMap = null;
            string layoutPath = string.Concat(AppConfiguration.ActiveLayoutName, ".xml"); // make the file observable
            layoutPath = Themes.GetResourcePath("Layouts", layoutPath); //automatic fallback to default theme

            if (cache.Contains(layoutPath))
            {
                contentMap = (XElement)cache[layoutPath];
            }
            else
            {
                string fullpath = HttpContext.Current.Server.MapPath(layoutPath);
                contentMap = XElement.Load(fullpath);
                cache.Add(layoutPath, contentMap, DateTimeOffset.Now.AddHours(24));
            }
            return (contentMap);
        }
    }
}