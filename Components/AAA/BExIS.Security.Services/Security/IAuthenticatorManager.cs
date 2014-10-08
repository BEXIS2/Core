using BExIS.Security.Entities.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Security.Services.Security
{
    public interface IAuthenticatorManager
    {
        Authenticator GetAuthenticatorById(long id);
        Authenticator GetAuthenticatorByAlias(string alias);
        Authenticator GetAuthenticatorByConnectionString(string connectionString);
        Authenticator GetAuthenticatorByClassPath(string classPath);

        Authenticator AddAuthenticator(string alias, string projectPath, string classPath, string connectionString);

        void DeleteAuthenticatorByAlias(string alias);
        void DeleteAuthenticatorByConnectionString(string connectionString);
        void DeleteAuthenticatorByClassPath(string classPath);

        Authenticator EditAutenticator(string authenticatorToEditAlias, string newAlias, string newProjectPath, string newClassPath, string newConnectionString);

        void LockAuthenticatorByAlias(string alias);
        void UnlockAuthenticatorByAlias(string alias);

        IQueryable<Authenticator> GetAllAuthenticators();
    }
}
