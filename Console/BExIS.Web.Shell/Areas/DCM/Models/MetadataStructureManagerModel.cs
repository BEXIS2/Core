using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BExIS.Ddm.Model;

namespace BExIS.Modules.Dcm.UI.Models
{
    public class MetadataStructureManagerModel
    {

        public List<MetadataStructureModel> MetadataStructureModels { get; set; }


        public MetadataStructureManagerModel()
        {
            MetadataStructureModels = new List<MetadataStructureModel>();
        }
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
        
        public long Id { get; set; }
        public string Name { get; set; }

        public EntityModel Entity { get; set; }

        [Display(Name = "Title Reference")]
        public string TitleNode { get; set; }

        [Display(Name = "Description Reference")]
        public string DescriptionNode { get; set; }

        public bool Active { get; set; }
        public bool HasSchema { get; set; }
        public List<SearchMetadataNode> MetadataNodes { get; set; }
        public List<EntityModel> EntityClasses { get; set; }

        public MetadataStructureModel()
        {
            EntityClasses = new List<EntityModel>();
            MetadataNodes = new List<SearchMetadataNode>();
            Entity = new EntityModel();
            HasSchema = false;
        }

    }

    public class EntityModel
    {
        [Display(Name = "Entity Name")]
        public string Name { get; set; }
        public string ClassPath { get; set; }

        public EntityModel()
        {
            Name = "";
            ClassPath = "";
        }
    }
}