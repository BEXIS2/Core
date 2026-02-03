using BExIS.Dlm.Entities.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Requests;
using BExIS.Security.Entities.Subjects;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Bcpg.Sig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Authorization
{
    public class FeaturePermissionManager
    {
        public FeaturePermission Create(long? subjectId, long featureId, PermissionType permissionType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featureRepository = uow.GetReadOnlyRepository<Feature>();
                var feature = featureRepository.Get(featureId);

                if (feature == null)
                    return null;

                var subjectRepository = uow.GetReadOnlyRepository<Subject>();

                if (existsInternal(uow, subjectId, featureId, permissionType)) 
                    return null;

                var featurePermission = new FeaturePermission
                {
                    Feature = featureRepository.Get(featureId),
                    PermissionType = permissionType,
                    Subject = subjectId == null ? null : subjectRepository.Query(s => s.Id == subjectId).FirstOrDefault()
                };

                var featurePermissionRepository = uow.GetRepository<FeaturePermission>();
                featurePermissionRepository.Put(featurePermission);
                uow.Commit();

                return featurePermission;
            }
        }

        public FeaturePermission Create(Subject subject, Feature feature, PermissionType permissionType = PermissionType.Grant)
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

                return featurePermission;
            }
        }

        public bool Delete(long? subjectId, long featureId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetRepository<FeaturePermission>();

                var featurePermissions = featurePermissionRepository.Get(f => f.Subject.Id == subjectId && f.Feature.Id == featureId);
                if (featurePermissions.Count() != 1)
                    return false;

                var result = featurePermissionRepository.Delete(featurePermissions.FirstOrDefault().Id);

                uow.Commit();

                return result;
            }
        }

        public bool Exists(Subject subject, Feature feature, PermissionType permissionType)
        {
            if (feature == null)
                return false;

            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();

                if (subject == null)
                    return featurePermissionRepository.Query(p => p.Subject == null && p.Feature.Id == feature.Id && p.PermissionType == permissionType).Take(2).Count() == 1;

                return featurePermissionRepository.Query(p => p.Subject.Id == subject.Id && p.Feature.Id == feature.Id && p.PermissionType == permissionType).Take(2).Count() == 1;
            }
        }

        private bool existsInternal(IUnitOfWork uow, long? subjectId, long featureId, PermissionType permissionType)
        {
            var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();

            if (subjectId == null)
                return featurePermissionRepository.Query(p => p.Subject == null && p.Feature.Id == featureId && p.PermissionType == permissionType).Take(2).Count() == 1;

            return featurePermissionRepository.Query(p => p.Subject.Id == subjectId && p.Feature.Id == featureId && p.PermissionType == permissionType).Take(2).Count() == 1;
        }

        public bool Exists(long? subjectId, long featureId, PermissionType permissionType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                return existsInternal(uow, subjectId, featureId, permissionType);
            }
        }

        public bool Exists(long? subjectId, long featureId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();

                if (subjectId == null)
                    return featurePermissionRepository.Query(p => p.Subject == null && p.Feature.Id == featureId).Take(2).Count() == 1;

                return featurePermissionRepository.Query(p => p.Subject.Id == subjectId && p.Feature.Id == featureId).Take(2).Count() == 1;
            }
        }

        public bool ExistsAny(IEnumerable<long> subjectIds, IEnumerable<long> featureIds, PermissionType permissionType)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();

                return featurePermissionRepository.Query(p => featureIds.Contains(p.Feature.Id) && subjectIds.Contains(p.Subject.Id) && p.PermissionType == permissionType).Any();
            }
        }

        public FeaturePermission Get(long? subjectId, long featureId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();
                return subjectId == null ? featurePermissionRepository.Query(f => f.Subject == null && f.Feature.Id == featureId).FirstOrDefault() : featurePermissionRepository.Query(f => f.Feature.Id == featureId && f.Subject.Id == subjectId).FirstOrDefault();
            }
        }

        public List<FeaturePermission> Get()
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();
                return featurePermissionRepository.Query().ToList();
            }
        }

        public List<FeaturePermission> Get(Expression<Func<FeaturePermission, bool>> predicate)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();
                return featurePermissionRepository.Query(predicate).ToList();
            }
        }

        public long GetId(long? subjectId, long featureId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();
                return featurePermissionRepository.Query(f => f.Subject == null && f.Feature.Id == featureId).Select(f => f.Id).SingleOrDefault();
            }
        }

        public FeaturePermission Get(Subject subject, Feature feature)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();
                return featurePermissionRepository.Query(f => f.Feature.Id == feature.Id && f.Subject.Id == subject.Id).FirstOrDefault();
            }
        }

        public FeaturePermission Get(long id)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();
                return featurePermissionRepository.Get(id);
            }
        }

        public int GetPermissionType(long? subjectId, long featureId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();

                var featurePermissions = featurePermissionRepository.Query(f => f.Feature.Id == featureId && (subjectId == null ? f.Subject == null : f.Subject.Id == subjectId)).Take(2);

                if (featurePermissions.Count() == 1)
                {
                    return (int)featurePermissions.FirstOrDefault().PermissionType;
                }

                return 0;
            }
        }

        public Dictionary<long, int> GetPermissionTypes(IEnumerable<long> subjectIds, long featureId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();

                var featurePermissions = featurePermissionRepository.Query(f => f.Feature.Id == featureId && subjectIds.Contains(f.Subject.Id)).ToList();

                return subjectIds.ToDictionary(
                    id => id,
                    id => (int?)featurePermissions.FirstOrDefault(p => p.Subject.Id == id)?.PermissionType ?? 2
                );
            }
        }

        public bool HasAccess(long? subjectId, long featureId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                return hasAccessInternal(uow, subjectId, featureId);
            }
        }

        private bool hasAccessInternal(IUnitOfWork uow, long? subjectId, long featureId)
        {
            var featureRepository = uow.GetReadOnlyRepository<Feature>();
            var subjectRepository = uow.GetReadOnlyRepository<Subject>();
            var featurePermissionRepository = uow.GetReadOnlyRepository<FeaturePermission>();
            var feature = featureRepository.Get(featureId);
            var subject = subjectId == null ? null : subjectRepository.Query(s => s.Id == subjectId).FirstOrDefault();
            // Anonymous
            if (subject == null)
            {
                if (featurePermissionRepository.Query(p => p.Subject == null && p.Feature.Id == feature.Id && p.PermissionType == PermissionType.Grant).Take(2).Count() == 1)
                    return true;
                return false;
            }
            // Non-Anonymous

            while(feature != null)
            {
                if (featurePermissionRepository.Query(p => p.Subject == null && p.Feature.Id == feature.Id && p.PermissionType == PermissionType.Grant).Take(2).Count() == 1)
                    return true;
                if (featurePermissionRepository.Query(p => p.Subject.Id == subjectId && p.Feature.Id == feature.Id && p.PermissionType == PermissionType.Deny).Take(2).Count() == 1)
                    return false;
                if (featurePermissionRepository.Query(p => p.Subject.Id == subjectId && p.Feature.Id == feature.Id && p.PermissionType == PermissionType.Grant).Take(2).Count() == 1)
                    return true;
                if (subject is User)
                {
                    var user = subject as User;
                    var groupIds = user.Groups.Select(g => g.Id).ToList();
                    if (featurePermissionRepository.Query(p => p.Feature.Id == feature.Id && groupIds.Contains(p.Subject.Id) && p.PermissionType == PermissionType.Deny).Any())
                    {
                        return false;
                    }
                    if (featurePermissionRepository.Query(p => p.Feature.Id == feature.Id && groupIds.Contains(p.Subject.Id) && p.PermissionType == PermissionType.Grant).Any())
                    {
                        return true;
                    }

                    feature = feature.Parent;
                }
            }
            
            return false;
        }

        public Dictionary<long, bool> GetAccessList(IEnumerable<Subject> subjects, long featureId)
        {
            Dictionary<long, bool> accessDictionary = new Dictionary<long, bool>();

            using (var uow = this.GetUnitOfWork())
            {
                return subjects.Where(s => s != null).ToDictionary(s => s.Id, s => hasAccessInternal(uow, s.Id, featureId));
            }
        }

        public bool HasAccess<T>(string subjectName, string module, string controller, string action) where T : Subject
        {
            using (var uow = this.GetUnitOfWork())
            {
                var operationRepository = uow.GetReadOnlyRepository<Operation>();
                var SubjectRepository = uow.GetReadOnlyRepository<Subject>();

                var operation = operationRepository.Query(x => x.Module== module && x.Controller == controller && x.Action == action).FirstOrDefault();
                if (operation == null) return false;

                var feature = operation?.Feature;
                var subject = SubjectRepository.Query(s => s.Name == subjectName && s is T).FirstOrDefault();

                //both exits
                if (feature != null)
                    return hasAccessInternal(uow, subject?.Id, feature.Id);

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

        public bool Update(FeaturePermission featurePermission)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetRepository<FeaturePermission>();
                featurePermissionRepository.Merge(featurePermission);
                var merged = featurePermissionRepository.Get(featurePermission.Id);
                var result = featurePermissionRepository.Put(merged);
                uow.Commit();

                return result;
            }
        }
    }
}