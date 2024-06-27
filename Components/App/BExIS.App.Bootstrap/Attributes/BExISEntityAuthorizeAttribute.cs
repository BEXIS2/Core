using BExIS.App.Bootstrap.Helpers;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using Newtonsoft.Json;
using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace BExIS.App.Bootstrap.Attributes
{
    public class BExISEntityAuthorizeAttribute : ActionFilterAttribute
    {
        //private string entityName;
        private Type entityType;

        private string keyName;
        private RightType rightType;

        public BExISEntityAuthorizeAttribute(Type entityType, string keyName, RightType rightType)
        {
            //this.entityName = entityName;
            this.entityType = entityType;
            this.keyName = keyName;
            this.rightType = rightType;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var entityPermissionManager = new EntityPermissionManager();
            var userManager = new UserManager();

            try
            {
                var userName = string.Empty;
                if (filterContext.HttpContext.User.Identity.IsAuthenticated)
                    userName = filterContext.HttpContext.User.Identity.Name;

                // if user is null check request header auth
                if (string.IsNullOrEmpty(userName))
                {
                    var authorization = filterContext.HttpContext.Request.Headers.Get("Authorization");
                    User user = null;
                    var res = BExISAuthorizeHelper.GetUserFromAuthorization(authorization, out user);
                    if (user != null) userName = user.Name;
                }

                if (!entityPermissionManager.HasEffectiveRight(userName, entityType, Convert.ToInt64(filterContext.ActionParameters[keyName]), rightType))
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
                entityPermissionManager.Dispose();
                userManager.Dispose();
            }
        }
    }
}