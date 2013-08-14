using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BExIS.Security.Entities;

namespace BExIS.Security.Api
{
    public interface IRoleManager
    {
        // A
        bool AddUserToRole(string userName, string roleName);
        bool AddUserToRole(User user, Role role);

        // C
        Role Create(string roleName, string description, string comment = "");

        // D
        bool Delete(string roleName);
        bool Delete(Role role);

        // E
        bool ExistsRole(string roleName);

        // F
        IEnumerable<User> FindUsersInRole(string roleName, string userNameToMatch, int pageIndex, int pageSize);

        // G
        IEnumerable<Role> GetAllRoles(int pageIndex, int pageSize);

        Role GetRoleByName(string roleName);
        Role GetRoleById(Int64 id);

        IEnumerable<Role> GetRoles(int pageIndex, int pageSize, Expression<Func<Role, bool>> expression);

        IEnumerable<Role> GetRolesFromUser(string userName, int pageIndex, int pageSize);

        IEnumerable<User> GetUsersFromRole(string roleName, int pageIndex, int pageSize);

        // I
        bool IsUserInRole(string userName, string roleName);

        // R
        bool RemoveUserFromRole(string userName, string roleName);
        bool RemoveUserFromRole(User user, Role role);

        // U
        Role Update(Role role);
    }
}
