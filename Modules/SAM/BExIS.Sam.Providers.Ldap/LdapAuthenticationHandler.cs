using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using System.Threading.Tasks;

namespace BExIS.Sam.Providers.Ldap
{
    public class LdapAuthenticationHandler : AuthenticationHandler<LdapAuthenticationOptions>
    {
        protected override Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}