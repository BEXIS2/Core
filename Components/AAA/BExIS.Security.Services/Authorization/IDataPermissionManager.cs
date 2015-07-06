using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;

namespace BExIS.Security.Services.Authorization
{
    public interface IDataPermissionManager
    {
        DataPermission CreateDataPermission(long subjectId, long entityId, long dataId, RightType rightType, PermissionType permissionType = PermissionType.Grant);

        bool DeleteDataPermissionById(long id);
        bool DeleteDataPermission(long subjectId, long entityId, long dataId, RightType rightType);

        bool DeleteDataPermissionsByEntity(long entityId, long dataId);

        bool ExistsDataPermissionId(long id);
        bool ExistsDataPermission(long subjectId, long entityId, long dataId, RightType rightType);
        bool ExistsDataPermission(IEnumerable<long> subjectIds, long entityId, long dataId, RightType rightType);

        IQueryable<long> GetAllDataIds(long userId, long entityId, RightType rightType);
        IQueryable<long> GetAllDataIds(IEnumerable<long> subjectIds, long entityId, RightType rightType);
        IQueryable<DataPermission> GetAllDataPermissions();
        IQueryable<int> GetAllRights(long subjectId, long entityId, long dataId);
        
        DataPermission GetDataPermissionById(long id);
        DataPermission GetDataPermission(long subjectId, long entityId, long dataId, RightType rightType);
        IQueryable<DataPermission> GetDataPermissionsFromEntity(long entityId, long dataId);

        bool HasUserDataAccess(long userId, long entityId, long dataId, RightType rightType); 

        DataPermission UpdateDataPermission(DataPermission dataPermission);
    }
}
