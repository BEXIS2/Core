using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Security;
using BExIS.Security.Entities.Subjects;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Security
{
    public class FeaturePermissionManager : IFeaturePermissionManager
    {
        public FeaturePermissionManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();

            this.FeaturePermissionsRepo = uow.GetReadOnlyRepository<FeaturePermission>();
        }

        #region Data Reader

        public IReadOnlyRepository<FeaturePermission> FeaturePermissionsRepo { get; private set; }

        #endregion

        #region Attributes

        #endregion

        #region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="feature"></param>
        /// <param name="permissionType"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public FeaturePermission Create(Subject subject, Feature feature, PermissionType permissionType, out PermissionCreateStatus status)
        {
            Contract.Requires(subject != null);
            Contract.Requires(feature != null);
            Contract.Requires(permissionType != null);

            FeaturePermission featurePermission = new FeaturePermission()
            {
                Subject = subject,
                Feature = feature,
                PermissionType = permissionType
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<FeaturePermission> featurePermissionsRepo = uow.GetRepository<FeaturePermission>();

                featurePermissionsRepo.Put(featurePermission);

                subject.Permissions.Add(featurePermission);
                feature.FeaturePermissions.Add(featurePermission);

                uow.Commit();
            }

            status = PermissionCreateStatus.Success;
            return (featurePermission);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="featurePermission"></param>
        /// <returns></returns>
        public bool Delete(FeaturePermission featurePermission)
        {
            Contract.Requires(featurePermission != null);

            // Computations
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<FeaturePermission> repo = uow.GetRepository<FeaturePermission>();

                featurePermission = repo.Reload(featurePermission);
                repo.Delete(featurePermission);

                uow.Commit();
            }

            return (true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<FeaturePermission> GetAllFeaturePermissions()
        {
            return (FeaturePermissionsRepo.Query());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IQueryable<FeaturePermission> GetFeaturePermissionsFromUser(User user)
        {
            return FeaturePermissionsRepo.Query(p => p.Subject == user).AsQueryable<FeaturePermission>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public IQueryable<FeaturePermission> GetFeaturePermissionsFromRole(Role role)
        {
            return FeaturePermissionsRepo.Query(p => p.Subject == role).AsQueryable<FeaturePermission>();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roles"></param>
        /// <returns></returns>
        public IQueryable<FeaturePermission> GetFeaturePermissionsFromRoles(List<Role> roles)
        {
            return FeaturePermissionsRepo.Query(p => roles.Cast<Subject>().Contains(p.Subject)).AsQueryable<FeaturePermission>();
        }

        public FeaturePermission Update(FeaturePermission featurePermission)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
