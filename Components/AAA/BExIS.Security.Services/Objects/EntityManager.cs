using BExIS.Security.Entities.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Objects
{
    public class EntityManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public EntityManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            EntityRepository = _guow.GetReadOnlyRepository<Entity>();
        }

        ~EntityManager()
        {
            Dispose(true);
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
            using (var uow = this.GetUnitOfWork())
            {
                var entity = new Entity()
                {
                    EntityType = entityType,
                    EntityStoreType = entityStoreType,
                    Parent = parent
                };

                var entityRepository = uow.GetRepository<Entity>();
                entityRepository.Put(entity);
                uow.Commit();

                return entity;
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

        public void Dispose()
        {
            Dispose(true);
        }

        public Entity FindById(long entityId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRepository = uow.GetReadOnlyRepository<Entity>();
                return entityRepository.Get(entityId);
            }
        }

        public Entity FindByName(string entityName)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRepository = uow.GetReadOnlyRepository<Entity>();
                return entityRepository.Query(m => m.Name.ToLowerInvariant() == entityName.ToLowerInvariant()).FirstOrDefault();
            }
        }

        public List<Entity> FindRoots()
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRepository = uow.GetReadOnlyRepository<Entity>();
                return entityRepository.Query(e => e.Parent == null).ToList();
            }
        }

        public void Update(Entity entity)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<Entity>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }
        }

        protected void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_guow != null)
                        _guow.Dispose();
                    _isDisposed = true;
                }
            }
        }
    }
}