using System;
using System.Web.Mvc;

namespace Vaiona.Web.Mvc.Filters
{
    public delegate void IsAuthorizedDelegate(string areaName, string controllerName, string actionName, string userName, bool isAuthenticated);

    /// <summary>
    /// intercept calls to all the actions, creates a context object based on the Area, Controller and Action and passes them
    /// to all registered events
    /// </summary>
    public class AuthorizationDelegationFilter : ActionFilterAttribute, IExceptionFilter//, IActionFilter, IResultFilter
    {
        private IsAuthorizedDelegate isAuthorizedDelegate = null;

        public AuthorizationDelegationFilter(IsAuthorizedDelegate isAuthorizedDelegateParam)
        {
            isAuthorizedDelegate = isAuthorizedDelegateParam;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string areaName = "Shell";
            try
            {
                //areaName = filterContext.RouteData.Values["area"].ToString();
                areaName = filterContext.RouteData.DataTokens["area"].ToString();
                //areaName = !string.IsNullOrWhiteSpace(areaName) ? areaName : "Shell";
            }
            catch { }
            string controllerName = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            string actionName = filterContext.ActionDescriptor.ActionName;
            string userName = string.Empty;
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                userName = filterContext.HttpContext.User.Identity.Name;
            }
            bool isAuthenticated = filterContext.HttpContext.User.Identity.IsAuthenticated;

            try
            {
                //throws an exception if the request is not validated or not granted
                isAuthorizedDelegate(areaName, controllerName, actionName, userName, isAuthenticated);
            }
            catch (Exception ex)
            {
                // set the result to unauthorized result
                filterContext.Result = new HttpUnauthorizedResult();
                // prevent the action from being executed
                return;
            }
        }

        //public override void OnActionExecuted(ActionExecutedContext filterContext)
        //{
        //}

        //public override void OnResultExecuted(ResultExecutedContext filterContext)
        //{
        //}

        //public override void OnResultExecuting(ResultExecutingContext filterContext)
        //{
        //}

        public void OnException(ExceptionContext filterContext)
        {
        }
    }
}