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
            this.TasksRepo = uow.GetReadOnlyRepository<Task>();
        }

        #region Data Reader
    
        public IReadOnlyRepository<Feature> FeaturesRepo { get; private set; }
       
        public IReadOnlyRepository<Task> TasksRepo { get; private set; }

        #endregion

        public bool AddTaskToFeature(long taskId, long featureId)
        {
            Task task = TasksRepo.Get(taskId);
            Feature feature = FeaturesRepo.Get(featureId);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Feature> repo = uow.GetRepository<Feature>();

                repo.LoadIfNot(feature.Tasks);
                if (!feature.Tasks.Contains(task))
                {
                    feature.Tasks.Add(task);
                    task.Feature = feature;
                    uow.Commit();

                    return (true);
                }

                return (false);
            }
        }

        public Feature CreateFeature(string name, string description, long parentId = 0)
        {
            Feature feature = new Feature()
            {
                Name = name,
                Description = description,
                Parent = GetFeatureById(parentId)
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Feature> featuresRepo = uow.GetRepository<Feature>();
                featuresRepo.Put(feature);

                uow.Commit();
            }

            return (feature);
        }

        public bool DeleteFeatureById(long id)
        {
            throw new NotImplementedException();
        }

        public bool ExistsFeatureId(long id)
        {
            return FeaturesRepo.Get(id) != null ? true : false;
        }

        public IQueryable<Feature> GetAllFeatures()
        {
            return FeaturesRepo.Query();
        }

        public IQueryable<Feature> GetRoots()
        {
            return FeaturesRepo.Query(f => f.Parent == null);
        }

        public Feature GetFeatureById(long id)
        {
            return FeaturesRepo.Get(id);
        }

        public bool RemoveTaskFromFeature(long taskId, long featureId)
        {
            throw new NotImplementedException();
        }

        public Feature UpdateFeature(Feature feature)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Feature> featuresRepo = uow.GetRepository<Feature>();
                featuresRepo.Put(feature);
                uow.Commit();
            }

            return (feature);
        }
    }
}
