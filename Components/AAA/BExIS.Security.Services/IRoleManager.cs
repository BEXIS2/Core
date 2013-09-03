using System;
using System.Linq;
using BExIS.Security.Entities;

namespace BExIS.Security.Services
{
    public interface IRoleManager
    {
        // A
        bool AddUserToRole(User user, Role role);

        // C
        Role Create(string roleName, string description, string comment, out RoleCreateStatus status);

        // D
        bool Delete(Role role);

        // E
        bool ExistsRoleName(string roleName);

        // F
        IQueryable<User> FindUsersInRole(string roleName, string userNameToMatch);

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
    }
}