using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<bool> CreateAsync(long? subjectId, long featureId, PermissionType permissionType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featureRepository = uow.GetReadOnlyRepository<Feature>();
                var subjectRepository = uow.GetReadOnlyRepository<Subject>();

                if (await ExistsAsync(subjectId, featureId, permissionType)) return await Task.FromResult(false);

                var featurePermission = new FeaturePermission
                {
                    Feature = featureRepository.Get(featureId),
                    PermissionType = permissionType,
                    Subject = subjectId == null ? null : subjectRepository.Query(s => s.Id == subjectId).FirstOrDefault()
                };

                var featurePermissionRepository = uow.GetRepository<FeaturePermission>();
                 var result = featurePermissionRepository.Put(featurePermission);
                uow.Commit();

                return await Task.FromResult(result);
            }
        }

        public async Task<bool> CreateAsync(Subject subject, Feature feature, PermissionType permissionType = PermissionType.Grant)
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
                var result = featurePermissionRepository.Put(featurePermission);
                uow.Commit();

                return await Task.FromResult(result);
            }
        }

        public async Task<bool> DeleteAsync(FeaturePermission featurePermission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetRepository<FeaturePermission>();
                var result = featurePermissionRepository.Delete(featurePermission);
                uow.Commit();

                return await Task.FromResult(result);
            }
        }

        public async Task<bool> DeleteAsync(long? subjectId, long featureId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermission = await FindAsync(subjectId, featureId);

                if (featurePermission == null) return await Task.FromResult(false);

                var featurePermissionRepository = uow.GetRepository<FeaturePermission>();
                var result = featurePermissionRepository.Delete(featurePermission);
                uow.Commit();

                return await Task.FromResult(result);
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public async Task<bool> ExistsAsync(Subject subject, Feature feature, PermissionType permissionType)
        {
            var featurePermissionRepository = _guow.GetReadOnlyRepository<FeaturePermission>();

            if (feature == null)
                return await Task.FromResult(false);

            if (subject == null)
                return await Task.FromResult(featurePermissionRepository.Query(p => p.Subject == null && p.Feature.Id == feature.Id && p.PermissionType == permissionType).Count() == 1);

            return await Task.FromResult(featurePermissionRepository.Query(p => p.Subject.Id == subject.Id && p.Feature.Id == feature.Id && p.PermissionType == permissionType).Count() == 1);
        }

        public async Task<bool> ExistsAsync(long? subjectId, long featureId, PermissionType permissionType)
        {
            var featurePermissionRepository = _guow.GetReadOnlyRepository<FeaturePermission>();

            if (subjectId == null)
                return await Task.FromResult(featurePermissionRepository.Query(p => p.Subject == null && p.Feature.Id == featureId && p.PermissionType == permissionType).Count() == 1);
            return await Task.FromResult(featurePermissionRepository.Query(p => p.Subject.Id == subjectId && p.Feature.Id == featureId && p.PermissionType == permissionType).Count() == 1);
        }

        public async Task<bool> ExistsAsync(long? subjectId, long featureId)
        {
            var featurePermissionRepository = _guow.GetReadOnlyRepository<FeaturePermission>();

            if (subjectId == null)
                return await Task.FromResult(featurePermissionRepository.Query(p => p.Subject == null && p.Feature.Id == featureId).Count() == 1);

            return await Task.FromResult(featurePermissionRepository.Query(p => p.Subject.Id == subjectId && p.Feature.Id == featureId).Count() == 1);
        }

        public async Task<bool> ExistsAsync(IEnumerable<long> subjectIds, IEnumerable<long> featureIds, PermissionType permissionType)
        {
            var featurePermissionRepository = _guow.GetReadOnlyRepository<FeaturePermission>();
            return await Task.FromResult(featurePermissionRepository.Query(p => featureIds.Contains(p.Feature.Id) && subjectIds.Contains(p.Subject.Id) && p.PermissionType == permissionType).Any());
        }

        public async Task<FeaturePermission> FindAsync(long? subjectId, long featureId)
        {
            var featurePermissionRepository = _guow.GetReadOnlyRepository<FeaturePermission>();
            return subjectId == null ? await Task.FromResult(featurePermissionRepository.Query(f => f.Subject == null && f.Feature.Id == featureId).FirstOrDefault()) : await Task.FromResult(featurePermissionRepository.Query(f => f.Feature.Id == featureId && f.Subject.Id == subjectId).FirstOrDefault());
        }

        public async Task<FeaturePermission> FindAsync(Subject subject, Feature feature)
        {
            var featurePermissionRepository = _guow.GetReadOnlyRepository<FeaturePermission>();
            return await Task.FromResult(featurePermissionRepository.Query(f => f.Feature.Id == feature.Id && f.Subject.Id == subject.Id).FirstOrDefault());
        }

        public async Task<FeaturePermission> FindByIdAsync(long id)
        {
            var featurePermissionRepository = _guow.GetReadOnlyRepository<FeaturePermission>();
            return await Task.FromResult(featurePermissionRepository.Get(id));
        }

        public async Task<int> GetPermissionTypeAsync(long subjectId, long featureId)
        {
            var featurePermission = await FindAsync(subjectId, featureId);

            if (featurePermission != null)
            {
                return await Task.FromResult<int>((int)featurePermission.PermissionType);
            }

            return await Task.FromResult(0);
        }

        public async Task<Dictionary<long, int>> GetPermissionTypeAsync(IEnumerable<long> subjectIds, long featureId)
        {
            Dictionary<long, int> tmp = new Dictionary<long, int>();

            foreach (var subjectId in subjectIds)
            {
                var featurePermissionRepository = _guow.GetReadOnlyRepository<FeaturePermission>();
                var featurePermission = featurePermissionRepository.Query(f => f.Feature.Id == featureId && f.Subject.Id == subjectId).FirstOrDefault();

                tmp.Add(subjectId, featurePermission != null ? (int)featurePermission.PermissionType : 2);
            }

            return await Task.FromResult(tmp);
        }

        public async Task<bool> HasAccessAsync(long? subjectId, long featureId)
        {
            var featureRepository = _guow.GetReadOnlyRepository<Feature>();
            var subjectRepository = _guow.GetReadOnlyRepository<Subject>();

            var feature = featureRepository.Get(featureId);
            var subject = subjectId == null ? null : subjectRepository.Query(s => s.Id == subjectId).FirstOrDefault();

            // Anonymous
            if (subject == null)
            {
                while (feature != null)
                {
                    if (await ExistsAsync(null, feature.Id, PermissionType.Grant))
                        return true;

                    feature = feature.Parent;
                }

                return false;
            }

            // Non-Anonymous
            while (feature != null)
            {
                if (await ExistsAsync(null, feature.Id, PermissionType.Grant))
                    return true;

                if (await ExistsAsync(subject.Id, feature.Id, PermissionType.Deny))
                    return false;

                if (await ExistsAsync(subject.Id, feature.Id, PermissionType.Grant))
                    return true;

                if (subject is User)
                {
                    var user = subject as User;
                    var groupIds = user.Groups.Select(g => g.Id).ToList();

                    if (await ExistsAsync(groupIds, new[] { feature.Id }, PermissionType.Deny))
                    {
                        return false;
                    }

                    if (await ExistsAsync(groupIds, new[] { feature.Id }, PermissionType.Grant))
                    {
                        return true;
                    }
                }

                feature = feature.Parent;
            }

            return false;
        }

        public async Task<Dictionary<long, bool>> GetAccessListAsync(IEnumerable<Subject> subjects, long featureId)
        {
            Dictionary<long, bool> accessDictionary = new Dictionary<long, bool>();

            // check user rights
            foreach (var subject in subjects)
            {
                if (subject != null)
                    accessDictionary.Add(subject.Id, await HasAccessAsync(subject.Id, featureId));
            }

            return await Task.FromResult(accessDictionary);
        }

        public async Task<bool> HasAccessAsync<T>(string subjectName, string module, string controller, string action) where T : Subject
        {
            var operationRepository = _guow.GetReadOnlyRepository<Operation>();
            var SubjectRepository = _guow.GetReadOnlyRepository<Subject>();

            var operation = operationRepository.Query(x => x.Module.ToUpperInvariant() == module.ToUpperInvariant() && x.Controller.ToUpperInvariant() == controller.ToUpperInvariant() && x.Action.ToUpperInvariant() == action.ToUpperInvariant()).FirstOrDefault();
            if (operation == null) return false;

            var feature = operation?.Feature;
            var subject = SubjectRepository.Query(s => s.Name.ToUpperInvariant() == subjectName.ToUpperInvariant() && s is T).FirstOrDefault();

            //both exits
            if (feature != null)
                return await HasAccessAsync(subject?.Id, feature.Id);

            // operation exist but feature not exist -  operatioen is public
            if (feature == null && subject != null)
                return await Task.FromResult(true);

            // operation exist but the features is null -> operation is public
            // subject = null if no user is logged in
            if (feature == null && subject == null)
                return await Task.FromResult(true);

            return await Task.FromResult(false);
        }

        public async Task<bool> UpdateAsync(FeaturePermission entity)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<FeaturePermission>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                var result = repo.Put(merged);
                uow.Commit();

                return await Task.FromResult(result);
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