using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Authentication;
using BExIS.Security.Entities.Subjects;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Providers.Authentication
{
    public abstract class AuthenticationProvider
    {
        public AuthenticationProvider()
        {
            IUnitOfWork uow = this.GetUnitOfWork();

            this.UsersRepo = uow.GetReadOnlyRepository<User>();
            this.AuthenticatorsRepo = uow.GetReadOnlyRepository<Authenticator>();
        }

        public IReadOnlyRepository<User> UsersRepo { get; private set; }
        public IReadOnlyRepository<Authenticator> AuthenticatorsRepo { get; private set; }

        public User CreateUser(string userName, long providerId)
        {
            User user = new User()
            {
                Name = userName,
                Authenticator = AuthenticatorsRepo.Get(providerId)
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<User> usersRepo = uow.GetRepository<User>();
                usersRepo.Put(user);

                uow.Commit();
            }

            return (user);
        }
    }
}
