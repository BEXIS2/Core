using BExIS.Web.Shell;
using Swagger.Net.Application;
using System.Linq;
using System.Web.Http;
using WebActivatorEx;

[assembly: PreApplicationStartMethod(typeof(SwaggerConfig), "Register")]

namespace BExIS.Web.Shell
{
    public class SwaggerConfig
    {
        public static void Register()
        {
            var thisAssembly = typeof(SwaggerConfig).Assembly;

            GlobalConfiguration.Configuration
                .EnableSwagger(c =>
                    {
                        c.SingleApiVersion("v1", "Bexis 2 API´s");
                        c.ApiKey("bearer", "header", "Filling bearer token here");
                        //c.PrettyPrint();
                        //c.IncludeXmlComments(GetXmlCommentsPath());
                        //c.IgnoreObsoleteProperties();
                        //c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                        //c.ApiKey("Authorization", "header", "Filling bearer token here");
                    })

                .EnableSwaggerUi("apihelp/{*assetPath}", c =>
                     {
                         c.DocumentTitle("API´s");
                         //c.InjectJavaScript(thisAssembly, "BExIS.Web.Shell.Scripts.custom-swagger.js");
                         c.BooleanValues(new[] { "0", "1" });
                         c.EnableDiscoveryUrlSelector();
                         c.ApiKeySupport("bearer", "header");
                     });
        }

        private static string GetXmlCommentsPath()
        {
            string path = string.Format(@"{0}\App_Data\api_documentation.xml", System.AppDomain.CurrentDomain.BaseDirectory);

            return path;
        }
    }
}