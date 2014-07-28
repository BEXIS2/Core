using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities;
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
    public interface IFeatureManager
    {
        // A

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="taskId"></param>
        /// <param name="featureId"></param>
        int AddTaskToFeature(long taskId, long featureId);

        // C

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureName"></param>
        /// <param name="description"></param>
        /// <param name="status"></param>
        /// <param name="parentId"></param>
        Feature CreateFeature(string featureName, string description, out FeatureCreateStatus status, long parentId = 0);

        // D

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureName"></param>
        bool DeleteFeatureByName(string featureName);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        bool DeleteFeatureById(long id);

        // E

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        bool ExistsFeatureId(long id);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureName"></param>
        bool ExistsFeatureName(string featureName);

        // G

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        IQueryable<Feature> GetAllFeatures();

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        IQueryable<Feature> GetChildren(long id);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        Feature GetFeatureById(long id);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureName"></param>
        Feature GetFeatureByName(string featureName);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        Feature GetFeatureFromTask(long id);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        Feature GetParent(long id);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        IQueryable<Feature> GetRoots();

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>       
        IQueryable<Task> GetTasksFromFeature(long id);
        
        // I

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="taskId"></param>
        /// <param name="featureId"></param>
        bool IsTaskInFeature(long taskId, long featureId);

        // R

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="taskId"></param>
        int RemoveTaskFromFeature(long taskId);

        // U 

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="feature"></param>
        Feature UpdateFeature(Feature feature);        
    }

    /// <summary>
    /// 
    /// </summary>
    public enum FeatureCreateStatus
    {
        Success,
        DuplicateFeatureName,
        InvalidFeatureName,
        InvalidParent
    }
}
