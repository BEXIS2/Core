using Microsoft.Owin;
using Microsoft.Owin.Security.Infrastructure;

namespace BExIS.Sam.Providers.Ldap
{
    public class LdapAuthenticationMiddleware : AuthenticationMiddleware<LdapAuthenticationOptions>
    {
        public LdapAuthenticationMiddleware(OwinMiddleware next, LdapAuthenticationOptions options) : base(next, options)
        {
        }

        protected override AuthenticationHandler<LdapAuthenticationOptions> CreateHandler()
        {
            throw new System.NotImplementedException();
        }
    }
}