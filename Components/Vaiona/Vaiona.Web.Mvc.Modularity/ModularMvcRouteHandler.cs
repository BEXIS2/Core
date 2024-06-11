using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Vaiona.Web.Mvc.Modularity
{
    public class ModularMvcRouteHandler : MvcRouteHandler
    {
        private string moduleId = string.Empty;

        public ModularMvcRouteHandler(string moduleId)
        {
            this.moduleId = moduleId;
        }

        protected override IHttpHandler GetHttpHandler(System.Web.Routing.RequestContext requestContext)
        {
            if (ModuleManager.IsActive(moduleId))
                return base.GetHttpHandler(requestContext);
            else
            {
                //throw new Exception(string.Format("Module '{0}' is not active", moduleId));
                return new ModuleNotAvailableHandler(requestContext);
            }
        }
    }

    public class ModuleNotAvailableHandler : MvcHandler
    {
        public ModuleNotAvailableHandler(RequestContext requestContext) : base(requestContext)
        {
        }

        protected override IAsyncResult BeginProcessRequest(HttpContext httpContext, AsyncCallback callback, object state)
        {
            httpContext.Response.ClearHeaders();
            httpContext.Response.Clear();

            httpContext.Response.StatusCode = 404;
            httpContext.Response.SuppressContent = true;

            return base.BeginProcessRequest(httpContext, callback, state);
        }
    }
}