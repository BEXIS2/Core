using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Security;

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
        int CreateDataPermission(long dataId, long entityId, long subjectId, RightType rightType);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataId"></param>
        /// <param name="entityId"></param>
        /// <param name="userName"></param>
        /// <param name="rightType"></param>
        int CreateDataPermission(long dataId, long entityId, string userName, RightType rightType);

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
        int DeleteDataPermission(long dataId, long entityId, long subjectId, RightType rightType);

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
        bool ExistsDataPermission(long dataId, long entityId, long subjectId, RightType rightType);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataId"></param>
        /// <param name="entityId"></param>
        /// <param name="subjectIds"></param>
        /// <param name="rightType"></param>
        bool ExistsDataPermission(long dataId, long entityId, IEnumerable<long> subjectIds, RightType rightType);

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
        /// <param name="dataId"></param>
        /// <param name="entityId"></param>
        /// <param name="subjectId"></param>
        /// <param name="rightType"></param>
        DataPermission GetDataPermission(long dataId, long entityId, long subjectId, RightType rightType);

        ICollection<long> GetDataIds(long entityId, string userName, RightType rightType);

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
        bool HasUserDataAccess(long dataId, long entityId, string userName, RightType rightType);
        #endregion
    }
}
