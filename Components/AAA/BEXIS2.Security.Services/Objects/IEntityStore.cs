using System.Collections.Generic;

namespace BExIS.Security.Services.Objects
{
    public interface IEntityStore
    {
        List<EntityStoreItem> GetEntities();

        bool Exist(long id);

        int CountEntities();

        string GetTitleById(long id);

        bool HasVersions();

        int CountVersions(long id);

        List<EntityStoreItem> GetVersionsById(long id);
    }

    public class EntityStoreItem
    {
        public long Id { get; set; }
        public int Version { get; set; }
        public string Title { get; set; }
        public string CommitComment { get; set; }
    }
}