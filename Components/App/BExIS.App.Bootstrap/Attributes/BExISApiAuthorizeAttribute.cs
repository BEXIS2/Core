using BExIS.Security.Entities.Requests;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Config;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Web;
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
                                if (state.Name.Equals("jwt"))
                                {
                                    jwt = state.Value;
                                }
                            }
                        }

                  
                        //actionContext.Request.

                        //HttpCookie cookie = actionContext.Request.Cookies["YourCookieName"];

                        //if (cookie != null)
                        //{
                        //    // Extend the expiration by 30 minutes from the current time
                        //    cookie.Expires = DateTime.Now.AddMinutes(30);
                        //    context.Response.Cookies.Add(cookie);
                        //}

                        if (!string.IsNullOrEmpty(jwt))
                        {
                            principal = JwtHelper.GetClaimsPrincipleByToken(jwt);
                        }
                    }

                    // 1.1. check basic auth in case of principal is empty!
                    if (principal == null || principal.Identity == null || !principal.Identity.IsAuthenticated)
                    {
                        if (actionContext.Request.Headers.Authorization != null && actionContext.Request.Headers.Authorization.Scheme.ToLower() == "basic")
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

                    // update jwt cookie
                    if (user != null)
                    {
                        var jwtConfiguration = GeneralSettings.JwtConfiguration;
                        var jwt = JwtHelper.GetTokenByUser(user);
              
                        // Create a new cookie
                        CookieHeaderValue cookie = new CookieHeaderValue("jwt", jwt);

                        // Set additional properties if needed
                        cookie.Expires = jwtConfiguration.ValidLifetime > 0 ? DateTime.Now.AddHours(jwtConfiguration.ValidLifetime) : DateTime.MaxValue;
                        cookie.Domain = actionContext.Request.RequestUri.Host; // Set the domain
                        cookie.Path = "/"; // Set the path

                        // Add the cookie to the response
                        actionContext.Response?.Headers?.AddCookies(new[] { cookie });
                    }

                    return;
                }
            }
            catch (Exception ex)
            {
            }
        }

    }
}