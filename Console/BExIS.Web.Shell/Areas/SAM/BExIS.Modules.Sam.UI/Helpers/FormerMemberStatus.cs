using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.FormerMember;
using BExIS.Security.Services.Subjects;
using System.Collections.Generic;
using System.Linq;
using Vaiona.IoC;

namespace BExIS.Modules.SAM.UI.Helpers
{
    public static class FormerMemberStatus
    {
        /// <summary>
        ///Check if the user is the former member role.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="formerMemberRole"></param>
        /// <returns>True if user is in role.</returns>
        public static bool IsFormerMember(long userId, string formerMemberRole)
        {
            bool isAlumni = false;
            try
            {
                var groupManager = IoCFactory.Container.Resolve<GroupManager>();
                var alumniGroup = groupManager.Roles.Where(g => g.Name.ToLower() == formerMemberRole.ToLower()).FirstOrDefault();
                isAlumni = alumniGroup.Users.Any(u => u.Id == userId);
            }
            catch
            {
                // do nothing
            }
            finally
            {
                // do nothing
            }

            return isAlumni;
        }

        /// <summary>
        /// Change status of a user to the defined former member role.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="formerMemberRole"></param>
        /// <returns>True if status changed.</returns>
        public static bool ChangeToFormerMember(User user, string formerMemberRole)
        {
            bool statuschanged = false;
            //entity and feature permissions
            using (var alumniEntityPermissionManager = new FormerMemberEntityPermissionManager())
            using (var alumniFeaturePermissionManager = new FormerMemberFeaturePermissionManager())
            using (var featurePermissionManager = new FeaturePermissionManager())
            using (var alumniUsersGroupsRelationManager = new FormerMemberUsersGroupsRelationManager())
            {
                //get former member group
                var groupManager = IoCFactory.Container.Resolve<GroupManager>();
                var group = groupManager.FindByNameAsync(formerMemberRole).Result;
                if (!group.Users.Contains(user))
                {
                    //transfer all feature permission
                    var featurePermissions = featurePermissionManager.FeaturePermissionRepository.Get(a => a.Subject.Id == user.Id).ToList();
                    if (featurePermissions.Count > 0)
                    {
                        //Create for each feature permission a alumni feature permission
                        featurePermissions.ForEach(u => alumniFeaturePermissionManager.Create(user, u.Feature, u.PermissionType));

                        //Remove original feature permissions
                        for (int i = 0; i < featurePermissions.Count; i++)
                        {
                            var result_delete = featurePermissionManager.DeleteAsync(featurePermissions[i].Subject.Id, featurePermissions[i].Feature.Id).Result;
                        }
                    }

                    //remove all groups and add alumni

                    //remove all groups from user and add to alumniUsersGroupsRelation

                    List<Group> tempList = user.Groups.ToList();

                    var userManager = IoCFactory.Container.Resolve<UserManager>();

                    for (int i = 0; i < tempList.Count; i++)
                    {
                        alumniUsersGroupsRelationManager.Create(user.Id, tempList[i].Id);
                        var remove = userManager.RemoveFromRoleAsync(user.Id, tempList[i].Name).Result;
                    }

                    //add alumni
                    var result = userManager.AddToRoleAsync(user.Id, formerMemberRole).Result;

                    statuschanged = true;
                }
            }

            return statuschanged;
        }

        /// <summary>
        /// Remove former member role.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="formerMemberRole"></param>
        /// <returns>True if status changed.</returns>
        public static bool ChangeToNonFormerMember(User user, string formerMemberRole)
        {
            bool statuschanged = false;

            using (var alumniEntityPermissionManager = new FormerMemberEntityPermissionManager())
            using (var alumniFeaturePermissionManager = new FormerMemberFeaturePermissionManager())
            using (var featurePermissionManager = new FeaturePermissionManager())
            using (var alumniUsersGroupsRelationManager = new FormerMemberUsersGroupsRelationManager())
            {
                var groupManager = IoCFactory.Container.Resolve<GroupManager>();
                var group = groupManager.FindByNameAsync(formerMemberRole).Result;
                if (group.Users.Any(u => u.Id == user.Id))
                {
                    //transfer all feature permission
                    var alumniFeaturePermissions = alumniFeaturePermissionManager.FormerMemberFeaturePermissionRepository.Get(a => a.Subject.Id == user.Id).ToList();
                    if (alumniFeaturePermissions.Count > 0)
                    {
                        foreach (var permission in alumniFeaturePermissions)
                        {
                            var r = featurePermissionManager.CreateAsync(user, permission.Feature, permission.PermissionType).Result;
                        }

                        //remove
                        for (int i = 0; i < alumniFeaturePermissions.Count; i++)
                        {
                            alumniFeaturePermissionManager.Delete(alumniFeaturePermissions[i].Subject.Id, alumniFeaturePermissions[i].Feature.Id);
                        }
                    }

                    //add all groups to user again
                    var relations = alumniUsersGroupsRelationManager.FormerMemberFeaturePermissions.Where(r => r.UserRef == user.Id).ToList();

                    var userManager = IoCFactory.Container.Resolve<UserManager>();

                    foreach (var r in relations)
                    {
                        //add all group to user again
                        var g = groupManager.FindByIdAsync(r.GroupRef).Result;
                        var add = userManager.AddToRoleAsync(user.Id, g.Name).Result;

                        //delete relation
                        alumniUsersGroupsRelationManager.Delete(r);
                    }

                    //remove alumni group
                    var result = userManager.RemoveFromRoleAsync(user.Id, formerMemberRole).Result;

                    statuschanged = true;
                }
            }

            return statuschanged;
        }
    }
}