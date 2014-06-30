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

        //G
        IQueryable<DataPermission> GetAllDataPermissions();

        IQueryable<Entity> GetAllEntities();

        #endregion
    }
}
