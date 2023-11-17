using BExIS.Dlm.Entities.Meanings;
using BExIS.Modules.Rpm.UI.Models;
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
    }
}