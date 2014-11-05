using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Objects
{
    public sealed class EntityManager : IEntityManager
    {
        public EntityManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();

            this.EntitiesRepo = uow.GetReadOnlyRepository<Entity>();
        }

        public IReadOnlyRepository<Entity> EntitiesRepo { get; private set; }

        public Entity CreateEntity(string name, string classPath, string assemblyPath)
        {
            Entity entity = new Entity()
            {
                Name = name,
                ClassPath = classPath,
                AssemblyPath = assemblyPath
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Entity> usersRepo = uow.GetRepository<Entity>();
                usersRepo.Put(entity);

                uow.Commit();
            }

            return (entity);
        }

        public bool DeleteEntityById(long id)
        {
            throw new NotImplementedException();
        }

        public bool ExistsEntityId(long id)
        {
            return EntitiesRepo.Get(id) != null ? true : false;
        }

        public IQueryable<Entity> GetAllEntities()
        {
            return EntitiesRepo.Query();
        }

        public Entity GetEntityById(long id)
        {
            return EntitiesRepo.Get(id);
        }

        public Entity UpdateEntity(Entity entity)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Entity> entitiesRepo = uow.GetRepository<Entity>();
                entitiesRepo.Put(entity);
                uow.Commit();
            }

            return (entity);
        }
    }
}