using Microsoft.Owin.Security;

namespace BExIS.Sam.Providers.Ldap
{
    public class LdapAuthenticationOptions : AuthenticationOptions
    {
        public LdapAuthenticationOptions(string authenticationType) : base(authenticationType)
        {
        }
    }
}