using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Security;
using Vaiona.Persistence.Api;
using System.Diagnostics.Contracts;

namespace BExIS.Security.Services.Security
{
    public sealed class AuthenticatorManager : IAuthenticatorManager
    {
        private IReadOnlyRepository<Authenticator> repo { get; set; }

        public AuthenticatorManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();
            this.repo = uow.GetReadOnlyRepository<Authenticator>();
        }

        public Authenticator GetAuthenticatorById(long id)
        {
            ICollection<Authenticator> authenticators = repo.Query(a => a.Id == id).ToArray();

            if (authenticators.Count() == 1)
            {
                return authenticators.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public Authenticator GetAuthenticatorByAlias(string alias)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(alias));

            ICollection<Authenticator> authenticators = repo.Query(r => r.Alias.ToLower() == alias.ToLower()).ToArray();

            if (authenticators.Count() == 1)
            {
                return authenticators.FirstOrDefault();
            }
            else
            {
                throw new NullReferenceException("Authenticator cannot be found");
            }
        }

        public Authenticator GetAuthenticatorByConnectionString(string connectionString)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(connectionString));

            ICollection<Authenticator> authenticators = repo.Query(r => r.ConnectionString.ToLower() == connectionString.ToLower()).ToArray();

            if (authenticators.Count() == 1)
            {
                return authenticators.FirstOrDefault();
            }
            else
            {
                throw new NullReferenceException("Authenticator cannot be found");
            }
        }

        public Authenticator GetAuthenticatorByClassPath(string classPath)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(classPath));

            ICollection<Authenticator> authenticators = repo.Query(r => r.ClassPath.ToLower() == classPath.ToLower()).ToArray();

            if (authenticators.Count() == 1)
            {
                return authenticators.FirstOrDefault();
            }
            else
            {
                throw new NullReferenceException("Authenticator cannot be found");
            }
        }

        public void DeleteAuthenticatorByAlias(string alias)
        {
            Authenticator authenticator = GetAuthenticatorByAlias(alias);

            if (authenticator != null)
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Authenticator> authRepo = uow.GetRepository<Authenticator>();

                    authRepo.Delete(authenticator);
                    uow.Commit();
                }
            }
            else
            {
                throw new NullReferenceException("Authenticator cannot be found");
            }
        }

        public void DeleteAuthenticatorByConnectionString(string connectionString)
        {
            Authenticator authenticator = GetAuthenticatorByConnectionString(connectionString);

            if (authenticator != null)
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Authenticator> authRepo = uow.GetRepository<Authenticator>();

                    authRepo.Delete(authenticator);
                    uow.Commit();
                }
            }
            else
            {
                throw new NullReferenceException("Authenticator cannot be found");
            }
        }

        public void DeleteAuthenticatorByClassPath(string classPath)
        {
            Authenticator authenticator = GetAuthenticatorByClassPath(classPath);

            if (authenticator != null)
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Authenticator> authRepo = uow.GetRepository<Authenticator>();

                    authRepo.Delete(authenticator);
                    uow.Commit();
                }
            }
            else
            {
                throw new NullReferenceException("Authenticator cannot be found");
            }
        }


        public Authenticator AddAuthenticator(string alias, string projectPath, string classPath, string connectionString)
        {
            Authenticator newAuthenticator = null;
            Authenticator searchAuthenticator = GetAuthenticatorByAlias(alias);

            if (searchAuthenticator == null)
            {
                newAuthenticator = new Authenticator(alias, projectPath, classPath, connectionString);
                if (newAuthenticator != null)
                {
                    using (IUnitOfWork uow = this.GetUnitOfWork())
                    {
                        IRepository<Authenticator> authRepo = uow.GetRepository<Authenticator>();
                        authRepo.Put(newAuthenticator);
                        uow.Commit();
                    }
                    return newAuthenticator;
                }
                else
                {
                    throw new NullReferenceException("Authenticator was not created");
                }
            }
            else
            {
                throw new ArgumentException("The authenticator with alias [" + alias + "] already exist");
            }
        }


        public Authenticator EditAutenticator(string authenticatorToEditAlias, string newAlias, string newProjectPath, string newClassPath, string newConnectionString)
        {
            Authenticator searchAuthenticator = GetAuthenticatorByAlias(authenticatorToEditAlias);

            if (searchAuthenticator != null)
            {
                // Fill the authenticator with new parameters, if not NULL
                if (!String.IsNullOrWhiteSpace(newAlias))
                {
                    searchAuthenticator.Alias = newAlias;
                }
                if (!String.IsNullOrWhiteSpace(newProjectPath))
                {
                    searchAuthenticator.ProjectPath = newClassPath;
                }
                if (!String.IsNullOrWhiteSpace(newClassPath))
                {
                    searchAuthenticator.ClassPath = newClassPath;
                }
                if (!String.IsNullOrWhiteSpace(newConnectionString))
                {
                    searchAuthenticator.ConnectionString = newConnectionString;
                }

                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Authenticator> authRepo = uow.GetRepository<Authenticator>();
                    authRepo.Put(searchAuthenticator);
                    uow.Commit();
                }
                return searchAuthenticator;

            }
            else
            {
                throw new ArgumentException("The authenticator with alias [" + authenticatorToEditAlias + "] does not exist, cannot edit");
            }
        }



        public void LockAuthenticatorByAlias(string alias)
        {
            Authenticator searchAuthenticator = GetAuthenticatorByAlias(alias);

            if (searchAuthenticator != null)
            {
                searchAuthenticator.Locked = true;
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Authenticator> repo = uow.GetRepository<Authenticator>();

                    repo.Put(searchAuthenticator);
                    uow.Commit();
                }
            }
            else
            {
                throw new ArgumentException("The authenticator with alias [" + alias + "] does not exist, cannot lock");
            }
        }

        public void UnlockAuthenticatorByAlias(string alias)
        {
            Authenticator searchAuthenticator = GetAuthenticatorByAlias(alias);

            if (searchAuthenticator != null)
            {
                searchAuthenticator.Locked = false;
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Authenticator> repo = uow.GetRepository<Authenticator>();

                    repo.Put(searchAuthenticator);
                    uow.Commit();
                }
            }
            else
            {
                throw new ArgumentException("The authenticator with alias [" + alias + "] does not exist, cannot lock");
            }
        }

        public IQueryable<Authenticator> GetAllAuthenticators()
        {
            return repo.Query();
        }
    }
}
