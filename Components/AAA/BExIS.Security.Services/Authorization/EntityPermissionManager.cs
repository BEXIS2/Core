using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Authorization
{
    // Sven
    // UoW -> Done
    public class EntityPermissionManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public EntityPermissionManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            EntityPermissionRepository = _guow.GetReadOnlyRepository<EntityPermission>();
        }

        ~EntityPermissionManager()
        {
            Dispose(true);
        }

        public IReadOnlyRepository<EntityPermission> EntityPermissionRepository { get; }
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

        public void Create(Subject subject, Entity entity, long key, int rights)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermission = new EntityPermission()
                {
                    Subject = subject,
                    Entity = entity,
                    Key = key,
                    Rights = rights
                };

                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Put(entityPermission);
                uow.Commit();
            }
        }

        public void Create(Subject subject, Entity entity, long key, List<RightType> rightTypes)
        {
        }

        public void Create(long? subjectId, long entityId, long key, int rights)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRepository = uow.GetReadOnlyRepository<Entity>();
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();
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
                var entityRepository = uow.GetReadOnlyRepository<Entity>();
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();

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
                    Rights = rights.Aggregate(0, (current, right) => current | (int)right)
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
                return subjectId == null ? entityPermissionRepository.Get(p => p.Subject == null && p.Entity.Id == entityId && p.Key == key).Count == 1 : entityPermissionRepository.Get(p => p.Subject.Id == subjectId && p.Entity.Id == entityId && p.Key == key).Count == 1;
            }
        }

        public EntityPermission Find(long? subjectId, long entityId, long instanceId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                return subjectId == null ? entityPermissionRepository.Get(p => p.Subject == null && p.Entity.Id == entityId && p.Key == instanceId).FirstOrDefault() : entityPermissionRepository.Get(p => p.Subject.Id == subjectId && p.Entity.Id == entityId && p.Key == instanceId).FirstOrDefault();
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
            using (var uow = this.GetUnitOfWork())
            {
                if (string.IsNullOrEmpty(subjectName))
                    return new List<long>();

                if (string.IsNullOrEmpty(entityName))
                    return new List<long>();

                if (entityType == null)
                    return new List<long>();

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
                        .Where(e => (e.Rights & (int)rightType) > 0)
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
                        (e.Rights & (int)rightType) > 0
                        )
                    .Select(e => e.Key)
                    .ToList();

                return entityPermissionRepository.Query(e =>
                    e.Subject.Id == subjectId &&
                    e.Entity.Id == entityId &&
                    (e.Rights & (int)rightType) > 0
                    )
                .Select(e => e.Key)
                .ToList();
            }
        }

        public int GetEffectiveRights(long? subjectId, long entityId, long key)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();
                var entityPermissionRepository = uow.GetReadOnlyRepository<EntityPermission>();

                var subject = subjectId == null ? null : subjectRepository.Query(s => s.Id == subjectId).FirstOrDefault();

                if (subject is User)
                {
                    var user = subject as User;
                    var subjectIds = new List<long>() { user.Id };
                    subjectIds.AddRange(user.Groups.Select(g => g.Id).ToList());
                    var rights = entityPermissionRepository.Get(m => subjectIds.Contains(m.Subject.Id) && m.Entity.Id == entityId && m.Key == key).Select(e => e.Rights).ToList();
                    return rights.Aggregate(0, (left, right) => left | right);
                }

                return entityPermissionRepository.Get(m => m.Subject.Id == subject.Id && m.Entity.Id == entityId && m.Key == key).FirstOrDefault()?.Rights ?? 0;
            }
        }

        public int GetRights(Subject subject, Entity entity, long key)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetReadOnlyRepository<EntityPermission>();

                if (subject == null)
                {
                    return entityPermissionRepository.Get(m => m.Subject == null && m.Entity.Id == entity.Id && m.Key == key).FirstOrDefault()?.Rights ?? 0;
                }
                if (entity == null)
                    return 0;
                return entityPermissionRepository.Get(m => m.Subject.Id == subject.Id && m.Entity.Id == entity.Id && m.Key == key).FirstOrDefault()?.Rights ?? 0;
            }
        }

        public int GetRights(long? subjectId, long entityId, long key)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();
                var entityRepository = uow.GetReadOnlyRepository<Entity>();

                var subject = subjectId == null ? null : subjectRepository.Query(s => s.Id == subjectId).FirstOrDefault();
                var entity = entityRepository.Get(entityId);
                return GetRights(subject, entity, key);
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

                return (GetRights(subject, entity, key) & (int)rightType) > 0;
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

                return (GetRights(subject, entity, key) & (int)rightType) > 0;
            }
        }

        public void Update(EntityPermission entity)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<EntityPermission>();
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