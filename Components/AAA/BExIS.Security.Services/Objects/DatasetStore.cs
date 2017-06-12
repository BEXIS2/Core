using System;
using System.Collections.Generic;

namespace BExIS.Security.Services.Objects
{
    public class DatasetStore : IEntityStore
    {
        public List<EntityModel> GetAllEntities()
        {
            var datasets = new List<EntityModel>();

            for (int i = 0; i < 25; i++)
            {
                var properties = new Dictionary<string, object>();
                properties.Add("Title", $"Dataset{i}");
                properties.Add("Description", $"Description of Dataset{i}");
                datasets.Add(new EntityModel() { Id = i, Properties = properties });
            }

            return datasets;
        }

        public List<long> GetAllIds()
        {
            throw new NotImplementedException();
        }
    }
}