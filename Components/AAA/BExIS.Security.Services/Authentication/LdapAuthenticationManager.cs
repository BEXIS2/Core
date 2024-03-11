using BExIS.Security.Entities.Subjects;
using BExIS.Utils.Config;
using BExIS.Utils.Config.Configurations;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Net;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Authentication
{
    public class LdapAuthenticationManager
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        private readonly List<LdapConfiguration> _ldapConfigurations;

        public LdapAuthenticationManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            _ldapConfigurations = GeneralSettings.LdapConfigurations;
        }

        ~LdapAuthenticationManager()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public User GetUser(string name, string username, string password)
        {
            User ldapUser = null;

            try
            {
                var ldapConfiguration = _ldapConfigurations.Find(l => l.Name == name);

                if (ldapConfiguration == null)
                    return null;

                using (var ldap = new LdapConnection(new LdapDirectoryIdentifier(ldapConfiguration.Host, ldapConfiguration.Port)))
                {
                    ldap.SessionOptions.ProtocolVersion = ldapConfiguration.Version;
                    ldap.AuthType = (AuthType)ldapConfiguration.AuthType;
                    ldap.SessionOptions.SecureSocketLayer = ldapConfiguration.Ssl;
                    ldap.Credential = new NetworkCredential($"{ldapConfiguration.Identifier}={username},{ldapConfiguration.BaseDn}", password);
                    ldap.Bind();

                    var searchResponse = (SearchResponse)ldap.SendRequest(new SearchRequest($"{ldapConfiguration.Identifier}={username},{ldapConfiguration.BaseDn}", "objectClass=*", (SearchScope)ldapConfiguration.Scope));

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

        public SignInStatus ValidateUser(string name, string username, string password)
        {
            var ldapuser = GetUser(name, username, password);

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