using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities;
using BExIS.Core.Persistence.Api;
using System.Diagnostics.Contracts;

namespace BExIS.Security.Services
{
    public sealed class RoleManager
    {
        public RoleManager()
        {
            this.Repo = this.GetUnitOfWork().GetReadOnlyRepository<Role>();
        }

        #region Data Readers

        public IReadOnlyRepository<Role> Repo { get; private set; }

        #endregion

        #region Role

        // A
        public bool AddUserToRole(User user, Role role)
        {
            Contract.Requires(user != null && user.Id >= 0);
            Contract.Requires(role != null && role.Id >= 0);

            bool result = false;
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Role> repo = uow.GetRepository<Role>();

                role = repo.Reload(role);
                repo.LoadIfNot(role.Users);
                if (!role.Users.Contains(user))
                {
                    user.Roles.Add(role);
                    role.Users.Add(user);
                    uow.Commit();
                    result = true;
                }
            }
            return (result);
        }

        // C
        public Role Create(string name, string description, string comment)
        {
            // TODO: SECURITY CHECK

            Role r = new Role()
            {
                Name = name,
                LowerCaseName = name.ToLower(),
                Description = description,
                Comment = comment
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Role> repo = uow.GetRepository<Role>();
                repo.Put(r);
                uow.Commit();
            }
            return (r);
        }

        // D
        public bool Delete(Role entity)
        {
            // TODO: SECURITY CHECK

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Role> repo = uow.GetRepository<Role>();

                entity = repo.Reload(entity);
                repo.Delete(entity);
                uow.Commit();
            }
            // if any problem was detected during the commit, an exception will be thrown!
            return (true);
        }
        public bool Delete(IEnumerable<Role> entities)
        {
            // TODO: SECURITY CHECK

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Role> repo = uow.GetRepository<Role>();

                foreach (var entity in entities)
                {
                    var latest = repo.Reload(entity);
                    repo.Delete(latest);
                }
                uow.Commit();
            }
            return (true);
        }

        // R
        public bool RemoveUserFromRole(User user, Role role)
        {
            Contract.Requires(user != null && user.Id >= 0);
            Contract.Requires(role != null && role.Id >= 0);

            bool result = false;
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Role> rRepo = uow.GetRepository<Role>();
                IRepository<User> uRepo = uow.GetRepository<User>();

                role = rRepo.Reload(role);
                rRepo.LoadIfNot(role.Users);

                user = uRepo.Reload(user);
                uRepo.LoadIfNot(user.Roles);

                if (role.Users.Contains(user) || user.Roles.Contains(role))
                {
                    user.Roles.Remove(role);
                    role.Users.Remove(user);
                    uow.Commit();
                    result = true;
                }
            }
            return (result);
        }

        // U
        public Role Update(Role entity)
        {
            // TODO: SECURITY CHECK

            using (IUnitOfWork uow = entity.GetUnitOfWork())
            {
                IRepository<Role> repo = uow.GetRepository<Role>();
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
