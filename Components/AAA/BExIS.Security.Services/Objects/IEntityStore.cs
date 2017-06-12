using System.Collections.Generic;

namespace BExIS.Security.Services.Objects
{
    public interface IEntityStore
    {
        List<EntityModel> GetAllEntities();

        List<long> GetAllIds();
    }

    public class EntityModel
    {
        public long Id { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }
}