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

/// <summary>
///
/// </summary>        
namespace BExIS.Security.Services.Objects
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public class PermissionManager : IPermissionManager
    {
        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
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

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public IReadOnlyRepository<DataPermission> DataPermissionsRepo { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public IReadOnlyRepository<FeaturePermission> FeaturePermissionsRepo { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public IReadOnlyRepository<Feature> FeaturesRepo { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public IReadOnlyRepository<Subject> SubjectsRepo { get; private set; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public IReadOnlyRepository<User> UsersRepo { get; private set; }     

        #endregion


        #region Attributes

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public bool DefaultFeaturePermission 
        {
            get { return false; } 
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public bool DrawFeaturePermission 
        {
            get { return false; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public bool FeaturePermissionHierarchy 
        {
            get { return true; }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        public int FeaturePermissionPolicy 
        {
            get { return 0; }
        }

        #endregion


        #region Methods

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureId"></param>
        /// <param name="subjectId"></param>
        public int CreateFeaturePermission(long featureId, long subjectId)
        {
            if(ExistsFeaturePermission(featureId, subjectId))
            {
                return 1;
            }

            Feature feature = FeaturesRepo.Get(featureId);

            if (feature == null)
            {
                return 3;
            }

            Subject subject = SubjectsRepo.Get(subjectId);

            if (subject == null)
            {
                return 2;
            }

            FeaturePermission featurePermission = new FeaturePermission()
            {
                Feature = feature,
                Subject = subject,
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

            return 0;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>       
        public int DeleteFeaturePermissionById(long id)
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
            }

            return 0;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureId"></param>
        /// <param name="subjectId"></param>
        public int DeleteFeaturePermissionByFeatureAndSubject(long featureId, long subjectId)
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
            }

            return 0;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureId"></param>
        /// <param name="subjectId"></param>
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

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureIds"></param>
        /// <param name="subjectIds"></param>
        public bool ExistsFeaturePermission(IEnumerable<long> featureIds, IEnumerable<long> subjectIds)
        {
            if (FeaturePermissionsRepo.Query(p => featureIds.Contains(p.Feature.Id) && subjectIds.Contains(p.Subject.Id)).Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
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

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureId"></param>
        /// <param name="subjectId"></param>
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

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        public IQueryable<FeaturePermission> GetAllFeaturePermissions()
        {
            return FeaturePermissionsRepo.Query();
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureId"></param>
        public IQueryable<FeaturePermission> GetFeaturePermissionsFromFeature(long featureId)
        {
            return FeaturePermissionsRepo.Query(p => p.Feature.Id == featureId);
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="subjectId"></param>
        public IQueryable<FeaturePermission> GetFeaturePermissionsFromSubject(long subjectId)
        {
            return FeaturePermissionsRepo.Query(p => p.Subject.Id == subjectId);
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featurePermission"></param>
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

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
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

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="entityId"></param>
        public string GetAreaFromEntity(string entityId)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            XDocument xDocument = XDocument.Load(assembly.GetManifestResourceStream("BExIS.Security.Services.Manifest.xml"));

            var entities = xDocument.Descendants("Entity").Where(n => n.Attribute("Id").Value == entityId);

            if (entities.Count() == 1)
            {
                return entities.FirstOrDefault().Attribute("Area").Value;
            }

            return "";
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="entityId"></param>
        public string GetControllerFromEntity(string entityId)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            XDocument xDocument = XDocument.Load(assembly.GetManifestResourceStream("BExIS.Security.Services.Manifest.xml"));

            var entities = xDocument.Descendants("Entity").Where(n => n.Attribute("Id").Value == entityId);

            if (entities.Count() == 1)
            {
                return entities.FirstOrDefault().Attribute("Controller").Value;
            }

            return "";
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="entityId"></param>
        public string GetActionFromEntity(string entityId)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            XDocument xDocument = XDocument.Load(assembly.GetManifestResourceStream("BExIS.Security.Services.Manifest.xml"));

            var entities = xDocument.Descendants("Entity").Where(n => n.Attribute("Id").Value == entityId);

            if (entities.Count() == 1)
            {
                return entities.FirstOrDefault().Attribute("Action").Value;
            }

            return "";
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataId"></param>
        /// <param name="entityId"></param>
        /// <param name="subjectId"></param>
        /// <param name="rightType"></param>
        public int CreateDataPermission(long dataId, string entityId, long subjectId, RightType rightType)
        {
            Subject subject = SubjectsRepo.Get(subjectId);

            if (subject != null)
            {
                if (!ExistsDataPermission(dataId, entityId, subjectId, rightType))
                {
                    DataPermission dataPermission = new DataPermission()
                    {
                        DataId = dataId,
                        EntityId = entityId,

                        Subject = subject,

                        RightType = rightType
                    };

                    using (IUnitOfWork uow = this.GetUnitOfWork())
                    {
                        IRepository<DataPermission> dataPermissionsRepo = uow.GetRepository<DataPermission>();
                        IRepository<Subject> subjectssRepo = uow.GetRepository<Subject>();

                        dataPermissionsRepo.Put(dataPermission);
                        subject.Permissions.Add(dataPermission);

                        uow.Commit();
                    }
                }

                return 0;
            }
            else
            {
                return 2;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataId"></param>
        /// <param name="entityId"></param>
        /// <param name="userName"></param>
        /// <param name="rightType"></param>
        public int CreateDataPermission(long dataId, string entityId, string userName, RightType rightType)
        {
            ICollection<User> users = UsersRepo.Get(u => u.Name.ToLower() == userName.ToLower());

            if (users.Count() == 1)
            {
                if (!ExistsDataPermission(dataId, entityId, users.FirstOrDefault().Id, rightType))
                {
                    DataPermission dataPermission = new DataPermission()
                    {
                        DataId = dataId,
                        EntityId = entityId,

                        Subject = users.FirstOrDefault(),

                        RightType = rightType
                    };

                    using (IUnitOfWork uow = this.GetUnitOfWork())
                    {
                        IRepository<DataPermission> dataPermissionsRepo = uow.GetRepository<DataPermission>();
                        IRepository<Subject> subjectssRepo = uow.GetRepository<Subject>();

                        dataPermissionsRepo.Put(dataPermission);
                        users.FirstOrDefault().Permissions.Add(dataPermission);

                        uow.Commit();
                    }
                }

                return 0;
            }
            else
            {
                return 2;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataId"></param>
        /// <param name="entityId"></param>
        /// <param name="subjectId"></param>
        /// <param name="rightType"></param>
        public int DeleteDataPermission(long dataId, string entityId, long subjectId, RightType rightType)
        {
            Subject subject = SubjectsRepo.Get(subjectId);

            if (subject != null)
            {
                DataPermission dataPermission = GetDataPermission(dataId, entityId, subjectId, rightType);

                if (dataPermission != null)
                {
                    using (IUnitOfWork uow = this.GetUnitOfWork())
                    {
                        IRepository<DataPermission> dataPermissionsRepo = uow.GetRepository<DataPermission>();
                        IRepository<Subject> subjectssRepo = uow.GetRepository<Subject>();

                        dataPermissionsRepo.Delete(dataPermission);
                        uow.Commit();
                    }
                }

                return 0;
            }
            else
            {
                return 2;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataId"></param>
        /// <param name="entityId"></param>
        /// <param name="subjectId"></param>
        /// <param name="rightType"></param>
        public bool ExistsDataPermission(long dataId, string entityId, long subjectId, RightType rightType)
        {
            if (DataPermissionsRepo.Query(p => p.DataId == dataId && p.EntityId == entityId && p.Subject.Id == subjectId && p.RightType == rightType).Count() == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataId"></param>
        /// <param name="entityId"></param>
        /// <param name="subjectIds"></param>
        /// <param name="rightType"></param>
        public bool ExistsDataPermission(long dataId, string entityId, IEnumerable<long> subjectIds, RightType rightType)
        {
            if (DataPermissionsRepo.Query(p => p.DataId == dataId && p.EntityId.ToLower() == entityId.ToLower() && subjectIds.Contains(p.Subject.Id) && p.RightType == rightType).Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataId"></param>
        /// <param name="entityId"></param>
        /// <param name="subjectId"></param>
        /// <param name="rightType"></param>
        public DataPermission GetDataPermission(long dataId, string entityId, long subjectId, RightType rightType)
        {
            ICollection<DataPermission> dataPermissions = DataPermissionsRepo.Query(p => p.DataId == dataId && p.EntityId == entityId && p.Subject.Id == subjectId && p.RightType == rightType).ToArray();

            if (dataPermissions.Count() == 1)
            {
                return dataPermissions.FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="dataId"></param>
        /// <param name="entityId"></param>
        /// <param name="userName"></param>
        /// <param name="rightType"></param>
        public bool HasUserDataAccess(long dataId, string entityId, string userName, RightType rightType)
        {
            ICollection<User> users = UsersRepo.Query(u => u.Name == userName).ToArray();

            if (users.Count() == 1)
            {
                List<long> subjectIds = new List<long>(users.FirstOrDefault().Groups.Select(r => r.Id));
                subjectIds.Add(users.FirstOrDefault().Id);

                if (ExistsDataPermission(dataId, entityId, subjectIds, rightType))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="userName"></param>
        /// <param name="areaName"></param>
        /// <param name="controllerName"></param>
        /// <param name="actionName"></param>
        public bool HasUserFeatureAccess(string userName, string areaName, string controllerName, string actionName)
        {
            ICollection<User> users = UsersRepo.Query(u => u.Name.ToLower() == userName.ToLower()).ToArray();

            if (users.Count() == 1)
            {
                ICollection<Feature> features = FeaturesRepo.Query(f => f.Tasks.Any(t => t.AreaName.ToLower() == areaName.ToLower() && t.ControllerName.ToLower() == controllerName.ToLower() && t.ActionName.ToLower() == actionName.ToLower())).ToArray();

                if (features.Count() == 1)
                {
                    List<long> featureIds = new List<long>(features.FirstOrDefault().Ancestors.Select(f => f.Id));
                    featureIds.Add(features.FirstOrDefault().Id);

                    List<long> subjectIds = new List<long>(users.FirstOrDefault().Groups.Select(r => r.Id));
                    subjectIds.Add(users.FirstOrDefault().Id);

                    if (ExistsFeaturePermission(featureIds, subjectIds))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="featureId"></param>
        /// <param name="subjectId"></param>
        public bool HasSubjectFeatureAccess(long featureId, long subjectId)
        {
            Feature feature = FeaturesRepo.Get(featureId);

            if (feature != null)
            {
                Subject subject = SubjectsRepo.Get(subjectId);

                if(subject != null)
                {
                    List<long> featureIds = new List<long>(feature.Ancestors.Select(f => f.Id));
                    featureIds.Add(feature.Id);

                    if(subject is Group)
                    {
                        Group role = (Group)subject;
                        return ExistsFeaturePermission(featureIds, new List<long>(){role.Id});
                    }

                    if(subject is User)
                    {
                        User user = (User)subject;

                        List<long> subjectIds = new List<long>(user.Groups.Select(r => r.Id));
                        subjectIds.Add(user.Id);

                        return ExistsFeaturePermission(featureIds, subjectIds);
                    }
                }
            }

            return false;
        }


        public ICollection<long> GetDataIds(string entityId, string userName, RightType rightType)
        {
            ICollection<User> users = UsersRepo.Query(u => u.Name.ToLower() == userName.ToLower()).ToArray();

            if (users.Count() == 1)
            {
                List<long> subjectIds = new List<long>(users.FirstOrDefault().Groups.Select(r => r.Id));
                subjectIds.Add(users.FirstOrDefault().Id);

                return DataPermissionsRepo.Query(p => subjectIds.Contains(p.Subject.Id) && p.EntityId.ToLower() == entityId.ToLower() && p.RightType == rightType).Select(p => p.DataId).ToList(); 
            }

            return null; 
        }
    }
}
