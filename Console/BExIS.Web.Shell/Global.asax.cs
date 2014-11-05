using BExIS.Ext.Services;
using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using Vaiona.IoC;
using Vaiona.Persistence.Api;
using Vaiona.Util.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Data;
using Vaiona.Web.Mvc.Filters;

namespace BExIS.Web.Shell
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new PersistenceContextProviderAttribute());
#if !DEBUG
            filters.Add(new AuthorizationDelegationFilter(new IsAuthorizedDelegate(AuthorizationDelegationImplementor.CheckAuthorization)));
#endif
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            //routes.MapRoute(
            //    "DDM", //Landing page
            //    "",
            //    new { area = "DDM", controller = "Home", action = "Index" }
            //    , new[] { "BExIS.Web.Shell.Areas.DDM.Controllers" }
            //).DataTokens["UseNamespaceFallback"] = false;

            //routes.MapRoute(
            //   "Home",
            //   "Home",
            //   new { controller = "Home", action = "Index" }
            //   , new[] { "BExIS.Web.Shell.Controllers" }
            //);

            routes.MapRoute(
               "Default", // Route name
               "{controller}/{action}/{id}", // URL with parameters
               new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
               , new[] { "BExIS.Web.Shell.Controllers" } // to prevent conflict between root controllers and area controllers that have same names
           );

          //  routes.MapRoute(
          //    "RPM",
          //    "RPM/{controller}/{action}/{id}",
          //    new { controller = "Home", action = "Index" }
          //    , new[] { "BExIS.Web.Shell.Areas.RPM.Controllers" }
            //).DataTokens = new RouteValueDictionary(new { area = "RPM" });


        


        }

        protected void Application_Start()
        {
            init();

            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        private void init()
        {
            IoCFactory.StartContainer(Path.Combine(AppConfiguration.AppRoot, "IoC.config"), "DefaultContainer"); // use AppConfig to access the app root folder
            loadModules();
            IPersistenceManager pManager = PersistenceFactory.GetPersistenceManager(); // just to prepare data access environment
            pManager.Configure(AppConfiguration.DefaultApplicationConnection.ConnectionString, AppConfiguration.DatabaseDialect, "Default", AppConfiguration.ShowQueries);
            if (AppConfiguration.CreateDatabase)
                pManager.ExportSchema();
            pManager.Start();
#if DEBUG
            //HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
            //just for testing purposes
            NHibernate.Glimpse.Plugin.RegisterSessionFactory(pManager.Factory as NHibernate.ISessionFactory);
#endif
       }

        private void loadModules()
        {
            ModuleManager.LoadModules(); // it does nothing yet, implement it!
        }

        protected void Application_End()
        {
            IPersistenceManager pManager = PersistenceFactory.GetPersistenceManager();
            
           pManager.Shutdown(); // release all data access related resources!
            IoCFactory.ShutdownContainer();
        }

        protected void Session_Start()
        {
            //set session culture using DefaultCulture key
            IoCFactory.Container.StartSessionLevelContainer();
            Session.ApplyCulture(AppConfiguration.DefaultCulture);
        }

        protected void Session_End()
        {
            //IoCContainer container = Session["SessionLevelContainer"] as IoCContainer;
            IoCFactory.Container.ShutdownSessionLevelContainer();
        }
    }
}