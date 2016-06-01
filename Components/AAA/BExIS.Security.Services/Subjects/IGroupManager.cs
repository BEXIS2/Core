using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Subjects;

namespace BExIS.Security.Services.Subjects
{
    public interface IGroupManager
    {
        bool AddUserToGroup(string username, string groupName);

        bool AddUserToGroup(long userId, long groupId);

        Group CreateGroup(string groupName, string description, bool isSystemSubject = false);

        bool DeleteGroupByName(string groupName);

        bool DeleteGroupById(long id);

        bool ExistsGroupId(long id);

        bool ExistsGroupName(string groupName);

        IQueryable<Group> GetAllGroups();

        Group GetGroupByName(string groupName);

        Group GetGroupById(long id);

        string GetGroupNameById(long id);

        IQueryable<Group> GetGroupsFromUsername(string username);

        IQueryable<User> GetUsersFromGroupName(string groupName);

        IQueryable<User> GetUsersFromGroupId(long groupId);

        bool IsGroupInUse(string groupName);

        bool IsUserInGroup(string username, string groupName);

        bool IsUserInGroup(long userId, long groupId);

        bool RemoveUserFromGroup(string username, string groupName);

        bool RemoveUserFromGroup(long userId, long groupId);

        Group UpdateGroup(Group group);
    }
}
