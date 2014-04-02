using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Objects
{
    public class PermissionManager : IPermissionManager
    {
        public PermissionManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();

            this.FeaturePermissionsRepo = uow.GetReadOnlyRepository<FeaturePermission>();
            this.FeaturesRepo = uow.GetReadOnlyRepository<Feature>();
            this.RolesRepo = uow.GetReadOnlyRepository<Role>();
            this.SubjectsRepo = uow.GetReadOnlyRepository<Subject>();
            this.UsersRepo = uow.GetReadOnlyRepository<User>();
        }

        #region Data Readers

        public IReadOnlyRepository<FeaturePermission> FeaturePermissionsRepo { get; private set; }
        public IReadOnlyRepository<Feature> FeaturesRepo { get; private set; }
        public IReadOnlyRepository<Role> RolesRepo { get; private set; }   
        public IReadOnlyRepository<Subject> SubjectsRepo { get; private set; }
        public IReadOnlyRepository<User> UsersRepo { get; private set; }     

        #endregion


        #region Attributes

        public bool DefaultFeaturePermission 
        {
            get { return false; } 
        }

        public bool DrawFeaturePermission 
        {
            get { return false; }
        }

        public bool FeaturePermissionHierarchy 
        {
            get { return true; }
        }

        public int FeaturePermissionPolicy 
        {
            get { return 0; }
        }

        #endregion


        #region Methods

        private bool CheckFeatureAccess(User user, Feature feature)
        {
            user = UsersRepo.Reload(user);
            UsersRepo.LoadIfNot(user.Roles);

            feature = FeaturesRepo.Reload(feature);
            FeaturesRepo.LoadIfNot(feature.Parent);

            // User Permission
            FeaturePermission userPermission = GetFeaturePermissionByFeatureAndSubject(feature.Id, user.Id);

            // Role Permission
            List<FeaturePermission> rolePermissions = new List<FeaturePermission>();

            foreach (Role role in user.Roles)
            {
                rolePermissions.AddRange(GetFeaturePermissionsFromFeature(feature.Id).Where(p => p.Subject.Id == role.Id));
            }

            //rolePermissions.AddRange(GetFeaturePermissionsFromFeature(feature.Id).Where(p => user.Roles.Any(r => r.Id == p.Subject.Id)).ToList<FeaturePermission>());

            if (userPermission != null)
            {
                return userPermission.PermissionType == PermissionType.Allow ? true : false;
            }
            else if (rolePermissions.Count() > 0)
            {
                switch (FeaturePermissionPolicy)
                {
                    case 0:
                        if (rolePermissions.Any(p => p.PermissionType == PermissionType.Deny))
                        {
                            return false;
                        }
                        else
                        {
                            return true;
                        }               

                    case 1:
                        if (rolePermissions.Any(p => p.PermissionType == PermissionType.Allow))
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }

                    case 2:
                        int grants = rolePermissions.Where(p => p.PermissionType == PermissionType.Allow).Count();
                        int denies = rolePermissions.Where(p => p.PermissionType == PermissionType.Deny).Count();

                        if (grants == denies)
                        {
                            return DrawFeaturePermission;
                        }
                        else
                        {
                            return grants > denies ? true : false;
                        }

                    default:
                        return false;
                }
            }
            else
            {
                if (FeaturePermissionHierarchy && feature.Parent != null)
                {
                    return CheckFeatureAccess(user, feature.Parent);
                }
                else
                {
                    return DefaultFeaturePermission;
                }
            }
        }

        public bool CheckFeatureAccessForUser(string userName, string areaName, string controllerName, string actionName)
        {
            if (UsersRepo.Query(u => u.Name.ToLower() == userName.ToLower()).Count() == 1)
            {
                User user = UsersRepo.Query(u => u.Name.ToLower() == userName.ToLower()).FirstOrDefault();

                if (FeaturesRepo.Query(f => f.Tasks.Any(t => t.AreaName.ToLower() == areaName.ToLower() && t.ControllerName.ToLower() == controllerName.ToLower() && t.ActionName.ToLower() == actionName.ToLower())).Count() == 1)
                {
                    Feature feature = FeaturesRepo.Query(f => f.Tasks.Any(t => t.AreaName.ToLower() == areaName.ToLower() && t.ControllerName.ToLower() == controllerName.ToLower() && t.ActionName.ToLower() == actionName.ToLower())).FirstOrDefault();

                    return CheckFeatureAccess(user, feature);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public FeaturePermission CreateFeaturePermission(long featureId, long subjectId, PermissionType permissionType, out FeaturePermissionCreateStatus status)
        {
            if(ExistsFeaturePermission(featureId, subjectId))
            {
                status = FeaturePermissionCreateStatus.DuplicateFeaturePermission;
                return null;
            }

            Feature feature = FeaturesRepo.Get(featureId);

            if (feature == null)
            {
                status = FeaturePermissionCreateStatus.InvalidFeature;
                return null;
            }

            Subject subject = SubjectsRepo.Get(subjectId);

            if (subject == null)
            {
                status = FeaturePermissionCreateStatus.InvalidSubject;
                return null;
            }

            FeaturePermission featurePermission = new FeaturePermission()
            {
                Feature = feature,
                Subject = subject,
                PermissionType = permissionType
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<FeaturePermission> featurePermissionsRepo = uow.GetRepository<FeaturePermission>();
                IRepository<Feature> featuresRepo = uow.GetRepository<Feature>();
                IRepository<Subject> subjectssRepo = uow.GetRepository<Subject>();

                featurePermissionsRepo.Put(featurePermission);
                feature.FeaturePermissions.Add(featurePermission);
                subject.FeaturePermissions.Add(featurePermission);

                uow.Commit();
            }

            status = FeaturePermissionCreateStatus.Success;
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

        public bool DeleteFeaturePermissionByFeatureAndSubject(long featureId, long subjectId)
        {
            FeaturePermission featurePermission = GetFeaturePermissionByFeatureAndSubject(featureId, subjectId);

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

        public bool ExistsFeaturePermission(long featureId, long subjectId)
        {
            if (GetFeaturePermissionByFeatureAndSubject(featureId, subjectId) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public FeaturePermission GetFeaturePermissionById(long id)
        {
            if (FeaturePermissionsRepo.Query(p => p.Id == id).Count() == 1)
            {
                return FeaturePermissionsRepo.Query(p => p.Id == id).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public FeaturePermission GetFeaturePermissionByFeatureAndSubject(long featureId, long subjectId)
        {
            if (FeaturePermissionsRepo.Query(p => p.Feature.Id == featureId && p.Subject.Id == subjectId).Count() == 1)
            {
                return FeaturePermissionsRepo.Query(p => p.Feature.Id == featureId && p.Subject.Id == subjectId).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public IQueryable<FeaturePermission> GetAllFeaturePermissions()
        {
            return FeaturePermissionsRepo.Query();
        }

        public IQueryable<FeaturePermission> GetFeaturePermissionsFromFeature(long featureId)
        {
            return FeaturePermissionsRepo.Query(p => p.Feature.Id == featureId);
        }

        public IQueryable<FeaturePermission> GetFeaturePermissionsFromSubject(long subjectId)
        {
            return FeaturePermissionsRepo.Query(p => p.Subject.Id == subjectId);
        }

        public int GetFeaturePermissionType(long featureId, long subjectId)
        {
            FeaturePermission featurePermission = GetFeaturePermissionByFeatureAndSubject(featureId, subjectId);

            if (featurePermission != null)
            {
                return (int)featurePermission.PermissionType;
            }
            else
            {
                return 2;
            }
        }

        public FeaturePermission UpdateFeaturePermission(FeaturePermission featurePermission)
        {
            Contract.Requires(featurePermission != null);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<FeaturePermission> featurePermissionsRepo = uow.GetRepository<FeaturePermission>();
                featurePermissionsRepo.Put(featurePermission);
                uow.Commit();
            }

            return (featurePermission);
        }

        #endregion


        
    }
}
