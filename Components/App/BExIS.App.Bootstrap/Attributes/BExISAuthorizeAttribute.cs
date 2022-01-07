using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using Newtonsoft.Json;
using System.Web.Mvc;
using System.Web.Routing;

namespace BExIS.App.Bootstrap.Attributes
{
    public class BExISAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var featurePermissionManager = new FeaturePermissionManager();
            var operationManager = new OperationManager();
            var userManager = new UserManager();

            try
            {
                //
                // get values from request
                var areaName = "Shell";
                try
                {
                    areaName = filterContext.RouteData.DataTokens["area"].ToString();
                }
                catch
                {
                    // ignored
                }
                var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                var actionName = filterContext.ActionDescriptor.ActionName;

                var userName = string.Empty;
                if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                    userName = filterContext.HttpContext.User.Identity.Name;

                //
                // check request

                var operation = operationManager.Find(areaName, controllerName, "*");
                if (operation == null)
                {
                    filterContext.Result = new RedirectToRouteResult(new
                    RouteValueDictionary{
                           { "action", "AccessDenied" },
                           { "controller", "Error" },
                           { "Area", string.Empty }
                       });
                    return;
                }

                var feature = operation.Feature;

                if (feature == null) return;

                var result = userManager.FindByNameAsync(userName);

                if (featurePermissionManager.HasAccess(result.Result?.Id, feature.Id)) return;

                if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
                {
                    HandleUnauthorizedRequest(filterContext);
                }
                else
                {
                    var returnType = ((ReflectedActionDescriptor)filterContext.ActionDescriptor).MethodInfo.ReturnType;
                    if (returnType == typeof(JsonResult))  // if the action work with json result a json object should be returned
                    {

                        ContentResult content = new ContentResult();
                        content.ContentType = "application/json";
                        content.Content = JsonConvert.SerializeObject(false);
                        filterContext.Result = content;

                    }
                    else // redirect to access denied page
                    {
                        filterContext.Result = new RedirectToRouteResult(new
                        RouteValueDictionary{
                           { "action", "AccessDenied" },
                           { "controller", "Error" },
                           { "Area", string.Empty }
                       });
                    }
                }
            }
            finally
            {
                featurePermissionManager.Dispose();
                operationManager.Dispose();
                userManager.Dispose();
            }
        }
    }
}