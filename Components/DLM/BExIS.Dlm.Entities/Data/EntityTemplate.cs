using BExIS.Security.Entities.Objects;
using Newtonsoft.Json;
using System.Collections.Generic;
using Vaiona.Entities.Common;

namespace BExIS.Dlm.Entities.Data
{

    public class EntityTemplate : BaseEntity
    {

        /// <summary>
        /// Name of the Entity Template
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Description of the Entity Template
        /// </summary>
        public virtual string Description { get; set; }

        /// <summary>
        /// Entity
        /// </summary>
        public virtual Entity EntityType { get; set; }

        /// <summary>
        ///Metadata Structure
        /// </summary>
        public virtual MetadataStructure.MetadataStructure MetadataStructure { get; set; }

        /// <summary>
        /// List of available Datatructures
        /// </summary>
        public virtual string JsonMetadataFields { get; set; }

        public virtual List<int> MetadataFields
        {
            get
            {
                return JsonConvert.DeserializeObject<List<int>>(JsonMetadataFields);
            }
            set
            {
                JsonMetadataFields = JsonConvert.SerializeObject(value);
            }
        }

        /// <summary>
        /// should the user be able to save the metadata if not alle mandatory are filled correct
        /// if true - also not valid metadat will be stored
        /// if false -  only valid metadata will be stored
        /// </summary>
        public virtual bool MetadataInvalidSaveMode { get; set; }

        /// <summary>
        /// should the user be able to save the metadata if not alle mandatory are filled correct
        /// if true - also not valid metadat will be stored
        /// if false -  only valid metadata will be stored
        /// </summary>
        public virtual bool HasDatastructure { get; set; }

        /// <summary>
        /// List of available Datatructures
        /// </summary>
        public virtual string JsonDatastructureList { get; set; }
        public virtual List<long> DatastructureList
        {
            get
            {
                return JsonConvert.DeserializeObject<List<long>>(JsonDatastructureList);
            }
            set
            {
                JsonDatastructureList = JsonConvert.SerializeObject(value);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public virtual string JsonAllowedFileTypes { get; set; }
        /// 
        public virtual List<string> AllowedFileTypes
        {
            get
            {
                return JsonConvert.DeserializeObject<List<string>>(JsonAllowedFileTypes);
            }
            set
            {
                JsonAllowedFileTypes = JsonConvert.SerializeObject(value);
            }
        }

        /// <summary>
        /// list of hidden hooks from ui
        /// </summary>
        public virtual string JsonDisabledHooks { get; set; }
        public virtual List<string> DisabledHooks
        {
            get
            {
                return JsonConvert.DeserializeObject<List<string>>(JsonDisabledHooks);
            }
            set
            {
                JsonDisabledHooks = JsonConvert.SerializeObject(value);
            }
        }

        /// <summary>
        /// add this groups to the notifications
        /// when a email is sended to the owner or admin send also to this groups
        /// </summary>
        public virtual string JsonNotificationGroups { get; set; }
        public virtual List<long> NotificationGroups
        {
            get
            {
                return JsonConvert.DeserializeObject<List<long>>(JsonNotificationGroups);
            }
            set
            {
                JsonNotificationGroups = JsonConvert.SerializeObject(value);
            }
        }

        /// <summary>
        /// add this groups to the permissions
        /// when a email is sended to the owner or admin send also to this groups
        /// </summary>
        public virtual string JsonPermissionGroups { get; set; }
        public virtual PermissionsType PermissionGroups
        {
            get
            {
                try{
                    return JsonConvert.DeserializeObject<PermissionsType>(JsonPermissionGroups);
                }
                catch {
                    return new PermissionsType();
                }
            }
            set
            {
                JsonPermissionGroups = JsonConvert.SerializeObject(value);
            }
        }

        public EntityTemplate()
        {
            Name = "";
            Description = "";
            AllowedFileTypes = new List<string>();
            DisabledHooks = new List<string>();
            DatastructureList = new List<long>();
            MetadataFields = new List<int>();
            PermissionGroups = new PermissionsType();
            NotificationGroups = new List<long>();


            JsonAllowedFileTypes = "";
            JsonDatastructureList = "";
            JsonDisabledHooks = "";
            JsonMetadataFields = "";
            JsonPermissionGroups = "";
            JsonNotificationGroups = "";
        }

        public EntityTemplate(string name, string description, Entity entityType, MetadataStructure.MetadataStructure metadataStructure)
        {
            Name = name;
            Description = description;
            EntityType = entityType;
            MetadataStructure = metadataStructure;
            AllowedFileTypes = new List<string>();
            DisabledHooks = new List<string>();
            DatastructureList = new List<long>();
            MetadataFields = new List<int>();
            PermissionGroups = new PermissionsType();
            NotificationGroups = new List<long>();

            JsonAllowedFileTypes = "";
            JsonDatastructureList = "";
            JsonDisabledHooks = "";
            JsonMetadataFields = "";
            JsonPermissionGroups = "";
            JsonNotificationGroups = "";
        }

    }

    public class PermissionsType
    {
        public List<long> Full { get; set; }
        public List<long> ViewEditGrant { get; set; }
        public List<long> ViewEdit { get; set; }
        public List<long> View { get; set; }

        public PermissionsType()
        { 
            Full = new List<long>();
            ViewEditGrant = new List<long>();
            View = new List<long>();
            ViewEdit = new List<long>();
        }

    }
}
