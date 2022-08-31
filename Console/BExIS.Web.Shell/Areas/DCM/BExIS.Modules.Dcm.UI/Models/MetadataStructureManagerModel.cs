using BExIS.Utils.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BExIS.Modules.Dcm.UI.Models
{
    public class EntityModel
    {
        public EntityModel()
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
            EntityClasses = new List<EntityModel>();
            MetadataNodes = new List<SearchMetadataNode>();
            Entity = new EntityModel();
            HasSchema = false;
        }

        public bool Active { get; set; }

        [Display(Name = "Description Reference")]
        public string DescriptionNode { get; set; }

        public EntityModel Entity { get; set; }
        public List<EntityModel> EntityClasses { get; set; }
        public bool HasSchema { get; set; }
        public long Id { get; set; }
        public List<SearchMetadataNode> MetadataNodes { get; set; }
        public string Name { get; set; }

        [Display(Name = "Title Reference")]
        public string TitleNode { get; set; }
    }
}