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
    public class EntityPermissionManager
    {
        public EntityPermissionManager()
        {
            var uow = this.GetUnitOfWork();

            EntityPermissionRepository = uow.GetReadOnlyRepository<EntityPermission>();
            EntityRepository = uow.GetReadOnlyRepository<Entity>();
            SubjectRepository = uow.GetReadOnlyRepository<Subject>();
        }

        public IReadOnlyRepository<EntityPermission> EntityPermissionRepository { get; }
        public IQueryable<EntityPermission> EntityPermissions => EntityPermissionRepository.Query();
        public IReadOnlyRepository<Entity> EntityRepository { get; }
        public IReadOnlyRepository<Subject> SubjectRepository { get; }

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
            var entityPermission = new EntityPermission()
            {
                Subject = subjectId == null ? null : SubjectRepository.Query(s => s.Id == subjectId).FirstOrDefault(),
                Entity = EntityRepository.Get(entityId),
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

        public EntityPermission Create<T>(string subjectName, string entityName, Type entityType, long key, List<RightType> rights) where T : Subject
        {
            if (string.IsNullOrEmpty(subjectName))
                return null;

            if (string.IsNullOrEmpty(entityName))
                return null;

            if (entityType == null)
                return null;

            var subject = SubjectRepository.Query(s => s.Name.ToUpperInvariant() == subjectName.ToUpperInvariant() && s is T).FirstOrDefault();
            if (subject == null)
                return null;

            var entity = EntityRepository.Query(e => e.Name.ToUpperInvariant() == entityName.ToUpperInvariant() && e.EntityType == entityType).FirstOrDefault();
            if (entity == null)
                return null;

            if (Exists(subject, entity))
                return null;

            var entityPermission = new EntityPermission()
            {
                Subject = subject,
                Entity = entity,
                Key = key,
                Rights = rights.ToInt()
            };

            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Put(entityPermission);
                uow.Commit();
            }

            return entityPermission;
        }

        public bool Exists(Subject subject, Entity entity)
        {
            if (entity == null)
                return false;

            if (subject == null)
                return EntityPermissionRepository.Get(p => p.Subject == null && p.Entity.Id == entity.Id).Count == 1;

            return EntityPermissionRepository.Get(p => p.Subject.Id == subject.Id && p.Entity.Id == entity.Id).Count == 1;
        }

        public bool Exists(long? subjectId, long entityId)
        {
            if (subjectId == null)
                return EntityPermissionRepository.Get(p => p.Subject == null && p.Entity.Id == entityId).Count == 1;
            return EntityPermissionRepository.Get(p => p.Subject.Id == subjectId && p.Entity.Id == entityId).Count == 1;
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
            var entityPermissions = EntityPermissionRepository.Query(p => p.Entity.EntityType == entityType && p.Key == key) as IEnumerable<EntityPermission>;

            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Delete(entityPermissions);
                uow.Commit();
            }
        }

        public void Delete(long entityPermissionId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Delete(EntityPermissionRepository.Get(entityPermissionId));
                uow.Commit();
            }
        }

        public EntityPermission FindById(long entityPermissionId)
        {
            return EntityPermissionRepository.Get(entityPermissionId);
        }

        public List<long> GetKeys<T>(string subjectName, string entityName, Type entityType, RightType rightType) where T : Subject
        {
            if (string.IsNullOrEmpty(subjectName))
                return new List<long>();

            if (string.IsNullOrEmpty(entityName))
                return new List<long>();

            if (entityType == null)
                return new List<long>();

            var subject = SubjectRepository.Query(s => s.Name.ToUpperInvariant() == subjectName.ToUpperInvariant() && s is T).FirstOrDefault();
            if (subject == null)
                return new List<long>();

            var entity = EntityRepository.Query(e => e.Name.ToUpperInvariant() == entityName.ToUpperInvariant() && e.EntityType == entityType).FirstOrDefault();
            if (entity == null)
                return new List<long>();

            return
                EntityPermissionRepository
                    .Query(e => e.Subject.Id == subject.Id && e.Entity.Id == entity.Id).AsEnumerable()
                    .Where(e => e.Rights.ToRightTypes().Contains(rightType))
                    .Select(e => e.Key)
                    .ToList();
        }

        public List<long> GetKeys(long? subjectId, long entityId, RightType rightType)
        {
            if (subjectId == null)
                return EntityPermissionRepository.Query(e =>
                    e.Subject == null &&
                    e.Entity.Id == entityId &&
                    e.Rights.ToRightTypes().Contains(rightType)
                    )
                .Select(e => e.Key)
                .ToList();

            return EntityPermissionRepository.Query(e =>
                e.Subject.Id == subjectId &&
                e.Entity.Id == entityId &&
                e.Rights.ToRightTypes().Contains(rightType)
                )
            .Select(e => e.Key)
            .ToList();
        }

        public int GetRights(Subject subject, Entity entity, long key)
        {
            if (subject == null)
            {
                return EntityPermissionRepository.Get(m => m.Subject == null && m.Entity.Id == entity.Id).FirstOrDefault()?.Rights ?? 0;
            }

            if (entity == null)
                return 0;

            return EntityPermissionRepository.Get(m => m.Subject.Id == subject.Id && m.Entity.Id == entity.Id).FirstOrDefault()?.Rights ?? 0;
        }

        public List<RightType> GetRights<T>(string subjectName, string entityName, Type entityType, long key) where T : Subject
        {
            if (string.IsNullOrEmpty(subjectName))
                return new List<RightType>();

            if (string.IsNullOrEmpty(entityName))
                return new List<RightType>();

            if (entityType == null)
                return new List<RightType>();

            var subject = SubjectRepository.Query(s => s.Name.ToUpperInvariant() == subjectName.ToUpperInvariant() && s is T).FirstOrDefault();
            if (subject == null)
                return new List<RightType>();

            var entity = EntityRepository.Query(e => e.Name.ToUpperInvariant() == entityName.ToUpperInvariant() && e.EntityType == entityType).FirstOrDefault();
            if (entity == null)
                return new List<RightType>();

            return GetRights(subject, entity, key).ToRightTypes();
        }

        public List<RightType> GetRights(long? subjectId, long entityId, long key)
        {
            var subject = subjectId == null ? null : SubjectRepository.Query(s => s.Id == subjectId).FirstOrDefault();
            var entity = EntityRepository.Get(entityId);
            return GetRights(subject, entity, key).ToRightTypes();
        }

        public bool HasRight<T>(string subjectName, string entityName, Type entityType, long key, RightType rightType) where T : Subject
        {
            var subject = SubjectRepository.Query(s => s.Name.ToUpperInvariant() == subjectName.ToUpperInvariant() && s is T).FirstOrDefault();
            var entity = EntityRepository.Query(e => e.Name.ToUpperInvariant() == entityName.ToUpperInvariant() && e.EntityType == entityType).FirstOrDefault();

            var binary = Convert.ToString(GetRights(subject, entity, key), 2);
            return (int)rightType < binary.Length && binary.ElementAt((binary.Length - 1) - (int)rightType) == '1';
        }

        public bool HasRight(long? subjectId, long entityId, long key, RightType rightType)
        {
            var subject = subjectId == null ? null : SubjectRepository.Query(s => s.Id == subjectId).FirstOrDefault();
            var entity = EntityRepository.Get(entityId);

            var binary = Convert.ToString(GetRights(subject, entity, key), 2);
            return (int)rightType < binary.Length && binary.ElementAt((binary.Length - 1) - (int)rightType) == '1';
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
    }
}