using System;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using System.Web.Mvc;
using System.Web.Routing;
using BExIS.Security.Entities.Authorization;

namespace BExIS.App.Bootstrap.Attributes
{
    public class BExISEntityAuthorizeAttribute : ActionFilterAttribute
    {
        private string entityName;
        private Type entityType;
        private string keyName;
        private RightType rightType;

        public BExISEntityAuthorizeAttribute(string entityName, Type entityType, string keyName, RightType rightType)
        {
            this.entityName = entityName;
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

                if (!entityPermissionManager.HasEffectiveRight(userName, entityType, Convert.ToInt64(filterContext.ActionParameters[keyName]), rightType))
                {
                    filterContext.Result = new RedirectToRouteResult(new
                        RouteValueDictionary{
                            { "action", "AccessDenied" },
                            { "controller", "Error" },
                            { "Area", string.Empty }
                        });
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