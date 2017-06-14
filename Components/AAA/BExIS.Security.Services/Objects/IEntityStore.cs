using System.Collections.Generic;

namespace BExIS.Security.Services.Objects
{
    public interface IEntityStore
    {
        List<EntityStoreItemModel> GetEntities();
    }

    public class EntityStoreItemModel
    {
        public long Id { get; set; }
        public string Title { get; set; }
    }
}