using System;
using System.Diagnostics.Contracts;
using System.Linq;
using BExIS.Security.Entities;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services
{
    public sealed class RoleManager : IRoleManager
    {
        public RoleManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();

            this.RolesRepo = uow.GetReadOnlyRepository<Role>();
            this.UsersRepo = uow.GetReadOnlyRepository<User>();
        }

        #region Data Readers

        public IReadOnlyRepository<Role> RolesRepo { get; private set; }
        public IReadOnlyRepository<User> UsersRepo { get; private set; }

        #endregion

        #region Role

        //
        public bool AddUserToRole(User user, Role role)
        {
            // Requirements
            Contract.Requires(user != null && user.Id >= 0);
            Contract.Requires(role != null && role.Id >= 0);

            // Variables
            bool result = false;

            // Computations
            if (!IsUserInRole(user.Name, role.Name))
            {
                role = RolesRepo.Reload(role);
                RolesRepo.LoadIfNot(role.Users);

                role.Users.Add(user);
                user.Roles.Add(role);

                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Role> repo = uow.GetRepository<Role>();

                    repo.Put(role);
                    uow.Commit();

                    result = true;
                }
            }

            return (result);
        }

        //
        public Role Create(string roleName, string description, string comment = "")
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(roleName));
            Contract.Requires(!String.IsNullOrWhiteSpace(description));
            Contract.Requires(comment != null);

            // Computations
            if (!ExistsRole(roleName))
            {
                Role role = new Role()
                {
                    // Subject Properties
                    Name = roleName,

                    Comment = comment,

                    // Role Properties
                    Description = description
                };

                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Role> repo = uow.GetRepository<Role>();
                    repo.Put(role);
                    uow.Commit();
                }

                return (role);  
            }
            else
            {
                return null;
            }

        }

        //
        public bool Delete(Role role)
        {
            // Requirements
            Contract.Requires(role != null);

            // Computations
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Role> repo = uow.GetRepository<Role>();

                role = repo.Reload(role);
                repo.Delete(role);
                uow.Commit();
            }

            return (true);
        }

        //
        public bool ExistsRole(string roleName)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(roleName));

            if (GetRoleByName(roleName) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //
        public IQueryable<User> FindUsersInRole(string roleName, string userNameToMatch)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(roleName));
            Contract.Requires(!String.IsNullOrWhiteSpace(userNameToMatch));

            // Variables
            Role role = GetRoleByName(roleName);

            if (role != null)
            {
                return role.Users.Where(u => u.Name.Contains(userNameToMatch)).AsQueryable<User>();
            }
            else
            {
                return null;
            }
        }

        //
        public IQueryable<Role> GetAllRoles()
        {
            return (RolesRepo.Query());
        }

        //
        public Role GetRoleById(long id)
        {
            // Requirements
            Contract.Requires(id >= 0);

            // Compuations
            if (RolesRepo.Get(r => r.Id == id).Count() == 1)
            {
                return RolesRepo.Get(r => r.Id == id).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        //
        public Role GetRoleByName(string roleName)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(roleName));

            // Computations
            if (RolesRepo.Get(r => r.Name == roleName).Count() == 1)
            {
                return RolesRepo.Get(r => r.Name == roleName).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        //
        public IQueryable<Role> GetRolesFromUser(string userName)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(userName));

            // Variables
            if (UsersRepo.Get(u => u.Name == userName).Count() == 1)
            {
                return (UsersRepo.Get(u => u.Name == userName).FirstOrDefault().Roles.AsQueryable<Role>());
            }
            else
            {
                return null;
            }
        }

        //
        public IQueryable<User> GetUsersFromRole(string roleName)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(roleName));

            // Variables
            if (RolesRepo.Get(r => r.Name == roleName).Count() == 1)
            {
                return (RolesRepo.Get(r => r.Name == roleName).FirstOrDefault().Users.AsQueryable<User>());
            }
            else
            {
                return null;
            }
        }

        //
        public bool IsUserInRole(string userName, string roleName)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(userName));
            Contract.Requires(!String.IsNullOrWhiteSpace(roleName));

            // Variables
            Role role = GetRoleByName(roleName);

            // Computations
            if (role != null)
            {
                if (role.Users.Count(u => u.Name == userName) == 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        //
        public bool RemoveUserFromRole(User user, Role role)
        {
            // Requirements
            Contract.Requires(user != null && user.Id >= 0);
            Contract.Requires(role != null && role.Id >= 0);

            // Variables
            bool result = false;

            // Computations
            if (IsUserInRole(user.Name, role.Name))
            {
                role = RolesRepo.Reload(role);
                RolesRepo.LoadIfNot(role.Users);

                role.Users.Remove(user);
                user.Roles.Remove(role);

                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Role> repo = uow.GetRepository<Role>();
                    repo.Put(role);
                    uow.Commit();

                    result = true;
                }
            }

            return (result);
        }

        //
        public Role Update(Role role)
        {
            // Requirements
            Contract.Requires(role != null);

            // Computations
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Role> repo = uow.GetRepository<Role>();
                repo.Put(role);
                uow.Commit();
            }
            return (role);
        }

        #endregion
    }
}
