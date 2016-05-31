using BExIS.Web.Shell.Areas.DIM.Models.Formatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace BExIS.Web.Shell
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();
            ConfigureApis(config);
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }

        public static void ConfigureApis(HttpConfiguration config)
        {
            config.Formatters.Add(new DataTupleCsvFormatter());
        }
    }
}
