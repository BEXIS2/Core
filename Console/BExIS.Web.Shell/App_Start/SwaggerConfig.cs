using BExIS.Web.Shell;
using Swashbuckle.Application;
using System.Configuration;
using System.IO;
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

            string rootUrl = "";
            if (ConfigurationManager.AppSettings["SwaggerRootUrl"] != null)
                rootUrl = ConfigurationManager.AppSettings["SwaggerRootUrl"].ToString();

            GlobalConfiguration.Configuration.EnableSwagger(c =>
            {
                c.SingleApiVersion("v1", "Bexis 2 APIs");
                c.ApiKey("Authorization").Description("Filling bearer token here").Name("Bearer").In("header");
                c.PrettyPrint();

                c.BasicAuth("basic").Description("Basic HTTP Authentication");

                addXmlDocumentations(c);

                c.IgnoreObsoleteProperties();
                c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
                //c.ApiKey("Authorization", "header", "Filling bearer token here");
                c.Schemes(new[] { "https" });

                if (!string.IsNullOrEmpty(rootUrl)) c.RootUrl(r => rootUrl);

                c.GroupActionsBy(api =>
                {
                    var pathArray = api.RelativePath.Split('/');
                    if (pathArray.Any())
                    {
                        var enity = pathArray[1].ToUpperInvariant().Split('?').First();
                        return enity;
                    }
                    return "not grouped"; // Default group name
                });

            }).EnableSwaggerUi("apihelp/{*assetPath}", c =>
            {
                c.DocumentTitle("API´s");
                c.InjectJavaScript(thisAssembly, "BExIS.Web.Shell.Scripts.custom-swagger.js");
                c.BooleanValues(new[] { "0", "1" });
                c.EnableDiscoveryUrlSelector();
                c.EnableApiKeySupport("Authorization", "header");
            });
        }

        private static void addXmlDocumentations(SwaggerDocsConfig c)
        {
            //get app_data path
            string path = string.Format(@"{0}\App_Data", System.AppDomain.CurrentDomain.BaseDirectory);

            // get all files from the direcory (app_data)
            var files = Directory.EnumerateFiles(path, "*.xml");

            if (files != null)
            {
                // add each file (xml documenation) to the swagger config
                files.ToList().ForEach(f => c.IncludeXmlComments(f));
            }
        }
    }
}