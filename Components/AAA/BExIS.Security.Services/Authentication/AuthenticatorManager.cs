using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Authentication;
using BExIS.Security.Entities.Objects;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Authentication
{
    public sealed class AuthenticatorManager : IAuthenticatorManager
    {
        public AuthenticatorManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();

            this.AuthenticatorsRepo = uow.GetReadOnlyRepository<Authenticator>();
        }

        #region Data Reader
    
        public IReadOnlyRepository<Authenticator> AuthenticatorsRepo { get; private set; }

        #endregion

        #region Methods

        public Authenticator CreateAuthenticator(string name, string classPath, string assemblyPath, string connectionString, AuthenticatorType authenticatorType)
        {
            Authenticator authenticator = new Authenticator()
            {
                Name = name,
                AssemblyPath = assemblyPath,
                ClassPath = classPath,
                ConnectionString = connectionString,
                AuthenticatorType = authenticatorType
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Authenticator> featuresRepo = uow.GetRepository<Authenticator>();
                featuresRepo.Put(authenticator);

                uow.Commit();
            }

            return (authenticator);
        }

        public bool DeleteAuthenticatorById(long id)
        {
            throw new NotImplementedException();
        }

        public bool DeleteAuthenticatorByName(string name)
        {
            throw new NotImplementedException();
        }

        public bool ExistsAuthenticatorId(long id)
        {
            return AuthenticatorsRepo.Get(id) != null ? true : false;
        }

        public bool ExistsAuthenticatorName(string name)
        {
            return AuthenticatorsRepo.Get(a => a.Name.ToLower() == name.ToLower()).Count == 1 ? true : false;
        }

        public IQueryable<Authenticator> GetAllAuthenticators()
        {
            return AuthenticatorsRepo.Query();
        }

        public IQueryable<Authenticator> GetExternalAuthenticators()
        {
            return AuthenticatorsRepo.Query(a => a.AuthenticatorType == AuthenticatorType.External);
        }

        public IQueryable<Authenticator> GetInternalAuthenticators()
        {
            return AuthenticatorsRepo.Query(a => a.AuthenticatorType == AuthenticatorType.Internal);
        }

        public Authenticator GetAuthenticatorById(long id)
        {
            return AuthenticatorsRepo.Get(id);
        }

        public Authenticator GetAuthenticatorByName(string name)
        {
            throw new NotImplementedException();
        }

        public Authenticator UpdateAuthenticator(Authenticator authenticator)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Authenticator> usersRepo = uow.GetRepository<Authenticator>();
                usersRepo.Put(authenticator);
                uow.Commit();
            }

            return (authenticator);
        }

        #endregion
    }
}
