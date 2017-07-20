using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace BExIS.Sam.Providers.Ldap
{
    public class LdapAuthenticationOptions : AuthenticationOptions
    {
        public LdapAuthenticationOptions(string callbackPath, string host) : base(Constants.DefaultAuthenticationType)
        {
            AuthenticationMode = AuthenticationMode.Passive;
            CallbackPath = new PathString(callbackPath);
            Description.Caption = Constants.DefaultAuthenticationType;
            Host = host;
        }

        public bool Anonymous { get; set; }
        public string BaseDn { get; set; }
        public PathString CallbackPath { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string SignInAsAuthenticationType { get; set; }
        public ISecureDataFormat<AuthenticationProperties> StateDataFormat { get; set; }
        public int Version { get; set; }
    }
}