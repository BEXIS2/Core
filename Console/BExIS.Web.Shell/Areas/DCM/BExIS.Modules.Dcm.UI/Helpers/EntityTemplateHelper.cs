using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Modules.Dcm.UI.Models.EntityTemplate;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dcm.UI.Helpers
{
    public class EntityTemplateHelper
    {
        public static EntityTemplate ConvertTo(EntityTemplateModel model)
        { 
            EntityTemplate entityTemplate = new EntityTemplate();
            entityTemplate.Name = model.Name;
            entityTemplate.Description = model.Description;
            entityTemplate.MetadataInvalidSaveMode = model.MetadataInvalidSaveMode;
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
                var metadataStructure = metadataSrtuctureManager.Repo.Get(model.MetadataStructure.Key);
                entityTemplate.MetadataStructure = metadataStructure;
            }


            // entity
            using (var entityManager = new EntityManager())
            {
                var entity = entityManager.EntityRepository.Get(model.EntityType.Key);
                entityTemplate.EntityType = entity;
            }

            return entityTemplate;

        }

        public static EntityTemplateModel ConvertTo(EntityTemplate entityTemplate)
        {
            EntityTemplateModel model = new EntityTemplateModel();
            model.Name = entityTemplate.Name;
            model.Description = entityTemplate.Description;
            model.MetadataInvalidSaveMode = entityTemplate.MetadataInvalidSaveMode;
            model.DisabledHooks = entityTemplate.DisabledHooks;
            model.DatastructureList = entityTemplate.DatastructureList;
            model.AllowedFileTypes = entityTemplate.AllowedFileTypes;
            model.PermissionGroups = entityTemplate.PermissionGroups;
            model.NotificationGroups = entityTemplate.NotificationGroups;
            model.MetadataFields = entityTemplate.MetadataFields;

            model.MetadataStructure = new EntityTemplateModel.KvP(entityTemplate.MetadataStructure.Id, entityTemplate.MetadataStructure.Name);
            model.EntityType = new EntityTemplateModel.KvP(entityTemplate.EntityType.Id, entityTemplate.EntityType.Name);


            return model;

        }

    }
}