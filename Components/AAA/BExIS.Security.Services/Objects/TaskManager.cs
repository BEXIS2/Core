using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Objects
{
    public sealed class TaskManager : ITaskManager
    {
        public TaskManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();

            this.TasksRepo = uow.GetReadOnlyRepository<Task>();
        }

        #region Data Reader

        public IReadOnlyRepository<Task> TasksRepo { get; private set; }

        #endregion

        #region Attributes

        #endregion

        #region Methods

        public Task Create(string name, string description, string areaName, string controllerName, string actionName, out TaskCreateStatus status)
        {
            Task task = new Task()
            {
                Name = name,
                Description = description,
                AreaName = areaName,
                ControllerName = controllerName,
                ActionName = actionName
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Task> tasksRepo = uow.GetRepository<Task>();
                tasksRepo.Put(task);
                uow.Commit();
            }

            status = TaskCreateStatus.Success;
            return (task);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="taskContext"></param>
        /// <returns></returns>
        public Task GetTaskByContext(TaskContext taskContext)
        {
            Contract.Requires(taskContext != null);

            if (TasksRepo.Get(t => t.AreaName == taskContext.AreaName && t.ControllerName == taskContext.ControllerName && t.ActionName == taskContext.ActionName).Count() == 1)
            {
                return TasksRepo.Get(t => t.AreaName == taskContext.AreaName && t.ControllerName == taskContext.ControllerName && t.ActionName == taskContext.ActionName).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}
