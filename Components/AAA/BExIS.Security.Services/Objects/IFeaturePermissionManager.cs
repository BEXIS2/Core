using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;

namespace BExIS.Security.Services.Objects
{
    public interface IFeaturePermissionManager
    {
        #region Attributes

        bool DefaultFeaturePermission { get; }

        bool DrawFeaturePermission { get; }

        bool FeaturePermissionHierarchy { get; }

        int FeaturePermissionPolicy { get; }

        #endregion


        #region Methods

        // C
        bool CheckFeatureAccessForUser(string userName, string areaName, string controllerName, string actionName);


        FeaturePermission CreateFeaturePermission(long featureId, long subjectId, PermissionType permissionType, out FeaturePermissionCreateStatus status);

        // D
        bool DeleteFeaturePermissionById(long id);
        bool DeleteFeaturePermissionByFeatureAndSubject(long featureId, long subjectId);

        // E
        bool ExistsFeaturePermission(long featureId, long subjectId);

        // G
        FeaturePermission GetFeaturePermissionById(long id);
        FeaturePermission GetFeaturePermissionByFeatureAndSubject(long featureId, long subjectId);

        IQueryable<FeaturePermission> GetAllFeaturePermissions();

        IQueryable<FeaturePermission> GetFeaturePermissionsFromFeature(long featureId);
        IQueryable<FeaturePermission> GetFeaturePermissionsFromSubject(long subjectId);

        int GetFeaturePermissionType(long featureId, long subjectId);

        // U
        FeaturePermission UpdateFeaturePermission(FeaturePermission featurePermission);

        #endregion
    }

    public enum FeaturePermissionCreateStatus
    {
        DuplicateFeaturePermission,

        InvalidSubject,
        InvalidFeature,

        Success
    }
}
