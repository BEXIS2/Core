using BExIS.Ext.Services;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Vaiona.IoC;
using Vaiona.Logging;
using Vaiona.Model.MTnt;
using Vaiona.MultiTenancy.Api;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Data;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Web.Shell
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode,
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new PersistenceContextProviderFilterAttribute());
#if !DEBUG
            //filters.Add(new Vaiona.Web.Mvc.Filters.AuthorizationDelegationFilter(new Vaiona.Web.Mvc.Filters.IsAuthorizedDelegate(AuthorizationDelegationImplementor.CheckAuthorization)));
#endif
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               "Default", // Route name
               "{controller}/{action}/{id}", // URL with parameters
               new { controller = "home", action = "index", id = UrlParameter.Optional } // Parameter defaults
               , new[] { "BExIS.Web.Shell.Controllers" } // to prevent conflict between root controllers and area controllers that have same names
           );
        }

        protected void Application_Start()
        {
            MvcHandler.DisableMvcResponseHeader = true;

            // This line registers an assembly resolver for the dynamically loaded modules. It also takes care of modules' statuses, so that inactive modules are not resolved.
            // This MUST be before IoC initialization.
            AppDomain.CurrentDomain.AssemblyResolve += ModuleManager.ResolveCurrentDomainAssembly;
            initIoC();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            // This method initializes the registered modules. It MUST be before initializing the persistence! 
            initModules();
            // The persistnce service starts up the ORM, binds the mapping files to the database and the entities and depndeing on the modules' statuses upgrades the database schema.
            // Also, it generates the seed data of the modules by calling their install methods.
            initPersistence();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            // This method builds a tree of the actions and apis exposed by each module. It tries to put them in a proper order and resolves name conflicts by some resolution rules.
            ModuleManager.BuildExportTree();
            //This method identifies and loads the current tenant of the application. Many other methods and layout sections depends upon the tenant identified here.
            initTenancy();
            // At application start, each modules obtains a chance to perform some initialization and warmup tasks. They are coded in each module's Start method.
            // This call starts them. 
            ModuleManager.StartModules();
        }

        private void initTenancy()
        {
            ITenantResolver tenantResolver = IoCFactory.Container.Resolve<ITenantResolver>();
            ITenantPathProvider pathProvider = new BExISTenantPathProvider(); // should be instantiated by the IoC. client app should provide the Path Ptovider based on its file and tenant structure
            tenantResolver.Load(pathProvider);
        }

        private void initIoC()
        {
            IoCFactory.StartContainer(Path.Combine(AppConfiguration.AppRoot, "IoC.config"), "DefaultContainer");
        }

        private void initModules()
        {
           ModuleManager.InitModules(Path.Combine(AppConfiguration.AppRoot, "Shell.Manifest.xml"), GlobalConfiguration.Configuration); // this should be called before RegisterAllAreas
          //AreaRegistration.RegisterAllAreas(GlobalConfiguration.Configuration); 
        }

        private void initPersistence()
        {
            IPersistenceManager pManager = PersistenceFactory.GetPersistenceManager(); // just to prepare data access environment
            pManager.Configure(AppConfiguration.DefaultApplicationConnection.ConnectionString, AppConfiguration.DatabaseDialect, "Default", AppConfiguration.ShowQueries);
            if (AppConfiguration.CreateDatabase)
            {
                pManager.ExportSchema();
            }
            pManager.Start();

            // If there is any active module in catalong at the DB creation time, it would be automatically installed.
            // Installation means, the modules' Install method is called, which usually generates the seed data
            if (AppConfiguration.CreateDatabase)
            {
                installModuleOnFreshDatabase();
            }
            // if there are pending modules, their schema (if exists) must be applied.
            else if (ModuleManager.HasPendingInstallation())
            {
                insatllPendingModules(pManager);
            }
        }

        private void insatllPendingModules(IPersistenceManager pManager)
        {
            if (!AppConfiguration.CreateDatabase && ModuleManager.HasPendingInstallation())
            {
                try
                {
                    pManager.UpdateSchema();
                }
                catch (Exception ex)
                { }
                // When the pending modules' schemas are ported, their potential seed data should be generated.
                // This is done through calling Install method.
                foreach (var moduleId in ModuleManager.PendingModules())
                {
                    // The install method of the plugin is called once and only once.
                    // It generates the seed data and other module specific initializations
                    try
                    {
                        ModuleManager.GetModuleInfo(moduleId).Plugin.Install();
                    }
                    catch (Exception ex)
                    {
                        string message = string.Format("Unable to install module '{0}' while in pending state. Details: {1}.", moduleId, ex.Message);
                        LoggerFactory.GetFileLogger().LogCustom(message);
                        throw new Exception(message);
                    }

                    // For security reasons, pending modules go to the "inactive" status after schema export.
                    // An administrator can endable them via the management console
                    try
                    {
                        ModuleManager.Disable(moduleId);
                    }
                    catch (Exception ex)
                    {
                        string message = string.Format("Unable to diable module '{0}' after recovering from pending state. Details: {1}.", moduleId, ex.Message);
                        LoggerFactory.GetFileLogger().LogCustom(message);
                        throw new Exception(message);
                    }
                }
            }
        }

        private void installModuleOnFreshDatabase()
        {
            if (AppConfiguration.CreateDatabase)
            {
                foreach (var module in ModuleManager.ModuleInfos.Where(p => ModuleManager.IsActive(p.Id)))
                {
                    try
                    {
                        ModuleManager.GetModuleInfo(module.Id).Plugin.Install();
                    }
                    catch (Exception ex)
                    {
                        string message = string.Format("Unable to install module '{0}'. Details: {1}.", module.Id, ex.Message);
                        LoggerFactory.GetFileLogger().LogCustom(message);
                        throw new Exception(message);
                    }
                }
            }
        }

        protected void Application_End()
        {
            ModuleManager.ShutdownModules();
            IPersistenceManager pManager = PersistenceFactory.GetPersistenceManager();
            pManager.Shutdown(); // release all data access related resources!
            IoCFactory.ShutdownContainer();
        }

        protected void Session_Start()
        {
            if (Context.Session != null)
            {
                if (Context.Session.IsNewSession)
                {
                    string sCookieHeader = Request.Headers["Cookie"];
                    if ((null != sCookieHeader) && (sCookieHeader.IndexOf("ASP.NET_SessionId") >= 0))
                    {
                        //intercept current route
                        HttpContextBase currentContext = new HttpContextWrapper(HttpContext.Current);
                        RouteData routeData = RouteTable.Routes.GetRouteData(currentContext);
                        Response.Redirect("~/Home/SessionTimeout");
                        Response.Flush();
                        Response.End();
                    }
                }
            }

            //set session culture using DefaultCulture key
            IoCFactory.Container.StartSessionLevelContainer();
            Session.ApplyCulture(AppConfiguration.DefaultCulture);

            ITenantResolver tenantResolver = IoCFactory.Container.Resolve<ITenantResolver>();
            Tenant tenant = tenantResolver.Resolve(this.Request);
            this.Session.SetTenant(tenant);
        }

        protected void Session_End()
        {
            IPersistenceManager pManager = PersistenceFactory.GetPersistenceManager();
            pManager.ShutdownConversation();
            IoCFactory.Container.ShutdownSessionLevelContainer();
        }

        protected virtual void Application_BeginRequest()
        {
        }

        /// <summary>
        /// the function is called on any http request, which include static resources too!
        /// conversation management is done using the global filter  PersistenceContextProviderAttribute.
        /// </summary>
        protected virtual void Application_EndRequest()
        {
            //var entityContext = HttpContext.Current.Items["NHibernateCurrentSessionFactory"] as IDictionary<ISessionFactory, Lazy<ISession>>;
            //IPersistenceManager pManager = PersistenceFactory.GetPersistenceManager();
            //pManager.ShutdownConversation();
        }

        protected void Application_PreSendRequestHeaders()
        {
            Response.Headers.Remove("Server");
            Response.Headers.Remove("X-AspNet-Version");
            Response.Headers.Remove("X-AspNetMvc-Version");
            Response.Headers.Remove("X-Powered-By");
        }
    }
}