using BExIS.Security.Services.Authentication;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.Web.Shell;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using NHibernate;
using Owin;
using Vaiona.Persistence.Api;
using Vaiona.Persistence.NH;
using Vaiona.PersistenceProviders.NH;

[assembly: OwinStartup(typeof(Startup))]

namespace BExIS.Web.Shell
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Auth.Configure(app);

            app.CreatePerOwinContext<SignInManager>(SignInManager.Create);
        }
    }
}