using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Subjects;

namespace BExIS.Security.Services.Subjects
{
    public interface IGroupManager
    {
        // A
        bool AddUserToGroup(string userName, string groupName);

        bool AddUserToGroup(long userId, long groupId);

        // C
        Group CreateGroup(string groupName, string description);

        // D
        bool DeleteGroupByName(string groupName);

        bool DeleteGroupById(long id);

        // E
        bool ExistsGroupId(long id);

        bool ExistsGroupName(string groupName);

        // F
        IQueryable<User> FindUsersInGroup(string groupName, string userNameToMatch);

        // G   
        IQueryable<Group> GetAllGroups();

        Group GetGroupByName(string groupName);

        Group GetGroupById(long id);

        string GetGroupNameById(long id);

        IQueryable<Group> GetGroupsFromUser(string userName);

        IQueryable<User> GetUsersFromGroup(string groupName);

        // I
        bool IsGroupInUse(string groupName);

        bool IsUserInGroup(string userName, string groupName);

        // R
        bool RemoveUserFromGroup(string userName, string groupName);

        bool RemoveUserFromGroup(long userId, long groupId);

        // U
        Group UpdateGroup(Group group);
    }
}
