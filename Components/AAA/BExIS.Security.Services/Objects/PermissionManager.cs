using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
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

            this.DataPermissionsRepo = uow.GetReadOnlyRepository<DataPermission>();
            this.FeaturePermissionsRepo = uow.GetReadOnlyRepository<FeaturePermission>();
            this.FeaturesRepo = uow.GetReadOnlyRepository<Feature>();
            this.SubjectsRepo = uow.GetReadOnlyRepository<Subject>();
            this.UsersRepo = uow.GetReadOnlyRepository<User>();
        }

        #region Data Readers

        public IReadOnlyRepository<DataPermission> DataPermissionsRepo { get; private set; }
        public IReadOnlyRepository<FeaturePermission> FeaturePermissionsRepo { get; private set; }
        public IReadOnlyRepository<Feature> FeaturesRepo { get; private set; } 
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

        private bool CheckFeatureAccess(long userId, List<long> roleIds, long featureId)
        {
            // User Permission
            FeaturePermission userPermission = GetFeaturePermissionByFeatureAndSubject(featureId, userId);

            if (userPermission != null)
            {
                return userPermission.PermissionType == PermissionType.Deny ? false : true;
            }
            else
            {
                // Role Permission
                List<FeaturePermission> rolePermissions = FeaturePermissionsRepo.Query(p => roleIds.Contains(p.Subject.Id) && p.Feature.Id == featureId).ToList();

                if (rolePermissions.Count() > 0)
                {
                    bool access = false;

                    switch (FeaturePermissionPolicy)
                    {
                        case 0:
                            if (rolePermissions.All(p => p.PermissionType == PermissionType.Allow))
                            {
                                access = true;
                            }
                            break;

                        case 1:
                            if (rolePermissions.Any(p => p.PermissionType == PermissionType.Allow))
                            {
                                access = true;
                            }
                            break;

                        case 2:
                            int grants = rolePermissions.Where(p => p.PermissionType == PermissionType.Allow).Count();
                            int denies = rolePermissions.Where(p => p.PermissionType == PermissionType.Deny).Count();

                            if (grants == denies)
                            {
                                access = DrawFeaturePermission;
                            }
                            else
                            {
                                access = grants > denies ? true : false;
                            }
                            break;

                        default:
                            access = false;
                            break;
                    }

                    return access;
                }
                else
                {
                    if (FeaturePermissionHierarchy && FeaturesRepo.Query(f => f.Children.Any(g => g.Id == featureId)).Count() == 1)
                    {
                        return CheckFeatureAccess(userId, roleIds, FeaturesRepo.Query(f => f.Children.Any(g => g.Id == featureId)).FirstOrDefault().Id);
                    }
                    else
                    {
                        return DefaultFeaturePermission;
                    }
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

                    return CheckFeatureAccess(user.Id, user.Roles.Select(r => r.Id).ToList(), feature.Id);
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
                subject.Permissions.Add(featurePermission);

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
            if (FeaturePermissionsRepo.Query(p => p.Feature.Id == featureId && p.Subject.Id == subjectId).Count() == 1)
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
            ICollection<FeaturePermission> featurePermissions = FeaturePermissionsRepo.Query(p => p.Id == id).ToArray();

            if (featurePermissions.Count() == 1)
            {
                return featurePermissions.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        public FeaturePermission GetFeaturePermissionByFeatureAndSubject(long featureId, long subjectId)
        {
            ICollection<FeaturePermission> subjectPermissions = FeaturePermissionsRepo.Query(p => p.Feature.Id == featureId && p.Subject.Id == subjectId).ToArray();

            if (subjectPermissions.Count() == 1)
            {
                return subjectPermissions.FirstOrDefault();
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

        public DataPermission CreateDataPermission()
        {
            throw new NotImplementedException();
        }

        public IQueryable<DataPermission> GetAllDataPermissions()
        {
            return DataPermissionsRepo.Query();
        }

        public IQueryable<Entity> GetAllEntities()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            XDocument xDocument = XDocument.Load(assembly.GetManifestResourceStream("BExIS.Security.Services.Manifest.xml"));

            List<Entity> entities = new List<Entity>();

            foreach (var entity in xDocument.Descendants("Entity"))
            {
                entities.Add(new Entity() { Id = entity.Attribute("Id").Value, Name = entity.Attribute("Name").Value, Assembly = entity.Attribute("Assembly").Value, Type = entity.Attribute("Type").Value });
            }

            return entities.AsQueryable<Entity>();
        }

        public IQueryable<Property> GetAllProperties(string entityName)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            XDocument xDocument = XDocument.Load(assembly.GetManifestResourceStream("BExIS.Security.Services.Manifest.xml"));

            List<Property> properties = new List<Property>();

            foreach (var property in xDocument.Descendants("Property").Where(n => n.Ancestors("Entity").FirstOrDefault().Attribute("Id").Value == entityName))
            {
                properties.Add(new Property() { Id = property.Attribute("Id").Value, Name = property.Attribute("Name").Value });

            }

            return properties.AsQueryable<Property>();
        }

        public bool DeleteDataPermission()
        {
            throw new NotImplementedException();
        }

        public IQueryable<DataPermission> GetDataPermissionsByEntityAndSubject(string entityName, long subjectId)
        {
            return DataPermissionsRepo.Query(p => p.Subject.Id == subjectId && p.EntityName == entityName);
        }


        //public Tuple<string, object[]> GetDataPermissionExpressionByEntityAndSubject(string entityName, long subjectId)
        //{
        //    List<DataPermission> dataPermissions = GetDataPermissionsByEntityAndSubject(entityName, subjectId).ToList();

        //    string predicate = "";
        //    object[] values = new object[dataPermissions.Count];

        //    for (int i = 0; i < dataPermissions.Count(); i++)
        //    {
        //        predicate += 
        //    }

        //}


        public Tuple<string, object[]> GetDataPermissionExpressionByEntityAndSubject(string entityName, long subjectId)
        {
            throw new NotImplementedException();
        }
    }
}
