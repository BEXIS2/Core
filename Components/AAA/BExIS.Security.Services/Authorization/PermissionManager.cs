using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Authorization
{
    public sealed class PermissionManager : IPermissionManager
    {
        public PermissionManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();

            this.DataPermissionsRepo = uow.GetReadOnlyRepository<DataPermission>();
            this.EntitiesRepo = uow.GetReadOnlyRepository<Entity>();
            this.FeaturePermissionsRepo = uow.GetReadOnlyRepository<FeaturePermission>();
            this.FeaturesRepo = uow.GetReadOnlyRepository<Feature>();
            this.SubjectsRepo = uow.GetReadOnlyRepository<Subject>();
            this.UsersRepo = uow.GetReadOnlyRepository<User>();
            this.GroupsRepo = uow.GetReadOnlyRepository<Group>();

        }

        #region Data Readers
    
        public IReadOnlyRepository<DataPermission> DataPermissionsRepo { get; private set; }
        public IReadOnlyRepository<Entity> EntitiesRepo { get; private set; }
        public IReadOnlyRepository<FeaturePermission> FeaturePermissionsRepo { get; private set; }
        public IReadOnlyRepository<Feature> FeaturesRepo { get; private set; }
        public IReadOnlyRepository<Group> GroupsRepo { get; private set; }
        public IReadOnlyRepository<Subject> SubjectsRepo { get; private set; }
        public IReadOnlyRepository<User> UsersRepo { get; private set; }
        
        #endregion

        public FeaturePermission CreateFeaturePermission(long subjectId, long featureId, PermissionType permissionType = PermissionType.Grant)
        {
            FeaturePermission featurePermission = new FeaturePermission()
            {
                Subject = SubjectsRepo.Get(subjectId),
                Feature = FeaturesRepo.Get(featureId),
                PermissionType = permissionType
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<FeaturePermission> featuresRepo = uow.GetRepository<FeaturePermission>();
                featuresRepo.Put(featurePermission);

                uow.Commit();
            }

            return (featurePermission);
        }

        public bool DeleteFeaturePermissionById(long id)
        {
            FeaturePermission featurePermission = GetFeaturePermissionById(id);

            if (featurePermission != null)
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<FeaturePermission> featurePermissionsRepo = uow.GetRepository<FeaturePermission>();

                    featurePermissionsRepo.Delete(featurePermission);
                    uow.Commit();
                }

                return (true);
            }

            return (false);
        }

        public bool DeleteFeaturePermission(long subjectId, long featureId)
        {
            FeaturePermission featurePermission = GetFeaturePermission(subjectId, featureId);

            if (featurePermission != null)
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<FeaturePermission> featurePermissionsRepo = uow.GetRepository<FeaturePermission>();

                    featurePermissionsRepo.Delete(featurePermission);
                    uow.Commit();
                }

                return (true);
            }

            return (false);
        }

        public bool ExistsFeaturePermissionId(long id)
        {
            return FeaturePermissionsRepo.Get(id) != null ? true : false;
        }

        public bool ExistsFeaturePermission(long subjectId, long featureId, PermissionType permissionType = PermissionType.Grant)
        {
            if (FeaturePermissionsRepo.Get(p => p.Subject.Id == subjectId && p.Feature.Id == featureId && p.PermissionType == permissionType).Count == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ExistsFeaturePermission(IEnumerable<long> subjectIds, IEnumerable<long> featureIds, PermissionType permissionType = PermissionType.Grant)
        {
            if (FeaturePermissionsRepo.Query(p => featureIds.Contains(p.Feature.Id) && subjectIds.Contains(p.Subject.Id) && p.PermissionType == permissionType).Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public IQueryable<FeaturePermission> GetAllFeaturePermissions()
        {
            return FeaturePermissionsRepo.Query();
        }

        public FeaturePermission GetFeaturePermissionById(long id)
        {
            return FeaturePermissionsRepo.Get(id);
        }

        public FeaturePermission GetFeaturePermission(long subjectId, long featureId)
        {
            return FeaturePermissionsRepo.Get(p => p.Subject.Id == subjectId && p.Feature.Id == featureId).FirstOrDefault();
        }

        public FeaturePermission UpdateFeaturePermission(FeaturePermission featurePermission)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<FeaturePermission> usersRepo = uow.GetRepository<FeaturePermission>();
                usersRepo.Put(featurePermission);
                uow.Commit();
            }

            return (featurePermission);
        }

        public DataPermission CreateDataPermission(long subjectId, long entityId, long dataId, RightType rightType, PermissionType permissionType = PermissionType.Grant)
        {
            DataPermission dataPermission = new DataPermission()
            {
                Subject = SubjectsRepo.Get(subjectId),
                Entity = EntitiesRepo.Get(entityId),
                DataId = dataId,
                RightType = rightType,
                PermissionType = permissionType
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DataPermission> featuresRepo = uow.GetRepository<DataPermission>();
                featuresRepo.Put(dataPermission);

                uow.Commit();
            }

            return (dataPermission);
        }

        public bool DeleteDataPermissionById(long id)
        {
            DataPermission dataPermission = GetDataPermissionById(id);

            if (dataPermission != null)
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<DataPermission> dataPermissionsRepo = uow.GetRepository<DataPermission>();

                    dataPermissionsRepo.Delete(dataPermission);
                    uow.Commit();
                }

                return (true);
            }

            return (false);
        }

        public bool DeleteDataPermission(long subjectId, long entityId, long dataId, RightType rightType)
        {
            DataPermission dataPermission = GetDataPermission(subjectId, entityId, dataId, rightType);

            if (dataPermission != null)
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<DataPermission> dataPermissionsRepo = uow.GetRepository<DataPermission>();

                    dataPermissionsRepo.Delete(dataPermission);
                    uow.Commit();
                }

                return (true);
            }

            return (false);
        }

        public bool DeleteDataPermissionsByEntity(long entityId, long dataId)
        {
            var dataPermissions = GetDataPermissionsFromEntity(entityId, dataId)
                                    //.Select(p=>p.Id) // if uncommented, uses another repository Delete method!
                                    .ToList();

            if (dataPermissions != null)
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<DataPermission> dataPermissionsRepo = uow.GetRepository<DataPermission>();

                    dataPermissionsRepo.Delete(dataPermissions);
                    uow.Commit();
                }

                return (true);
            }

            return (false);
        }

        public bool ExistsDataPermissionId(long id)
        {
            return DataPermissionsRepo.Get(id) != null ? true : false;
        }

        public bool ExistsDataPermission(long subjectId, long entityId, long dataId, RightType rightType)
        {
            if (DataPermissionsRepo.Get(p => p.Subject.Id == subjectId && p.Entity.Id == entityId && p.DataId == dataId && p.RightType == rightType).Count() == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public IQueryable<long> GetAllDataIds(long userId, long entityId, RightType rightType)
        {
            User user = UsersRepo.Get(userId);

            List<long> subjectIds = new List<long>(user.Groups.Select(g => g.Id));
            subjectIds.Add(user.Id);

            return GetAllDataIds(subjectIds, entityId, rightType);
        }

        public IQueryable<long> GetAllDataIds(long userId, long entityId, List<RightType> rightTypes)
        {
            User user = UsersRepo.Get(userId);

            List<long> subjectIds = new List<long>(user.Groups.Where(g => g.Name != "everyone").Select(g => g.Id));
            subjectIds.Add(user.Id);

            return GetAllDataIds(subjectIds, entityId, rightTypes);
        }

        public IQueryable<long> GetAllDataIds(IEnumerable<long> subjectIds, long entityId, RightType rightType)
        {
            return DataPermissionsRepo.Query(p => p.Entity.Id == entityId && subjectIds.Contains(p.Subject.Id) && p.RightType == rightType).Select(p => p.DataId);
        }

        public IQueryable<long> GetAllDataIds(IEnumerable<long> subjectIds, long entityId, List<RightType> rightTypes)
        {
            return DataPermissionsRepo.Query(p => p.Entity.Id == entityId && subjectIds.Contains(p.Subject.Id) && rightTypes.Contains(p.RightType)).Select(p => p.DataId).Distinct();
        }

        public IQueryable<DataPermission> GetAllDataPermissions()
        {
            return DataPermissionsRepo.Query();
        }

        public IQueryable<int> GetAllRights(long subjectId, long entityId, long dataId)
        {
            return DataPermissionsRepo.Query(p => p.Subject.Id == subjectId && p.Entity.Id == entityId && p.DataId == dataId).Select(p => (int)p.RightType);
        }

        public DataPermission GetDataPermissionById(long id)
        {
            return DataPermissionsRepo.Get(id);
        }

        public DataPermission GetDataPermission(long subjectId, long entityId, long dataId, RightType rightType)
        {
            return DataPermissionsRepo.Get(p => p.Subject.Id == subjectId && p.Entity.Id == entityId && p.DataId == dataId && p.RightType == rightType).FirstOrDefault();
        }

        public IQueryable<DataPermission> GetDataPermissionsFromEntity(long entityId, long dataId)
        {
            return DataPermissionsRepo.Query(p => p.Entity.Id == entityId && p.DataId == dataId);
        }

        public int GetFeaturePermissionType(long subjectId, long featureId)
        {
            FeaturePermission featurePermission = GetFeaturePermission(subjectId, featureId);

            if (featurePermission != null)
            {
                return (int)featurePermission.PermissionType;
            }
            else
            {
                return 2;
            }
        }

        public bool HasUserDataAccess(long userId, long entityId, long dataId, RightType rightType)
        {
            User user = UsersRepo.Get(userId);

            List<long> subjectIds = new List<long>(user.Groups.Select(g => g.Id));
            subjectIds.Add(user.Id);

            return ExistsDataPermission(subjectIds, entityId, dataId, rightType);
        }

        public bool HasUserDataAccess(string username, long entityId, long dataId, RightType rightType)
        {
            Group everyone = GroupsRepo.Get(g => g.Name == "everyone").FirstOrDefault();

            if (ExistsDataPermission(everyone.Id, entityId, dataId, rightType))
            {
                return true;
            }
            else
            {
                User user = UsersRepo.Get(u => u.Name.ToLower() == username.ToLower()).FirstOrDefault();

                if (user == null)
                {
                    return false;
                }
                else
                {
                    List<long> subjectIds = new List<long>(user.Groups.Select(g => g.Id));
                    subjectIds.Add(user.Id);

                    return ExistsDataPermission(subjectIds, entityId, dataId, rightType);
                }
            }
        }

        public bool ExistsDataPermission(IEnumerable<long> subjectIds, long entityId, long dataId, RightType rightType)
        {
            if (DataPermissionsRepo.Query(p => subjectIds.Contains(p.Subject.Id) && p.Entity.Id == entityId && p.DataId == dataId && p.RightType == rightType).Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool HasSubjectFeatureAccess(long subjectId, long featureId)
        {
            Feature feature = FeaturesRepo.Get(featureId);
            Subject subject = SubjectsRepo.Get(subjectId);

            FeaturePermission featurePermission;

            if (subject is Group)
            {
                while (feature != null)
                {
                    featurePermission = GetFeaturePermission(subject.Id, feature.Id);

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
                User user = subject as User;
                List<long> groupIds = user.Groups.Select(g => g.Id).ToList();

                while (feature != null)
                {
                    featurePermission = GetFeaturePermission(subject.Id, feature.Id);

                    if (featurePermission != null)
                    {
                        return featurePermission.PermissionType == PermissionType.Grant;
                    }
                    
                    if (ExistsFeaturePermission(groupIds, new[] { feature.Id }, PermissionType.Deny))
                    {
                        return false;
                    }
                    else
                    {
                        if (ExistsFeaturePermission(groupIds, new[] { feature.Id }, PermissionType.Grant))
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

        public DataPermission UpdateDataPermission(DataPermission dataPermission)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<DataPermission> usersRepo = uow.GetRepository<DataPermission>();
                usersRepo.Put(dataPermission);
                uow.Commit();
            }

            return (dataPermission);
        }
    }
}
