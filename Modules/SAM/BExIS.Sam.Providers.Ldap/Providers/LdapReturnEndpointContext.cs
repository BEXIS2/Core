using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Provider;

namespace BExIS.Sam.Providers.Ldap.Providers
{
    public class LdapReturnEndpointContext : ReturnEndpointContext
    {
        public LdapReturnEndpointContext(IOwinContext context, AuthenticationTicket ticket) : base(context, ticket)
        {
        }
    }
}