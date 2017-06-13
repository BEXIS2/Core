using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using BExIS.Ddm.Model;
using BExIS.Security.Entities.Objects;

namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class EntityMetadataModel
    {
        public EntityMetadataModel()
        {
            Name = "";
            ClassPath = "";
        }

        public string ClassPath { get; set; }

        [Display(Name = "Entity Name")]
        public string Name { get; set; }
    }

    public class MetadataStructureManagerModel
    {
        public MetadataStructureManagerModel()
        {
            MetadataStructureModels = new List<MetadataStructureModel>();
        }

        public List<MetadataStructureModel> MetadataStructureModels { get; set; }
    }

    /*
     * grid columns:
        - Id
        - name (title of the metadata structure)
        - entity
        - mapping paths
        - description paths
        - functions

        functions needed:
        - delete
        - activate/deactivate
        - (change entity)
     */

    public class MetadataStructureModel
    {
        public MetadataStructureModel()
        {
            EntityClasses = new List<EntityMetadataModel>();
            MetadataNodes = new List<SearchMetadataNode>();
            Entity = new EntityMetadataModel();
            HasSchema = false;
        }

        public bool Active { get; set; }

        [Display(Name = "Description Reference")]
        public string DescriptionNode { get; set; }

        public EntityMetadataModel Entity { get; set; }
        public List<EntityMetadataModel> EntityClasses { get; set; }
        public bool HasSchema { get; set; }
        public long Id { get; set; }
        public List<SearchMetadataNode> MetadataNodes { get; set; }
        public string Name { get; set; }

        [Display(Name = "Title Reference")]
        public string TitleNode { get; set; }
    }
}