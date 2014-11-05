using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities;
using BExIS.Security.Entities.Objects;
      
namespace BExIS.Security.Services.Objects
{
    public interface IFeatureManager
    {
        bool AddTaskToFeature(long taskId, long featureId);

        Feature CreateFeature(string name, string description, long parentId = 0);

        bool DeleteFeatureById(long id);

        bool ExistsFeatureId(long id);

        IQueryable<Feature> GetAllFeatures();

        IQueryable<Feature> GetRoots();

        Feature GetFeatureById(long id);

        bool RemoveTaskFromFeature(long taskId, long featureId);

        Feature UpdateFeature(Feature feature);
    }
}
