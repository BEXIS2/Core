using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Linq;
using BExIS.Security.Services.Subjects;
using System;
using System.Net.Http;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Security.Entities.Subjects;

namespace BExIS.App.Bootstrap.Attributes
{
    public class BExISApiAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            using (var featurePermissionManager = new FeaturePermissionManager())
            using (var operationManager = new OperationManager())
            using (var userManager = new UserManager())
            {
                
                // Check for HTTPS
                //if (actionContext.Request.RequestUri.Scheme != Uri.UriSchemeHttps)
                //{
                //    actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
                //    return;
                //}

                var areaName = "Api";
                var controllerName = actionContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                var actionName = actionContext.ActionDescriptor.ActionName;
                var operation = operationManager.Find(areaName, controllerName, "*");
                if (operation == null)
                {
                    actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
                    return;
                }

                var feature = operation.Feature;
                if (feature != null && !featurePermissionManager.Exists(null, feature.Id))
                {
                    User user = null;
                    string auth = actionContext.Request.Headers.Authorization?.ToString();
                    actionContext.Response = BExISAuthorizeHelper.HttpRequestAuthorization(auth, feature.Id, out user);
                    actionContext.ControllerContext.RouteData.Values.Add("user", user);

                    return;
                }
            }
        }
    }
}