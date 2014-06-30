using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;

namespace BExIS.Security.Services.Objects
{
    public interface IDataPermissionManager
    {
        #region Attributes

        #endregion

        #region Methods

        //C
        DataPermission CreateDataPermission();

        bool DeleteDataPermission();

        //G
        IQueryable<DataPermission> GetAllDataPermissions();

        IQueryable<DataPermission> GetDataPermissionsByEntityAndSubject(string entityName, long subjectId);

        IQueryable<Entity> GetAllEntities();
        IQueryable<Property> GetAllProperties(string entityName);

        Tuple<string, object[]> GetDataPermissionExpressionByEntityAndSubject(string entityName, long subjectId);

        #endregion
    }
}
