using BExIS.Security.Entities.Objects;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Objects
{
    public class EntityManager
    {
        public IReadOnlyRepository<Entity> EntityRepository { get; }

        public EntityManager()
        {
            var uow = this.GetUnitOfWork();

            EntityRepository = uow.GetReadOnlyRepository<Entity>();
        }

        public IQueryable<Entity> Entities => EntityRepository.Query();

        public void Create(Entity entity)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRepository = uow.GetRepository<Entity>();
                entityRepository.Put(entity);
                uow.Commit();
            }
        }

        public void Update(Entity entity)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRepository = uow.GetRepository<Entity>();
                entityRepository.Put(entity);
                uow.Commit();
            }
        }

        public void Delete(Entity entity)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRepository = uow.GetRepository<Entity>();
                entityRepository.Delete(entity);
                uow.Commit();
            }
        }

        public Entity FindByIdAsync(long entityId)
        {
            return EntityRepository.Get(entityId);
        }

        public Entity FindByNameAsync(string entityName)
        {
            return EntityRepository.Query(m => m.Name.ToLowerInvariant() == entityName.ToLowerInvariant()).FirstOrDefault();
        }
    }
}