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
        // A
        int AddTaskToFeature(long taskId, long featureId);

        // C
        Feature CreateFeature(string featureName, string description, out FeatureCreateStatus status, long parentId = 0);

        // D
        bool DeleteFeatureByName(string featureName);
        bool DeleteFeatureById(long id);

        // E
        bool ExistsFeatureId(long id);
        bool ExistsFeatureName(string featureName);

        // G
        IQueryable<Feature> GetAllFeatures();

        IQueryable<Feature> GetChildren(long id);

        Feature GetFeatureById(long id);
        Feature GetFeatureByName(string featureName);

        Feature GetFeatureFromTask(long id);

        Feature GetParent(long id);

        IQueryable<Feature> GetRoots();

        IQueryable<Task> GetTasksFromFeature(long id);
        
        // I
        bool IsTaskInFeature(long taskId, long featureId);

        // R
        int RemoveTaskFromFeature(long taskId);

        // U 
        Feature UpdateFeature(Feature feature);        
    }

    public enum FeatureCreateStatus
    {
        Success,
        DuplicateFeatureName,
        InvalidFeatureName,
        InvalidParent
    }
}
