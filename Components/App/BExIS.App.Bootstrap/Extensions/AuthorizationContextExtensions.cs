using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace BExIS.App.Bootstrap.Extensions
{
    public static class AuthorizationContextExtensions
    {
        public static AuthorizationContext SetResponse(this AuthorizationContext filterContext, HttpStatusCode status)
        {
            try
            {
                var returnType = ((ReflectedActionDescriptor)filterContext.ActionDescriptor).MethodInfo.ReturnType;

                if (returnType == typeof(JsonResult))
                {
                    filterContext.Result = new HttpStatusCodeResult(status, status.ToString());
                    return filterContext;
                }
                else
                {
                    filterContext.Result = new System.Web.Mvc.RedirectToRouteResult(new
                            RouteValueDictionary{
                           { "action", "AccessDenied" },
                           { "controller", "Error" },
                           { "Area", string.Empty }
                       });
                    return filterContext;
                }
            }
            catch (Exception)
            {
                return filterContext;
            }
        }
    }
}