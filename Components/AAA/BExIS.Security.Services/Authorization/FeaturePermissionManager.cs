using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using System;
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

        public void Delete(long subjectId, long featureId)
        {
            var featurePermission = Find(featureId, subjectId);

            using (var uow = this.GetUnitOfWork())
            {
                var featurePermissionRepository = uow.GetRepository<FeaturePermission>();
                featurePermissionRepository.Delete(featurePermission);
                uow.Commit();
            }
        }

        public bool Exists(long subjectId, long featureId, PermissionType permissionType = PermissionType.Grant)
        {
            return FeaturePermissionRepository.Get(p => p.Subject.Id == subjectId && p.Feature.Id == featureId && p.PermissionType == permissionType).Count == 1;
        }

        public bool Exists(IEnumerable<long> subjectIds, IEnumerable<long> featureIds, PermissionType permissionType = PermissionType.Grant)
        {
            return FeaturePermissionRepository.Query(p => featureIds.Contains(p.Feature.Id) && subjectIds.Contains(p.Subject.Id) && p.PermissionType == permissionType).Any();
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
                    featurePermission = Find(feature.Id, subject.Id);

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
                    featurePermission = Find(feature.Id, subject.Id);

                    if (featurePermission != null)
                    {
                        return featurePermission.PermissionType == PermissionType.Grant;
                    }

                    if (Exists(groupIds, new[] { feature.Id }, PermissionType.Deny))
                    {
                        return false;
                    }
                    else
                    {
                        if (Exists(groupIds, new[] { feature.Id }, PermissionType.Grant))
                        {
                            return true;
                        }
                    }

                    feature = feature.Parent;
                }

                return false;
            }

            return false;
        }

        public FeaturePermission Find(long subjectId, long featureId)
        {
            return
                FeaturePermissionRepository.Query(f => f.Feature.Id == featureId && f.Subject.Id == subjectId)
                    .FirstOrDefault();
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