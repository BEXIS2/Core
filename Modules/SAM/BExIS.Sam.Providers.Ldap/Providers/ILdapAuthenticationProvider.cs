using System.Threading.Tasks;

namespace BExIS.Sam.Providers.Ldap.Providers
{
    public interface ILdapAuthenticationProvider
    {
        Task Authenticated(LdapAuthenticatedContext context);

        Task ReturnEndpoint(LdapReturnEndpointContext context);
    }
}