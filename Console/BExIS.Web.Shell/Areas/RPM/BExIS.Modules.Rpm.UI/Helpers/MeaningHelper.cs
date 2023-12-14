using BExIS.Dlm.Entities.Meanings;
using BExIS.Dlm.Services.Meanings;
using BExIS.Modules.Rpm.UI.Models;
using BExIS.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Rpm.UI.Helpers
{
    public class MeaningsHelper
    {
        public static PrefixCategoryListItem ConvertTo(PrefixCategory prefixCategory)
        { 
            if (prefixCategory == null)
                return null;
            if (prefixCategory.Name == null)
                throw new ArgumentNullException(nameof(prefixCategory));
            
            PrefixCategoryListItem prefixCategoryListItem = new PrefixCategoryListItem();
            prefixCategoryListItem.Name = prefixCategory.Name;
            prefixCategoryListItem.Description = prefixCategory.Description;
            prefixCategoryListItem.Id = prefixCategory.Id;

            return prefixCategoryListItem;
        }

        public static PrefixCategory ConvertTo(PrefixCategoryListItem prefixCategoryListItem)
        {
            if (prefixCategoryListItem == null)
                return null;
            if (prefixCategoryListItem.Name == null)
                throw new ArgumentNullException(nameof(prefixCategoryListItem));

            PrefixCategory prefixCategory = new PrefixCategory();
            prefixCategory.Name = prefixCategoryListItem.Name;
            prefixCategory.Description = prefixCategoryListItem.Description;
            prefixCategory.Id = prefixCategoryListItem.Id;

            return prefixCategory;
        }

        public static ExternalLinkModel ConvertTo(ExternalLink link)
        {
            PrefixListItem prefix = null;
            PrefixCategoryListItem prefixCategoryListItem = null;

            if (link.Type!=ExternalLinkType.prefix && link.Prefix != null)
            {
                prefix = new PrefixListItem(link.Prefix.Id, link.Prefix.Name, link.Prefix.prefixCategory?.Name, link.Prefix.URI);           
            }

            if (link.prefixCategory != null && link.Type == ExternalLinkType.prefix) prefixCategoryListItem = ConvertTo(link.prefixCategory);

            ExternalLinkModel model = new ExternalLinkModel(
                link.Id,
                link.Name,
                link.URI,
                link.Type,
                prefix,
                prefixCategoryListItem
                );

            return model;
        }

        public static ExternalLink ConvertTo(ExternalLinkModel model)
        {
            if(model == null ) throw new ArgumentNullException(nameof(model));
            if(model.Type.Id.Equals(ExternalLinkType.prefix) && model.PrefixCategory == null ) throw new ArgumentNullException(nameof(model.PrefixCategory));

            ExternalLink link = new ExternalLink();
            using (var meaningManager = new MeaningManager())
            {
                PrefixCategory prefixCategory = null;
                ExternalLink prefix = null;

                if (model.PrefixCategory != null)
                {
                    prefixCategory = meaningManager.getPrefixCategory(model.PrefixCategory.Id);
                }

                if (model.Prefix != null)
                {
                    prefix = meaningManager.getExternalLink(model.Prefix.Id);
                }

              
                link.Id = model.Id;
                link.Name = model.Name;
                link.URI = model.Uri;
                link.Type = (ExternalLinkType)model.Type.Id;
                link.Prefix = prefix;
                link.prefixCategory = prefixCategory;
            }
            return link;
        }

        public static ListItem ConvertToListItem(ExternalLink link)
        {
            return new ListItem()
            {
                Id = link.Id,
                Text = link.Name,
                Group = link.Type.ToString()
            };
        }

        public static PrefixListItem ConvertToPrefixListItem(ExternalLink link)
        {
            return new PrefixListItem()
            {
                Id = link.Id,
                Text = link.Name,
                Group = link.prefixCategory?.Name,
                Url = link.URI
            };
        }
    }
}