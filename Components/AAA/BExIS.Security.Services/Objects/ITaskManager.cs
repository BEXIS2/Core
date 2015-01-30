using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;
      
namespace BExIS.Security.Services.Objects
{
    public interface ITaskManager
    {
        Task CreateTask(string areaName, string controllerName, string actionName);

        bool DeleteTaskById(long id);

        bool ExistsTaskId(long id);

        IQueryable<Task> GetAllTasks();

        Task GetTask(string areaName, string controllerName, string actionName);

        Task GetTaskById(long id);

        Task UpdateTask(Task task);
    }
}
