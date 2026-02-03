using BExIS.App.Bootstrap.Extensions;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using System;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BExIS.Utils.Config;

namespace BExIS.App.Bootstrap.Attributes
{
    public class BExISAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            try
            {
                using (var userManager = new UserManager())
                using (var identityUserService = new IdentityUserService(userManager))
                {
                    var operationManager = new OperationManager();
                    var featurePermissionManager = new FeaturePermissionManager();
                    var areaName = filterContext.RouteData.DataTokens.Keys.Contains("area") ? filterContext.RouteData.DataTokens["area"].ToString() : "Shell";
                    var controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
                    var actionName = filterContext.ActionDescriptor.ActionName;
                    var operation = operationManager.Get(areaName, controllerName, "*");

                    if (operation == null)
                    {
                        filterContext.SetResponse(HttpStatusCode.Forbidden);
                        //filterContext.Result = new HttpStatusCodeResult(HttpStatusCode.Unauthorized);
                    }
                    else
                    {
                        var feature = operation.Feature;
                        if (feature != null && !featurePermissionManager.HasAccess(null, feature.Id))
                        {
                            User user = BExISAuthorizeHelper.GetUserFromAuthorizationAsync(filterContext.HttpContext).Result;

                            if (user == null)
                            {
                                filterContext.SetResponse(HttpStatusCode.Unauthorized);
                            }
                            else
                            {
                                if (!featurePermissionManager.HasAccess(user.Id, feature.Id))
                                {
                                    filterContext.SetResponse(HttpStatusCode.Forbidden);
                                }

                                // update jwt cookie
                                if (user != null)
                                {
                                    var jwtConfiguration = GeneralSettings.JwtConfiguration;
                                    var jwt = JwtHelper.GetTokenByUser(user);

                                    // Create a new cookie
                                    HttpCookie cookie = new HttpCookie("jwt", jwt);

                                    // Set additional properties if needed
                                    cookie.Expires = jwtConfiguration.ValidLifetime > 0 ? DateTime.Now.AddHours(jwtConfiguration.ValidLifetime) : DateTime.MaxValue;
                                    cookie.Domain = filterContext.HttpContext.Request.Url.Host; // Set the domain
                                    cookie.Path = "/"; // Set the path

                                    // Add the cookie to the response
                                    filterContext.HttpContext.Response.Cookies.Add(cookie);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }
    }
}