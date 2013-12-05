using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Security;
using BExIS.Security.Entities.Subjects;

namespace BExIS.Security.Services.Security
{
    public interface IFeaturePermissionManager
    {
        // C
        FeaturePermission Create(Subject subject, Feature feature, PermissionType permissionType, out PermissionCreateStatus status);

        // D
        bool Delete(FeaturePermission featurePermission);

        // G
        IQueryable<FeaturePermission> GetAllFeaturePermissions();

        IQueryable<FeaturePermission> GetFeaturePermissionsFromUser(User user);
        IQueryable<FeaturePermission> GetFeaturePermissionsFromRole(Role role);
        IQueryable<FeaturePermission> GetFeaturePermissionsFromRoles(List<Role> roles);

        // U
        FeaturePermission Update(FeaturePermission featurePermission);

    }
}
