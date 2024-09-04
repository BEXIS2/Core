using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using System.Xml.Linq;

namespace Vaiona.Web.Mvc.Modularity
{
    public class FakeView : IView
    {
        public void Render(ViewContext viewContext, TextWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// IMC: Inter Module Communication
    /// </summary>
    public static class InterModuleComm
    {
        /// <summary>
        /// Runs the requested action and returns the rendered Html result.
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="moduleId"></param>
        /// <param name="controllerName"></param>
        /// <param name="actionName"></param>
        /// <param name="routeValues"></param>
        /// <returns>The html result</returns>
        /// <remarks>The target module must exist, be active and loaded.</remarks>
        public static MvcHtmlString Render(this HtmlHelper helper, string moduleId, string controllerName, string actionName, RouteValueDictionary routeValues = null)
        {
            MvcHtmlString htmlError = new MvcHtmlString("");
            if (string.IsNullOrWhiteSpace(moduleId))
            {
                htmlError = new MvcHtmlString(string.Format("Module identifier is not provided. Operation aborted."));
                return (htmlError);
            }
            if (!ModuleManager.Exists(moduleId))
            {
                htmlError = new MvcHtmlString(string.Format("Module '{0}' does not exist.", moduleId));
                return (htmlError);
            }
            if (!(ModuleManager.IsActive(moduleId) && ModuleManager.IsLoaded(moduleId)))
            {
                htmlError = new MvcHtmlString(string.Format("Module '{0}' is not active. Operation aborted.", moduleId));
                return (htmlError);
            }
            if (!isModuleCallValid(moduleId) || !isActionCallValid(moduleId, controllerName, actionName))
            {
                htmlError = new MvcHtmlString(string.Format("The requested action is not accessible."));
                return (htmlError);
            }
            try
            {
                if (routeValues == null)
                    routeValues = new RouteValueDictionary();
                routeValues.Add("area", moduleId);
                return (helper.Action(actionName, controllerName, routeValues));
            }
            catch (Exception ex)
            {
                htmlError = new MvcHtmlString(string.Format("Could not render the requested action. Operation aborted. Detailed error message: {0}", ex.Message));
                return (htmlError);
            }
        }

        public static HtmlHelper GetHtmlHelper(this Controller controller)
        {
            var viewContext = new ViewContext(controller.ControllerContext, new FakeView(), controller.ViewData, controller.TempData, TextWriter.Null);
#pragma warning disable CA2000 // Dispose objects before losing scope
            return new HtmlHelper(viewContext, new ViewPage());
#pragma warning restore CA2000 // Dispose objects before losing scope
        }

        public static MvcHtmlString Render(this Controller helper, string moduleId, string controllerName, string actionName, RouteValueDictionary routeValues = null)
        {
            MvcHtmlString htmlError = new MvcHtmlString("");
            if (!isModuleCallValid(moduleId) || !isActionCallValid(moduleId, controllerName, actionName))
            {
                htmlError = new MvcHtmlString(string.Format("The requested action is not accessible."));
                return (htmlError);
            }
            try
            {
                if (routeValues == null)
                    routeValues = new RouteValueDictionary();

                if (routeValues.ContainsKey("area"))
                    routeValues["area"] = moduleId;
                else
                    routeValues.Add("area", moduleId);
                return (helper.GetHtmlHelper().Action(actionName, controllerName, routeValues));
            }
            catch (Exception ex)
            {
                htmlError = new MvcHtmlString(string.Format("Could not render the requested action. Operation aborted. Detailed error message: {0}", ex.Message));
                return (htmlError);
            }
        }

        public static ActionResult Run(this Controller helper, string moduleId, string controllerName, string actionName, RouteValueDictionary routeValues = null)
        {
            if (!isModuleCallValid(moduleId) || !isActionCallValid(moduleId, controllerName, actionName, false))
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotAcceptable, "Not Acceptable.");
                //throw new InvalidOperationException("Bad Request");
            }
            controllerName = controllerName + "Controller";

            Type controllerType = ModuleManager.GetModuleInfo(moduleId).Plugin.GetType().Assembly
                .GetTypes().Where(p => p.Name.Equals(controllerName)).First();

            if (!ControllerBuilder.Current.DefaultNamespaces.Contains(controllerType.Namespace))
                ControllerBuilder.Current.DefaultNamespaces.Add(controllerType.Namespace);
            try
            {
                Controller ctrl = DependencyResolver.Current.GetService(controllerType) as Controller;
                var ctrlContext = new ControllerContext(helper.Request.RequestContext, ctrl);
                ctrl.ControllerContext = ctrlContext;

                var ctrlDesc = new ReflectedControllerDescriptor(ctrl.GetType());
                // get the action descriptor
                var actionDesc = ctrlDesc.FindAction(ctrlContext, actionName);

                try
                {
                    // execute
                    var result = actionDesc.Execute(ctrlContext, routeValues) as ActionResult;
                    return result;
                }
                catch (Exception ex)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, String.Format("Action failed to execute. Reason: {0}", ex.Message));
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.ServiceUnavailable, String.Format("Service Unavailable. Reason: {0}", ex.Message));
            }
        }

        public static ActionResult Call(this Controller helper, string moduleId, string controllerName, string actionName, RouteValueDictionary routeValues = null)
        {
            if (!isModuleCallValid(moduleId) || !isActionCallValid(moduleId, controllerName, actionName, true))
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotAcceptable, "Not Acceptable.");
                //throw new InvalidOperationException("Bad Request");
            }
            controllerName = controllerName + "Controller";

            Type controllerType = ModuleManager.GetModuleInfo(moduleId).Plugin.GetType().Assembly
                .GetTypes().Where(p => p.Name.Equals(controllerName)).First();

            if (!ControllerBuilder.Current.DefaultNamespaces.Contains(controllerType.Namespace))
                ControllerBuilder.Current.DefaultNamespaces.Add(controllerType.Namespace);
            try
            {
                // prepare the call
                try
                {
                    // perform the call and return the result
                    return null;
                }
                catch (Exception ex)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, String.Format("Action failed to execute. Reason: {0}", ex.Message));
                }
            }
            catch (Exception ex)
            {
                return new HttpStatusCodeResult(HttpStatusCode.ServiceUnavailable, String.Format("Service Unavailable. Reason: {0}", ex.Message));
            }
        }

        //public static MvcHtmlString RenderDeleted(this Controller helper, string moduleId, string controllerName, string actionName, RouteValueDictionary routeValues = null)
        //{
        //    MvcHtmlString htmlError = new MvcHtmlString("");
        //    if (string.IsNullOrWhiteSpace(moduleId))
        //    {
        //        htmlError = new MvcHtmlString(string.Format("No module identifier is not provided. Operation aborted."));
        //        return (htmlError);
        //    }
        //    if (!ModuleManager.Exists(moduleId))
        //    {
        //        htmlError = new MvcHtmlString(string.Format("Module '{0}' does not exist.", moduleId));
        //        return (htmlError);
        //    }
        //    if (!(ModuleManager.IsActive(moduleId) && ModuleManager.IsLoaded(moduleId)))
        //    {
        //        htmlError = new MvcHtmlString(string.Format("Module '{0}' is not active. Operation aborted.", moduleId));
        //        return (htmlError);
        //    }
        //    try
        //    {
        //        if (routeValues == null)
        //            routeValues = new RouteValueDictionary();
        //        routeValues.Add("area", moduleId);
        //        Type controllerType = ModuleManager.GetModuleInfo(moduleId).Plugin.GetType().Assembly.GetType(controllerName + "Controller");
        //        Controller controller = (Controller)Activator.CreateInstance(controllerType);
        //        controller.ControllerContext = new ControllerContext(helper.Request.RequestContext, controller);
        //        // how to call the action method? parameters
        //        //return (helper.ActionInvoker.InvokeAction(context, actionName));
        //        return htmlError;
        //    }
        //    catch (Exception ex)
        //    {
        //        htmlError = new MvcHtmlString(string.Format("Could not render the requested action. Operation aborted. Detailed error message: {0}", ex.Message));
        //        return (htmlError);
        //    }
        //}

        public static bool IsAccessible(this HtmlHelper helper, string moduleId, string controllerName, string actionName)
        {
            return IsAccessible(moduleId, controllerName, actionName);
        }

        public static bool IsAccessible(this Controller helper, string moduleId, string controllerName, string actionName)
        {
            return IsAccessible(moduleId, controllerName, actionName);
        }

        public static bool IsAccessible(string moduleId, string controllerName, string actionName)
        {
            return ((isModuleCallValid(moduleId) && (isActionCallValid(moduleId, controllerName, actionName))));
        }

        //public static ActionResult CallForXml(this HtmlHelper helper, string moduleId, string controllerName, string actionName, RouteValueDictionary routeValues)
        //{
        //    if (!isModuleCallValid(moduleId))
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");
        //    }

        //    if(!isActionCallValid(moduleId, controllerName, actionName, true))
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Bad Request");
        //    }
        //        // check if the action is part of the module's API

        //    routeValues.Add("area", moduleId);

        //}

        //public static JsonResult CallForJson(this HtmlHelper helper, string moduleId, string controllerName, string actionName, RouteValueDictionary routeValues)
        //{
        //    if (!string.IsNullOrWhiteSpace(moduleId) && ModuleManager.Exists(moduleId) && ModuleManager.IsActive(moduleId) && ModuleManager.IsLoaded(moduleId))
        //    {
        //        // check if the action is part of the module's API
        //        routeValues.Add("area", moduleId);
        //        return null;
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        private static bool isModuleCallValid(string moduleId)
        {
            if (!string.IsNullOrWhiteSpace(moduleId)
                && ModuleManager.Exists(moduleId)
                && ModuleManager.IsActive(moduleId)
                && ModuleManager.IsLoaded(moduleId)
                )
                return true;
            return false;
        }

        private static bool isActionCallValid(string moduleId, string controllerName, string actionName, bool api = false)
        {
            try
            {
                List<XElement> exports;
                if (api)
                {
                    exports = ModuleManager.GetModuleInfo(moduleId).Manifest
                      .ManifestDoc.Element("Exports").Elements("Export")
                      .Where(p => p.Attribute("tag").Value.Equals("api", StringComparison.InvariantCultureIgnoreCase)).ToList();
                }
                else
                {
                    exports = ModuleManager.GetModuleInfo(moduleId).Manifest
                      .ManifestDoc.Element("Exports").Elements("Export")
                      .Where(p => !p.Attribute("tag").Value.Equals("api", StringComparison.InvariantCultureIgnoreCase)).ToList();
                }

                if (
                    exports.Count(p =>
                            p.Attribute("controller").Value.Equals(controllerName, StringComparison.InvariantCultureIgnoreCase)
                            && p.Attribute("action").Value.Equals(actionName, StringComparison.InvariantCultureIgnoreCase)
                            ) == 1
                        )
                    return true;
                return false;
            }
            catch { return false; }
        }
    }
}