using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Authentication;
using BExIS.Security.Entities.Objects;

namespace BExIS.Security.Services.Authentication
{
    public interface IAuthenticatorManager
    {
        Authenticator CreateAuthenticator(string name, string classPath, string assemblyPath, string connectionString, AuthenticatorType authenticatorType);

        bool DeleteAuthenticatorById(long id);
        bool DeleteAuthenticatorByName(string name);

        bool ExistsAuthenticatorId(long id);
        bool ExistsAuthenticatorName(string name);

        IQueryable<Authenticator> GetAllAuthenticators();
        IQueryable<Authenticator> GetExternalAuthenticators();
        IQueryable<Authenticator> GetInternalAuthenticators();

        Authenticator GetAuthenticatorById(long id);
        Authenticator GetAuthenticatorByName(string name);

        Authenticator UpdateAuthenticator(Authenticator authenticator);
    }
}
