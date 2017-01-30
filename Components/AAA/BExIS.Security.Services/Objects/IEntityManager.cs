using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;

namespace BExIS.Security.Services.Objects
{
    public interface IEntityManager
    {
        Entity CreateEntity(string name, string classPath, string assemblyPath, bool secureable, bool useMetadata);

        bool DeleteEntityById(long id);

        bool ExistsEntityId(long id);

        IQueryable<Entity> GetAllEntities();

        Entity GetEntityById(long id);

        Entity UpdateEntity(Entity entity);
    }
}
