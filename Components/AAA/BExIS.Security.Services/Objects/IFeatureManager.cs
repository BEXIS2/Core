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
        bool AddTaskToFeature(Task task, Feature feature);

        // C
        Feature Create(string featureName, string description, out FeatureCreateStatus status);

        // D
        bool Delete(Feature feature);

        // E
        bool ExistsFeatureName(string featureName);

        // G
        IQueryable<Feature> GetAllFeatures();

        IQueryable<Feature> GetChildren(Feature feature);

        Feature GetFeatureById(Int64 id);

        Feature GetFeatureByName(string featureName);

        Feature GetParent(Feature feature);

        IQueryable<Task> GetTasksFromFeature(Feature feature);
        
        // I
        bool IsTaskInFeature(Task task, Feature feature);

        // R
        bool RemoveTaskFromFeature(Task task, Feature feature);

        // U 
        Feature Update(Feature feature);        
    }
}
