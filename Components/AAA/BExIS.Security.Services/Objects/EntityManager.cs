using BExIS.Security.Entities.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Objects
{
    public class EntityManager
    {
        public EntityManager()
        {
            var uow = this.GetUnitOfWork();

            EntityRepository = uow.GetReadOnlyRepository<Entity>();
        }

        public IQueryable<Entity> Entities => EntityRepository.Query();
        public IReadOnlyRepository<Entity> EntityRepository { get; }

        public void Create(Entity entity)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRepository = uow.GetRepository<Entity>();
                entityRepository.Put(entity);
                uow.Commit();
            }
        }

        public Entity Create(Type entityType, Type entityStoreType, Entity parent = null)
        {
            var entity = new Entity()
            {
                EntityType = entityType,
                EntityStoreType = entityStoreType,
                Parent = parent
            };

            using (var uow = this.GetUnitOfWork())
            {
                var entityRepository = uow.GetRepository<Entity>();
                entityRepository.Put(entity);
                uow.Commit();
            }

            return entity;
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

        public Entity FindById(long entityId)
        {
            return EntityRepository.Get(entityId);
        }

        public Entity FindByName(string entityName)
        {
            return EntityRepository.Query(m => m.Name.ToLowerInvariant() == entityName.ToLowerInvariant()).FirstOrDefault();
        }

        public List<Entity> FindRoots()
        {
            return EntityRepository.Query(e => e.Parent == null).ToList();
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
    }
}