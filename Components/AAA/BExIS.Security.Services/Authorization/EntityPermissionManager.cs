using BExIS.Dlm.Entities.Party;
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
            throw new NotImplementedException();
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
                    return entityPermissionRepository.Query(p => p.Subject == null && p.Entity.Id == entity.Id && p.Key == key).Count() == 1;

                return entityPermissionRepository.Query(p => p.Subject.Id == subject.Id && p.Entity.Id == entity.Id && p.Key == key).Count() == 1;
            }
        }

        // Get public status and date of publication
        public Tuple<bool, DateTime> GetPublicAndDate(long entityId, long key)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                var permission = entityPermissionRepository.Query(p => p.Subject == null && p.Entity.Id == entityId && p.Key == key).ToList();

                bool isPublic = false;
                DateTime publicationDate = DateTime.MinValue;

                if (permission.Count == 1)
                {
                    isPublic = true;
                    publicationDate = permission.FirstOrDefault().CreationDate;
                }

                return Tuple.Create(isPublic, publicationDate);
            }
        }

        public bool Exists(long? subjectId, long entityId, long key)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                return subjectId == null ? entityPermissionRepository.Query(p => p.Subject == null && p.Entity.Id == entityId && p.Key == key).Count() == 1 : entityPermissionRepository.Query(p => p.Subject.Id == subjectId && p.Entity.Id == entityId && p.Key == key).Count() == 1;
            }
        }

        public bool Exists(long? subjectId, long entityId, long key, RightType rightType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                return subjectId == null ? entityPermissionRepository.Query(p => p.Subject == null && p.Entity.Id == entityId && p.Key == key && (p.Rights & (int)rightType) > 0).Count() == 1 : entityPermissionRepository.Query(p => p.Subject.Id == subjectId && p.Entity.Id == entityId && p.Key == key && (p.Rights & (int)rightType) > 0).Count() == 1;
            }
        }

        public EntityPermission Find(long? subjectId, long entityId, long instanceId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                return subjectId == null ? entityPermissionRepository.Query(p => p.Subject == null && p.Entity.Id == entityId && p.Key == instanceId).FirstOrDefault() : entityPermissionRepository.Query(p => p.Subject.Id == subjectId && p.Entity.Id == entityId && p.Key == instanceId).FirstOrDefault();
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

        public int GetEffectiveRights(long? subjectId, long entityId, long key)
        {
            using (var uow = this.GetUnitOfWork())
            {
                return (GetEffectiveRights(subjectId, new List<long>() { entityId }, key));
            }
        }

        /// <summary>
        /// get effective rights of subjects(incoming as ids) on a entity,
        /// return a dictionary with subjectid , right
        /// </summary>
        /// <param name="subjectIds"></param>
        /// <param name="entityId"></param>
        /// <param name="key"></param>
        /// <returns>Dictionary<long, int> == subjectId and rights</returns>
        public Dictionary<long, int> GetEffectiveRights(IEnumerable<long> subjectIds, long entityId, long key)
        {
            Dictionary<long, int> tmp = new Dictionary<long, int>();

            using (var uow = this.GetUnitOfWork())
            {
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();
                var entityRepository = uow.GetReadOnlyRepository<Entity>();
                var entityPermissionRepository = uow.GetReadOnlyRepository<EntityPermission>();
                var partyUserRepository = uow.GetReadOnlyRepository<PartyUser>();
                var partyRepository = uow.GetReadOnlyRepository<Party>();
                var partyRelationshipRepository = uow.GetReadOnlyRepository<PartyRelationship>();

                var entity = entityRepository.Get(entityId);

                foreach (var subjectId in subjectIds)
                {
                    var subject = subjectRepository.Get(subjectId);

                    int effectiveRights = getEffectiveRights(subject, new List<Entity>() { entity }, key, entityPermissionRepository, partyUserRepository, partyRepository, partyRelationshipRepository);

                    tmp.Add(subjectId, effectiveRights);
                }
            }

            return tmp;
        }

        /// <summary>
        /// get effective rights of subjects on a entity,
        /// return a dictionary with subjectid , right
        /// </summary>
        /// <param name="subjects"></param>
        /// <param name="entityId"></param>
        /// <param name="key"></param>
        /// <returns>Dictionary<long, int> == subjectId and rights</returns>
        public Dictionary<long, int> GetEffectiveRights(IEnumerable<Subject> subjects, long entityId, long key)
        {
            Dictionary<long, int> tmp = new Dictionary<long, int>();

            using (var uow = this.GetUnitOfWork())
            {
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();
                var entityRepository = uow.GetReadOnlyRepository<Entity>();
                var entityPermissionRepository = uow.GetReadOnlyRepository<EntityPermission>();
                var partyUserRepository = uow.GetReadOnlyRepository<PartyUser>();
                var partyRepository = uow.GetReadOnlyRepository<Party>();
                var partyRelationshipRepository = uow.GetReadOnlyRepository<PartyRelationship>();

                var entity = entityRepository.Get(entityId);

                foreach (var subject in subjects)
                {
                    int effectiveRights = getEffectiveRights(subject, new List<Entity>() { entity }, key, entityPermissionRepository, partyUserRepository, partyRepository, partyRelationshipRepository);

                    tmp.Add(subject.Id, effectiveRights);
                }
            }

            return tmp;
        }

        public int GetEffectiveRights(long? subjectId, List<long> entityIds, long key)
        {
            //if (subjectId == null) return 0;

            long id = Convert.ToInt64(subjectId);

            using (var uow = this.GetUnitOfWork())
            {
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();
                var entityRepository = uow.GetReadOnlyRepository<Entity>();
                var entityPermissionRepository = uow.GetReadOnlyRepository<EntityPermission>();
                var partyUserRepository = uow.GetReadOnlyRepository<PartyUser>();
                var partyRepository = uow.GetReadOnlyRepository<Party>();
                var partyRelationshipRepository = uow.GetReadOnlyRepository<PartyRelationship>();

                var subject = subjectRepository.Get(id);
                List<Entity> entities = new List<Entity>();

                foreach (var i in entityIds)
                {
                    entities.Add(entityRepository.Get(i));
                }

                return getEffectiveRights(subject, entities, key, entityPermissionRepository, partyUserRepository, partyRepository, partyRelationshipRepository);
            }
        }

        private int getEffectiveRights(Subject subject, List<Entity> entities, long key,
            IReadOnlyRepository<EntityPermission> entityPermissionRepository,
            IReadOnlyRepository<PartyUser> partyUserRepository,
            IReadOnlyRepository<Party> partyRepository,
            IReadOnlyRepository<PartyRelationship> partyRelationshipRepository)
        {
            //var subject = subject.Id == null ? null : subjectRepository.Query(s => s.Id == subjectId).FirstOrDefault();
            if (entities.Count == 0)
                return 0;

            var eids = entities.Select(e => e.Id);

            var rights = new List<int>
                {
                    entityPermissionRepository.Query(m => m.Subject == null && eids.Contains(m.Entity.Id) && m.Key == key).FirstOrDefault()?.Rights ?? 0
                };

            if (subject is User)
            {
                var partyUser = partyUserRepository.Query(m => m.UserId == subject.Id).FirstOrDefault();

                if (partyUser != null)
                {
                    var userParty = partyRepository.Get(partyUser.PartyId);

                    var entityNames = entities.Select(m => m.Name);

                    var entityParty = partyRepository
                        .Query(m => entityNames.Contains(m.PartyType.Title) && m.Name == key.ToString())
                        .FirstOrDefault();

                    if (userParty != null && entityParty != null)
                    {
                        var partyRelationships = partyRelationshipRepository.Query(m => m.SourceParty.Id == userParty.Id && m.TargetParty.Id == entityParty.Id);

                        rights.AddRange(partyRelationships.Select(m => m.Permission));
                    }
                }

                var user = subject as User;
                var subjectIds = new List<long>() { user.Id };
                subjectIds.AddRange(user.Groups.Select(g => g.Id).ToList());
                rights.AddRange(entityPermissionRepository.Query(m => subjectIds.Contains(m.Subject.Id) && eids.Contains(m.Entity.Id) && m.Key == key).Select(e => e.Rights).ToList());
            }

            if (subject is Group)
            {
                rights.Add(entityPermissionRepository.Query(m => m.Subject.Id == subject.Id && eids.Contains(m.Entity.Id) && m.Key == key).FirstOrDefault()?.Rights ?? 0);
            }

            return rights.Aggregate(0, (left, right) => left | right);
        }

        public IEnumerable<long> GetKeys<T>(string name, string v, Type type, RightType delete)
        {
            throw new NotImplementedException();
        }

        // Entity Null
        // User Null
        public List<long> GetKeys(string userName, string entityName, Type entityType, RightType rightType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                if (entityType == null)
                    return new List<long>();

                if (string.IsNullOrEmpty(entityName))
                    return new List<long>();

                var entityRepository = uow.GetReadOnlyRepository<Entity>();

                var entity = entityRepository.Query(e => e.Name.ToUpperInvariant() == entityName.ToUpperInvariant() && e.EntityType == entityType).FirstOrDefault();
                if (entity == null)
                    return new List<long>();

                var entityPermissionRepository = uow.GetReadOnlyRepository<EntityPermission>();
                var userRepository = uow.GetReadOnlyRepository<User>();

                var user = userRepository.Query(s => s.Name.ToUpperInvariant() == userName.ToUpperInvariant()).FirstOrDefault();
                if (user == null)
                    return entityPermissionRepository
                        .Query(e => e.Subject == null && e.Entity.Id == entity.Id).AsEnumerable()
                        .Where(e => (e.Rights & (int)rightType) > 0)
                        .Select(e => e.Key)
                        .ToList();

                var subjectIds = new List<long>() { user.Id };
                subjectIds.AddRange(user.Groups.Select(g => g.Id).ToList());

                return
                    entityPermissionRepository
                        .Query(e => (subjectIds.Contains(e.Subject.Id) || e.Subject == null) && e.Entity.Id == entity.Id).AsEnumerable()
                        .Where(e => (e.Rights & (int)rightType) > 0)
                        .Select(e => e.Key)
                        .Distinct()
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

        public int GetRights(Subject subject, Entity entity, long key)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetReadOnlyRepository<EntityPermission>();

                return getRights(subject, entity, key, entityPermissionRepository);
            }
        }

        private int getRights(Subject subject, Entity entity, long key, IReadOnlyRepository<EntityPermission> entityPermissionRepository)
        {
            if (subject == null)
            {
                return entityPermissionRepository.Query(m => m.Subject == null && m.Entity.Id == entity.Id && m.Key == key).FirstOrDefault()?.Rights ?? 0;
            }
            if (entity == null)
                return 0;
            return entityPermissionRepository.Query(m => m.Subject.Id == subject.Id && m.Entity.Id == entity.Id && m.Key == key).FirstOrDefault()?.Rights ?? 0;
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

        public Dictionary<long, int> GetRights(IEnumerable<long> subjectIds, long entityId, long key)
        {
            Dictionary<long, int> tmp = new Dictionary<long, int>();

            using (var uow = this.GetUnitOfWork())
            {
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();
                var entityRepository = uow.GetReadOnlyRepository<Entity>();
                var entityPermissionRepository = uow.GetReadOnlyRepository<EntityPermission>();
                var entity = entityRepository.Get(entityId);

                foreach (var subjectId in subjectIds)
                {
                    var subject = subjectId == null ? null : subjectRepository.Query(s => s.Id == subjectId).FirstOrDefault();

                    int rights = getRights(subject, entity, key, entityPermissionRepository);

                    tmp.Add(subjectId, rights);
                }
            }

            return tmp;
        }

        public Dictionary<long, int> GetRights(IEnumerable<Subject> subjects, long entityId, long key)
        {
            Dictionary<long, int> tmp = new Dictionary<long, int>();

            using (var uow = this.GetUnitOfWork())
            {
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();
                var entityRepository = uow.GetReadOnlyRepository<Entity>();
                var entityPermissionRepository = uow.GetReadOnlyRepository<EntityPermission>();

                var entity = entityRepository.Get(entityId);

                foreach (var subject in subjects)
                {
                    int rights = getRights(subject, entity, key, entityPermissionRepository);

                    tmp.Add(subject.Id, rights);
                }
            }

            return tmp;
        }

        public bool HasEffectiveRight(Subject subject, List<Entity> entities, long key, RightType rightType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetReadOnlyRepository<EntityPermission>();

                if (subject == null)
                {
                    return (entityPermissionRepository.Query(m => m.Subject == null && entities.Select(e => e.Id).Contains(m.Entity.Id) && m.Key == key).FirstOrDefault()?.Rights & (int)rightType) > 0;
                }
                if (entities.Count != 1)
                    return false;

                return (GetEffectiveRights(subject.Id, entities.Select(e => e.Id).ToList(), key) & (int)rightType) > 0;
            }
        }

        public bool HasEffectiveRight(string username, Type entityType, long key, RightType rightType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetReadOnlyRepository<User>();
                var entityRepository = uow.GetReadOnlyRepository<Entity>();

                var user = userRepository.Query(s => s.Name.ToUpperInvariant() == username.ToUpperInvariant()).FirstOrDefault();
                var entities = entityRepository.Query(e => e.EntityType == entityType).Select(e => e.Id).ToList();

                return HasEffectiveRight(user?.Id, entities, key, rightType);
            }
        }

        public bool HasEffectiveRight(long? subjectId, List<long> entityIds, long key, RightType rightType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                return (GetEffectiveRights(subjectId, entityIds, key) & (int)rightType) > 0;
            }
        }

        public string[] GetRights(short rights)
        {
            return Enum.GetNames(typeof(RightType)).Select(n => n)
                .Where(n => (rights & (int)Enum.Parse(typeof(RightType), n)) > 0).ToArray();
        }

        public short GetRights(string[] rights)
        {
            throw new NotImplementedException();
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

        public bool HasRight(long? subjectId, List<long> entityIds, long key, RightType rightType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();

                if (entityIds.Count != 1)
                    return false;

                return subjectId == null ? (entityPermissionRepository.Query(p => p.Subject == null && entityIds.Contains(p.Entity.Id) && p.Key == key).FirstOrDefault()?.Rights & (int)rightType) > 0 : (entityPermissionRepository.Query(p => (p.Subject.Id == subjectId || p.Subject == null) && entityIds.Contains(p.Entity.Id) && p.Key == key).FirstOrDefault()?.Rights & (int)rightType) > 0;
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
            if (_isDisposed || !disposing) return;
            _guow?.Dispose();
            _isDisposed = true;
        }
    }
}