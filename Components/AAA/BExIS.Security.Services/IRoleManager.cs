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
        Role Create(string roleName, string description, string comment = "");

        // D
        bool Delete(Role role);

        // E
        bool ExistsRole(string roleName);

        // F
        IQueryable<User> FindUsersInRole(string roleName, string userNameToMatch);

        // G
        IQueryable<Role> GetAllRoles();

        Role GetRoleByName(string roleName);
        Role GetRoleById(Int64 id);

        IQueryable<Role> GetRolesFromUser(string userName);

        IQueryable<User> GetUsersFromRole(string roleName);

        // I
        bool IsUserInRole(string userName, string roleName);

        // R
        bool RemoveUserFromRole(User user, Role role);

        // U
        Role Update(Role role);
    }
}
