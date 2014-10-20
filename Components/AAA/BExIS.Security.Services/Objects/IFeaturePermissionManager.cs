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
    public interface IFeaturePermissionManager
    {
        #region Attributes

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        bool DefaultFeaturePermission { get; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        bool DrawFeaturePermission { get; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        bool FeaturePermissionHierarchy { get; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        int FeaturePermissionPolicy { get; }

        #endregion


        #region Methods

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureId"></param>
        /// <param name="subjectId"></param>
        int CreateFeaturePermission(long featureId, long subjectId);

        // D

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        int DeleteFeaturePermissionById(long id);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureId"></param>
        /// <param name="subjectId"></param>
        int DeleteFeaturePermissionByFeatureAndSubject(long featureId, long subjectId);

        // E

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureId"></param>
        /// <param name="subjectId"></param>
        bool ExistsFeaturePermission(long featureId, long subjectId);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureId"></param>
        /// <param name="subjectId"></param>
        bool ExistsFeaturePermission(IEnumerable<long> featureId, IEnumerable<long> subjectId);

        // G

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        FeaturePermission GetFeaturePermissionById(long id);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureId"></param>
        /// <param name="subjectId"></param>
        FeaturePermission GetFeaturePermissionByFeatureAndSubject(long featureId, long subjectId);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        IQueryable<FeaturePermission> GetAllFeaturePermissions();

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// ^<param name="featureId"></param>
        IQueryable<FeaturePermission> GetFeaturePermissionsFromFeature(long featureId);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="subjectId"></param>
        IQueryable<FeaturePermission> GetFeaturePermissionsFromSubject(long subjectId);

        // H

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="userName"></param>
        /// <param name="areaName"></param>
        /// <param name="controllerName"></param>
        /// <param name="actionName"></param>
        bool HasUserFeatureAccess(string userName, string areaName, string controllerName, string actionName);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureId"></param>
        /// <param name="subjectId"></param>
        bool HasSubjectFeatureAccess(long featureId, long subjectId);

        // U

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featurePermission"></param>
        FeaturePermission UpdateFeaturePermission(FeaturePermission featurePermission);

        #endregion
    }
}
