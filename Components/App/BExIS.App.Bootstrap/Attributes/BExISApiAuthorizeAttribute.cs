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
                    if (actionContext.Request.Headers.Authorization == null)
                    {
                        actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
                        return;
                    }

                    if(actionContext.Request.Headers.Authorization.Scheme == "Bearer")
                    {
                        var token = actionContext.Request.Headers.Authorization?.ToString().Substring("Bearer ".Length).Trim();
                        if (token != null)
                        {
                            // resolve the token to the corresponding user
                            var users = userManager.Users.Where(u => u.Token == token);

                            if (users == null || users.Count() != 1)
                            {
                                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
                                actionContext.Response.Content = new StringContent("Bearer token not exist.");
                                return;
                            }

                            if (!featurePermissionManager.HasAccess(users.Single().Id, feature.Id))
                            {
                                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
                                actionContext.Response.Content = new StringContent("Token is not valid.");

                                return;
                            }

                            actionContext.ControllerContext.RouteData.Values.Add("user", users.FirstOrDefault()); ;
                            return;
                        }
                    }

                    if (actionContext.Request.Headers.Authorization.Scheme == "Basic")
                    {
                        var basic = actionContext.Request.Headers.Authorization?.ToString().Substring("Basic ".Length).Trim();
                        if (basic != null)
                        {
                            using (var identityUserService = new IdentityUserService())
                            {
                                var user = userManager.FindByNameAsync(System.Text.Encoding.UTF8.GetString(
                                Convert.FromBase64String(basic)).Split(':')[0]).Result;

                                if (user != null)
                                {
                                    var result = identityUserService.CheckPasswordAsync(user, System.Text.Encoding.UTF8.GetString(
                                Convert.FromBase64String(basic)).Split(':')[1]).Result;

                                    if (!result)
                                    {
                                        actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
                                        actionContext.Response.Content = new StringContent("Username and/or Password are incorrect.");
                                        return;
                                    }

                                    if (!featurePermissionManager.HasAccess(user.Id, feature.Id))
                                    {
                                        actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
                                        actionContext.Response.Content = new StringContent("User is not valid.");

                                        return;
                                    }

                                    actionContext.ControllerContext.RouteData.Values.Add("user", user);
                                    return;
                                }
                            }
                        }
                    }

                    actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
                    return;
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