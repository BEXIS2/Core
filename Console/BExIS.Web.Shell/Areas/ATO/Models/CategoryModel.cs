using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Ato.UI.Models
{
    public class CategoryModel
    {
        public string Name;
        public string Description;
        public List<FileModel> FileModels;
       
        public CategoryModel()
        {
            Name = string.Empty;
            Description = string.Empty;
            
            FileModels = new List<FileModel>();
        }
    }
}