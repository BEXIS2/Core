using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;

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
                    var areaName = "Api";
                    var controllerName = actionContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                    var actionName = actionContext.ActionDescriptor.ActionName;
                    var operation = operationManager.Find(areaName, controllerName, "*");

                    if (operation == null)
                    {
                        actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                        return;
                    }

                    User user = null;

                    // 1. principal
                    var principal = actionContext.ControllerContext.RequestContext.Principal;

                    // get jwt from cookie
                    if ((principal == null || !principal.Identity.IsAuthenticated) && actionContext.Request?.Headers?.GetCookies("jwt") != null)
                    {
                        string jwt = "";
                        foreach (var cookie in actionContext.Request.Headers.GetCookies())
                        {
                            foreach (CookieState state in cookie.Cookies)
                            {
                                if (state.Name.Equals("jwt")) jwt = state.Value;
                            }
                        }

                        if (!string.IsNullOrEmpty(jwt))
                        {
                            principal = JwtHelper.Get(jwt);
                        }
                    }

                    // 1.1. check basic auth in case of principal is empty!
                    if (principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated)
                    {
                        if (actionContext.Request.Headers.Authorization.Scheme.ToLower() == "basic")
                        {
                            string basicParameter = actionContext.Request.Headers.Authorization.Parameter;

                            var name = Encoding.UTF8.GetString(Convert.FromBase64String(basicParameter)).Split(':')[0];
                            if (name.Contains('@'))
                            {
                                user = userManager.FindByEmailAsync(name).Result;
                            }
                            else
                            {
                                user = userManager.FindByNameAsync(name).Result;
                            }

                            if (user == null)
                            {
                                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                                actionContext.Response.Content = new StringContent("There is no user with the given username.");
                                return;
                            }

                            var result = identityUserService.CheckPasswordAsync(user, Encoding.UTF8.GetString(Convert.FromBase64String(basicParameter)).Split(':')[1]).Result;

                            if (!result)
                            {
                                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                                actionContext.Response.Content = new StringContent("The username and/or password are incorrect.");
                                return;
                            }
                        }
                    }
                    else
                    {
                        user = userManager.FindByNameAsync(principal.Identity.Name).Result;

                        if (user == null)
                        {
                            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
                            actionContext.Response.Content = new StringContent("The system denied the access.");
                            return;
                        }
                    }

                    var feature = operation.Feature;
                    if (feature != null && !featurePermissionManager.ExistsAsync(null, feature.Id).Result)
                    {
                        if (!featurePermissionManager.HasAccessAsync(user.Id, feature.Id).Result)
                        {
                            actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden);
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