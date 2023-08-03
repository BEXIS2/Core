using Microsoft.AspNet.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Owin;
using System.Configuration;
using System.Text;

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

            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = ConfigurationManager.AppSettings["JwtIssuer"], //some string, normally web url,  
                        ValidAudience = ConfigurationManager.AppSettings["JwtIssuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["JwtKey"]))
                    }
                });

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "dfgdfgdfgd",
            //   consumerSecret: "dfgdfffffffg");

            //app.UseLdapAuthentication(new LdapAuthenticationOptions());

            //app.UseFacebookAuthentication(
            //   appId: "134998043776447",
            //   appSecret: "c8b3b4a0878ebe70f9494f93202e203b"
            //);

            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "587369244487-4cd2h297b0gispb44lvvbns8ohlvm2fj.apps.googleusercontent.com",
            //    ClientSecret = "lm7FGNTY0Bz_Y3rh0apf2T6F"
            //});

            //app.UseOrcidAuthentication(new OrcidAuthenticationOptions()
            //{
            //    Endpoints = new OrcidAuthenticationEndpoints
            //    {
            //        ApiEndpoint = "https://pub.sandbox.orcid.org/v1.2/0000-0003-0514-2115/orcid-profile",
            //        TokenEndpoint = "https://sandbox.orcid.org/oauth/token",
            //        AuthorizationEndpoint = h"https://sandbox.orcid.org/oauth/authorize?client_idttps://sandbox.orcid.org/oauth/authorize?client_id="
            //                        + clientId + "&response_type=code&scope="
            //                        + "/read-limited" + "&redirect_uri="
            //                        + "http://localhost:55247/Acces"
            //    },
            //    ClientId = clientId,
            //    ClientSecret = clientSecret
            //});
        }
    }
}