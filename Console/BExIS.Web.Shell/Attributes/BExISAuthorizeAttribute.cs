using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using System.Web.Mvc;
using System.Web.Routing;

namespace BExIS.Web.Shell.Attributes
{
    public class BExISAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
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
            var operationManager = new OperationManager();

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

            var userManager = new UserManager();
            var result = userManager.FindByNameAsync(userName);

            var featurePermissionManager = new FeaturePermissionManager();
            if (featurePermissionManager.HasAccess(result.Result, feature)) return;

            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                HandleUnauthorizedRequest(filterContext);
            }
            else
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
}