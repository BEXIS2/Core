using BExIS.Dlm.Entities.Data;
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
                var datasetRepository = uow.GetReadOnlyRepository<Dataset>();
                var datasetHelper = new XmlDatasetHelper();

                var entities = datasetRepository.Query().Select(x => new EntityStoreItem() { Id = x.Id, Title = datasetHelper.GetInformation(x.Id, NameAttributeValues.title) });
                return entities.ToList();
            }
        }
    }
}
