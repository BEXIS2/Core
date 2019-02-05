using BExIS.App.Bootstrap;
using BExIS.UI.Helpers;
using BExIS.Utils.Config;
using System;
using System.Web;
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
            GeneralSettings generalSettings = IoCFactory.Container.Resolve<GeneralSettings>();
            var landingPage = generalSettings.GetEntryValue("landingPage").ToString();
            tenant.LandingPage = landingPage; // checks and sets

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
            HttpUnhandledException httpUnhandledException =
               new HttpUnhandledException(Server.GetLastError().Message, Server.GetLastError());
            //SendEmailWithErrors(httpUnhandledException.GetHtmlErrorMessage());

            ErrorHelper.SendEmailWithErrors(
                httpUnhandledException.GetHtmlErrorMessage()
                );

            ErrorHelper.Log(Server.GetLastError().Message);

        }


    }
}