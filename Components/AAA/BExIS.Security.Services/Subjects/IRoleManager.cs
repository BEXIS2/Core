using System;
using System.Linq;
using BExIS.Security.Entities.Subjects;

/// <summary>
///
/// </summary>        
namespace BExIS.Security.Services.Subjects
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public interface IRoleManager
    {
        //#region Attributes

        //#endregion


        //#region Methods

        //// A

        ///// <summary>
        /////
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param name="roleName"></param>
        ///// <param name="userName"></param>
        //int AddUserToRole(string userName, string roleName);

        ///// <summary>
        /////
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param name="userId"></param>
        ///// <param name="roleId"></param>
        //int AddUserToRole(long userId, long roleId);

        //// C

        ///// <summary>
        /////
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param name="roleName"></param>
        ///// <param name="description"></param>
        ///// <param name="status"></param>
        //Role CreateRole(string roleName, string description, out RoleCreateStatus status);

        //// D

        ///// <summary>
        /////
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param name="roleName"></param>
        //bool DeleteRoleByName(string roleName);

        ///// <summary>
        /////
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param name="id"></param>
        //bool DeleteRoleById(long id);

        //// E

        ///// <summary>
        /////
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param name="id"></param>
        //bool ExistsRoleId(long id);

        ///// <summary>
        /////
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param name="roleName"></param>
        //bool ExistsRoleName(string roleName);

        //// F

        ///// <summary>
        /////
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param name="roleName"></param>
        ///// <param name="userNameToMatch"></param>
        //IQueryable<User> FindUsersInRole(string roleName, string userNameToMatch); 

        //// G

        ///// <summary>
        /////
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param>NA</param>       
        //IQueryable<Role> GetAllRoles();

        ///// <summary>
        /////
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param name="roleName"></param>
        //Role GetRoleByName(string roleName);

        ///// <summary>
        /////
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param name="id"></param>
        //Role GetRoleById(long id);

        ///// <summary>
        /////
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param name="id"></param>
        //string GetRoleNameById(long id);

        ///// <summary>
        /////
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param name="userName"></param>
        //IQueryable<Role> GetRolesFromUser(string userName);

        ///// <summary>
        /////
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param name="roleName"></param>
        //IQueryable<User> GetUsersFromRole(string roleName);

        //// I

        ///// <summary>
        /////
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param name="roleName"></param>
        //bool IsRoleInUse(string roleName);

        ///// <summary>
        /////
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param name="userName"></param>
        ///// <param name="roleName"></param>
        //bool IsUserInRole(string userName, string roleName);

        //// R

        ///// <summary>
        /////
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param name="userName"></param>
        ///// <param name="roleName"></param>
        //int RemoveUserFromRole(string userName, string roleName);

        ///// <summary>
        /////
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param name="userId"></param>
        ///// <param name="roleId"></param>
        //int RemoveUserFromRole(long userId, long roleId);

        //// U

        ///// <summary>
        /////
        ///// </summary>
        ///// <remarks></remarks>
        ///// <seealso cref=""/>
        ///// <param name="role"></param>
        //Role UpdateRole(Role role);

        //#endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public enum RoleCreateStatus
    {
        Success,
        DuplicateRoleName,
        InvalidRoleName
    }
}