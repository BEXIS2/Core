using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace BExIS.Sam.Providers.Ldap
{
    public class LdapAuthenticationOptions : AuthenticationOptions
    {
        public LdapAuthenticationOptions() : base(Constants.DefaultAuthenticationType)
        {
            AuthenticationMode = AuthenticationMode.Passive;
            CallbackPath = new PathString("/Ldap/Login");
            Description.Caption = Constants.DefaultAuthenticationType;
        }

        public PathString CallbackPath { get; set; }
        public string SignInAsAuthenticationType { get; set; }
        public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }
    }
}