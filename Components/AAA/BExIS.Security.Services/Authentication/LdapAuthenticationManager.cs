using BExIS.Security.Entities.Authentication;
using BExIS.Security.Entities.Subjects;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.DirectoryServices.Protocols;
using System.Net;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Authentication
{
    public class LdapAuthenticationManager
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        private readonly LdapConfiguration _ldapConfiguration;

        public LdapAuthenticationManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            _ldapConfiguration = new LdapConfiguration();
        }

        ~LdapAuthenticationManager()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public User GetUser(string username, string password)
        {
            User ldapUser = null;

            try
            {
                using (var ldap = new LdapConnection(new LdapDirectoryIdentifier(_ldapConfiguration.HostName, _ldapConfiguration.HostPort)))
                {
                    ldap.SessionOptions.ProtocolVersion = _ldapConfiguration.HostVersion;
                    ldap.AuthType = (AuthType)_ldapConfiguration.HostAuthType;
                    ldap.SessionOptions.SecureSocketLayer = _ldapConfiguration.HostSsl;
                    ldap.Credential = new NetworkCredential($"{_ldapConfiguration.UserIdentifier}={username},{_ldapConfiguration.HostBaseDn}", password);
                    ldap.Bind();

                    var searchResponse = (SearchResponse)ldap.SendRequest(new SearchRequest($"{_ldapConfiguration.UserIdentifier}={username},{_ldapConfiguration.HostBaseDn}", "objectClass=*", (SearchScope)_ldapConfiguration.HostScope));

                    if (searchResponse.Entries.Count == 1)
                    {
                        var attributes = searchResponse.Entries[0].Attributes;

                        ldapUser = new User()
                        {
                            Email = (attributes["mail"]?[0] ?? "").ToString(),
                            UserName = username,
                            IsEmailConfirmed = !string.IsNullOrEmpty((attributes["mail"]?[0] ?? "").ToString())
                        };
                    }
                    else
                    {
                        throw new Exception("User not found");
                    }
                }
            }
            catch (Exception e)
            {
                //Todo: Pass error to logging framework instead of console!
                ldapUser = null;
            }
            return ldapUser;
        }

        public SignInStatus ValidateUser(string username, string password)
        {
            var ldapuser = GetUser(username, password);

            if (ldapuser != null)
                return SignInStatus.Success;

            return SignInStatus.Failure;
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
    }
}