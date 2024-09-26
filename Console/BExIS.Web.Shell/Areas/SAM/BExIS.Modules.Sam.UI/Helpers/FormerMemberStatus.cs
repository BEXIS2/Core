using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.FormerMember;
using BExIS.Security.Services.Subjects;
using System.Collections.Generic;
using System.Linq;

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
                using (GroupManager groupManager = new GroupManager())
                {
                    var alumniGroup = groupManager.Groups.Where(g => g.Name.ToLower() == formerMemberRole.ToLower()).FirstOrDefault();
                    isAlumni = alumniGroup.Users.Any(u => u.Id == userId);
                }
            }
            catch
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
            using (var groupManager = new GroupManager())
            using (var alumniUsersGroupsRelationManager = new FormerMemberUsersGroupsRelationManager())
            {
                //get former meber group
                var group = groupManager.FindByNameAsync(formerMemberRole).Result;
                if (!group.Users.Contains(user))
                {
                    //transfer all feature permission
                    var featurePermissions = featurePermissionManager.FeaturePermissionRepository.Get(a => a.Subject.Id == user.Id).ToList();
                    if (featurePermissions.Count > 0)
                    {
                        //Create for each feature permission a alumni feature permission
                        featurePermissions.ForEach(u => alumniFeaturePermissionManager.Create(user, u.Feature, u.PermissionType));

                        //Remove orginal feature permissions
                        for (int i = 0; i < featurePermissions.Count; i++)
                        {
                            var result_delete = featurePermissionManager.DeleteAsync(featurePermissions[i].Subject.Id, featurePermissions[i].Feature.Id).Result;
                        }
                    }

                    //remove all groups and add alumni

                    //remove all groups from user and add to alumniUsersGroupsRelation

                    List<Group> tempList = user.Groups.ToList();

                    using (var identityUserService = new IdentityUserService())
                    {
                        for (int i = 0; i < tempList.Count; i++)
                        {
                            alumniUsersGroupsRelationManager.Create(user.Id, tempList[i].Id);
                            var remove = identityUserService.RemoveFromRoleAsync(user.Id, tempList[i].Name).Result;
                        }

                        //add alumni
                        var result = identityUserService.AddToRoleAsync(user.Id, formerMemberRole).Result;

                        statuschanged = true;
                    }
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
            using (var groupManager = new GroupManager())
            using (var alumniUsersGroupsRelationManager = new FormerMemberUsersGroupsRelationManager())
            {
                var group = groupManager.FindByNameAsync(formerMemberRole).Result;
                if (group.Users.Contains(user))
                {
                    //transfer all feature permission
                    var alumniFeaturePermissions = alumniFeaturePermissionManager.FormerMemberFeaturePermissionRepository.Get(a => a.Subject.Id == user.Id).ToList();
                    if (alumniFeaturePermissions.Count > 0)
                    {
                        alumniFeaturePermissions.ForEach(async u => await featurePermissionManager.CreateAsync(user, u.Feature, u.PermissionType));
                        //remove
                        for (int i = 0; i < alumniFeaturePermissions.Count; i++)
                        {
                            alumniFeaturePermissionManager.Delete(alumniFeaturePermissions[i].Subject.Id, alumniFeaturePermissions[i].Feature.Id);
                        }
                    }

                    //add all groups to user again
                    var relations = alumniUsersGroupsRelationManager.FormerMemberFeaturePermissions.Where(r => r.UserRef == user.Id).ToList();
                    using (var identityUserService = new IdentityUserService())
                    {
                        foreach (var r in relations)
                        {
                            //add all group to user again
                            var g = groupManager.FindByIdAsync(r.GroupRef).Result;
                            var add = identityUserService.AddToRoleAsync(user.Id, g.Name).Result;

                            //delete relation
                            alumniUsersGroupsRelationManager.Delete(r);
                        }

                        //remove alumni group
                        var result = identityUserService.RemoveFromRoleAsync(user.Id, formerMemberRole).Result;
                    }

                    statuschanged = true;
                }
            }

            return statuschanged;
        }
    }
}