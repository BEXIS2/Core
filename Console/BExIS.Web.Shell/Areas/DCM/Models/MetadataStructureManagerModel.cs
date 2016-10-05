using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using BExIS.Ddm.Model;

namespace BExIS.Web.Shell.Areas.DCM.Models
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

        [Display(Name = "Entity Class Path")]
        public string EntityClassPath { get; set; }

        [Display(Name = "Title Reference")]
        public string TitleNode { get; set; }

        [Display(Name = "Description Reference")]
        public string DescriptionNode { get; set; }
        public bool Active { get; set; }
        public List<SearchMetadataNode> MetadataNodes { get; set; }
        public List<string> EnitiesClassPaths { get; set; }

        public MetadataStructureModel()
        {
            EnitiesClassPaths = new List<string>();
            MetadataNodes = new List<SearchMetadataNode>();
        }

    }
}