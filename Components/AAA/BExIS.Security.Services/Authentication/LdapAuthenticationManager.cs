using System;
using System.Collections.Generic;
using BExIS.Security.Entities.Subjects;
using Vaiona.Persistence.Api;
using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Linq;
using System.Net;
using Microsoft.AspNet.Identity.Owin;

namespace BExIS.Security.Services.Authentication
{
    public class LdapAuthenticationManager
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        private string baseDn { get; }
        private string host { get; }
        private int port { get; }
        private bool secureSocket { get; }
        private string authUid { get; }
        private int protocolVersion { get; }

        private IReadOnlyRepository<User> UserRepository { get; }

        public LdapAuthenticationManager(string connectionString)
        {
            _guow = this.GetIsolatedUnitOfWork();
            UserRepository = _guow.GetReadOnlyRepository<User>();

            var parameters = connectionString
                .Split(';')
                .Select(x => x.Split(':'))
                .ToDictionary(x => x[0], x => x[1]);

            foreach (var entry in parameters)
            {
                switch (entry.Key)
                {
                    case "ldapBaseDn":
                        baseDn = entry.Value;
                        break;
                    case "ldapHost":
                        host = entry.Value;
                        break;
                    case "ldapPort":
                        port = Convert.ToInt32(entry.Value);
                        break;
                    case "ldapSecure":
                        secureSocket = Convert.ToBoolean(entry.Value);
                        break;
                    case "ldapAuthUid":
                        authUid = entry.Value;
                        break;
                    case "ldapProtocolVersion":
                        protocolVersion = Convert.ToInt32(entry.Value);
                        break;
                    default:
                        break;
                }
            }
        }

        ~LdapAuthenticationManager()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_guow != null)
                        _guow.Dispose();
                    _isDisposed = true;
                }
            }
        }

        public SignInStatus ValidateUser(string username, string password)
        {
            try
            {
                var ldap = new LdapConnection(new LdapDirectoryIdentifier(host, port));
                ldap.SessionOptions.ProtocolVersion = protocolVersion;
                ldap.AuthType = AuthType.Anonymous;
                ldap.SessionOptions.SecureSocketLayer = secureSocket;
                ldap.Bind();

                ldap.AuthType = AuthType.Basic;
                var searchRequest = new SearchRequest(
                    baseDn,
                    string.Format(CultureInfo.InvariantCulture, "{0}={1}", authUid, username),
                    SearchScope.Subtree
                );

                var searchResponse = (SearchResponse)ldap.SendRequest(searchRequest);
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
                return SignInStatus.Failure;
            }
            return SignInStatus.Success;
        }

        public User GetUser(string username, string password)
        {
            var ldapUser = new User();

            try
            {
                var ldap = new LdapConnection(new LdapDirectoryIdentifier(host, port));
                ldap.SessionOptions.ProtocolVersion = protocolVersion;
                ldap.AuthType = AuthType.Anonymous;
                ldap.SessionOptions.SecureSocketLayer = secureSocket;
                ldap.Bind();

                ldap.AuthType = AuthType.Basic;
                var searchRequest = new SearchRequest(
                    baseDn,
                    string.Format(CultureInfo.InvariantCulture, "{0}={1}", authUid, username),
                    SearchScope.Subtree
                );

                var searchResponse = (SearchResponse)ldap.SendRequest(searchRequest);
                if (1 == searchResponse.Entries.Count)
                {
                    ldap.Bind(new NetworkCredential(searchResponse.Entries[0].DistinguishedName, password));

                    var attributes = searchResponse.Entries[0].Attributes;

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
