using BExIS.Dlm.Services.Data;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Xml.Helpers
{
    public class DatasetStore : IEntityStore
    {
        public List<EntityStoreItem> GetEntities()
        {
            using (var uow = this.GetUnitOfWork())
            {
                DatasetManager dm = new DatasetManager();

                try
                {
                    var datasetIds = dm.GetDatasetLatestIds();
                    var datasetHelper = new XmlDatasetHelper();

                    var entities = datasetIds.Select(id => new EntityStoreItem()
                    {
                        Id = id,
                        Title = datasetHelper.GetInformation(id, NameAttributeValues.title),
                        Version = dm.GetDataset(id).Versions.Count
                    });
                    return entities.ToList();
                }
                finally
                {
                    dm.Dispose();
                }
            }
        }

        public string GetTitleById(long id)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var dm = new DatasetManager();

                try
                {
                    var datasetHelper = new XmlDatasetHelper();

                    return datasetHelper.GetInformation(id, NameAttributeValues.title);
                }
                finally
                {
                    dm.Dispose();
                }
            }
        }

        public int GetVersionById(long id)
        {
            DatasetManager dm = new DatasetManager();

            try
            {
                var datasetIds = dm.GetDatasetLatestIds();
                var datasetHelper = new XmlDatasetHelper();

                int version = dm.GetDataset(id).Versions.Count;

                return version;
            }
            catch (Exception ex)
            {
                return 0;
            }
            finally
            {
                dm.Dispose();
            }
        }
    }
}