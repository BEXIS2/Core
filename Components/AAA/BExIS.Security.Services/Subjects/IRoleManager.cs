using System;
using System.Linq;
using BExIS.Security.Entities.Subjects;

namespace BExIS.Security.Services.Subjects
{
    public interface IRoleManager
    {
        #region Attributes

        #endregion


        #region Methods

        // A
        bool AddUserToRole(User user, Role role);

        // C
        Role Create(string roleName, string description, out RoleCreateStatus status);

        // D
        bool Delete(Role role);

        // E
        bool ExistsRoleName(string roleName);

        // F
        IQueryable<User> FindUsersInRole(Role role, string userNameToMatch); 

        // G
        IQueryable<Role> GetAllRoles();

        Role GetRoleByName(string roleName);
        Role GetRoleById(Int64 id);

        IQueryable<Role> GetRolesFromUser(User user);

        IQueryable<User> GetUsersFromRole(Role role);

        // I
        bool IsUserInRole(User user, Role role);

        // R
        bool RemoveUserFromRole(User user, Role role);

        // U
        Role Update(Role role);

        #endregion
    }
}