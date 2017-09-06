using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Utils.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Authorization
{
    public class EntityPermissionManager : IDisposable
    {
        private IUnitOfWork guow = null;
        private bool isDisposed = false;

        public EntityPermissionManager()
        {
            guow = this.GetIsolatedUnitOfWork();
            //EntityPermissionRepository = guow.GetReadOnlyRepository<EntityPermission>();
        }

        ~EntityPermissionManager()
        {
            Dispose(true);
        }

        //public IReadOnlyRepository<EntityPermission> EntityPermissionRepository { get; }
        //public IQueryable<EntityPermission> EntityPermissions => EntityPermissionRepository.Query();

        public void Create(EntityPermission entityPermission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Put(entityPermission);
                uow.Commit();
            }
        }

        public void Create(Subject subject, Entity entity, long key, short rights)
        {
            var entityPermission = new EntityPermission()
            {
                Subject = subject,
                Entity = entity,
                Key = key,
                Rights = rights
            };

            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Put(entityPermission);
                uow.Commit();
            }
        }

        public void Create(long? subjectId, long entityId, long key, short rights)
        {
            using (var uow = this.GetUnitOfWork())
            {
                IReadOnlyRepository<Entity> entityRepository = uow.GetReadOnlyRepository<Entity>();
                IReadOnlyRepository<Subject> subjectRepository = uow.GetReadOnlyRepository<Subject>();
                var entityPermission = new EntityPermission()
                {
                    Subject = subjectId == null ? null : subjectRepository.Query(s => s.Id == subjectId).FirstOrDefault(),
                    Entity = entityRepository.Get(entityId),
                    Key = key,
                    Rights = rights
                };

                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Put(entityPermission);
                uow.Commit();
            }
        }

        public EntityPermission Create<T>(string subjectName, string entityName, Type entityType, long key, List<RightType> rights) where T : Subject
        {
            if (string.IsNullOrEmpty(subjectName))
                return null;

            if (string.IsNullOrEmpty(entityName))
                return null;

            if (entityType == null)
                return null;

            EntityPermission entityPermission = null;
            using (var uow = this.GetUnitOfWork())
            {
                IReadOnlyRepository<Entity> entityRepository = uow.GetReadOnlyRepository<Entity>();
                IReadOnlyRepository<Subject> subjectRepository = uow.GetReadOnlyRepository<Subject>();

                var subject = subjectRepository.Query(s => s.Name.ToUpperInvariant() == subjectName.ToUpperInvariant() && s is T).FirstOrDefault();
                if (subject == null)
                    return null;

                var entity = entityRepository.Query(e => e.Name.ToUpperInvariant() == entityName.ToUpperInvariant() && e.EntityType == entityType).FirstOrDefault();
                if (entity == null)
                    return null;

                if (Exists(subject, entity, key))
                    return null;

                entityPermission = new EntityPermission()
                {
                    Subject = subject,
                    Entity = entity,
                    Key = key,
                    Rights = rights.ToInt()
                };

                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Put(entityPermission);
                uow.Commit();
            }

            return entityPermission;
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

        public void Delete(Type entityType, long key)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                var entityPermissions = entityPermissionRepository.Query(p => p.Entity.EntityType == entityType && p.Key == key) as IEnumerable<EntityPermission>;
                entityPermissionRepository.Delete(entityPermissions);
                uow.Commit();
            }
        }

        public void Delete(long entityPermissionId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Delete(entityPermissionRepository.Get(entityPermissionId));
                uow.Commit();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public bool Exists(Subject subject, Entity entity, long key)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();

                if (entity == null)
                    return false;

                if (subject == null)
                    return entityPermissionRepository.Get(p => p.Subject == null && p.Entity.Id == entity.Id && p.Key == key).Count == 1;

                return entityPermissionRepository.Get(p => p.Subject.Id == subject.Id && p.Entity.Id == entity.Id && p.Key == key).Count == 1;
            }
        }

        public bool Exists(long? subjectId, long entityId, long key)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();

                if (subjectId == null)
                    return entityPermissionRepository.Get(p => p.Subject == null && p.Entity.Id == entityId && p.Key == key).Count == 1;
                return entityPermissionRepository.Get(p => p.Subject.Id == subjectId && p.Entity.Id == entityId && p.Key == key).Count == 1;
            }
        }

        public EntityPermission FindById(long entityPermissionId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                return entityPermissionRepository.Get(entityPermissionId);
            }
        }

        public List<long> GetKeys<T>(string subjectName, string entityName, Type entityType, RightType rightType) where T : Subject
        {
            if (string.IsNullOrEmpty(subjectName))
                return new List<long>();

            if (string.IsNullOrEmpty(entityName))
                return new List<long>();

            if (entityType == null)
                return new List<long>();

            using (var uow = this.GetUnitOfWork())
            {
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();
                var entityRepository = uow.GetReadOnlyRepository<Entity>();
                var entityPermissionRepository = uow.GetReadOnlyRepository<EntityPermission>();

                var subject = subjectRepository.Query(s => s.Name.ToUpperInvariant() == subjectName.ToUpperInvariant() && s is T).FirstOrDefault();
                if (subject == null)
                    return new List<long>();

                var entity = entityRepository.Query(e => e.Name.ToUpperInvariant() == entityName.ToUpperInvariant() && e.EntityType == entityType).FirstOrDefault();
                if (entity == null)
                    return new List<long>();

                return
                    entityPermissionRepository
                        .Query(e => e.Subject.Id == subject.Id && e.Entity.Id == entity.Id).AsEnumerable()
                        .Where(e => e.Rights.ToRightTypes().Contains(rightType))
                        .Select(e => e.Key)
                        .ToList();
            }
        }

        public List<long> GetKeys(long? subjectId, long entityId, RightType rightType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetReadOnlyRepository<EntityPermission>();

                if (subjectId == null)
                    return entityPermissionRepository.Query(e =>
                        e.Subject == null &&
                        e.Entity.Id == entityId &&
                        e.Rights.ToRightTypes().Contains(rightType)
                        )
                    .Select(e => e.Key)
                    .ToList();

                return entityPermissionRepository.Query(e =>
                    e.Subject.Id == subjectId &&
                    e.Entity.Id == entityId &&
                    e.Rights.ToRightTypes().Contains(rightType)
                    )
                .Select(e => e.Key)
                .ToList();
            }
        }

        public int GetRights(Subject subject, Entity entity, long key)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetReadOnlyRepository<EntityPermission>();

                if (subject == null)
                {
                    return entityPermissionRepository.Get(m => m.Subject == null && m.Entity.Id == entity.Id).FirstOrDefault()?.Rights ?? 0;
                }
                if (entity == null)
                    return 0;
                return entityPermissionRepository.Get(m => m.Subject.Id == subject.Id && m.Entity.Id == entity.Id).FirstOrDefault()?.Rights ?? 0;
            }
        }

        public List<RightType> GetRights<T>(string subjectName, string entityName, Type entityType, long key) where T : Subject
        {
            if (string.IsNullOrEmpty(subjectName))
                return new List<RightType>();

            if (string.IsNullOrEmpty(entityName))
                return new List<RightType>();

            if (entityType == null)
                return new List<RightType>();

            using (var uow = this.GetUnitOfWork())
            {
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();
                var entityRepository = uow.GetReadOnlyRepository<Entity>();

                var subject = subjectRepository.Query(s => s.Name.ToUpperInvariant() == subjectName.ToUpperInvariant() && s is T).FirstOrDefault();
                if (subject == null)
                    return new List<RightType>();

                var entity = entityRepository.Query(e => e.Name.ToUpperInvariant() == entityName.ToUpperInvariant() && e.EntityType == entityType).FirstOrDefault();
                if (entity == null)
                    return new List<RightType>();
                return GetRights(subject, entity, key).ToRightTypes();
            }
        }

        public List<RightType> GetRights(long? subjectId, long entityId, long key)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();
                var entityRepository = uow.GetReadOnlyRepository<Entity>();

                var subject = subjectId == null ? null : subjectRepository.Query(s => s.Id == subjectId).FirstOrDefault();
                var entity = entityRepository.Get(entityId);
                return GetRights(subject, entity, key).ToRightTypes();
            }
        }

        public bool HasRight<T>(string subjectName, string entityName, Type entityType, long key, RightType rightType) where T : Subject
        {
            using (var uow = this.GetUnitOfWork())
            {
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();
                var entityRepository = uow.GetReadOnlyRepository<Entity>();

                var subject = subjectRepository.Query(s => s.Name.ToUpperInvariant() == subjectName.ToUpperInvariant() && s is T).FirstOrDefault();
                var entity = entityRepository.Query(e => e.Name.ToUpperInvariant() == entityName.ToUpperInvariant() && e.EntityType == entityType).FirstOrDefault();

                var binary = Convert.ToString(GetRights(subject, entity, key), 2);
                return (int)rightType < binary.Length && binary.ElementAt((binary.Length - 1) - (int)rightType) == '1';
            }
        }

        public bool HasRight(long? subjectId, long entityId, long key, RightType rightType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();
                var entityRepository = uow.GetReadOnlyRepository<Entity>();

                var subject = subjectId == null ? null : subjectRepository.Query(s => s.Id == subjectId).FirstOrDefault();
                var entity = entityRepository.Get(entityId);

                var binary = Convert.ToString(GetRights(subject, entity, key), 2);
                return (int)rightType < binary.Length && binary.ElementAt((binary.Length - 1) - (int)rightType) == '1';
            }
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

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    if (guow != null)
                        guow.Dispose();
                    isDisposed = true;
                }
            }
        }
    }
}