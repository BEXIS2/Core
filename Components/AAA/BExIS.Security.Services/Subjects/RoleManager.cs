using System;
using System.Diagnostics.Contracts;
using System.Linq;
using BExIS.Security.Entities.Subjects;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Subjects
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


        #region Attributes

        #endregion


        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public bool AddUserToRole(User user, Role role)
        {
            bool result = false;

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Role> repo = uow.GetRepository<Role>();

                role = repo.Reload(role);
                repo.LoadIfNot(role.Users);
                if (!role.Users.Contains(user))
                {
                    role.Users.Add(user);
                    user.Roles.Add(role);
                    uow.Commit();
                    result = true;
                }
            }

            return (result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="description"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public Role Create(string roleName, string description, out RoleCreateStatus status)
        {
            if(ExistsRoleName(roleName))
            {
                status = RoleCreateStatus.DuplicateRoleName;
                return null;
            }

            Role role = new Role()
            {
                // Subject Properties
                Name = roleName,

                // Role Properties
                Description = description
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Role> rolesRepo = uow.GetRepository<Role>();
                rolesRepo.Put(role);
                uow.Commit();
            }

            status = RoleCreateStatus.Success;
            return (role);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public bool Delete(Role role)
        {
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public bool ExistsRoleName(string roleName)
        {
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleName"></param>
        /// <param name="userNameToMatch"></param>
        /// <returns></returns>
        public IQueryable<User> FindUsersInRole(Role role, string userNameToMatch)
        {
            Contract.Requires(role != null);

            role = RolesRepo.Reload(role);
            RolesRepo.LoadIfNot(role.Users);

            return role.Users.Where(u => u.Name.Contains(userNameToMatch)).AsQueryable<User>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<Role> GetAllRoles()
        {
            return (RolesRepo.Query());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Role GetRoleById(long id)
        {
            Contract.Requires(id >= 0);

            if (RolesRepo.Get(r => r.Id == id).Count() == 1)
            {
                return RolesRepo.Get(r => r.Id == id).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public Role GetRoleByName(string roleName)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(roleName));

            if (RolesRepo.Get(r => r.Name == roleName).Count() == 1)
            {
                return RolesRepo.Get(r => r.Name == roleName).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public IQueryable<Role> GetRolesFromUser(User user)
        {
            Contract.Requires(user != null);

            user = UsersRepo.Reload(user);
            UsersRepo.LoadIfNot(user.Roles);

            return user.Roles.AsQueryable<Role>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public IQueryable<User> GetUsersFromRole(Role role)
        {
            Contract.Requires(role != null);

            role = RolesRepo.Reload(role);
            RolesRepo.LoadIfNot(role.Users);

            return role.Users.AsQueryable<User>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public bool IsUserInRole(User user, Role role)
        {
            Contract.Requires(role != null);
            Contract.Requires(user != null);

            role = RolesRepo.Reload(role);
            RolesRepo.LoadIfNot(role.Users);

            if (role.Users.Contains(user))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        public bool RemoveUserFromRole(User user, Role role)
        {
            Contract.Requires(user != null && user.Id >= 0);
            Contract.Requires(role != null && role.Id >= 0);

            // Variables
            bool result = false;

            // Computations
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Role> rolesRepo = uow.GetRepository<Role>();
                IRepository<User> usersRepo = uow.GetRepository<User>();

                role = rolesRepo.Reload(role);
                rolesRepo.LoadIfNot(role.Users);

                user = usersRepo.Reload(user);
                usersRepo.LoadIfNot(user.Roles);

                if (role.Users.Contains(user))
                {
                    role.Users.Remove(user);
                    user.Roles.Remove(role);
                    uow.Commit();
                    result = true;
                }
            }
            return (result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public Role Update(Role role)
        {
            Contract.Requires(role != null);

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