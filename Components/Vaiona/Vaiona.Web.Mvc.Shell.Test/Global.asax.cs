using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Vaiona.IoC;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;
using System.IO;
using Vaiona.Web.Mvc.Data;
using Vaiona.MultiTenancy.Api;
using Vaiona.Model.MTnt;
using Vaiona.Web.Extensions;
using Vaiona.MultiTenancy.Services;

namespace Vaiona.Web.Mvc.Shell.Test
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new PersistenceContextProviderFilterAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );

        }

        protected void Application_Start()
        {
            IoCFactory.StartContainer(Path.Combine(AppConfiguration.AppRoot, "IoC.config"), "DefaultContainer", HttpContext.Current); // use AppConfig to access the app root folder
            //loadModules();
            IPersistenceManager pManager = PersistenceFactory.GetPersistenceManager(); // just to prepare data access environment
            pManager.Configure(AppConfiguration.DefaultApplicationConnection.ConnectionString, AppConfiguration.DatabaseDialect); //, AppConfiguration.DefaultApplicationConnection.ConnectionString);
            if (AppConfiguration.CreateDatabase)
                pManager.ExportSchema();
            pManager.Start();

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            ITenantResolver tenantResolver = IoCFactory.Container.Resolve<ITenantResolver>();
            ITenantPathProvider pathProvider = new DefaultTenantPathProvider(); // should be instantiated by the IoC. client app should provide the Path Ptovider based on its file and tenant structure
            tenantResolver.Load(pathProvider); 
            var x = tenantResolver.Manifest;
        }

        protected void Session_Start()
        {
            ITenantResolver tenantResolver = IoCFactory.Container.Resolve<ITenantResolver>();
            Tenant tenant = tenantResolver.Resolve(this.Request);
            this.Session.SetTenant(tenant);


        }
    }
}