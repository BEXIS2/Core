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
    /// <summary>
    /// management of entity permissions
    /// </summary>
    public class EntityPermissionManager
    {
        /// <summary>
        /// base constructor to initialize an entity permission manager
        /// </summary>
        public EntityPermissionManager()
        {
            var uow = this.GetUnitOfWork();

            EntityPermissionRepository = uow.GetReadOnlyRepository<EntityPermission>();
            EntityRepository = uow.GetReadOnlyRepository<Entity>();
            SubjectRepository = uow.GetReadOnlyRepository<Subject>();
        }

        /// <summary>
        /// repository that manages entity permissions
        /// </summary>
        public IReadOnlyRepository<EntityPermission> EntityPermissionRepository { get; }

        /// <summary>
        ///  repository that manages entities
        /// </summary>
        public IReadOnlyRepository<Entity> EntityRepository { get; }

        /// <summary>
        /// repository that manages subjects
        /// </summary>
        public IReadOnlyRepository<Subject> SubjectRepository { get; }

        /// <summary>
        /// property to get all entity permissions as an IQueryable
        /// </summary>
        public IQueryable<EntityPermission> EntityPermissions => EntityPermissionRepository.Query();

        /// <summary>
        /// persist a given entity permission
        /// </summary>
        /// <param name="entityPermission">the entity permission to be persisted</param>
        public void Create(EntityPermission entityPermission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Put(entityPermission);
                uow.Commit();
            }
        }

        /// <summary>
        /// create and persist an entity permission based on necessary input
        /// </summary>
        /// <param name="subject">to whom the entity permission belongs to</param>
        /// <param name="entity">to what type of data the entity permission belongs to</param>
        /// <param name="key">the id of the instance of the entity</param>
        /// <param name="rights">the set of rights the subject should get in respect to the entity instance</param>
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

        /// <summary>
        /// create and persist an entity permission based on necessary input
        /// </summary>
        /// <typeparam name="T">the type of the subject (either user or group)</typeparam>
        /// <param name="subjectName">the name of the subject whom the entity permission should belong to</param>
        /// <param name="entityName">the name of the entity which the entity permission should belong to</param>
        /// <param name="entityType">the type of the entity which the entity permission should belong to</param>
        /// <param name="key">the id of the instance of the entity</param>
        /// <param name="rights">the set of rights the subject should get in respect to the entity instance</param>
        /// <returns>the created and persisted entity permission</returns>
        public EntityPermission Create<T>(string subjectName, string entityName, Type entityType, long key, List<RightType> rights) where T : Subject
        {
            var entityPermission = new EntityPermission()
            {
                Subject = SubjectRepository.Query(s => s.Name.ToUpperInvariant() == subjectName.ToUpperInvariant() && s is T).FirstOrDefault(),
                Entity = EntityRepository.Query(e => e.Name.ToUpperInvariant() == entityName.ToUpperInvariant() && e.EntityType == entityType).FirstOrDefault(),
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

        /// <summary>
        /// delete a given entity permission
        /// </summary>
        /// <param name="entityPermission">the entity permission to be deleted</param>
        public void Delete(EntityPermission entityPermission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Delete(entityPermission);
                uow.Commit();
            }
        }

        /// <summary>
        /// delete an entity permission based on the id
        /// </summary>
        /// <param name="entityPermissionId">the id of the entity permission that should be deleted</param>
        public void Delete(long entityPermissionId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                entityPermissionRepository.Delete(EntityPermissionRepository.Get(entityPermissionId));
                uow.Commit();
            }
        }

        /// <summary>
        /// try to get entity permission based on given id
        /// </summary>
        /// <param name="entityPermissionId">the id of the entity permission that should be deleted</param>
        /// <returns>the entity permission with id = entityPermissionId or null (if the id doesn't exist)</returns>
        public EntityPermission FindById(long entityPermissionId)
        {
            return EntityPermissionRepository.Get(entityPermissionId);
        }

        /// <summary>
        /// get a list of entity ids which belong to a given subject and entity in respect to a certain right type
        /// </summary>
        /// <typeparam name="T">the type of the subject (either user or group)</typeparam>
        /// <param name="subjectName">the name of the subject whom the entity permissions should belong to</param>
        /// <param name="entityName">the name of the entity which the entity permissions should belong to</param>
        /// <param name="entityType">the type of the entity which the entity permissions should belong to</param>
        /// <param name="rightType">the right type which the entity permissions should satisfy</param>
        /// <returns>a list of entity instance ids</returns>
        public List<long> GetKeys<T>(string subjectName, string entityName, Type entityType, RightType rightType) where T : Subject
        {
            var subject = SubjectRepository.Query(s => s.Name.ToUpperInvariant() == subjectName.ToUpperInvariant() && s is T).FirstOrDefault();
            var entity = EntityRepository.Query(e => e.Name.ToUpperInvariant() == entityName.ToUpperInvariant() && e.EntityType == entityType).FirstOrDefault();

            return
                EntityPermissionRepository.Get().Where(
                    e =>
                        e.Subject.Id == subject.Id && e.Entity.Id == entity.Id &&
                        e.Rights.ToRightTypes().Contains(rightType)).Select(e => e.Key).ToList();
        }

        /// <summary>
        /// get a list of entity ids which belong to a given subject and entity in respect to a certain right type
        /// </summary>
        /// <param name="subjectId">the id of the subject whom the entity permissions belongs to</param>
        /// <param name="entityId">the id of the entity which the entity permissions should belong to</param>
        /// <param name="rightType">the right type which the entity permissions should satisfy</param>
        /// <returns>a list of entity instance ids</returns>
        public List<long> GetKeys(long subjectId, long entityId, RightType rightType)
        {
            var subject = SubjectRepository.Get(subjectId);
            var entity = EntityRepository.Get(entityId);

            return EntityPermissionRepository.Query(e =>
                e.Subject.Id == subject.Id &&
                e.Entity.Id == entity.Id &&
                e.Rights.ToRightTypes().Contains(rightType)
                )
                .Select(e => e.Key)
                .ToList();
        }

        public int GetRights(Subject subject, Entity entity, long key)
        {
            var entityPermission = EntityPermissionRepository.Get(m => m.Subject.Id == subject.Id && m.Entity.Id == entity.Id).FirstOrDefault();
            return entityPermission?.Rights ?? 0;
        }

        public List<RightType> GetRights<T>(string subjectName, string entityName, Type entityType, long key) where T : Subject
        {
            var subject = SubjectRepository.Query(s => s.Name.ToUpperInvariant() == subjectName.ToUpperInvariant() && s is T).FirstOrDefault();
            var entity = EntityRepository.Query(e => e.Name.ToUpperInvariant() == entityName.ToUpperInvariant() && e.EntityType == entityType).FirstOrDefault();
            return GetRights(subject, entity, key).ToRightTypes();
        }

        public List<RightType> GetRights(long subjectId, long entityId, long key)
        {
            var subject = SubjectRepository.Get(subjectId);
            var entity = EntityRepository.Get(entityId);
            return GetRights(subject, entity, key).ToRightTypes();
        }

        public bool HasRight<T>(string subjectName, string entityName, Type entityType, long key, RightType rightType) where T : Subject
        {
            var subject = SubjectRepository.Query(s => s.Name.ToUpperInvariant() == subjectName.ToUpperInvariant() && s is T).FirstOrDefault();
            var entity = EntityRepository.Query(e => e.Name.ToUpperInvariant() == entityName.ToUpperInvariant() && e.EntityType == entityType).FirstOrDefault();

            var binary = Convert.ToString(GetRights(subject, entity, key), 2);
            return binary.ElementAt((binary.Length - 1) - (int)rightType) == '1';
        }

        public bool HasRight(long subjectId, long entityId, long key, RightType rightType)
        {
            var subject = SubjectRepository.Get(subjectId);
            var entity = EntityRepository.Get(entityId);

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
    }
}