using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities;
using BExIS.Core.Persistence.Api;

namespace BExIS.Security.Services
{
    public sealed class UserManager
    {
        public UserManager()
        {
            this.Repo = this.GetUnitOfWork().GetReadOnlyRepository<User>();
        }

        #region Data Readers

        public IReadOnlyRepository<User> Repo { get; private set; }

        #endregion

        #region User

        // C
        public User Create(string name)
        {
            // TODO: SECURITY CHECK

            User u = new User()
            {
                Name = name,
                LowerCaseName = name.ToLower(),

                LastActivityDate = DateTime.Now,
                LastLockOutDate = DateTime.Now,
                LastLoginDate = DateTime.Now,
                LastPasswordChangeDate = DateTime.Now,
                LastPasswordFailureDate = DateTime.Now,
                RegistrationDate = DateTime.Now,
                IsLockedOut = false
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<User> repo = uow.GetRepository<User>();
                repo.Put(u);
                uow.Commit();
            }
            return (u);
        }

        // D
        public bool Delete(User entity)
        {
            // TODO: SECURITY CHECK

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<User> repo = uow.GetRepository<User>();

                entity = repo.Reload(entity);
                repo.Delete(entity);
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }
        public bool Delete(IEnumerable<User> entities)
        {
            // TODO: SECURITY CHECK

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<User> repo = uow.GetRepository<User>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        // U
        public User Update(User entity)
        {
            // TODO: SECURITY CHECK

            using (IUnitOfWork uow = entity.GetUnitOfWork())
            {
                IRepository<User> repo = uow.GetRepository<User>();
                repo.Put(entity);
                uow.Commit();
            }
            return (entity);
        }

        #endregion

        #region Associations

        #endregion
    }
}
