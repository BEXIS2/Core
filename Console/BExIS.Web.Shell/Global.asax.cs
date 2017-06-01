using BExIS.Ext.Services;
using System;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
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
            filters.Add(new Vaiona.Web.Mvc.Filters.AuthorizationDelegationFilter(new Vaiona.Web.Mvc.Filters.IsAuthorizedDelegate(AuthorizationDelegationImplementor.CheckAuthorization)));
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

            initIoC();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            AppDomain.CurrentDomain.AssemblyResolve += ModuleManager.ResolveCurrentDomainAssembly;
            initPersistence();
            initModules();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
            ModuleManager.BuildExportTree();
            initTenancy();
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
            IoCFactory.StartContainer(Path.Combine(AppConfiguration.AppRoot, "IoC.config"), "DefaultContainer"); // use AppConfig to access the app root folder
        }

        private void initModules()
        {
            ModuleManager.RegisterShell(Path.Combine(AppConfiguration.AppRoot, "Shell.Manifest.xml")); // this should be called before RegisterAllAreas
            AreaRegistration.RegisterAllAreas(GlobalConfiguration.Configuration); // this is the starting point of geting modules registered
            // at the time of this call, the PluginInitilizer has already loaded the plug-ins
            //ModuleBootstrapper.Initialize();
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
                foreach (var module in ModuleManager.ModuleInfos.Where(p => ModuleManager.IsActive(p.Id)))
                {
                    ModuleManager.GetModuleInfo(module.Id).Plugin.Install();
                }
            }
            // if there are pending modules, their schema (if exists) must be applied.
            else if (ModuleManager.HasPendingInstallation())
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
                        // For security reasons, pending modules go to the "inactive" status after schema export. 
                        // An administrator can endable them via the management console
                        ModuleManager.Disable(moduleId);
                    }
                    catch (Exception ex)
                    {
                        LoggerFactory.LogCustom(string.Format("Error installing module {0}. {1}", moduleId, ex.Message));
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