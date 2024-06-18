using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Authorization
{
    public class FeaturePermissionManager : IDisposable
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public FeaturePermissionManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            FeaturePermissionRepository = _guow.GetReadOnlyRepository<FeaturePermission>();
        }

        ~FeaturePermissionManager()
        {
            Dispose(true);
        }

        public IReadOnlyRepository<FeaturePermission> FeaturePermissionRepository { get; }

        public IQueryable<FeaturePermission> FeaturePermissions => FeaturePermissionRepository.Query();

        public void Create(long? subjectId, long featureId, PermissionType permissionType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featureRepository = uow.GetReadOnlyRepository<Feature>();
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();

                if (Exists(subjectId, featureId, permissionType)) return;

                var featurePermission = new FeaturePermission
                {
                    Feature = featureRepository.Get(featureId),
                    PermissionType = permissionType,
                    Subject = subjectId == null ? null : subjectRepository.Query(s => s.Id == subjectId).FirstOrDefault()
                };

                var featurePermissionRepository = uow.GetRepository<FeaturePermission>();
                featurePermissionRepository.Put(featurePermission);
                uow.Commit();
            }
        }

        public void Create(Subject subject, Feature feature, PermissionType permissionType = PermissionType.Grant)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();
                var featureRepository = uow.GetReadOnlyRepository<Feature>();

                var featurePermission = new FeaturePermission()
                {
                    Subject = subjectRepository.Get(subject.Id),
                    Feature = featureRepository.Get(feature.Id),
                    PermissionType = permissionType
                };

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
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermission = Find(subjectId, featureId);

                if (featurePermission == null) return;

                var featurePermissionRepository = uow.GetRepository<FeaturePermission>();
                featurePermissionRepository.Delete(featurePermission);
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
                var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();

                if (feature == null)
                    return false;

                if (subject == null)
                    return featurePermissionRepository.Query(p => p.Subject == null && p.Feature.Id == feature.Id && p.PermissionType == permissionType).Count() == 1;

                return featurePermissionRepository.Query(p => p.Subject.Id == subject.Id && p.Feature.Id == feature.Id && p.PermissionType == permissionType).Count() == 1;
            }
        }

        public bool Exists(long? subjectId, long featureId, PermissionType permissionType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();

                if (subjectId == null)
                    return featurePermissionRepository.Query(p => p.Subject == null && p.Feature.Id == featureId && p.PermissionType == permissionType).Count() == 1;
                return featurePermissionRepository.Query(p => p.Subject.Id == subjectId && p.Feature.Id == featureId && p.PermissionType == permissionType).Count() == 1;
            }
        }

        public bool Exists(long? subjectId, long featureId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();

                if (subjectId == null)
                    return featurePermissionRepository.Query(p => p.Subject == null && p.Feature.Id == featureId).Count() == 1;

                return featurePermissionRepository.Query(p => p.Subject.Id == subjectId && p.Feature.Id == featureId).Count() == 1;
            }
        }

        public bool Exists(IEnumerable<long> subjectIds, IEnumerable<long> featureIds, PermissionType permissionType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();
                return featurePermissionRepository.Query(p => featureIds.Contains(p.Feature.Id) && subjectIds.Contains(p.Subject.Id) && p.PermissionType == permissionType).Any();
            }
        }

        public FeaturePermission Find(long? subjectId, long featureId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();
                return subjectId == null ? featurePermissionRepository.Query(f => f.Subject == null && f.Feature.Id == featureId).FirstOrDefault() : featurePermissionRepository.Query(f => f.Feature.Id == featureId && f.Subject.Id == subjectId).FirstOrDefault();
            }
        }

        public FeaturePermission Find(Subject subject, Feature feature)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();
                return featurePermissionRepository.Query(f => f.Feature.Id == feature.Id && f.Subject.Id == subject.Id).FirstOrDefault();
            }
        }

        public FeaturePermission FindById(long id)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();
                return featurePermissionRepository.Get(id);
            }
        }

        public int GetPermissionType(long subjectId, long featureId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermission = Find(subjectId, featureId);

                if (featurePermission != null)
                {
                    return (int)featurePermission.PermissionType;
                }

                return 2;
            }
        }

        public Dictionary<long, int> GetPermissionType(IEnumerable<long> subjectIds, long featureId)
        {
            Dictionary<long, int> tmp = new Dictionary<long, int>();

            using (var uow = this.GetUnitOfWork())
            {
                foreach (var subjectId in subjectIds)
                {
                    var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();
                    var featurePermission = subjectId == null ? featurePermissionRepository.Query(f => f.Subject == null && f.Feature.Id == featureId).FirstOrDefault() : featurePermissionRepository.Query(f => f.Feature.Id == featureId && f.Subject.Id == subjectId).FirstOrDefault();

                    if (featurePermission != null)
                    {
                        tmp.Add(subjectId, (int)featurePermission.PermissionType);
                    }
                    else
                    {
                        tmp.Add(subjectId, 2);
                    }
                }
            }

            return tmp;
        }

        public bool HasAccess(long? subjectId, long featureId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featureRepository = uow.GetReadOnlyRepository<Feature>();
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();

                var feature = featureRepository.Get(featureId);
                var subject = subjectId == null ? null : subjectRepository.Query(s => s.Id == subjectId).FirstOrDefault();

                // Anonymous
                if (subject == null)
                {
                    while (feature != null)
                    {
                        if (Exists(null, feature.Id, PermissionType.Grant))
                            return true;

                        feature = feature.Parent;
                    }

                    return false;
                }

                // Non-Anonymous
                while (feature != null)
                {
                    if (Exists(null, feature.Id, PermissionType.Grant))
                        return true;

                    if (Exists(subject.Id, feature.Id, PermissionType.Deny))
                        return false;

                    if (Exists(subject.Id, feature.Id, PermissionType.Grant))
                        return true;

                    if (subject is User)
                    {
                        var user = subject as User;
                        var groupIds = user.Groups.Select(g => g.Id).ToList();

                        if (Exists(groupIds, new[] { feature.Id }, PermissionType.Deny))
                        {
                            return false;
                        }

                        if (Exists(groupIds, new[] { feature.Id }, PermissionType.Grant))
                        {
                            return true;
                        }
                    }

                    feature = feature.Parent;
                }

                return false;
            }
        }

        public Dictionary<long, bool> GetAccessList(IEnumerable<Subject> subjects, long featureId)
        {
            Dictionary<long, bool> accessDictionary = new Dictionary<long, bool>();

            // check user rights
            foreach (var subject in subjects)
            {
                if (subject != null)
                    accessDictionary.Add(subject.Id, HasAccess(subject.Id, featureId));
            }

            return accessDictionary;
        }

        public bool HasAccess<T>(string subjectName, string module, string controller, string action) where T : Subject
        {
            using (var uow = this.GetUnitOfWork())
            {
                var operationRepository = uow.GetReadOnlyRepository<Operation>();
                var SubjectRepository = uow.GetReadOnlyRepository<Subject>();

                var operation = operationRepository.Query(x => x.Module.ToUpperInvariant() == module.ToUpperInvariant() && x.Controller.ToUpperInvariant() == controller.ToUpperInvariant() && x.Action.ToUpperInvariant() == action.ToUpperInvariant()).FirstOrDefault();
                if (operation == null) return false;

                var feature = operation?.Feature;
                var subject = SubjectRepository.Query(s => s.Name.ToUpperInvariant() == subjectName.ToUpperInvariant() && s is T).FirstOrDefault();

                //both exits
                if (feature != null)
                    return HasAccess(subject?.Id, feature.Id);

                // operation exist but feature not exist -  operatioen is public
                if (feature == null && subject != null)
                    return true;

                // operation exist but the features is null -> operation is public
                // subject = null if no user is logged in
                if (feature == null && subject == null)
                    return true;

                return false;
            }
        }

        public void Update(FeaturePermission entity)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<FeaturePermission>();
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