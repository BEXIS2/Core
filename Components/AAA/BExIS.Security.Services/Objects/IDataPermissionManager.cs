using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;

/// <summary>
///
/// </summary>        
namespace BExIS.Security.Services.Objects
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public interface IDataPermissionManager
    {
        #region Attributes

        #endregion

        #region Methods

        // C

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataId"></param>
        /// <param name="entityId"></param>
        /// <param name="subjectId"></param>
        /// <param name="rightType"></param>
        int CreateDataPermission(long dataId, string entityId, long subjectId, RightType rightType);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataId"></param>
        /// <param name="entityId"></param>
        /// <param name="userName"></param>
        /// <param name="rightType"></param>
        int CreateDataPermission(long dataId, string entityId, string userName, RightType rightType);

        // D

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataId"></param>
        /// <param name="entityId"></param>
        /// <param name="subjectId"></param>
        /// <param name="rightType"></param>
        int DeleteDataPermission(long dataId, string entityId, long subjectId, RightType rightType);

        // E

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataId"></param>
        /// <param name="entityId"></param>
        /// <param name="subjectId"></param>
        /// <param name="rightType"></param>
        bool ExistsDataPermission(long dataId, string entityId, long subjectId, RightType rightType);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataId"></param>
        /// <param name="entityId"></param>
        /// <param name="subjectIds"></param>
        /// <param name="rightType"></param>
        bool ExistsDataPermission(long dataId, string entityId, IEnumerable<long> subjectIds, RightType rightType);

        // G

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        IQueryable<Entity> GetAllEntities();

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="entityId"></param>
        string GetAreaFromEntity(string entityId);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="entityId"></param>
        string GetControllerFromEntity(string entityId);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="entityId"></param>
        string GetActionFromEntity(string entityId);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataId"></param>
        /// <param name="entityId"></param>
        /// <param name="subjectId"></param>
        /// <param name="rightType"></param>
        DataPermission GetDataPermission(long dataId, string entityId, long subjectId, RightType rightType);

        ICollection<long> GetDataIds(string entityId, string userName, RightType rightType);

        // U

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataId"></param>
        /// <param name="entityId"></param>
        /// <param name="userName"></param>
        /// <param name="rightType"></param>
        bool HasUserDataAccess(long dataId, string entityId, string userName, RightType rightType);
        #endregion
    }
}
