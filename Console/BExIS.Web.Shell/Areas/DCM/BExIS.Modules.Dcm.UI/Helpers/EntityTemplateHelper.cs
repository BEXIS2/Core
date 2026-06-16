using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Modules.Dcm.UI.Models.EntityTemplate;
using BExIS.Security.Services.Objects;
using BExIS.UI.Models;
using BExIS.Xml.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Modules.Dcm.UI.Helpers
{
    public class EntityTemplateHelper
    {
        public static EntityTemplate ConvertTo(EntityTemplateModel model)
        {
            EntityTemplate entityTemplate = new EntityTemplate();
            entityTemplate.Id = model.Id;
            entityTemplate.Name = model.Name;
            entityTemplate.Description = model.Description;
            entityTemplate.MetadataInvalidSaveMode = model.MetadataInvalidSaveMode;
            entityTemplate.HasDatastructure = model.HasDatastructure;
            entityTemplate.DisabledHooks = model.DisabledHooks;
            entityTemplate.DatastructureList = model.DatastructureList;
            entityTemplate.HasExtension = model.HasExtension;
            entityTemplate.ExtensionList = model.ExtensionList;
            entityTemplate.AllowedFileTypes = model.AllowedFileTypes;
            entityTemplate.PermissionGroups = model.PermissionGroups;
            entityTemplate.NotificationGroups = model.NotificationGroups;
            entityTemplate.MetadataFields = model.MetadataFields;
            entityTemplate.Activated = model.Activated;

            // load entites
            // metadata

            using (var metadataSrtuctureManager = new MetadataStructureManager())
            {
                var metadataStructure = metadataSrtuctureManager.Repo.Get(model.MetadataStructure.Id);
                entityTemplate.MetadataStructure = metadataStructure;
            }

            // entity
            using (var entityManager = new EntityManager())
            {
                var entity = entityManager.EntityRepository.Get(model.EntityType.Id);
                entityTemplate.EntityType = entity;
            }

            return entityTemplate;
        }

        public static EntityTemplate Merge(EntityTemplateModel model)
        {
            using (var entityTemplateManager = new EntityTemplateManager())
            using (var entityManager = new EntityManager())
            using (var metadataSrtuctureManager = new MetadataStructureManager())
            {
                EntityTemplate entityTemplate = entityTemplateManager.Repo.Get(model.Id);
                if (entityTemplate != null)
                {
                    entityTemplate.Name = model.Name;
                    entityTemplate.Description = model.Description;
                    entityTemplate.MetadataInvalidSaveMode = model.MetadataInvalidSaveMode;
                    entityTemplate.HasDatastructure = model.HasDatastructure;
                    entityTemplate.HasExtension = model.HasExtension;
                    entityTemplate.DisabledHooks = model.DisabledHooks;
                    entityTemplate.DatastructureList = model.DatastructureList;
                    entityTemplate.ExtensionList = model.ExtensionList;
                    entityTemplate.AllowedFileTypes = model.AllowedFileTypes;
                    entityTemplate.PermissionGroups = model.PermissionGroups;
                    entityTemplate.NotificationGroups = model.NotificationGroups;
                    entityTemplate.MetadataFields = model.MetadataFields;
                    entityTemplate.Activated = model.Activated;

                    // load entites
                    // metadata
                    var metadataStructure = metadataSrtuctureManager.Repo.Get(model.MetadataStructure.Id);
                    entityTemplate.MetadataStructure = metadataStructure;

                    // entity
                    var entity = entityManager.EntityRepository.Get(model.EntityType.Id);
                    entityTemplate.EntityType = entity;
                }

                return entityTemplate;
            }
        }

        public static EntityTemplateModel ConvertTo(EntityTemplate entityTemplate, bool withLinkedSubjects = true)
        {
            EntityTemplateModel model = new EntityTemplateModel();
            model.Id = entityTemplate.Id;
            model.Name = entityTemplate.Name;
            model.Description = entityTemplate.Description;
            model.MetadataInvalidSaveMode = entityTemplate.MetadataInvalidSaveMode;
            model.HasDatastructure = entityTemplate.HasDatastructure;
            model.HasExtension = entityTemplate.HasExtension;
            model.DisabledHooks = entityTemplate.DisabledHooks != null ? entityTemplate.DisabledHooks : new List<string>(); ;
            model.DatastructureList = entityTemplate.DatastructureList != null ? entityTemplate.DatastructureList : new List<long>();
            model.ExtensionList = entityTemplate.ExtensionList != null ? entityTemplate.ExtensionList : new List<ExtensionType>();
            model.AllowedFileTypes = entityTemplate.AllowedFileTypes != null ? entityTemplate.AllowedFileTypes : new List<string>();
            model.PermissionGroups = entityTemplate.PermissionGroups != null ? entityTemplate.PermissionGroups : new PermissionsType();
            model.NotificationGroups = entityTemplate.NotificationGroups != null ? entityTemplate.NotificationGroups : new List<long>();
            model.MetadataFields = entityTemplate.MetadataFields != null ? entityTemplate.MetadataFields : new List<int>();
            model.Activated = entityTemplate.Activated;


            var xmlDatasetHelper = new XmlDatasetHelper();
            var entity = xmlDatasetHelper.GetEntityNameFromMetadatStructure(entityTemplate.MetadataStructure.Id); // get entity name from metadata structure
            model.MetadataStructure = new ListItem(entityTemplate.MetadataStructure.Id, entityTemplate.MetadataStructure.Name, entity);


            model.EntityType = new ListItem(entityTemplate.EntityType.Id, entityTemplate.EntityType.Name);

            // check if subject are allready created, and list them for the view
            using (var datasetManager = new DatasetManager())
            {
                long etId = entityTemplate.Id;
                var datasetsetIdsWithThisTemplate = datasetManager.DatasetRepo.Query().Where(d => d.EntityTemplate.Id.Equals(etId)).Select(d => d.Id).ToList();
                model.InUse = datasetsetIdsWithThisTemplate.Count > 0 ? true : false; // set in use if count greater then 0

                if (withLinkedSubjects) // load linked subject - not needed in create
                {
                    var dsvs = datasetManager.GetDatasetLatestVersions(datasetsetIdsWithThisTemplate);

                    // get throw all the subjects that are linked to the entitytemplate
                    foreach (var dsv in dsvs)
                    {
                        var l = new ListItem();
                        l.Id = dsv.Dataset.Id;
                        l.Text = dsv != null ? dsv.Title.ToString() : "Dataset is checked out."; // if a version is available, get the title
                        l.Group = entityTemplate.EntityType.Name; // add entity name
                        model.LinkedSubjects.Add(l);
                    }
                }
            }

            return model;
        }
    }
}