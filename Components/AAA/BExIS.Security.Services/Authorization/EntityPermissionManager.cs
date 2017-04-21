using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using System;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Authorization
{
    public class EntityPermissionManager
    {
        public IReadOnlyRepository<EntityPermission> EntityPermissionRepository { get; private set; }
        public IReadOnlyRepository<Entity> EntityRepository { get; private set; }
        public IReadOnlyRepository<Subject> SubjectRepository { get; private set; }

        public EntityPermissionManager()
        {
            var uow = this.GetUnitOfWork();

            EntityPermissionRepository = uow.GetReadOnlyRepository<EntityPermission>();
            EntityRepository = uow.GetReadOnlyRepository<Entity>();
            SubjectRepository = uow.GetReadOnlyRepository<Subject>();
        }

        public IQueryable<EntityPermission> EntityPermissions => EntityPermissionRepository.Query();

        public void Create(EntityPermission entityPermission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Put(entityPermission);
                uow.Commit();
            }
        }

        public short GetRights(Subject subject, Entity entity, long key)
        {
            var entityPermission = EntityPermissionRepository.Get(m => m.Subject.Id == subject.Id && m.Entity.Id == entity.Id && m.Key == key).FirstOrDefault();
            return entityPermission?.Rights ?? 0;
        }

        [Obsolete]
        public bool HasRight(Subject subject, Entity entity, long key, RightType rightType)
        {
            var binary = Convert.ToString(GetRights(subject, entity, key), 2);
            return binary.ElementAt((binary.Length - 1) - (int)rightType) == '1';
        }

        public void Update(EntityPermission entityPermission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Put(entityPermission);
                uow.Commit();
            }
        }

        public void Delete(EntityPermission entityPermission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Delete(entityPermission);
                uow.Commit();
            }
        }
    }
}
