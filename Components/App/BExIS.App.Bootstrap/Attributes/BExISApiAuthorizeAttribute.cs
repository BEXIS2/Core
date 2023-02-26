using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Linq;
using BExIS.Security.Services.Subjects;
using System;
using System.Net.Http;
using BExIS.Security.Entities.Subjects;

namespace BExIS.App.Bootstrap.Attributes
{
    public class BExISApiAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            try
            {
                using (var featurePermissionManager = new FeaturePermissionManager())
                using (var operationManager = new OperationManager())
                using (var userManager = new UserManager())
                using (var identityUserService = new IdentityUserService())
                {
                    User user = null;

                    // User
                    switch(actionContext.Request.Headers.Authorization.Scheme.ToLower())
                    {
                        case "basic":
                            var basic = actionContext.Request.Headers.Authorization?.ToString().Substring("basic ".Length).Trim();
                            if (basic == null)
                            {
                                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
                                actionContext.Response.Content = new StringContent("The basic authentication information is not valid.");
                                return;
                            }

                            var name = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(basic)).Split(':')[0];
                            if(name.Contains('@'))
                            {
                                user = userManager.FindByEmailAsync(name).Result;
                            }
                            else
                            {
                                user = userManager.FindByNameAsync(name).Result;
                            }

                            if(user == null)
                            {
                                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
                                actionContext.Response.Content = new StringContent("There is no user with the given username.");
                                return;
                            }

                            var result = identityUserService.CheckPasswordAsync(user, System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(basic)).Split(':')[1]).Result;
                            if (!result)
                            {
                                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
                                actionContext.Response.Content = new StringContent("The username and/or password are incorrect.");
                                return;
                            }

                            break;
                        case "bearer":
                            var token = actionContext.Request.Headers.Authorization?.ToString().Substring("bearer ".Length).Trim();
                            if (token == null)
                            {
                                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
                                actionContext.Response.Content = new StringContent("The token is not valid.");
                                return;
                            }

                            var users = userManager.Users.Where(u => u.Token == token);
                            if(users.Count() != 1)
                            {
                                actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
                                actionContext.Response.Content = new StringContent("The token is not valid.");
                                return;
                            }

                            user = users.First();

                            break;
                        default:
                            actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
                            return;
                    }

                    // Feature & Operation
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
                        if (!featurePermissionManager.HasAccess(user.Id, feature.Id))
                        {
                            actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden);
                            actionContext.Response.Content = new StringContent("The system denied the access.");
                            return;
                        }
                    }

                    actionContext.ControllerContext.RouteData.Values.Add("user", user);
                    return;
                }
            } 
            catch (Exception ex)
            {

            }
        }
    }
}