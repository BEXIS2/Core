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
        int AddUserToRole(long userId, long roleId);

        // C
        Role CreateRole(string roleName, string description, out RoleCreateStatus status);

        // D
        bool DeleteRoleByName(string roleName);
        bool DeleteRoleById(long id);

        // E
        bool ExistsRoleId(long id);
        bool ExistsRoleName(string roleName);

        // F
        IQueryable<User> FindUsersInRole(string roleName, string userNameToMatch); 

        // G
        IQueryable<Role> GetAllRoles();

        Role GetRoleByName(string roleName);
        Role GetRoleById(long id);

        string GetRoleNameById(long id);

        IQueryable<Role> GetRolesFromUser(string userName);

        IQueryable<User> GetUsersFromRole(string roleName);

        // I
        bool IsRoleInUse(string roleName);

        bool IsUserInRole(string userName, string roleName);

        // R
        int RemoveUserFromRole(string userName, string roleName);
        int RemoveUserFromRole(long userId, long roleId);

        // U
        Role UpdateRole(Role role);

        #endregion
    }

    public enum RoleCreateStatus
    {
        Success,
        DuplicateRoleName,
        InvalidRoleName
    }
}