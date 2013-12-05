using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Security;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;

namespace BExIS.Security.Services.Security
{
    public sealed class SecurityService : ISecurityService
    {
        public bool HasDataAccess(User user, DataContext dataContext)
        {
            throw new UnauthorizedAccessException();
        }

        public bool HasTaskAccess(User user, TaskContext taskContext)
        {
            TaskManager taskManager = new TaskManager();
            Task task = taskManager.GetTaskByContext(taskContext);

            if (task != null && task.Feature != null)
            {
                FeaturePermissionManager featurePermissionManager = new FeaturePermissionManager();
                
                // User Check
                List<FeaturePermission> featurePermissions = new List<FeaturePermission>();
                featurePermissions.AddRange(featurePermissionManager.GetFeaturePermissionsFromUser(user).Where(p => p.Feature == task.Feature).ToList<FeaturePermission>());

                
                // Roles Check
                RoleManager roleManager = new RoleManager();
                List<Role> roles = roleManager.GetRolesFromUser(user).ToList<Role>();
                featurePermissions.AddRange(featurePermissionManager.GetFeaturePermissionsFromRoles(roles).ToList<FeaturePermission>());

                // Permissions Check
                if (featurePermissions.Count == 0 || featurePermissions.Count(p => p.PermissionType == PermissionType.Deny) > 0)
                {
                    throw new UnauthorizedAccessException();
                }
                else
                {
                    return true;
                }
            }
            else
            {
                throw new UnauthorizedAccessException();
            }           
        }
    }
}
