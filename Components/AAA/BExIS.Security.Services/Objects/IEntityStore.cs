using System.Collections.Generic;

namespace BExIS.Security.Services.Objects
{
    public interface IEntityStore
    {
        List<EntityStoreItem> GetEntities();

        string GetTitleById(long id);
    }

    public class EntityStoreItem
    {
        public long Id { get; set; }
        public string Title { get; set; }
    }
}