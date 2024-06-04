using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Vaiona.Logging;

namespace Vaiona.Web.Mvc.Modularity
{
    /// <summary>
    /// This class should have all the information needed to load and work with the a module.
    /// One important element is the module manifest, which conatins:
    /// name, descrription, dependencies, assemblies, exposed services, exposed menus, etc.
    /// </summary>
    public abstract class ModuleBase : AreaRegistration
    {
        protected AreaRegistrationContext context = null;
        private Dictionary<string, Route> moduleRoutes = new Dictionary<string, Route>();

        public Dictionary<string, Route> ModuleRoutes
        { get { return moduleRoutes; } }

        public ModuleInfo Metadata { get; set; }
        private string moduleId = "";

        public ModuleBase(string moduleId)
        {
            this.moduleId = moduleId;
            LoggerFactory.GetFileLogger().LogCustom(string.Format("Instantiating module '{0}'", moduleId));
            load(moduleId);
            RegisterModuleRoute("default", DefaultRoute);
            LoggerFactory.GetFileLogger().LogCustom(string.Format("Module '{0}' was successfully instantiated.", moduleId));
        }

        private void load(string moduleId)
        {
            var moduleInfo = ModuleManager.ModuleInfos.Where(p => p.Id.Equals(moduleId, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
            if (moduleInfo == null)
            {
                string message = string.Format("Module '{0}' is not loaded by the module manager. Instantiating the module was aborted!", moduleId);
                LoggerFactory.GetFileLogger().LogCustom(message);
                throw new Exception(message);
            }

            moduleInfo.Plugin = this;
            this.Metadata = moduleInfo;
            // load the manifest file
            try
            {
                string manifestPath = Path.Combine(moduleInfo.Path.FullName, string.Format("{0}.Manifest.xml", moduleInfo.Id));
                //XElement manifest = XElement.Load(manifestPath); // may need to go into the element itself
                moduleInfo.Manifest = new ModuleManifest(manifestPath);
            }
            catch (Exception ex)
            {
                string message = string.Format("Could not load the manifest file for module '{0}'. Details: {1}", moduleId, ex.Message);
                LoggerFactory.GetFileLogger().LogCustom(message);
                throw new Exception(message);
            }
            this.Settings = new ModuleSettings(moduleId);
        }

        //private string resovlePath(string moduleId)
        //{
        //    throw new NotImplementedException();
        //}

        public ModuleSettings Settings { get; set; }

        public string Name
        {
            get { return Metadata.Manifest.Name; }
        }

        public string DefaultRouteName
        {
            get { return AreaName + "_default"; }
        }

        public override string AreaName
        {
            get
            {
                return this.Name;
            }
        }

        protected void RegisterModuleRoute(string name, Route route, bool replaceIfExists = false)
        {
            // overwrite the route handler of the incoming route
            route.RouteHandler = new ModularMvcRouteHandler(AreaName);
            // prefix the route name with the area name
            var routeName = AreaName + "_" + name.ToLower();
            if (moduleRoutes.ContainsKey(routeName))
                if (replaceIfExists)
                    moduleRoutes[routeName] = route;
                else
                    throw new Exception(string.Format("Rule name '{0}' is already registered.", name));
            else
                moduleRoutes.Add(routeName, route);
        }

        /// <summary>
        /// AreaName is set in the code and acts as the identifier of the module.
        /// It is used to create the url space, identify the module among the others, and to link the module to its manifest.
        /// </summary>
        /// <remarks>It would be good to weaken or remove this dependency and load everything from the manifest.
        /// In that case the AreaName could be a used as the module identifier, but the url space could come from the manifest.
        /// In this scenario, any url generation is the controllers and views should consider using the manifest's url space identifer for area selection.
        /// </remarks>
        public override void RegisterArea(AreaRegistrationContext context)
        {
            LoggerFactory.GetFileLogger().LogCustom(string.Format("Registering the area for module '{0}'...", moduleId));

            try
            {
                this.context = context;
                context.Routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
                // the route handler associated with the route objects check for module status
                //if (!PluginManager.IsActive(this.Name))
                //    return; // do not register thr routes
                foreach (var routeItem in moduleRoutes)
                {
                    context.MapRoute(
                        routeItem.Key,
                        routeItem.Value.Url, routeItem.Value.Defaults
                    //AreaName + "/{controller}/{action}/{id}",
                    //new { action = "Index", id = UrlParameter.Optional }
                    ).RouteHandler = routeItem.Value.RouteHandler;// new ModularMvcRouteHandler(AreaName);
                }
                LoggerFactory.GetFileLogger().LogCustom(string.Format("The area for module '{0}' was successfully registered.", moduleId));
            }
            catch (Exception ex)
            {
                string message = string.Format("Could not register the area for module '{0}'. It throws exception: {1}", moduleId, ex.Message);
                LoggerFactory.GetFileLogger().LogCustom(message);
                throw new Exception(message);
            }
        }

        public Route DefaultRoute
        {
            get
            {
                return new Route(
                        url: AreaName + "/{controller}/{action}/{id}",
                        defaults:
                        new RouteValueDictionary {
                            { "controller", "Home" }, // module/home/index would be shortened to /module only. not tested yet.
                            { "action", "Index" },
                            { "id", UrlParameter.Optional }
                        },
                        routeHandler: new ModularMvcRouteHandler(AreaName)
                    );
            }
        }

        /// <summary>
        /// An optional custom code that is executed each time the module is loaded, usauly once per application start
        /// , but also upon Plugin Manager's request.
        /// </summary>
        public virtual void Start()
        { }

        /// <summary>
        /// An optional custom code that is executed when the application ends
        /// or when the Plugin Manager asks the module to shutdown.
        /// </summary>
        public virtual void Shutdown()
        { }

        /// <summary>
        /// The method is called by the Plugin Manager when the module is just installed.
        /// Its a good place to create the workspace or set the seed data
        /// </summary>
        public virtual void Install()
        { }

        /// <summary>
        /// The method is called by the Plugin Manager when the module is going to be un-installed.
        /// The actual un-installation happens after calling this method.
        /// </summary>
        public virtual void Uninstall()
        { }
    }
}