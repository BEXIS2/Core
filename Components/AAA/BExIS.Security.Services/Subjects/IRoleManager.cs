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
        int AddUserToRole(string userName, string roleName);
        int AddUserToRole(Int64 userId, Int64 roleId);

        // C
        Role CreateRole(string roleName, string description, out RoleCreateStatus status);

        // D
        bool DeleteRoleByName(string roleName);
        bool DeleteRoleById(Int64 id);

        // E
        bool ExistsRoleId(Int64 id);
        bool ExistsRoleName(string roleName);

        // F
        IQueryable<User> FindUsersInRole(string roleName, string userNameToMatch); 

        // G
        IQueryable<Role> GetAllRoles();

        Role GetRoleByName(string roleName);
        Role GetRoleById(Int64 id);

        string GetRoleNameById(Int64 id);

        IQueryable<Role> GetRolesFromUser(string userName);

        IQueryable<User> GetUsersFromRole(string roleName);

        // I
        bool IsRoleInUse(string roleName);

        bool IsUserInRole(string userName, string roleName);

        // R
        int RemoveUserFromRole(string userName, string roleName);
        int RemoveUserFromRole(Int64 userId, Int64 roleId);

        // U
        Role UpdateRole(Role role);

        #endregion
    }
}