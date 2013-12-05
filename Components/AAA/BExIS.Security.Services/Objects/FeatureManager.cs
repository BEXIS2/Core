using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Objects
{
    public class FeatureManager : IFeatureManager
    {
        public FeatureManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();

            this.FeaturesRepo = uow.GetReadOnlyRepository<Feature>();
        }

        #region Data Reader

        public IReadOnlyRepository<Feature> FeaturesRepo { get; private set; }

        #endregion

        #region Attributes

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <param name="feature"></param>
        /// <returns></returns>
        public bool AddTaskToFeature(Task task, Feature feature)
        {
            bool result = false;

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Feature> repo = uow.GetRepository<Feature>();

                feature = repo.Reload(feature);
                repo.LoadIfNot(feature.Tasks);
                if (!feature.Tasks.Contains(task))
                {
                    feature.Tasks.Add(task);
                    task.Feature = feature;
                    uow.Commit();
                    result = true;
                }
            }

            return (result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="featureName"></param>
        /// <param name="description"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public Feature Create(string featureName, string description, out FeatureCreateStatus status)
        {
            if (ExistsFeatureName(featureName))
            {
                status = FeatureCreateStatus.DuplicateFeatureName;
                return null;
            }

            Feature feature = new Feature()
            {
                // Subject Properties
                Name = featureName,

                // Role Properties
                Description = description
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Feature> rolesRepo = uow.GetRepository<Feature>();
                rolesRepo.Put(feature);
                uow.Commit();
            }

            status = FeatureCreateStatus.Success;
            return (feature);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        public bool Delete(Feature feature)
        {
            Contract.Requires(feature != null);

            // Computations
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Feature> repo = uow.GetRepository<Feature>();

                feature = repo.Reload(feature);
                repo.Delete(feature);
                uow.Commit();
            }

            return (true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="featureName"></param>
        /// <returns></returns>
        public bool ExistsFeatureName(string featureName)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(featureName));

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
        /// <returns></returns>
        public IQueryable<Feature> GetAllFeatures()
        {
            return (FeaturesRepo.Query());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        public IQueryable<Feature> GetChildren(Feature feature)
        {
            Contract.Requires(feature != null);

            feature = FeaturesRepo.Reload(feature);
            FeaturesRepo.LoadIfNot(feature.Children);

            return feature.Children.AsQueryable<Feature>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Feature GetFeatureById(long id)
        {
            Contract.Requires(id >= 0);

            if (FeaturesRepo.Get(f => f.Id == id).Count() == 1)
            {
                return FeaturesRepo.Get(f => f.Id == id).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="featureName"></param>
        /// <returns></returns>
        public Feature GetFeatureByName(string featureName)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(featureName));

            if (FeaturesRepo.Get(f => f.Name == featureName).Count() == 1)
            {
                return FeaturesRepo.Get(f => f.Name == featureName).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        public IQueryable<Task> GetTasksFromFeature(Feature feature)
        {
            Contract.Requires(feature != null);

            feature = FeaturesRepo.Reload(feature);
            FeaturesRepo.LoadIfNot(feature.Tasks);

            return feature.Tasks.AsQueryable<Task>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        public Feature GetParent(Feature feature)
        {
            Contract.Requires(feature != null);

            feature = FeaturesRepo.Reload(feature);
            FeaturesRepo.LoadIfNot(feature.Parent);

            return feature.Parent;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        /// <param name="feature"></param>
        /// <returns></returns>
        public bool IsTaskInFeature(Task task, Feature feature)
        {
            Contract.Requires(feature != null);
            Contract.Requires(task != null);

            feature = FeaturesRepo.Reload(feature);
            FeaturesRepo.LoadIfNot(feature.Tasks);

            if (feature.Tasks.Contains(task))
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
        /// <param name="task"></param>
        /// <param name="feature"></param>
        /// <returns></returns>
        public bool RemoveTaskFromFeature(Task task, Feature feature)
        {
            Contract.Requires(feature != null);
            Contract.Requires(task != null);

            // Variables
            bool result = false;

            // Computations
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Feature> featuresRepo = uow.GetRepository<Feature>();
                IRepository<Task> tasksRepo = uow.GetRepository<Task>();

                feature = featuresRepo.Reload(feature);
                featuresRepo.LoadIfNot(feature.Tasks);

                task = tasksRepo.Reload(task);
                tasksRepo.LoadIfNot(task.Feature);

                if (feature.Tasks.Contains(task))
                {
                    feature.Tasks.Remove(task);
                    task.Feature = null;

                    uow.Commit();

                    result = true;
                }
            }
            return (result);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="feature"></param>
        /// <returns></returns>
        public Feature Update(Feature feature)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Feature> repo = uow.GetRepository<Feature>();
                repo.Put(feature);
                uow.Commit();
            }

            return (feature);
        }

        #endregion
    }
}
