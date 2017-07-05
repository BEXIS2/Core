using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Google;
using Owin;

namespace BExIS.Security.Services.Utilities
{
    public static class Auth
    {
        public static IDataProtectionProvider DataProtectionProvider { get; private set; }

        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        public static void Configure(IAppBuilder app)
        {
            DataProtectionProvider = app.GetDataProtectionProvider();

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
            });
            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "dfgdfgdfgd",
            //   consumerSecret: "dfgdfffffffg");

            //app.UseLdapAuthentication(new LdapAuthenticationOptions("/signin-ldap", "host"));

            //app.UseFacebookAuthentication(
            //   appId: "sfg",
            //   appSecret: "ffffffffffff");

            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "587369244487-l9567jtupsnsc0j8pa7gatvsj9bfipic.apps.googleusercontent.com",
                ClientSecret = "DtLaArW-ybAKCKv3aFVFGsac"
            });
        }
    }
}