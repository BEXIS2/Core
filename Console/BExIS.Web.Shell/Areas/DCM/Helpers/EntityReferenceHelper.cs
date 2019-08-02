using BExIS.Modules.Dcm.UI.Models.EntityReference;
using BExIS.Security.Services.Objects;
using BExIS.Security.Entities.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Dim.Helpers.Mapping;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Entities.Data;
using System.Xml;
using BExIS.Xml.Helpers;
using System.Xml.Linq;

namespace BExIS.Modules.Dcm.UI.Helpers
{
    public class EntityReferenceHelper
    {
        public EntityReferenceHelper()
        {
        }

        public string GetEntityTitle(long id, long typeId)
        {
            EntityManager entityManager = new EntityManager();

            try
            {
                var instanceStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(typeId).EntityStoreType);
                return instanceStore.GetTitleById(id);
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

                return new SelectList(list, "Value", "Text");
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
            tmp.TargetId = model.Target;
            tmp.TargetEntityId = model.TargetType;
            tmp.Context = model.Context;

            return tmp;
        }

        public SimpleReferenceModel GetSimplereferenceModel(long id, long typeId)
        {
            SimpleReferenceModel tmp = new SimpleReferenceModel();
            tmp.Id = id;
            tmp.TypeId = typeId;
            tmp.Title = GetEntityTitle(id, typeId);
            tmp.Type = GetEntityTypeName(typeId);

            return tmp;
        }

        public SimpleReferenceModel GetTarget(EntityReference entityReference)
        {
            SimpleReferenceModel tmp = new SimpleReferenceModel();
            tmp.Id = entityReference.TargetId;
            tmp.TypeId = entityReference.TargetEntityId;
            tmp.Title = GetEntityTitle(entityReference.TargetId, entityReference.TargetEntityId);
            tmp.Type = GetEntityTypeName(entityReference.TargetEntityId);
            tmp.Context = entityReference.Context;

            return tmp;
        }

        public SimpleReferenceModel GetSource(EntityReference entityReference)
        {
            SimpleReferenceModel tmp = new SimpleReferenceModel();
            tmp.Id = entityReference.SourceId;
            tmp.TypeId = entityReference.SourceEntityId;
            tmp.Title = GetEntityTitle(entityReference.SourceId, entityReference.SourceEntityId);
            tmp.Type = GetEntityTypeName(entityReference.SourceEntityId);
            tmp.Context = entityReference.Context;

            return tmp;
        }

        public List<SimpleReferenceModel> GetAllReferences(long id, long typeid)
        {
            List<SimpleReferenceModel> tmp = new List<SimpleReferenceModel>();
            EntityReferenceManager entityReferenceManager = new EntityReferenceManager();
            EntityReferenceHelper helper = new EntityReferenceHelper();

            try
            {
                // get all references where incoming is source
                var list = entityReferenceManager.References.Where(r => r.SourceId.Equals(id) && r.SourceEntityId.Equals(typeid)).ToList();
                list.ForEach(r => tmp.Add(helper.GetTarget(r)));

                //get all refs where incoming is taret
                list = entityReferenceManager.References.Where(r => r.TargetId.Equals(id) && r.TargetEntityId.Equals(typeid)).ToList();

                list.ForEach(r => tmp.Add(helper.GetSource(r)));
            }
            catch (Exception)
            {
            }
            finally
            {
                entityReferenceManager.Dispose();
            }

            return tmp;
        }

        public List<SimpleReferenceModel> GetAllMetadataReferences(long id, long typeid)
        {
            List<SimpleReferenceModel> tmp = new List<SimpleReferenceModel>();
            EntityReferenceHelper helper = new EntityReferenceHelper();
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            MappingUtils mappingUtils = new MappingUtils();
            XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();
            DatasetManager datasetManager = new DatasetManager();
            DatasetVersion datasetVersion;
            EntityManager entityManager = new EntityManager();

            try
            {
                datasetVersion = datasetManager.GetDatasetLatestVersion(id);
                if (datasetVersion != null)
                {
                    long metadataStrutcureId = datasetVersion.Dataset.MetadataStructure.Id;

                    if (MappingUtils.ExistMappingWithEntityFromRoot(
                        datasetVersion.Dataset.MetadataStructure.Id,
                        BExIS.Dim.Entities.Mapping.LinkElementType.MetadataStructure,
                        typeid))
                    {
                        //load metadata and searching for the entity Attrs
                        XDocument metadata = XmlUtility.ToXDocument(datasetVersion.Metadata);

                        IEnumerable<XElement> xelements = XmlUtility.GetXElementsByAttribute("entityid", metadata);

                        string entityName = xmlDatasetHelper.GetEntityNameFromMetadatStructure(metadataStrutcureId, new Dlm.Services.MetadataStructure.MetadataStructureManager());
                        Entity entityType = entityManager.Entities.Where(e => e.Name.Equals(entityName)).FirstOrDefault();

                        foreach (XElement e in xelements)
                        {
                            // get enitiy id from xelement
                            long entityId = 0;
                            if (Int64.TryParse(e.Attribute("entityid").Value.ToString(), out entityId))
                            {
                                tmp.Add(new SimpleReferenceModel(
                                        entityId,
                                        1,
                                        entityType.Id,
                                        entityType.Name,
                                        helper.GetEntityTitle(entityId, entityType.Id)
                                    ));
                            }
                        }
                    }
                }

                return tmp;
            }
            catch (Exception)
            {
            }
            finally
            {
            }

            return tmp;
        }
    }
}