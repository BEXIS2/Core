using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using BExIS.Core.IoC;
using BExIS.Core.Persistence.Api;
using BExIS.Core.Util.Cfg;
using System.IO;

namespace BExIS.Web.Shell
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
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
            //HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();

            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }

        private void init()
        {
            IoCFactory.StartContainer(Path.Combine(AppConfiguration.AppRoot, "IoC.config"), "DefaultContainer"); // use AppConfig to access the app root folder
            loadModules();
            IPersistenceManager pManager = IoCFactory.Container.Resolve<IPersistenceManager>() as IPersistenceManager; // just to prepare data access environment
            pManager.Configure(); //, AppConfiguration.DefaultApplicationConnection.ConnectionString);
            if (AppConfiguration.CreateDatabase)
                pManager.ExportSchema();
            pManager.Start();
        }

        private void loadModules()
        {
            //throw new NotImplementedException();
        }

        protected void Application_End()
        {
            IPersistenceManager pManager = IoCFactory.Container.Resolve<IPersistenceManager>() as IPersistenceManager;
            pManager.Shutdown(); // release all data access related resources!
            IoCFactory.ShutdownContainer();
        }

        protected void Session_Start()
        {
            Session["SessionLevelContainer"] = IoCFactory.Container.CreateSessionLevelContainer();
        }

        protected void Session_End()
        {
            //IoCContainer container = Session["SessionLevelContainer"] as IoCContainer;
            Session.Remove("SessionLevelContainer");
        }
    }
}