using System;
using System.Collections.Generic;
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

        #region Data Readers

        public IReadOnlyRepository<Task> TasksRepo { get; private set; }    

        #endregion



        public Task CreateTask(string areaName, string controllerName, string actionName, bool isPublic = false)
        {
            Task task = new Task()
            {
                AreaName = areaName,
                ControllerName = controllerName,
                ActionName = actionName,
                IsPublic = isPublic
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Task> tasksRepo = uow.GetRepository<Task>();
                tasksRepo.Put(task);

                uow.Commit();
            }

            return (task);
        }

        public bool DeleteTaskById(long id)
        {
            throw new NotImplementedException();
        }

        public bool ExistsTaskId(long id)
        {
            return TasksRepo.Get(id) != null ? true : false;
        }

        public IQueryable<Task> GetAllTasks()
        {
            return TasksRepo.Query();
        }

        public Task GetTask(string areaName, string controllerName, string actionName)
        {
            return TasksRepo.Get(t => t.AreaName.ToLower() == areaName.ToLower() && t.ControllerName.ToLower() == controllerName.ToLower() && t.ActionName.ToLower() == actionName.ToLower()).FirstOrDefault();
        }

        public Task GetTaskById(long id)
        {
            return TasksRepo.Get(id);
        }

        public Task UpdateTask(Task task)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Task> tasksRepo = uow.GetRepository<Task>();
                tasksRepo.Put(task);
                uow.Commit();
            }

            return (task);
        }
    }
}
