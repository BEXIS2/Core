using System.Collections.Generic;

namespace BExIS.Security.Services.Objects
{
    public class DatasetStore : IEntityStore
    {
        public int CountEntities()
        {
            throw new System.NotImplementedException();
        }

        public int CountVersions(long id)
        {
            throw new System.NotImplementedException();
        }

        public List<EntityStoreItem> GetEntities()
        {
            var datasets = new List<EntityStoreItem>();

            for (int i = 0; i < 25; i++)
            {
                datasets.Add(new EntityStoreItem() { Id = i, Title = $"Dataset with Title {i}" });
            }

            return datasets;
        }

        public List<EntityStoreItem> GetEntities(int skip, int take)
        {
            throw new System.NotImplementedException();
        }

        public string GetTitleById(long id)
        {
            throw new System.NotImplementedException();
        }

        public int GetVersionById(long id)
        {
            throw new System.NotImplementedException();
        }

        public List<EntityStoreItem> GetVersionsById(long id)
        {
            throw new System.NotImplementedException();
        }

        public bool HasVersions()
        {
            throw new System.NotImplementedException();
        }
    }
}