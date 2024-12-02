using BExIS.Dlm.Entities.Party;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;

// This comment is needed due to github issues.

namespace BExIS.Security.Services.Authorization
{
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

        public async Task<bool> CreateAsync(EntityPermission entityPermission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                var result = entityPermissionRepository.Put(entityPermission);
                uow.Commit();

                return result;
            }
        }

        public async Task<bool> CreateAsync(Subject subject, Entity entity, long key, int rights)
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
                var result = entityPermissionRepository.Put(entityPermission);
                uow.Commit();

                return result;
            }
        }

        public async Task<bool> CreateAsync(long? subjectId, long entityId, long key, int rights)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRepository = uow.GetReadOnlyRepository<Entity>();
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();
                var entityPermission = new EntityPermission()
                {
                    Subject = subjectId.HasValue ? subjectRepository.Get(subjectId.Value) : null,
                    Entity = entityRepository.Get(entityId),
                    Key = key,
                    Rights = rights
                };

                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                var result = entityPermissionRepository.Put(entityPermission);
                uow.Commit();

                return result;
            }
        }

        public async Task<bool> CreateAsync<T>(string subjectName, string entityName, Type entityType, long key, List<RightType> rights) where T : Subject
        {
            if (string.IsNullOrEmpty(subjectName))
                return false;

            if (string.IsNullOrEmpty(entityName))
                return false;

            if (entityType == null)
                return false;

            using (var uow = this.GetUnitOfWork())
            {
                var entityRepository = uow.GetReadOnlyRepository<Entity>();
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();

                var subjects = subjectRepository.Query(s => s.Name.ToLowerInvariant() == subjectName.ToLowerInvariant() && s is T);

                if (subjects.Count() != 1)
                    return false;

                var subject = subjects.Single();

                if (subject == null)
                    return false;

                var entity = entityRepository.Query(e => e.Name.ToLowerInvariant() == entityName.ToLowerInvariant() && e.EntityType == entityType).FirstOrDefault();

                if (entity == null)
                    return false;

                if (await ExistsAsync(subject, entity, key))
                    return false;

                var entityPermission = new EntityPermission()
                {
                    Subject = subject,
                    Entity = entity,
                    Key = key,
                    Rights = rights.Aggregate(0, (current, right) => current | (int)right)
                };

                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                var result = entityPermissionRepository.Put(entityPermission);
                uow.Commit();

                return result;
            }
        }

        public async Task<bool> DeleteAsync(EntityPermission entityPermission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                var result = entityPermissionRepository.Delete(entityPermission);
                uow.Commit();

                return result;
            }
        }

        public async Task<bool> DeleteAsync(Type entityType, long key)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                var entityPermissions = entityPermissionRepository.Query(p => p.Entity.EntityType == entityType && p.Key == key) as IEnumerable<EntityPermission>;
                var result = entityPermissionRepository.Delete(entityPermissions);
                uow.Commit();

                return result;
            }
        }

        public async Task<bool> DeleteAsync(long entityPermissionId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                var result = entityPermissionRepository.Delete(entityPermissionRepository.Get(entityPermissionId));
                uow.Commit();

                return result;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public async Task<bool> ExistsAsync(long entityId, long key)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                return entityPermissionRepository.Query(p => p.Subject == null && p.Entity.Id == entityId && p.Key == key).Count() == 1;
            }
        }

        public async Task<bool> ExistsAsync(long subjectId, long entityId, long key)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                return entityPermissionRepository.Query(p => p.Subject.Id == subjectId && p.Entity.Id == entityId && p.Key == key).Count() == 1;
            }
        }

        public async Task<bool> ExistsAsync(long entityId, long key, RightType rightType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                return entityPermissionRepository.Query(p => p.Subject == null && p.Entity.Id == entityId && p.Key == key && (p.Rights & (int)rightType) > 0).Count() == 1;
            }
        }

        public async Task<bool> ExistsAsync(long subjectId, long entityId, long key, RightType rightType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                return entityPermissionRepository.Query(p => p.Subject.Id == subjectId && p.Entity.Id == entityId && p.Key == key && (p.Rights & (int)rightType) > 0).Count() == 1;
            }
        }

        public async Task<bool> ExistsAsync(Subject subject, Entity entity, long key)
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

        public async Task<EntityPermission> FindAsync(long entityId, long instanceId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();

                var entityPermissions = entityPermissionRepository.Query(p => p.Subject == null && p.Entity.Id == entityId && p.Key == instanceId);

                if (entityPermissions.Count() != 1)
                    return null;

                return entityPermissions.Single();
            }
        }

        public async Task<EntityPermission> FindAsync(long subjectId, long entityId, long instanceId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();

                var entityPermissions = entityPermissionRepository.Query(p => p.Subject.Id == subjectId && p.Entity.Id == entityId && p.Key == instanceId);

                if (entityPermissions.Count() != 1)
                    return null;

                return entityPermissions.Single();
            }
        }

        public async Task<EntityPermission> FindAsync(long entityPermissionId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();

                return entityPermissionRepository.Get(entityPermissionId);
            }
        }

        public async Task<int> GetEffectiveRightsAsync(long entityId, long key)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();

                var rights = new List<int>
                {
                    await GetRightsAsync(entityId, key),
                };

                return rights.Aggregate(0, (left, right) => left | right);
            }
        }

        public async Task<int> GetEffectiveRightsAsync(long subjectId, long entityId, long key)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                var subjectRepository = uow.GetRepository<Subject>();
                var partyUserRepository = uow.GetRepository<PartyUser>();
                var entityRepository = uow.GetRepository<Entity>();
                var partyRepository = uow.GetRepository<Party>();
                var partyRelationshipRepository = uow.GetRepository<PartyRelationship>();

                var rights = new List<int>
                {
                    // public
                    await GetRightsAsync(entityId, key),

                    // private
                    await GetRightsAsync(subjectId, entityId, key)
                };

                var subject = subjectRepository.Get(subjectId);

                if (subject is User)
                {
                    var partyUser = partyUserRepository.Query(m => m.UserId == subject.Id).FirstOrDefault();

                    if (partyUser != null)
                    {
                        var userParty = partyRepository.Get(partyUser.PartyId);

                        var entityName = entityRepository.Get(entityId).Name;

                        var entityParty = partyRepository
                            .Query(m => entityName.ToLowerInvariant() == m.PartyType.Title.ToLowerInvariant() && m.Name.ToLowerInvariant() == key.ToString().ToLowerInvariant())
                            .FirstOrDefault();

                        if (userParty != null && entityParty != null)
                        {
                            var partyRelationships = partyRelationshipRepository.Query(m => m.SourceParty.Id == userParty.Id && m.TargetParty.Id == entityParty.Id);

                            rights.AddRange(partyRelationships.Select(m => m.Permission));
                        }
                    }

                    var user = subject as User;

                    foreach (var groupId in user.Groups.Select(g => g.Id).ToList())
                    {
                        rights.Add(await GetRightsAsync(groupId, entityId, key));
                    }
                }

                return rights.Aggregate(0, (left, right) => left | right);
            }
        }

        public async Task<Dictionary<long, int>> GetEffectiveRightsAsync(IEnumerable<long> subjectIds, long entityId, long key)
        {
            Dictionary<long, int> rights = new Dictionary<long, int>();

            foreach (var subjectId in subjectIds)
            {
                rights.Add(subjectId, await GetEffectiveRightsAsync(subjectId, entityId, key));
            }

            return rights;
        }

        public async Task<int> GetEffectiveRightsAsync(long subjectId, IEnumerable<long> entityIds, long key)
        {
            int rights = 0;

            foreach (var entityId in entityIds)
            {
                rights = Math.Max(rights, await GetEffectiveRightsAsync(subjectId, entityId, key));
            }

            return rights;
        }

        public async Task<List<long>> GetKeys(string userName, string entityName, Type entityType, RightType rightType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                if (entityType == null)
                    return new List<long>();

                if (string.IsNullOrEmpty(entityName))
                    return new List<long>();

                var entityRepository = uow.GetReadOnlyRepository<Entity>();

                var entities = entityRepository.Query(e => e.Name.ToUpperInvariant() == entityName.ToUpperInvariant() && e.EntityType == entityType);

                if (entities.Count() != 1)
                    return new List<long>();

                var entity = entities.Single();

                var entityPermissionRepository = uow.GetReadOnlyRepository<EntityPermission>();
                var userRepository = uow.GetReadOnlyRepository<User>();

                var user = userRepository.Query(s => s.Name.ToUpperInvariant() == userName.ToUpperInvariant()).FirstOrDefault();
                if (user == null)
                    return await Task.FromResult(entityPermissionRepository
                        .Query(e => e.Subject == null && e.Entity.Id == entity.Id).AsEnumerable()
                        .Where(e => (e.Rights & (int)rightType) > 0)
                        .Select(e => e.Key)
                        .ToList());

                var subjectIds = new List<long>() { user.Id };
                subjectIds.AddRange(user.Groups.Select(g => g.Id).ToList());

                return entityPermissionRepository
                        .Query(e => (subjectIds.Contains(e.Subject.Id) || e.Subject == null) && e.Entity.Id == entity.Id).AsEnumerable()
                        .Where(e => (e.Rights & (int)rightType) > 0)
                        .Select(e => e.Key)
                        .Distinct()
                        .ToList();
            }
        }

        public async Task<List<long>> GetKeysAsync(long entityId, RightType rightType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetReadOnlyRepository<EntityPermission>();

                return entityPermissionRepository.Query(e =>
                        e.Subject == null &&
                        e.Entity.Id == entityId &&
                        (e.Rights & (int)rightType) > 0
                        )
                    .Select(e => e.Key)
                    .ToList();
            }
        }

        public async Task<List<long>> GetKeysAsync(long subjectId, long entityId, RightType rightType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetReadOnlyRepository<EntityPermission>();

                return entityPermissionRepository.Query(e =>
                    e.Subject.Id == subjectId &&
                    e.Entity.Id == entityId &&
                    (e.Rights & (int)rightType) > 0
                    )
                .Select(e => e.Key)
                .ToList();
            }
        }

        [Obsolete]
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

        public async Task<string[]> GetRightsAsync(short rights)
        {
            return Enum.GetNames(typeof(RightType)).Select(n => n)
                .Where(n => (rights & (int)Enum.Parse(typeof(RightType), n)) > 0).ToArray();
        }

        public async Task<Dictionary<long, int>> GetRightsAsync(List<long> subjectIds, long entityId, long key)
        {
            Dictionary<long, int> rights = new Dictionary<long, int>();

            foreach (var subjectId in subjectIds)
            {
                rights.Add(subjectId, await GetRightsAsync(subjectId, entityId, key));
            }

            return rights;
        }

        public async Task<int> GetRightsAsync(long entityId, long key)
        {
            var entityPermission = await FindAsync(entityId, key);
            return entityPermission == null ? 0 : entityPermission.Rights;
        }

        public async Task<int> GetRightsAsync(long subjectId, long entityId, long key)
        {
            var entityPermission = await FindAsync(subjectId, entityId, key);
            return entityPermission == null ? 0 : entityPermission.Rights;
        }

        public async Task<bool> HasEffectiveRightsAsync(string username, Type entityType, long key, RightType rightType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityRepository = uow.GetReadOnlyRepository<Entity>();
                var entityIds = entityRepository.Query(e => e.EntityType == entityType).Select(e => e.Id).ToList();
                if (entityIds.Count == 0)
                    return false;

                if (string.IsNullOrEmpty(username))
                {
                    return await HasEffectiveRightsAsync(0, entityIds, key, rightType);
                }

                var userRepository = uow.GetReadOnlyRepository<User>();
                var userIds = userRepository.Query(s => s.Name.ToLowerInvariant() == username.ToLowerInvariant()).Select(u => u.Id);
                if(userIds.Count() != 1)
                    return await HasEffectiveRightsAsync(0, entityIds, key, rightType);

                return await HasEffectiveRightsAsync(userIds.Single(), entityIds, key, rightType);
            }
        }

        public async Task<bool> HasEffectiveRightsAsync(long entityId, long key, RightType rightType)
        {
            return (await GetEffectiveRightsAsync(entityId, key) & (int)rightType) > 0;
        }

        public async Task<bool> HasEffectiveRightsAsync(long subjectId, long entityId, long key, RightType rightType)
        {
            return (await GetEffectiveRightsAsync(subjectId, entityId, key) & (int)rightType) > 0;
        }

        private async Task<bool> HasEffectiveRightsAsync(long subjectId, List<long> entityIds, long key, RightType rightType)
        {
            return (await GetEffectiveRightsAsync(subjectId, entityIds, key) & (int)rightType) > 0;
        }

        public async Task<bool> HasRightsAsync(long entityId, long key, RightType rightType)
        {
            return (await GetRightsAsync(entityId, key) & (int)rightType) > 0;
        }

        public async Task<bool> HasRightsAsync(long subjectId, long entityId, long key, RightType rightType)
        {
            return (await GetRightsAsync(subjectId, entityId, key) & (int)rightType) > 0;
        }

        public async Task<bool> IsPublicAsync(long entityId, long key)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();

                var permissions = entityPermissionRepository.Query(p => p.Subject == null && p.Entity.Id == entityId && p.Key == key && p.Rights > (int)RightType.Read).ToList();

                return permissions.Count == 1;
            }
        }

        public async Task<bool> IsPublicAsync(List<long> entityIds, long key)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();

                var permissions = entityPermissionRepository.Query(p => p.Subject == null && entityIds.Contains(p.Entity.Id) && p.Key == key && p.Rights > (int)RightType.Read).ToList();

                return permissions.Count == 1;
            }
        }


        public async Task UpdateAsync(EntityPermission entity)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<EntityPermission>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }

            await Task.CompletedTask;
        }

        protected void Dispose(bool disposing)
        {
            if (_isDisposed || !disposing) return;
            _guow?.Dispose();
            _isDisposed = true;
        }
    }
}
