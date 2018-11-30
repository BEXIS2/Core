using BExIS.Dlm.Services.Data;
using BExIS.Security.Services.Objects;
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

                    var entities = datasetIds.Select(id => new EntityStoreItem() { Id = id, Title = datasetHelper.GetInformation(id, NameAttributeValues.title) });
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
    }
}
