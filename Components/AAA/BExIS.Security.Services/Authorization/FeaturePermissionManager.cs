using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using System.Collections.Generic;
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
            OperationRepository = uow.GetReadOnlyRepository<Operation>();
        }

        public IReadOnlyRepository<FeaturePermission> FeaturePermissionRepository { get; }
        public IQueryable<FeaturePermission> FeaturePermissions => FeaturePermissionRepository.Query();
        public IReadOnlyRepository<Feature> FeatureRepository { get; }
        public IReadOnlyRepository<Operation> OperationRepository { get; }
        public IReadOnlyRepository<Subject> SubjectRepository { get; }

        public void Create(long? subjectId, long featureId, PermissionType permissionType)
        {
            if (Exists(subjectId, featureId, permissionType)) return;

            var featurePermission = new FeaturePermission
            {
                Feature = FeatureRepository.Get(featureId),
                PermissionType = permissionType,
                // Sven
                // Workaround:
                // FirstOrDefault may is not the proper method to call. But this is necessary because of possible empty call to "Get()"
                // because subjectId can be null.
                Subject = subjectId == null ? null : SubjectRepository.Query(s => s.Id == subjectId).FirstOrDefault()
            };

            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetRepository<FeaturePermission>();
                featurePermissionRepository.Put(featurePermission);
                uow.Commit();
            }
        }

        public void Create(Subject subject, Feature feature, PermissionType permissionType)
        {
            var featurePermission = new FeaturePermission()
            {
                Subject = subject,
                Feature = feature,
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

        public void Delete(long? subjectId, long featureId)
        {
            var featurePermission = Find(subjectId, featureId);

            if (featurePermission == null) return;

            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetRepository<FeaturePermission>();
                featurePermissionRepository.Delete(featurePermission);
                uow.Commit();
            }
        }

        public bool Exists(Subject subject, Feature feature, PermissionType permissionType)
        {
            if (feature == null)
                return false;

            if (subject == null)
                return FeaturePermissionRepository.Get(p => p.Subject == null && p.Feature.Id == feature.Id && p.PermissionType == permissionType).Count == 1;

            return FeaturePermissionRepository.Get(p => p.Subject.Id == subject.Id && p.Feature.Id == feature.Id && p.PermissionType == permissionType).Count == 1;
        }

        public bool Exists(long? subjectId, long featureId, PermissionType permissionType)
        {
            if (subjectId == null)
                return FeaturePermissionRepository.Get(p => p.Subject == null && p.Feature.Id == featureId && p.PermissionType == permissionType).Count == 1;
            return FeaturePermissionRepository.Get(p => p.Subject.Id == subjectId && p.Feature.Id == featureId && p.PermissionType == permissionType).Count == 1;
        }

        public bool Exists(long? subjectId, long featureId)
        {
            if (subjectId == null)
                return FeaturePermissionRepository.Get(p => p.Subject == null && p.Feature.Id == featureId).Count == 1;

            return FeaturePermissionRepository.Get(p => p.Subject.Id == subjectId && p.Feature.Id == featureId).Count == 1;
        }

        public bool Exists(IEnumerable<long> subjectIds, IEnumerable<long> featureIds, PermissionType permissionType)
        {
            return FeaturePermissionRepository.Query(p => featureIds.Contains(p.Feature.Id) && subjectIds.Contains(p.Subject.Id) && p.PermissionType == permissionType).Any();
        }

        public FeaturePermission Find(long? subjectId, long featureId)
        {
            return subjectId == null ? FeaturePermissionRepository.Query(f => f.Subject == null && f.Feature.Id == featureId).FirstOrDefault() : FeaturePermissionRepository.Query(f => f.Feature.Id == featureId && f.Subject.Id == subjectId).FirstOrDefault();
        }

        public FeaturePermission Find(Subject subject, Feature feature)
        {
            return
                FeaturePermissionRepository.Query(f => f.Feature.Id == feature.Id && f.Subject.Id == subject.Id)
                    .FirstOrDefault();
        }

        public FeaturePermission FindById(long id)
        {
            return FeaturePermissionRepository.Get(id);
        }

        public int GetPermissionType(long subjectId, long featureId)
        {
            var featurePermission = Find(subjectId, featureId);

            if (featurePermission != null)
            {
                return (int)featurePermission.PermissionType;
            }

            return 2;
        }

        public bool HasAccess(long subjectId, long featureId)
        {
            var feature = FeatureRepository.Get(featureId);
            var subject = SubjectRepository.Get(subjectId);

            FeaturePermission featurePermission;

            if (subject is Group)
            {
                while (feature != null)
                {
                    if (Exists(null, feature.Id, PermissionType.Grant))
                        return true;

                    featurePermission = Find(subject.Id, feature.Id);

                    if (featurePermission != null)
                    {
                        return featurePermission.PermissionType == PermissionType.Grant;
                    }

                    feature = feature.Parent;
                }

                return false;
            }

            if (subject is User)
            {
                var user = subject as User;
                var groupIds = user.Groups.Select(g => g.Id).ToList();

                while (feature != null)
                {
                    if (Exists(null, feature.Id, PermissionType.Grant))
                        return true;

                    featurePermission = Find(subjectId, featureId);

                    if (featurePermission != null)
                    {
                        return featurePermission.PermissionType == PermissionType.Grant;
                    }

                    if (Exists(groupIds, new[] { feature.Id }, PermissionType.Deny))
                    {
                        return false;
                    }

                    if (Exists(groupIds, new[] { feature.Id }, PermissionType.Grant))
                    {
                        return true;
                    }

                    feature = feature.Parent;
                }

                return false;
            }

            return false;
        }

        public bool HasAccess<T>(string subjectName, string module, string controller, string action) where T : Subject
        {
            var operation =
                OperationRepository.Query(x => x.Module == module && x.Controller == controller && x.Action == action)
                    .FirstOrDefault();

            var feature = operation?.Workflow.Feature;
            var subject = SubjectRepository.Query(s => s.Name.ToUpperInvariant() == subjectName.ToUpperInvariant() && s is T).FirstOrDefault();
            if (feature != null && subject != null)
                return HasAccess(subject.Id, feature.Id);

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