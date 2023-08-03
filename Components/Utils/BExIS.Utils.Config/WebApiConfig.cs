using Microsoft.Owin.Security.OAuth;
using System.Web.Http;

namespace BExIS.Utils.Config
{
    public static class WebApiConfig
    {
        private static bool isRegistered; // this is to prevent double registering the routes during unit testing the Application's Global class

        public static void Register(HttpConfiguration config)
        {
            if (isRegistered)
            {
                return;
            }
            config.MapHttpAttributeRoutes();
            ConfigureApis(config);
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.SuppressDefaultHostAuthentication();
            config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            isRegistered = true;
        }

        public static void ConfigureApis(HttpConfiguration config)
        {
        }
    }
}