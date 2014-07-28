using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;
using Vaiona.Persistence.Api;

/// <summary>
///
/// </summary>        
namespace BExIS.Security.Services.Objects
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class FeatureManager : IFeatureManager, ITaskManager
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public FeatureManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();

            this.FeaturesRepo = uow.GetReadOnlyRepository<Feature>();
            this.TasksRepo = uow.GetReadOnlyRepository<Task>();
        }

        #region Data Reader

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public IReadOnlyRepository<Feature> FeaturesRepo { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public IReadOnlyRepository<Task> TasksRepo { get; private set; }

        #endregion

        #region Attributes

        #endregion

        #region Methods

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="taskId"></param>       
        /// <param name="featureId"></param>       
        public int AddTaskToFeature(long taskId, long featureId)
        {
            Contract.Requires(taskId > 0);
            Contract.Requires(featureId > 0);

            Feature feature = GetFeatureById(featureId);

            if (feature != null)
            {
                Task task = GetTaskById(taskId);

                if (task != null)
                {
                    using (IUnitOfWork uow = this.GetUnitOfWork())
                    {
                        IRepository<Feature> featuresRepo = uow.GetRepository<Feature>();

                        featuresRepo.LoadIfNot(feature.Tasks);
                        if (!feature.Tasks.Contains(task))
                        {
                            feature.Tasks.Add(task);
                            task.Feature = feature;
                            uow.Commit();
                        }
                    }

                    return 0;
                }
                else
                {
                    return 12;
                }
            }
            else
            {
                return 11;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureName"></param>   
        /// <param name="description"></param>
        /// <param name="parentId"></param>
        public Feature CreateFeature(string featureName, string description, out FeatureCreateStatus status, long parentId = 0)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(featureName));
            Contract.Requires(!String.IsNullOrWhiteSpace(description));

            if (ExistsFeatureName(featureName))
            {
                status = FeatureCreateStatus.DuplicateFeatureName;
                return null;
            }

            Feature feature = new Feature()
            {
                Name = featureName,
                Description = description,
                Parent = GetFeatureById(parentId)
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Feature> featuresRepo = uow.GetRepository<Feature>();
                featuresRepo.Put(feature);

                uow.Commit();
            }

            status = FeatureCreateStatus.Success;
            return (feature);
        }

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
        public Task CreateTask(string taskName, string description, string areaName, string controllerName, string actionName, out TaskCreateStatus status, long featureId = 0)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(taskName));
            Contract.Requires(!String.IsNullOrWhiteSpace(description));
            Contract.Requires(!String.IsNullOrWhiteSpace(areaName));
            Contract.Requires(!String.IsNullOrWhiteSpace(controllerName));
            Contract.Requires(!String.IsNullOrWhiteSpace(actionName));

            Task task = new Task()
            {
                Name = taskName,
                Description = description,
                AreaName = areaName,
                ControllerName = controllerName,
                ActionName = actionName,
                Feature = GetFeatureById(featureId)
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
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        public bool DeleteFeatureById(long id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureName"></param>
        public bool DeleteFeatureByName(string featureName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        public bool DeleteTaskById(long id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="taskName"></param>
        public bool DeleteTaskByName(string taskName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>     
        public bool ExistsFeatureId(long id)
        {
            if (GetFeatureById(id) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureName"></param>
        public bool ExistsFeatureName(string featureName)
        {
            if (GetFeatureByName(featureName) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        public bool ExistsTaskId(long id)
        {
            if (GetTaskById(id) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="taskName"></param>
        public bool ExistsTaskName(string taskName)
        {
            if (GetTaskByName(taskName) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        public IQueryable<Feature> GetAllFeatures()
        {
            return (FeaturesRepo.Query());
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        public IQueryable<Feature> GetChildren(long id)
        {
            return (FeaturesRepo.Query(f => f.Parent.Id == id));
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        public Feature GetFeatureById(long id)
        {
            if (FeaturesRepo.Query(f => f.Id == id).Count() == 1)
            {
                return FeaturesRepo.Query(f => f.Id == id).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureName"></param>
        public Feature GetFeatureByName(string featureName)
        {
            if (FeaturesRepo.Query(f => f.Name.ToLower() == featureName.ToLower()).Count() == 1)
            {
                return FeaturesRepo.Query(f => f.Name.ToLower() == featureName.ToLower()).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        public Feature GetFeatureFromTask(long id)
        {
            if (FeaturesRepo.Query(f => f.Tasks.Any(t => t.Id == id)).Count() == 1)
            {
                return FeaturesRepo.Query(f => f.Tasks.Any(t => t.Id == id)).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        public Feature GetParent(long id)
        {
            if (FeaturesRepo.Query(f => f.Id == id).Count() == 1)
            {
                return FeaturesRepo.Query(f => f.Id == id).FirstOrDefault().Parent;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        public IQueryable<Feature> GetRoots()
        {
            return (FeaturesRepo.Query(f => f.Parent == null));
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        public Task GetTaskById(long id)
        {
            if (TasksRepo.Query(t => t.Id == id).Count() == 1)
            {
                return TasksRepo.Query(t => t.Id == id).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="taskName"></param>
        public Task GetTaskByName(string taskName)
        {
            if (TasksRepo.Query(t => t.Name.ToLower() == taskName.ToLower()).Count() == 1)
            {
                return TasksRepo.Query(t => t.Name.ToLower() == taskName.ToLower()).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="areaName"></param>
        /// <param name="controllerName"></param>
        /// <param name="actionName"></param>
        public Task GetTaskByContext(string areaName, string controllerName, string actionName)
        {
            if (TasksRepo.Query(t => t.AreaName == areaName && t.ControllerName == controllerName && t.ActionName == actionName).Count() == 1)
            {
                return TasksRepo.Query(t => t.AreaName == areaName && t.ControllerName == controllerName && t.ActionName == actionName).FirstOrDefault();
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        public IQueryable<Task> GetTasksFromFeature(long id)
        {
            return TasksRepo.Query(t => t.Feature.Id == id);
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="taskId"></param>
        /// <param name="featureId"></param>
        public bool IsTaskInFeature(long taskId, long featureId)
        {
            if (GetTasksFromFeature(featureId).Where(t => t.Id == taskId).Count() == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="taskId"></param>
        public int RemoveTaskFromFeature(long taskId)
        {
            Task task = GetTaskById(taskId);

            if (task != null)
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Task> tasksRepo = uow.GetRepository<Task>();

                    if (task.Feature != null)
                    {
                        task.Feature = null;
                        uow.Commit();
                    }
                }

                return 0;
            }
            else
            {
                return 11;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="feature"></param>
        public Feature UpdateFeature(Feature feature)
        {
            Contract.Requires(feature != null);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Feature> featuresRepo = uow.GetRepository<Feature>();
                featuresRepo.Put(feature);
                uow.Commit();
            }

            return (feature);
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="task"></param>
        public Task UpdateTask(Task task)
        {
            Contract.Requires(task != null);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Task> tasksRepo = uow.GetRepository<Task>();
                tasksRepo.Put(task);
                uow.Commit();
            }

            return (task);
        }

        #endregion
    }
}
