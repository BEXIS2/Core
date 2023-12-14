using BExIS.Dlm.Entities.Meanings;
using BExIS.UI.Models;
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
            Id = 0;
            Name = String.Empty;
            Description = String.Empty;
        }

        public PrefixCategoryListItem(long _id, string _name, string _description)
        {
            Id = _id;
            Name = _name;
            Description = _description;
        }

    }

    public class PrefixListItem:ListItem
    {
        public string Url { get; set; }


        public PrefixListItem()
        {
            Id = 0;
            Text = String.Empty;
            Description = String.Empty;
            Url = String.Empty;
        }

        public PrefixListItem(long _id, string _name, string _description, string _url)
        {
            Id = _id;
            Text = _name;
            Description = _description;
            Url = _url;
        }

    }

    public class ExternalLinkModel
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public string Uri { get; set; }
        public ListItem Type { get; set; }
        public ListItem Prefix { get; set; }
        public PrefixCategoryListItem PrefixCategory { get; set; }

        public ExternalLinkModel()
        {
            Id = 0;
            Name= String.Empty;
            Uri = String.Empty;
            Type = new ListItem();
            Prefix = new PrefixListItem();
            PrefixCategory = new PrefixCategoryListItem();
        }

        public ExternalLinkModel(long id, string name,string uri, ExternalLinkType type, PrefixListItem prefix, PrefixCategoryListItem prefixCategory)
        {
            Id = id;
            Name = name;
            Uri = uri;
            Type = new ListItem((Int32)type,type.ToString());
            Prefix = prefix;
            PrefixCategory = prefixCategory;
        }
    }
}