using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dcm.UI.Models.EntityTemplate
{
    public class EntityTemplateModel
    {

        /// <summary>
        /// Name of the Entity Template
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Description of the Entity Template
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Entity
        /// </summary>
        public KvP EntityType { get; set; }


        /// <summary>
        ///Metadata Structure
        /// </summary>
        public virtual KvP MetadataStructure { get; set; }


        public virtual List<int> MetadataFields { get; set; }

        /// <summary>
        /// should the user be able to save the metadata if not alle mandatory are filled correct
        /// if true - also not valid metadat will be stored
        /// if false -  only valid metadata will be stored
        /// </summary>
        public virtual bool MetadataInvalidSaveMode { get; set; }
        /// <summary>
        /// List of available Datatructures
        /// </summary>
        public virtual List<long> DatastructureList { get; set; }

        public List<string> AllowedFileTypes { get; set; }

        /// <summary>
        /// list of hidden hooks from ui
        /// </summary>

        public virtual List<string> DisabledHooks { get; set; }

        /// <summary>
        /// add this groups to the notifications
        /// when a email is sended to the owner or admin send also to this groups
        /// </summary>
        public virtual List<long> NotificationGroups { get; set; }


        /// <summary>
        /// add this groups to the permissions
        /// when a email is sended to the owner or admin send also to this groups
        /// </summary>
        public virtual List<long> PermissionGroups { get; set; }

        public EntityTemplateModel()
        {
            Name = "";
            Description = "";
            AllowedFileTypes = new List<string>();
            DisabledHooks = new List<string>();
            DatastructureList = new List<long>();
            MetadataFields = new List<int>();
            PermissionGroups = new List<long>();
            NotificationGroups = new List<long>();
        }

        public class KvP
        {
            public long Key { get; set; }
            public string Value { get; set; }

            public KvP()
            {
                Key = 0;
                Value = ""; 
            }

            public KvP(long key, string value)
            {
                Key = key;
                Value = value;
            }
        }
    }
}