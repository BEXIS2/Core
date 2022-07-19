using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.FormerMember;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using System;
using System.Linq;
using Vaiona.Persistence.Api;


namespace BExIS.Security.Services.FormerMember
{
    public class FormerMemberFeaturePermissionManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public FormerMemberFeaturePermissionManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            FormerMemberFeaturePermissionRepository = _guow.GetReadOnlyRepository<FeaturePermissionFormerMember>();
        }

        ~FormerMemberFeaturePermissionManager()
        {
            Dispose(true);
        }

        public IReadOnlyRepository<FeaturePermissionFormerMember> FormerMemberFeaturePermissionRepository { get; }

        public IQueryable<FeaturePermissionFormerMember> FormerMemberFeaturePermissions => FormerMemberFeaturePermissionRepository.Query();

        public void Create(long? subjectId, long featureId, BExIS.Security.Entities.Authorization.PermissionType permissionType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featureRepository = uow.GetReadOnlyRepository<Feature>();
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();

                if (Exists(subjectId, featureId, permissionType)) return;

                var formerMemberFeaturePermission = new FeaturePermissionFormerMember
                {
                    Feature = featureRepository.Get(featureId),
                    PermissionType = permissionType,
                    Subject = subjectId == null ? null : subjectRepository.Query(s => s.Id == subjectId).FirstOrDefault()
                };

                var formerMemberFeaturePermissionRepository = uow.GetRepository<FeaturePermissionFormerMember>();
                formerMemberFeaturePermissionRepository.Put(formerMemberFeaturePermission);
                uow.Commit();
            }
        }

        public void Create(Subject subject, Feature feature, PermissionType permissionType = PermissionType.Grant)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();
                var featureRepository = uow.GetReadOnlyRepository<Feature>();

                var formerMemberFeaturePermission = new FeaturePermissionFormerMember()
                {
                    Subject = subjectRepository.Get(subject.Id),
                    Feature = featureRepository.Get(feature.Id),
                    PermissionType = permissionType
                };

                var formerMemberFeaturePermissionRepository = uow.GetRepository<FeaturePermissionFormerMember>();
                formerMemberFeaturePermissionRepository.Put(formerMemberFeaturePermission);
                uow.Commit();
            }
        }

        public void Delete(FeaturePermissionFormerMember formerMemberFeaturePermission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var formerMemberFeaturePermissionRepository = uow.GetRepository<FeaturePermissionFormerMember>();
                formerMemberFeaturePermissionRepository.Delete(formerMemberFeaturePermission);
                uow.Commit();
            }
        }

        public void Delete(long? subjectId, long featureId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var formerMemberFeaturePermission = Find(subjectId, featureId);

                if (formerMemberFeaturePermission == null) return;

                var formerMemberfeaturePermissionRepository = uow.GetRepository<FeaturePermissionFormerMember>();
                formerMemberfeaturePermissionRepository.Delete(formerMemberFeaturePermission);
                uow.Commit();
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public bool Exists(Subject subject, Feature feature, PermissionType permissionType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var formerMemberFeaturePermissionRepository = uow.GetReadOnlyRepository<FeaturePermissionFormerMember>();

                if (feature == null)
                    return false;

                if (subject == null)
                    return formerMemberFeaturePermissionRepository.Get(p => p.Subject == null && p.Feature.Id == feature.Id && p.PermissionType == permissionType).Count == 1;

                return formerMemberFeaturePermissionRepository.Get(p => p.Subject.Id == subject.Id && p.Feature.Id == feature.Id && p.PermissionType == permissionType).Count == 1;
            }
        }

        public bool Exists(long? subjectId, long featureId, PermissionType permissionType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var formerMemberFeaturePermissionRepository = uow.GetReadOnlyRepository<FeaturePermissionFormerMember>();

                if (subjectId == null)
                    return formerMemberFeaturePermissionRepository.Get(p => p.Subject == null && p.Feature.Id == featureId && p.PermissionType == permissionType).Count == 1;
                return formerMemberFeaturePermissionRepository.Get(p => p.Subject.Id == subjectId && p.Feature.Id == featureId && p.PermissionType == permissionType).Count == 1;
            }
        }

       
        public FeaturePermissionFormerMember Find(long? subjectId, long featureId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var formerMemberFeaturePermissionRepository = uow.GetReadOnlyRepository<FeaturePermissionFormerMember>();
                return subjectId == null ? formerMemberFeaturePermissionRepository.Query(f => f.Subject == null && f.Feature.Id == featureId).FirstOrDefault() : formerMemberFeaturePermissionRepository.Query(f => f.Feature.Id == featureId && f.Subject.Id == subjectId).FirstOrDefault();
            }
        }

        public FeaturePermissionFormerMember Find(Subject subject, Feature feature)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var formerMemberFeaturePermissionRepository = uow.GetReadOnlyRepository<FeaturePermissionFormerMember>();
                return formerMemberFeaturePermissionRepository.Query(f => f.Feature.Id == feature.Id && f.Subject.Id == subject.Id).FirstOrDefault();
            }
        }

        public FeaturePermissionFormerMember FindById(long id)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var formerMemberFeaturePermissionRepository = uow.GetReadOnlyRepository<FeaturePermissionFormerMember>();
                return formerMemberFeaturePermissionRepository.Get(id);
            }
        }

        public void Update(FeaturePermissionFormerMember entity)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<FeaturePermissionFormerMember>();
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