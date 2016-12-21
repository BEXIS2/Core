using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;

namespace BExIS.Security.Services.Authentication
{
    public sealed class ADAuthenticationManager : AuthenticationManager, IAuthenticationManager
    {
        private string baseDn { get; set; }
        private string host { get; set; }
        private int port { get; set; }
        private bool secureSocket { get; set; }
        private string authUid { get; set; }
        private int protocolVersion { get; set; }
        private string adminUsername { get; set; }
        private string adminPassword { get; set; }

        /// <summary>
        /// Initialize a new LdapAuthenticationProvider
        /// </summary>
        /// <param name="connectionString">connection string with all necessary paramters:
        ///     <list type="table">
        ///         <listheader>
        ///             <term>Connection string key</term>
        ///             <description>Description</description>
        ///         </listheader>
        ///         <item>
        ///             <term>ldapBaseDn</term>
        ///             <description>Point of entry to the LDAP</description>
        ///         </item>
        ///         <item>
        ///             <term>ldapHost</term>
        ///             <description>Hostname of the LDAP server</description>
        ///         </item>
        ///         <item>
        ///             <term>ldapPort</term>
        ///             <description>Port of the LDAP server. Expect a int value.</description>
        ///         </item>
        ///         <item>
        ///             <term>ldapSecure</term>
        ///             <description>Enable a SecureSocketLayer connection. Expect a boolean value.</description>
        ///         </item>
        ///         <item>
        ///             <term>ldapAuthUid</term>
        ///             <description>Attribute of the user, which sould used as unique identifier</description>
        ///         </item>
        ///         <item>
        ///             <term>ldapProtocolVersion</term>
        ///             <description>Version of the LDAP protocol, normally 3. Expect a int value.</description>
        ///         </item>
        ///     </list>
        ///     To assign a value to a key you have to use colon (:) and the seperation of the keys is done by using semicolon (;)
        ///     E.g.: ldapPort:389;ldapSecure:true;ldapProtocolVersion:3;...
        /// </param>
        public ADAuthenticationManager(string connectionString)
        {
            Dictionary<string, string> parameters = connectionString
                .Split(';')
                    .Select(x => x.Split(':'))
                        .ToDictionary(x => x[0], x => x[1]);

            foreach (KeyValuePair<string, string> entry in parameters)
            {
                switch (entry.Key)
                {
                    case "ldapBaseDn":
                        this.baseDn = entry.Value;
                        break;
                    case "ldapHost":
                        this.host = entry.Value;
                        break;
                    case "ldapPort":
                        this.port = Convert.ToInt32(entry.Value);
                        break;
                    case "ldapSecure":
                        this.secureSocket = Convert.ToBoolean(entry.Value);
                        break;
                    case "ldapAuthUid":
                        this.authUid = entry.Value;
                        break;
                    case "ldapProtocolVersion":
                        this.protocolVersion = Convert.ToInt32(entry.Value);
                        break;
                    case "ldapAdminUsername":
                        this.adminUsername = entry.Value;
                        break;
                    case "ldapAdminPassword":
                        this.adminPassword = entry.Value;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Authenticate a user against a AD server
        /// </summary>
        /// <param name="username">username to check</param>
        /// <param name="password">password of the user</param>
        /// <returns></returns>
        public bool ValidateUser(string username, string password)
        {
            try
            {
                LdapConnection ldap = new LdapConnection(new LdapDirectoryIdentifier(host, port));
                ldap.SessionOptions.ProtocolVersion = protocolVersion;
                ldap.AuthType = AuthType.Basic;
                ldap.Credential = new NetworkCredential(adminUsername, adminPassword);
                ldap.SessionOptions.SecureSocketLayer = secureSocket;
                ldap.Bind();

                ldap.AuthType = AuthType.Basic;
                SearchRequest searchRequest = new SearchRequest(
                       baseDn,
                       string.Format(CultureInfo.InvariantCulture, "{0}={1}", authUid, username),
                       SearchScope.Subtree
                );

                SearchResponse searchResponse = (SearchResponse)ldap.SendRequest(searchRequest);
                if (1 == searchResponse.Entries.Count)
                {
                    //ldap.Bind(new NetworkCredential(searchResponse.Entries[0].DistinguishedName, password));
                }
                else
                {
                    throw new Exception("Login failed.");
                }
            }
            catch (Exception e)
            {
                //Todo: Pass error to logging framework instead of console!
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }
    }
}
