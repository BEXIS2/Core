using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using System;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Authorization
{
    public class FeaturePermissionManager
    {
        public FeaturePermissionManager()
        {
            var uow = this.GetUnitOfWork();

            FeaturePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();
            FeatureRepository = uow.GetReadOnlyRepository<Feature>();
            SubjectRepository = uow.GetReadOnlyRepository<Subject>();
        }

        public IReadOnlyRepository<FeaturePermission> FeaturePermissionRepository { get; }
        public IQueryable<FeaturePermission> FeaturePermissions => FeaturePermissionRepository.Query();
        public IReadOnlyRepository<Feature> FeatureRepository { get; }
        public IReadOnlyRepository<Subject> SubjectRepository { get; }

        public void Create(FeaturePermission featurePermission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetRepository<FeaturePermission>();
                featurePermissionRepository.Put(featurePermission);
                uow.Commit();
            }
        }

        public void Create(long subjectId, long featureId, PermissionType permissionType = PermissionType.Grant)
        {
            var featurePermission = new FeaturePermission()
            {
                Subject = SubjectRepository.Get(subjectId),
                Feature = FeatureRepository.Get(featureId),
                PermissionType = permissionType
            };

            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetRepository<FeaturePermission>();
                featurePermissionRepository.Put(featurePermission);
                uow.Commit();
            }
        }

        public void Delete(FeaturePermission featurePermission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetRepository<FeaturePermission>();
                featurePermissionRepository.Delete(featurePermission);
                uow.Commit();
            }
        }

        public FeaturePermission Find(Feature feature, Subject subject)
        {
            return
                FeaturePermissionRepository.Query(f => f.Feature.Id == feature.Id && f.Subject.Id == subject.Id)
                    .FirstOrDefault();
        }

        public FeaturePermission FindById(long id)
        {
            return FeaturePermissionRepository.Get(id);
        }

        public bool HasAccess(string subjectName, Type subjecType, string module, string controller, string action)
        {
            return false;
        }

        public void Update(FeaturePermission featurePermission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetRepository<FeaturePermission>();
                featurePermissionRepository.Put(featurePermission);
                uow.Commit();
            }
        }
    }
}