﻿using BExIS.UI.Models;
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
        public long Id { get; set; }

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
        public ListItem EntityType { get; set; }


        /// <summary>
        ///Metadata Structure
        /// </summary>
        public virtual ListItem MetadataStructure { get; set; }


        public virtual List<int> MetadataFields { get; set; }

        /// <summary>
        /// should the user be able to save the metadata if not alle mandatory are filled correct
        /// if true - also not valid metadat will be stored
        /// if false -  only valid metadata will be stored
        /// </summary>
        public virtual bool MetadataInvalidSaveMode { get; set; }

        /// <summary>
        /// If this flag is true, the entity will have a datastructure
        /// </summary>
        public virtual bool HasDatastructure { get; set; }

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
            Id = 0;
            Name = "";
            Description = "";
            MetadataStructure = null;
            EntityType = null;
            AllowedFileTypes = new List<string>();
            DisabledHooks = new List<string>();
            DatastructureList = new List<long>();
            MetadataFields = new List<int>();
            PermissionGroups = new List<long>();
            NotificationGroups = new List<long>();
            MetadataInvalidSaveMode = false;
            HasDatastructure = false;
        }


    }
}