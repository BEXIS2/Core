using Microsoft.Owin;
using Microsoft.Owin.Security.Provider;

namespace BExIS.Sam.Providers.Ldap.Providers
{
    public class LdapAuthenticatedContext : BaseContext
    {
        public LdapAuthenticatedContext(IOwinContext context) : base(context)
        {
        }
    }
}