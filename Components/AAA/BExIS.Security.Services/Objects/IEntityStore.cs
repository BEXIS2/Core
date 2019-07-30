using System.Collections.Generic;

namespace BExIS.Security.Services.Objects
{
    public interface IEntityStore
    {
        List<EntityStoreItem> GetEntities();

        string GetTitleById(long id);

        int GetVersionById(long id);
    }

    public class EntityStoreItem
    {
        public long Id { get; set; }
        public int Version { get; set; }
        public string Title { get; set; }
    }
}