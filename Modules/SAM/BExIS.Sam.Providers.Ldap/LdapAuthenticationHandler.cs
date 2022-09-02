using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using System.Threading.Tasks;

namespace BExIS.Sam.Providers.Ldap
{
    public class LdapAuthenticationHandler : AuthenticationHandler<LdapAuthenticationOptions>
    {
        public override async Task<bool> InvokeAsync()
        {
            return await Task.FromResult(true);
        }

        protected override Task ApplyResponseChallengeAsync()
        {
            Response.Redirect("/Ldap/Login");
            return Task.FromResult(0);
        }

        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            return await Task.FromResult(new AuthenticationTicket(null, null));
        }
    }
}