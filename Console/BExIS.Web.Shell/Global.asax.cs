using Autofac;
using Autofac.Integration.Mvc;
using BExIS.App.Bootstrap;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Config;
using BExIS.Utils.Config.Configurations;
using BExIS.Web.Shell.Helpers;
using BEXIS2.App.Model.MTnt;
using BEXIS2.App.MultiTenancy.Api;
using BEXIS2.DB.Persistence.Api;
using BEXIS2.DB.Persistence.NH;
using BEXIS2.Dlm.Services.Data;
using BEXIS2.Security.Entities.Subjects;
using BEXIS2.UI.Helpers;
using BEXIS2.Utils.Cfg;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NHibernate;
using NHibernate.Cfg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Vaiona.IoC;
using Vaiona.Web.Extensions;

namespace BExIS.Web.Shell
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode,
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : HttpApplication
    {
        private static ISessionFactory _sessionFactory;

        private BExIS.App.Bootstrap.Application app = null;

        
        protected void Application_Start()
        {
            // Json Settings
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            // Extension of the view search engine by the case that the UI project with view can be found one directory lower.
            // This extension allows to store a complete module with libraries and Ui project in a parent directory.
            var tmp = new CustomViewEngine();

            foreach (var engine in ViewEngines.Engines)
            {
                if (engine is RazorViewEngine)
                {
                    ((RazorViewEngine)engine).AreaMasterLocationFormats = tmp.AreaMasterLocationFormats;
                    ((RazorViewEngine)engine).AreaPartialViewLocationFormats = tmp.AreaPartialViewLocationFormats;
                    ((RazorViewEngine)engine).AreaViewLocationFormats = tmp.AreaViewLocationFormats;
                }
            }

            app = BExIS.App.Bootstrap.Application.GetInstance(RunStage.Production);
            app.Start(WebApiConfig.Register, true);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //AreaRegistration.RegisterAllAreas();

            //IPersistenceManager pManager = PersistenceFactory.GetPersistenceManager();
            //_sessionFactory = (ISessionFactory)pManager.Factory;

            // 1. NHibernate SessionFactory einmalig initialisieren (Beispiel)
            var connectionString = AppConfiguration.DefaultApplicationConnection.ConnectionString;
            var cfg = new Configuration();
            string configFileName = string.Format(@"{0}.hibernate.cfg.xml", "PostgreSQL82Dialect");
            string configFileFullPath = Path.Combine(AppConfiguration.WorkspaceGeneralRoot, "Db", "Settings", configFileName);
            cfg.Configure(configFileFullPath);

            cfg.SetProperty(NHibernate.Cfg.Environment.ConnectionString, connectionString);

            string workspacepath = Path.Combine(AppConfiguration.WorkspaceRootPath, "Components");

            DirectoryInfo mappingDirectory = new DirectoryInfo(workspacepath);
            cfg.AddDirectory(mappingDirectory);
            ISessionFactory _sessionFactory = cfg.BuildSessionFactory();

            var builder = new ContainerBuilder();
            // 2. ISessionFactory als Singleton registrieren
            builder.RegisterInstance(_sessionFactory).As<ISessionFactory>().SingleInstance();
          
            // 3. ISession als "Per Request" binden (Wichtig: Verwerfen am Request-Ende inklusive)
            builder.Register(context => context.Resolve<ISessionFactory>().OpenSession())
                   .As<ISession>()
                   .InstancePerRequest();

            // 4. Alle Manager/Services im Assembly automatisch registrieren
            // Sucht nach Klassen, deren Name auf "Manager" oder "Service" endet
            builder.RegisterAssemblyTypes(typeof(MvcApplication).Assembly) // bzw. Assembly.GetExecutingAssembly()
                   .Where(t => t.Name.EndsWith("Manager") || t.Name.EndsWith("Service"))
                   .AsSelf()
                   .InstancePerRequest();

            builder.RegisterControllers(typeof(MvcApplication).Assembly);


            builder.RegisterType<UserStore>()
               .As<IUserStore<User,long>> () // or IUserStore<ApplicationUser>, depending on UserStore's generic/non-generic signature
               .InstancePerRequest();

            builder.RegisterType<GroupStore>()
               .As<IRoleStore<Group, long>>() // or IUserStore<ApplicationUser>, depending on UserStore's generic/non-generic signature
               .InstancePerRequest();

            builder.RegisterType<GroupManager>().AsSelf()
                   .InstancePerRequest();
            builder.RegisterType<EntityTemplate2Manager>().AsSelf()
                   .InstancePerRequest();
            builder.RegisterType<UserManager>().AsSelf()
                   .InstancePerRequest();
            builder.RegisterType<GroupManager>().AsSelf()
                   .InstancePerRequest();


            // add modules
            var areasRoot = HostingEnvironment.MapPath("~/Areas");
            var moduleAssemblies = new List<Assembly>();

            if (!string.IsNullOrEmpty(areasRoot) && Directory.Exists(areasRoot))
            {
                foreach (var dllPath in Directory.GetFiles(areasRoot, "*.UI.dll", SearchOption.AllDirectories))
                {
                    // Optional: bestehende bin-Assemblies überspringen, falls doppelt vorhanden
                    //var asmName = AssemblyName.GetAssemblyName(dllPath);
                    //if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == asmName.Name))
                    //    continue;

                    var assembly = Assembly.LoadFrom(dllPath);

                    if(!moduleAssemblies.Contains(assembly)) moduleAssemblies.Add(assembly);

                }
            }

            //// Falls AreaRegistration.RegisterAllAreas() schon vor dem Laden der DLLs aufgerufen wurde:
            //foreach (var asm in moduleAssemblies)
            //{
            //    var areaRegTypes = asm.GetTypes()
            //        .Where(t => typeof(AreaRegistration).IsAssignableFrom(t) && !t.IsAbstract);

            //    foreach (var regType in areaRegTypes)
            //    {
            //        var reg = (AreaRegistration)Activator.CreateInstance(regType);
            //        var ctx = new AreaRegistrationContext(reg.AreaName, RouteTable.Routes);
            //        reg.RegisterArea(ctx);
            //    }
            //}

            foreach (var asm in moduleAssemblies)
            {
                builder.RegisterControllers(asm);
                builder.RegisterAssemblyModules(asm);
            }

            //builder.RegisterFilterProvider();

            // 5. Container bauen und den MVC Dependency Resolver setzen
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            ControllerBuilder.Current.SetControllerFactory(new DefaultControllerFactory());
            var x = ControllerBuilder.Current.GetControllerFactory().GetType().FullName;

        }

        protected void Application_End()
        {
            app.Stop();
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
            Tenant tenant = tenantResolver.Resolve();

            this.Session.SetTenant(tenant);
        }

        protected void Session_End()
        {
            //IPersistenceManager pManager = PersistenceFactory.GetPersistenceManager();
            //pManager.ShutdownConversation();
            try
            {
                IoCFactory.Container.ShutdownSessionLevelContainer();
            }
            catch { }
        }

        protected virtual void Application_BeginRequest()
        {
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "POST, GET, OPTIONS, DELETE, PUT");
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "content-type, append,delete,entries,foreach,get,has,keys,set,values,Authorization, __requestverificationtoken");

            if (Request.HttpMethod == "OPTIONS")
            {
                Response.End();
            }
        }

        /// <summary>
        /// the function is called on any http request, which include static resources too!
        /// conversation management is done using the global filter  PersistenceContextProviderAttribute.
        /// </summary>
        protected virtual void Application_EndRequest()
        {
            //var entityContext = HttpContext.Current.Items[NHibernateCurrentSessionProvider.CURRENT_SESSION_CONTEXT_KEY];
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

        protected void Application_Error(object sender, EventArgs e)
        {
            ExceptionConfiguration exceptionConfiguration = GeneralSettings.ExceptionConfiguration;

            var error = Server.GetLastError();
            var code = (error is HttpException) ? (error as HttpException).GetHttpCode() : 500;



            if (
                exceptionConfiguration.sendExceptions &&
                !exceptionConfiguration.ExcludedHttpStatusCodes.Contains(code) &&
                exceptionConfiguration.ExcludedMessages.All(m => !error.Message.Contains(m))
                )
            {
                HttpUnhandledException httpUnhandledException =
                   new HttpUnhandledException(error.Message, error);

                ErrorHelper.SendEmailWithErrors(
                    httpUnhandledException.GetHtmlErrorMessage()
                    );

                
            }

            ErrorHelper.Log("*************Error***********");
            ErrorHelper.Log(Server.GetLastError().Message);
            ErrorHelper.Log(Server.GetLastError().StackTrace);
        }
    }
}