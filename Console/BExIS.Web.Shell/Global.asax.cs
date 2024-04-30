using BExIS.App.Bootstrap;
using BExIS.UI.Helpers;
using BExIS.Utils.Config;
using BExIS.Web.Shell.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Vaiona.IoC;
using Vaiona.Model.MTnt;
using Vaiona.MultiTenancy.Api;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;

namespace BExIS.Web.Shell
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode,
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
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
            Tenant tenant = tenantResolver.Resolve(this.Request);

            // if the tenant has no landing page, set the application's default landing page for it.

            //var landingPage = GeneralSettings.LandingPage;
            //tenant.LandingPage = landingPage; // checks and sets

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
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "content-type, append,delete,entries,foreach,get,has,keys,set,values,Authorization");

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



            bool sendExceptions = GeneralSettings.SendExceptions;

            var error = Server.GetLastError();
            var code = (error is HttpException) ? (error as HttpException).GetHttpCode() : 500;

            if (
                sendExceptions &&
                code != 404 && // not existing action is called
                !(error is InvalidOperationException) && !error.Message.StartsWith("Multiple types were found that match the controller named") // same controller name in multpily controller, and no correct action call
               )
            {
                HttpUnhandledException httpUnhandledException =
                   new HttpUnhandledException(error.Message, error);

                ErrorHelper.SendEmailWithErrors(
                    httpUnhandledException.GetHtmlErrorMessage()
                    );

                ErrorHelper.Log(Server.GetLastError().Message);
            }
        }
    }
}