using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;

namespace BExIS.Security.Services.Objects
{
    public interface ITaskManager
    {
        // C
        Task CreateTask(string taskName, string description, string areaName, string controllerName, string actionName, out TaskCreateStatus status, long featureId = 0);

        // D
        bool DeleteTaskById(long id);
        bool DeleteTaskByName(string taskName);

        // E
        bool ExistsTaskId(long id);
        bool ExistsTaskName(string taskName);

        // G
        Task GetTaskById(long id);
        Task GetTaskByName(string taskName);

        Task GetTaskByContext(string areaName, string controllerName, string actionName);

        // U
        Task UpdateTask(Task task);
    }

    public enum TaskCreateStatus
    {
        Success,
        DuplicateTaskContext,
        InvalidFeature
    }
}
