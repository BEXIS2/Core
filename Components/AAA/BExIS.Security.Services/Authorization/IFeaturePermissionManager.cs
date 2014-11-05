using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;

namespace BExIS.Security.Services.Authorization
{
    public interface IFeaturePermissionManager
    {
        FeaturePermission CreateFeaturePermission(long subjectId, long featureId, PermissionType permissionType = PermissionType.Grant);

        bool DeleteFeaturePermissionById(long id);
        bool DeleteFeaturePermission(long subjectId, long featureId);

        bool ExistsFeaturePermissionId(long id);
        bool ExistsFeaturePermission(long subjectId, long featureId, PermissionType permissionType = PermissionType.Grant);
        bool ExistsFeaturePermission(IEnumerable<long> subjectIds, IEnumerable<long> featureIds, PermissionType permissionType = PermissionType.Grant);

        IQueryable<FeaturePermission> GetAllFeaturePermissions();

        FeaturePermission GetFeaturePermissionById(long id);
        FeaturePermission GetFeaturePermission(long subjectId, long featureId);

        int GetFeaturePermissionType(long subjectId, long featureId);

        bool HasSubjectFeatureAccess(long subjectId, long featureId);

        FeaturePermission UpdateFeaturePermission(FeaturePermission featurePermission);
    }
}
