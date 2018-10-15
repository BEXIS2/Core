using System.Collections.Generic;

namespace BExIS.Security.Services.Objects
{
    public class DatasetStore : IEntityStore
    {
        public List<EntityStoreItem> GetEntities()
        {
            var datasets = new List<EntityStoreItem>();

            for (int i = 0; i < 25; i++)
            {
                datasets.Add(new EntityStoreItem() { Id = i, Title = $"Dataset with Title {i}" });
            }

            return datasets;
        }

        public string GetTitleById(long id)
        {
            throw new System.NotImplementedException();
        }
    }
}