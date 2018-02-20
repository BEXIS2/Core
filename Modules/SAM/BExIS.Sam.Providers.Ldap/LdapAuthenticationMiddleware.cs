using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.DataProtection;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace BExIS.Sam.Providers.Ldap
{
    public class LdapAuthenticationMiddleware : AuthenticationMiddleware<LdapAuthenticationOptions>
    {
        public LdapAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, LdapAuthenticationOptions options) : base(next, options)
        {
            if (string.IsNullOrEmpty(Options.SignInAsAuthenticationType))
            {
                options.SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType();
            }

            if (options.StateDataFormat != null) return;

            var dataProtector = app.CreateDataProtector(typeof(LdapAuthenticationMiddleware).FullName,
                options.AuthenticationType);

            options.StateDataFormat = new PropertiesDataFormat(dataProtector);
        }

        protected override AuthenticationHandler<LdapAuthenticationOptions> CreateHandler()
        {
            return new LdapAuthenticationHandler();
        }
    }
}