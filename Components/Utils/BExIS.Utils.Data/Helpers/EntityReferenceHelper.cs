using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using BExIS.UI.Models.EntityReference;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;

namespace BExIS.Utils.Data.Helpers
{
    public class EntityReferenceHelper
    {
        private ReferenceConfig _config = null;

        public EntityReferenceHelper()
        {
            _config = getReferenceConfig();
        }

        public bool EntityExist(long id, long typeId)
        {
            EntityManager entityManager = new EntityManager();

            try
            {
                var instanceStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(typeId).EntityStoreType);
                return instanceStore.Exist(id);
            }
            finally
            {
                entityManager.Dispose();
            }
        }

        public string GetEntityTitle(long id, long typeId, int version = 0)
        {
            EntityManager entityManager = new EntityManager();

            try
            {
                var instanceStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(typeId).EntityStoreType);

                if (version == 0) return instanceStore.GetTitleById(id);
                else
                {
                    var entityStoreItem = instanceStore.GetVersionsById(id).FirstOrDefault(e => e.Version.Equals(version));
                    if (entityStoreItem != null) return entityStoreItem.Title;

                    return "Not Available";
                }
            }
            finally
            {
                entityManager.Dispose();
            }
        }

        public int CountVersions(long id, long typeId)
        {
            EntityManager entityManager = new EntityManager();

            try
            {
                var instanceStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(typeId).EntityStoreType);
                return instanceStore.CountVersions(id);
            }
            finally
            {
                entityManager.Dispose();
            }
        }

        public SelectList GetEntityVersions(long id, long typeId)
        {
            EntityManager entityManager = new EntityManager();
            List<SelectListItem> list = new List<SelectListItem>();

            try
            {
                var instanceStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(typeId).EntityStoreType);

                instanceStore.GetVersionsById(id).ForEach(v => list.Add(new SelectListItem() { Text = v.Version + " :  " + v.CommitComment, Value = v.Version.ToString() }));

                return new SelectList(list, "Value", "Text");
            }
            finally
            {
                entityManager.Dispose();
            }
        }

        public SelectList GetEntityVersionsDesc(long id, long typeId)
        {
            EntityManager entityManager = new EntityManager();
            List<SelectListItem> list = new List<SelectListItem>();

            try
            {
                var instanceStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(typeId).EntityStoreType);

                instanceStore.GetVersionsById(id).OrderByDescending(l => l.Version).ToList().ForEach(v => list.Add(new SelectListItem() { Text = v.Version + " :  " + v.CommitComment, Value = v.Version.ToString() }));

                return new SelectList(list, "Value", "Text");
            }
            finally
            {
                entityManager.Dispose();
            }
        }

        public string GetEntityTypeName(long id)
        {
            EntityManager entityManager = new EntityManager();

            try
            {
                return entityManager.Entities.Where(e => e.Id.Equals(id)).Select(e => e.Name).FirstOrDefault();
            }
            finally
            {
                entityManager.Dispose();
            }
        }

        public SelectList GetEntityTypes()
        {
            List<SelectListItem> list = new List<SelectListItem>();

            EntityManager entityManager = new EntityManager();

            try
            {
                entityManager.Entities.ToList().ForEach(e => list.Add(new SelectListItem() { Text = e.Name, Value = e.Id.ToString() }));

                var entityWhitelist = GetEntityTypesWhitlist();

                List<SelectListItem> listNew = new List<SelectListItem>();
                // Check if whitelist has entries, if not use full list otherwise only use items from the whitelist
                if (entityWhitelist.Count() > 0)
                {
                    foreach (SelectListItem item in list)
                    {
                        foreach (var item2 in entityWhitelist)
                        {
                            if (item2.Text == item.Text)
                            {
                                listNew.Add(item);
                            }
                        }
                    }
                }
                else
                {
                    listNew = list;
                }

                return new SelectList(listNew, "Value", "Text");
            }
            finally
            {
                entityManager.Dispose();
            }
        }

        public List<EntityStoreItem> GetEntities(long typeId)
        {
            EntityManager entityManager = new EntityManager();

            try
            {
                var instanceStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(typeId).EntityStoreType);
                return instanceStore.GetEntities();
            }
            finally
            {
                entityManager.Dispose();
            }
        }

        public EntityReference Convert(CreateSimpleReferenceModel model)
        {
            EntityReference tmp = new EntityReference();
            tmp.SourceId = model.SourceId;
            tmp.SourceEntityId = model.SourceTypeId;
            tmp.SourceVersion = model.SourceVersion;
            tmp.TargetId = model.Target;
            tmp.TargetEntityId = model.TargetType;
            tmp.TargetVersion = model.TargetVersion;
            tmp.Context = model.Context;
            tmp.ReferenceType = model.ReferenceType;
            tmp.CreationDate = DateTime.Now;

            // get additional informations
            ReferenceConfigElement config = GetReferenceConfigByType(model.ReferenceType);

            if (config != null)
            {
                tmp.LinkType = config.LinkType;
                tmp.Category = config.Category;
            }

            return tmp;
        }

        public SimpleSourceReferenceModel GetSimpleReferenceModel(long id, long typeId, int version)
        {
            SimpleSourceReferenceModel tmp = new SimpleSourceReferenceModel();
            tmp.Id = id;
            tmp.TypeId = typeId;
            tmp.Version = version;
            tmp.Title = GetEntityTitle(id, typeId);
            tmp.Type = GetEntityTypeName(typeId);

            return tmp;
        }

        public ReferenceModel Convert(EntityReference entityReference)
        {
            ReferenceModel tmp = new ReferenceModel();

            if (EntityExist(entityReference.TargetId, entityReference.TargetEntityId) && EntityExist(entityReference.SourceId, entityReference.SourceEntityId))
            {
                tmp.Target = new ReferenceElementModel(
                    entityReference.TargetId,
                    entityReference.TargetVersion,
                    entityReference.TargetEntityId,
                    GetEntityTitle(entityReference.TargetId, entityReference.TargetEntityId, entityReference.TargetVersion),
                    GetEntityTypeName(entityReference.TargetEntityId),
                    entityReference.TargetVersion == CountVersions(entityReference.TargetId, entityReference.TargetEntityId) ? true : false
                    );

                tmp.Source = new ReferenceElementModel(
                    entityReference.SourceId,
                    entityReference.SourceVersion,
                    entityReference.SourceEntityId,
                    GetEntityTitle(entityReference.SourceId, entityReference.SourceEntityId, entityReference.SourceVersion),
                    GetEntityTypeName(entityReference.SourceEntityId),
                    entityReference.SourceVersion == CountVersions(entityReference.SourceId, entityReference.SourceEntityId) ? true : false
                    );

                tmp.Context = entityReference.Context;
                tmp.ReferenceType = entityReference.ReferenceType;
                tmp.RefId = entityReference.Id;
                tmp.LinkType = entityReference.LinkType;
                tmp.Category = entityReference.Category;

                return tmp;
            }

            return null;
        }

        public List<ReferenceModel> GetAllReferences(long id, long typeid)
        {
            List<ReferenceModel> tmp = new List<ReferenceModel>();
            EntityReferenceManager entityReferenceManager = new EntityReferenceManager();
            EntityReferenceHelper helper = new EntityReferenceHelper();

            try
            {
                // get all references where incoming is source
                var list = entityReferenceManager.References.Where(r => r.SourceId.Equals(id) && r.SourceEntityId.Equals(typeid)).ToList();
                list.ForEach(r => tmp.Add(helper.Convert(r)));
                tmp.RemoveAll(item => item == null);

                //get all refs where incoming is taret
                list = entityReferenceManager.References.Where(r => r.TargetId.Equals(id) && r.TargetEntityId.Equals(typeid)).ToList();
                list.ForEach(r => tmp.Add(helper.Convert(r)));
                tmp.RemoveAll(item => item == null);
            }
            catch (Exception ex)
            {
                throw new Exception("References could not be loaded", ex);
            }
            finally
            {
                entityReferenceManager.Dispose();
            }

            return tmp;
        }

        public List<ReferenceModel> GetSourceReferences(long id, long typeid, int version)
        {
            List<ReferenceModel> tmp = new List<ReferenceModel>();
            EntityReferenceManager entityReferenceManager = new EntityReferenceManager();
            EntityReferenceHelper helper = new EntityReferenceHelper();

            try
            {
                //get all refs where incoming is taret
                var list = entityReferenceManager.References.Where(r =>
                        r.TargetId.Equals(id) &&
                        r.TargetEntityId.Equals(typeid) &&
                        r.TargetVersion <= version
                        ).ToList();

                list.ForEach(r => tmp.Add(helper.Convert(r)));
                tmp.RemoveAll(item => item == null);
            }
            catch (Exception ex)
            {
                throw new Exception("References could not be loaded", ex);
            }
            finally
            {
                entityReferenceManager.Dispose();
            }

            return tmp;
        }

        public List<ReferenceModel> GetTargetReferences(long id, long typeid, int version)
        {
            List<ReferenceModel> tmp = new List<ReferenceModel>();
            EntityReferenceManager entityReferenceManager = new EntityReferenceManager();
            EntityReferenceHelper helper = new EntityReferenceHelper();

            try
            {
                // get all references where incoming is source
                var list = entityReferenceManager.References.Where(r =>
                            r.SourceId.Equals(id) &&
                            r.SourceEntityId.Equals(typeid) &&
                            r.SourceVersion <= version).ToList();

                list.ForEach(r => tmp.Add(helper.Convert(r)));
                tmp.RemoveAll(item => item == null);
            }
            catch (Exception ex)
            {
                throw new Exception("References could not be loaded", ex);
            }
            finally
            {
                entityReferenceManager.Dispose();
            }

            return tmp;
        }

        #region Entity Reference Config

        private ReferenceConfig getReferenceConfig()
        {
            string filepath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "EntityReferenceConfig.json");
            string dir = Path.GetDirectoryName(filepath);

            if (Directory.Exists(dir) && File.Exists(filepath))
            {
                ReferenceConfig referenceConfig = new ReferenceConfig();
                referenceConfig = JsonConvert.DeserializeObject<ReferenceConfig>(File.ReadAllText(filepath));

                return referenceConfig;
            }
            else
            {
                throw new FileNotFoundException("File EntityReferenceConfig.json not found in :" + dir, "EntityReferenceConfig.json");
            }
        }

        public ReferenceConfigElement GetReferenceConfigByType(string type)
        {
            if (_config != null)
            {
                return _config.ReferenceTypes.Where(e => e.ReferenceType.Equals(type)).FirstOrDefault();
            }

            return null;
        }


        /// <summary>
        /// this function return a list of all reference types. This types are listed in the entity reference config.xml in the workspace
        /// </summary>
        /// <returns></returns>
        public SelectList GetReferencesTypes()
        {
            
            if (_config != null)
            {
                var types = _config.ReferenceTypes.Select(e => new SelectListItem()
                {
                    Text = String.IsNullOrEmpty(e.Description) ? e.ReferenceType : e.Description,
                    Value = e.ReferenceType
                }).ToList();

                return new SelectList(types, "Value", "Text");
            }

            return new SelectList(new List<SelectListItem>(), "Value", "Text");
        }

        public SelectList GetReferencesTypes(string type)
        {

            if (_config != null)
            {
                var types = _config.ReferenceTypes.Where(e => e.LinkType.Equals(type)).Select(e => new SelectListItem()
                {
                    Text = String.IsNullOrEmpty(e.Description) ? e.ReferenceType : e.Description,
                    Value = e.ReferenceType
                }).ToList();

                return new SelectList(types, "Value", "Text");
            }

            return new SelectList(new List<SelectListItem>(), "Value", "Text");
        }

        public List<KeyValuePair<string, string>> GetReferencesTypesAsKVP(string type)
        {
            List<KeyValuePair<string,string>> tmp = new List<KeyValuePair<string, string>>();
            if (_config != null)
            {
                tmp = _config.ReferenceTypes.Where(e => e.LinkType.Equals(type)).Select(e => 
                    new KeyValuePair<string,string>(e.ReferenceType, string.IsNullOrEmpty(e.Description) ? e.ReferenceType : e.Description)).ToList();

            }

            return tmp;
        }

        public SelectList GetReferencesHelpTypes()
        {
            return GetReferencesTypes();
        }

        #endregion Entity Reference Config

        #region Entity Config

        /// <summary>
        /// this function return a list of all allowed entity types. This types are listed in the entity reference config.xml in the workspace
        /// </summary>
        /// <returns></returns>
        public SelectList GetEntityTypesWhitlist()
        {
            if (_config != null)
            {
                var types = _config.EntityWhiteList.Select(e => new SelectListItem()
                {
                    Text = e,
                    Value = e
                }).ToList();

                return new SelectList(types, "Value", "Text");
            }

            return new SelectList(new List<SelectListItem>(), "Value", "Text");
        }

        #endregion Entity Config
    }
}

