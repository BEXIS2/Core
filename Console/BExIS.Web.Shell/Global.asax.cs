using System.IO;
using System.Web.Mvc;
using System.Web.Routing;
using Vaiona.IoC;
using Vaiona.Persistence.Api;
using Vaiona.Util.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Data;

namespace BExIS.Web.Shell
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new PersistenceContextProviderAttribute());
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
                , new[] {"BExIS.Web.Shell.Controllers"} // to prevent conflict between root controllers and area controllers that have same names
            );

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
            pManager.Configure(AppConfiguration.DefaultApplicationConnection.ConnectionString);
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