using System;
using System.Threading;
using Vaiona.Web.Mvc.Modularity;
using System.Web.Http;
using System.Web.Mvc;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;
using System.IO;
using Vaiona.Logging;
using System.Collections.Generic;
using Vaiona.IoC;
using Vaiona.MultiTenancy.Api;
using System.Web.Routing;
using Vaiona.Model.MTnt;
using BExIS.Ext.Services;
using System.Linq;

namespace BExIS.App.Bootstrap
{
    public enum RunStage
    {
        Test,
        Production,
    }
    public class Application
    {
        private RunStage runStage = RunStage.Production;
        private bool started = false;

        public bool Started { get { return started; } }
        public Application(RunStage runStage= RunStage.Production)
        {
            this.runStage = runStage;
        }

        public void Start(Action<HttpConfiguration> configurationCallback, bool configureModules)
        {
            try
            {
                switch (runStage)
                {
                    case RunStage.Test:
                        runForTest(configurationCallback, configureModules);
                        break;
                    case RunStage.Production:
                        runForProduction(configurationCallback, configureModules);
                        break;
                    default:
                        break;
                }
                started = true;
            }
            catch (Exception ex)
            {
                started = false;
                throw ex;
            }
        }

        public void Stop()
        {
            ModuleManager.ShutdownModules();
            IPersistenceManager pManager = PersistenceFactory.GetPersistenceManager();
            pManager.Shutdown(); // release all data access related resources!
            IoCFactory.ShutdownContainer();
            started = false;
        }

        private void runForProduction(Action<HttpConfiguration> configurationCallback, bool configureModules)
        {
            application_Start(configurationCallback, configureModules); // this will error b/c not fully loaded yet.
        }

        private void runForTest(Action<HttpConfiguration> configurationCallback, bool configureModules)
        {
            //ModuleInitializer.Initialize();
            try
            {
                application_Start(configurationCallback, configureModules); // this will error b/c not fully loaded yet.
            }
            catch (InvalidOperationException)
            {
                Thread.Sleep(2000); // give the app time to launch

                application_Start(configurationCallback, configureModules);
            }
        }

        private void application_Start(Action<HttpConfiguration> configurationCallback, bool configureModules)
        {
            MvcHandler.DisableMvcResponseHeader = true;

            // This line registers an assembly resolver for the dynamically loaded modules. It also takes care of modules' statuses, so that inactive modules are not resolved.
            // This MUST be before IoC initialization.
            AppDomain.CurrentDomain.AssemblyResolve += ModuleManager.ResolveCurrentDomainAssembly;
            initIoC();
            GlobalConfiguration.Configure(configurationCallback);
            // This method initializes the registered modules. It MUST be before initializing the persistence! 
            initModules();
            // The persistnce service starts up the ORM, binds the mapping files to the database and the entities and depndeing on the modules' statuses upgrades the database schema.
            // Also, it generates the seed data of the modules by calling their install methods.
            initPersistence(configureModules);
            registerGlobalFilters(GlobalFilters.Filters);
            registerRoutes(RouteTable.Routes);
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
            try
            {
                IoCFactory.StartContainer(Path.Combine(AppConfiguration.AppRoot, "IoC.config"), "DefaultContainer");
            }
            catch (System.TypeLoadException) { } // swallow this exception, as it means that the IoC is already running.
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void initModules()
        {
            ModuleManager.InitModules(Path.Combine(AppConfiguration.AppRoot, "Shell.Manifest.xml"), GlobalConfiguration.Configuration); // this should be called before RegisterAllAreas
                                                                                                                                        //AreaRegistration.RegisterAllAreas(GlobalConfiguration.Configuration); 
        }

        private void initPersistence(bool configureModules = true)
        {
            IPersistenceManager pManager = PersistenceFactory.GetPersistenceManager(); // just to prepare data access environment
            pManager.Configure(AppConfiguration.DefaultApplicationConnection.ConnectionString, 
                AppConfiguration.DatabaseDialect, "Default", AppConfiguration.ShowQueries, configureModules);

            if (AppConfiguration.CreateDatabase)
            {
                pManager.ExportSchema();
            }
            pManager.Start();

            // If there is any active module in catalong at the DB creation time, it would be automatically installed.
            // Installation means, the modules' Install method is called, which usually generates the seed data
            if (AppConfiguration.CreateDatabase)
            {
                IModuleSeedDataGenerator shellSeedDataGenerator = IoCFactory.Container.Resolve<IModuleSeedDataGenerator>();
                shellSeedDataGenerator.GenerateSeedData();
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
                        //throw new Exception(message);
                    }

                    // For security reasons, pending modules go to the "inactive" status after schema export.
                    // An administrator can endable them via the management console
                    try
                    {
                        ModuleManager.Disable(moduleId);
                    }
                    catch (Exception ex)
                    {
                        string message = string.Format("Unable to disable module '{0}' after recovering from pending state. Details: {1}.", moduleId, ex.Message);
                        LoggerFactory.GetFileLogger().LogCustom(message);
                        //throw new Exception(message);
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
                        //throw new Exception(message);
                    }
                }
            }
        }

        private void registerGlobalFilters(GlobalFilterCollection filters)
        {
            //filters.Add(new PersistenceContextProviderFilterAttribute()); // disabled by Javad on 22.08.2017
#if !DEBUG
            filters.Add(new BExIS.Web.Shell.Attributes.BExISAuthorizeAttribute());
#endif
            //filters.Add(new Vaiona.Web.Mvc.Filters.AuthorizationDelegationFilter(new Vaiona.Web.Mvc.Filters.IsAuthorizedDelegate(AuthorizationDelegationImplementor.CheckAuthorization)));
            filters.Add(new HandleErrorAttribute());
        }

        private void registerRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
               "Default", // Route name
               "{controller}/{action}/{id}", // URL with parameters
               new { controller = "home", action = "index", id = UrlParameter.Optional } // Parameter defaults
               , new[] { "BExIS.Web.Shell.Controllers" } // to prevent conflict between root controllers and area controllers that have same names
           );
        }

    }
}
