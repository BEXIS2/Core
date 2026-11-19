using BExIS.App.Bootstrap;
using BEXIS2.UI.Helpers;
using BExIS.Utils.Config;
using BExIS.Utils.Config.Configurations;
using BExIS.Web.Shell.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NHibernate;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Vaiona.IoC;
using BEXIS2.App.Model.MTnt;
using BEXIS2.App.MultiTenancy.Api;
using BEXIS2.Utils.Cfg;
using Vaiona.Web.Extensions;
using Autofac;
using Autofac.Integration.Mvc;

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


            // Autofac
            // 1. NHibernate SessionFactory einmalig initialisieren (Beispiel)
            _sessionFactory = new NHibernate.Cfg.Configuration().Configure().BuildSessionFactory();

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

            // 5. Container bauen und den MVC Dependency Resolver setzen
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

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