using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Linq;
using BExIS.Security.Services.Subjects;
using System;
using System.Net.Http;

namespace BExIS.App.Bootstrap.Attributes
{
    public class BExISApiAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var featurePermissionManager = new FeaturePermissionManager();
            var operationManager = new OperationManager();
            var userManager = new UserManager();

            try
            {
                if (actionContext.Request.RequestUri.Scheme != Uri.UriSchemeHttps)
                {
                    actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
                    {
                        ReasonPhrase = "HTTPS Required"
                    };
                }
                else
                {
                    if(actionContext.Request.Headers.Authorization == null)
                    {
                        actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
                        {
                            ReasonPhrase = "Authentication Required"
                        };
                    }
                    else
                    {
                        var token = actionContext.Request.Headers.Authorization?.ToString().Substring("Bearer ".Length).Trim();
                        // resolve the token to the corresponding user
                        var users = userManager.Users.Where(u => u.Token == token);

                        if (users == null || users.Count() != 1)
                        {
                            actionContext.Response = new HttpResponseMessage() { StatusCode = System.Net.HttpStatusCode.Forbidden, ReasonPhrase = "Access Denied" };
                        }
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