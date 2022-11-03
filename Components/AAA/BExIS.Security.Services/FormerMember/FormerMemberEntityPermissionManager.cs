using BExIS.Security.Entities.FormerMember;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using System;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.FormerMember
{
    public class FormerMemberEntityPermissionManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public FormerMemberEntityPermissionManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            FormerMemberEntityPermissionRepository = _guow.GetReadOnlyRepository<EntityPermissionFormerMember>();
        }

        ~FormerMemberEntityPermissionManager()
        {
            Dispose(true);
        }

        public IReadOnlyRepository<EntityPermissionFormerMember> FormerMemberEntityPermissionRepository { get; }
        public IQueryable<EntityPermissionFormerMember> EntityPermissions => FormerMemberEntityPermissionRepository.Query();

        public void Create(EntityPermissionFormerMember formerMemberEntityPermission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var entityPermissionRepository = uow.GetRepository<EntityPermissionFormerMember>();
                entityPermissionRepository.Put(formerMemberEntityPermission);
                uow.Commit();
            }
        }

        public void Create(Subject subject, Entity entity, long key, int rights)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var alumniEntityPermission = new EntityPermissionFormerMember()
                {
                    Subject = subject,
                    Entity = entity,
                    Key = key,
                    Rights = rights
                };

                var alumniEntityPermissionRepository = uow.GetRepository<EntityPermissionFormerMember>();
                alumniEntityPermissionRepository.Put(alumniEntityPermission);
                uow.Commit();
            }
        }
     
        public void Delete(EntityPermissionFormerMember formerMemberEntityPermission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var formerMemberEntityPermissionRepository = uow.GetRepository<EntityPermissionFormerMember>();
                formerMemberEntityPermissionRepository.Delete(formerMemberEntityPermission);
                uow.Commit();
            }
        }

        public void Delete(long formerMemberEntityPermissionId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var formerMemberEntityPermissionRepository = uow.GetRepository<EntityPermissionFormerMember>();
                formerMemberEntityPermissionRepository.Delete(formerMemberEntityPermissionRepository.Get(formerMemberEntityPermissionId));
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
                var formerMemberEntityPermissionRepository = uow.GetRepository<EntityPermissionFormerMember>();

                if (entity == null)
                    return false;

                if (subject == null)
                    return formerMemberEntityPermissionRepository.Get(p => p.Subject == null && p.Entity.Id == entity.Id && p.Key == key).Count == 1;

                return formerMemberEntityPermissionRepository.Get(p => p.Subject.Id == subject.Id && p.Entity.Id == entity.Id && p.Key == key).Count == 1;
            }
        }

        public EntityPermissionFormerMember Find(long? subjectId, long entityId, long instanceId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var formerMemberEntityPermissionRepository = uow.GetRepository<EntityPermissionFormerMember>();
                return subjectId == null ? formerMemberEntityPermissionRepository.Get(p => p.Subject == null && p.Entity.Id == entityId && p.Key == instanceId).FirstOrDefault() : formerMemberEntityPermissionRepository.Get(p => p.Subject.Id == subjectId && p.Entity.Id == entityId && p.Key == instanceId).FirstOrDefault();
            }
        }

        public EntityPermissionFormerMember FindById(long formerMemberEntityPermissionId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var formerMemberEntityPermissionRepository = uow.GetRepository<EntityPermissionFormerMember>();
                return formerMemberEntityPermissionRepository.Get(formerMemberEntityPermissionId);
            }
        }
      
        public void Update(EntityPermissionFormerMember entity)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<EntityPermissionFormerMember>();
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