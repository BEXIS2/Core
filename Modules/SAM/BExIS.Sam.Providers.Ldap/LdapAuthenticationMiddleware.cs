using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;
using Owin;

namespace BExIS.Sam.Providers.Ldap
{
    public class LdapAuthenticationMiddleware : AuthenticationMiddleware<LdapAuthenticationOptions>
    {
        public LdapAuthenticationMiddleware(OwinMiddleware next, IAppBuilder app, LdapAuthenticationOptions options) : base(next, options)
        {
        }

        protected override AuthenticationHandler<LdapAuthenticationOptions> CreateHandler()
        {
            return new LdapAuthenticationHandler();
        }
    }
}