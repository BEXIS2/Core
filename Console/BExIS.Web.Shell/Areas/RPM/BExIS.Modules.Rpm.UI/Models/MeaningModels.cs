using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Rpm.UI.Models
{
    public class PrefixCategoryListItem
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public PrefixCategoryListItem()
        { 
            Id= 0;
            Name=String.Empty;
            Description= String.Empty;
        }

        public PrefixCategoryListItem(long _id, string _name, string _description)
        {
            Id = _id;
            Name = _name;
            Description = _description;
        }

    }
}