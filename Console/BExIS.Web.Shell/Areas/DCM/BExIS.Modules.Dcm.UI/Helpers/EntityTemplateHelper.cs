using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Modules.Dcm.UI.Models.EntityTemplate;
using BExIS.Security.Services.Objects;
using System.Collections.Generic;

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
            entityTemplate.AllowedFileTypes = model.AllowedFileTypes;
            entityTemplate.PermissionGroups = model.PermissionGroups;
            entityTemplate.NotificationGroups = model.NotificationGroups;
            entityTemplate.MetadataFields = model.MetadataFields;

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
                    entityTemplate.DisabledHooks = model.DisabledHooks;
                    entityTemplate.DatastructureList = model.DatastructureList;
                    entityTemplate.AllowedFileTypes = model.AllowedFileTypes;
                    entityTemplate.PermissionGroups = model.PermissionGroups;
                    entityTemplate.NotificationGroups = model.NotificationGroups;
                    entityTemplate.MetadataFields = model.MetadataFields;

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

        public static EntityTemplateModel ConvertTo(EntityTemplate entityTemplate)
        {
            EntityTemplateModel model = new EntityTemplateModel();
            model.Id = entityTemplate.Id;
            model.Name = entityTemplate.Name;
            model.Description = entityTemplate.Description;
            model.MetadataInvalidSaveMode = entityTemplate.MetadataInvalidSaveMode;
            model.HasDatastructure = entityTemplate.HasDatastructure;
            model.DisabledHooks = entityTemplate.DisabledHooks != null ? entityTemplate.DisabledHooks : new List<string>(); ;
            model.DatastructureList = entityTemplate.DatastructureList != null ? entityTemplate.DatastructureList : new List<long>();
            model.AllowedFileTypes = entityTemplate.AllowedFileTypes != null ? entityTemplate.AllowedFileTypes : new List<string>();
            model.PermissionGroups = entityTemplate.PermissionGroups != null ? entityTemplate.PermissionGroups : new List<long>();
            model.NotificationGroups = entityTemplate.NotificationGroups != null ? entityTemplate.NotificationGroups : new List<long>();
            model.MetadataFields = entityTemplate.MetadataFields != null ? entityTemplate.MetadataFields : new List<int>();

            model.MetadataStructure = new EntityTemplateModel.KvP(entityTemplate.MetadataStructure.Id, entityTemplate.MetadataStructure.Name);
            model.EntityType = new EntityTemplateModel.KvP(entityTemplate.EntityType.Id, entityTemplate.EntityType.Name);


            return model;

        }



    }
}