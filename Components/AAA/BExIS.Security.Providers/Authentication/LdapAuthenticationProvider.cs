using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices.Protocols;
using System.Web.Configuration;
using System.Net;
using System.Globalization;
using BExIS.Security.Entities.Subjects;
using Vaiona.Entities;

namespace BExIS.Security.Providers.Authentication
{
    public sealed class LdapAuthenticationProvider : IAuthenticationProvider
    {
        private string baseDn { get; set; }
        private string host { get; set; }
        private int port { get; set; }
        private bool secureSocket { get; set; }
        private string authUid { get; set; }
        private int protocolVersion { get; set; }

        public LdapAuthenticationProvider(string connectionString)
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
                    default:
                        break;
                }
            }
        }


        public bool IsUserAuthenticated(string userName, string password)
        {
            try
            {
                LdapConnection ldap = new LdapConnection(new LdapDirectoryIdentifier(host, port));
                ldap.SessionOptions.ProtocolVersion = protocolVersion;
                ldap.AuthType = AuthType.Anonymous;
                ldap.SessionOptions.SecureSocketLayer = secureSocket;
                ldap.Bind();

                ldap.AuthType = AuthType.Basic;
                SearchRequest searchRequest = new SearchRequest(
                       baseDn,
                       string.Format(CultureInfo.InvariantCulture, "{0}={1}", authUid, userName),
                       SearchScope.Subtree
                );

                SearchResponse searchResponse = (SearchResponse)ldap.SendRequest(searchRequest);
                if (1 == searchResponse.Entries.Count)
                {
                    ldap.Bind(new NetworkCredential(searchResponse.Entries[0].DistinguishedName, password));
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

        public User GetUser(string username, string password)
        {
            User ldapUser = new User();

            try
            {
                LdapConnection ldap = new LdapConnection(new LdapDirectoryIdentifier(host, port));
                ldap.SessionOptions.ProtocolVersion = protocolVersion;
                ldap.AuthType = AuthType.Anonymous;
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
                    ldap.Bind(new NetworkCredential(searchResponse.Entries[0].DistinguishedName, password));

                    SearchResultAttributeCollection attributes = searchResponse.Entries[0].Attributes;

                    ldapUser.Name = Convert.ToString(((DirectoryAttribute)attributes["cn"])[0]);
                    ldapUser.Email = Convert.ToString(((DirectoryAttribute)attributes["mail"])[0]);

                }
                else
                {
                    throw new Exception("User not found");
                }
            }
            catch (Exception e)
            {
                //Todo: Pass error to logging framework instead of console!
                Console.WriteLine(e.Message);
                ldapUser = null;
            }
            return ldapUser;
        }
    }
}
