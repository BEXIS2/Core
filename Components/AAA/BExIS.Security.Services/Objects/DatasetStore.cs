using System.Collections.Generic;

namespace BExIS.Security.Services.Objects
{
    public class DatasetStore : IEntityStore
    {
        public List<EntityStoreItemModel> GetEntities()
        {
            var datasets = new List<EntityStoreItemModel>();

            for (int i = 0; i < 25; i++)
            {
                datasets.Add(new EntityStoreItemModel() { Id = i, Title = $"Dataset with Title {i}" });
            }

            return datasets;
        }
    }
}