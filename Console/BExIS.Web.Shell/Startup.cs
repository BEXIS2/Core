using BExIS.Security.Services.Utilities;
using BExIS.Web.Shell;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Startup))]

namespace BExIS.Web.Shell
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Auth.Configure(app);
        }
    }
}