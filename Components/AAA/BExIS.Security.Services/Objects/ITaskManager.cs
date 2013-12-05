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
        Task Create(string name, string description, string areaName, string controllerName, string actionName, out TaskCreateStatus status);

        // G
        Task GetTaskByContext(TaskContext taskContext);
    }
}
