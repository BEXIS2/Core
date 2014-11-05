using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Subjects;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Authentication
{
    public abstract class AuthenticationManager
    {
        public IReadOnlyRepository<User> UsersRepo { get; private set; }      

        public AuthenticationManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();

            this.UsersRepo = uow.GetReadOnlyRepository<User>();
        }

        public User CreateUser(User user)
        {
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
