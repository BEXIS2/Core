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
    public interface ITaskManager
    {
        // C

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="taskName"></param>
        /// <param name="description"></param>
        /// <param name="areaName"></param>
        /// <param name="controllerName"></param>
        /// <param name="actionName"></param>
        /// <param name="status"></param>
        /// <param name="featureId"></param>
        Task CreateTask(string taskName, string description, string areaName, string controllerName, string actionName, out TaskCreateStatus status, long featureId = 0);

        // D

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        bool DeleteTaskById(long id);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="taskName"></param>
        bool DeleteTaskByName(string taskName);

        // E

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        bool ExistsTaskId(long id);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="taskName"></param>
        bool ExistsTaskName(string taskName);

        // G

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        Task GetTaskById(long id);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="taskName"></param>
        Task GetTaskByName(string taskName);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="areaName"></param>
        /// <param name="controllerName"></param>
        /// <param name="actionName"></param>
        Task GetTaskByContext(string areaName, string controllerName, string actionName);

        // U

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="task"></param>
        Task UpdateTask(Task task);
    }

    /// <summary>
    /// 
    /// </summary>
    public enum TaskCreateStatus
    {
        Success,
        DuplicateTaskContext,
        InvalidFeature
    }
}
