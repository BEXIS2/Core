using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Meanings;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.Meanings;
using BExIS.Modules.Rpm.UI.Models;
using BExIS.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Modules.Rpm.UI.Helpers
{
    public class MeaningsHelper
    {
        public static MeaningModel ConvertTo(Meaning meaning)
        {
            if (meaning == null) return null;
            MeaningModel model = new MeaningModel();
            model.Id = meaning.Id;
            model.Name = meaning.Name;
            if (meaning.Description != null) model.Description = meaning.Description;
            model.Approved = meaning.Approved;
            model.Selectable = meaning.Selectable;

            if (meaning.Related_meaning != null && meaning.Related_meaning.Any())
            {
                meaning.Related_meaning.ToList().ForEach(x => model.Related_meaning.Add(ConvertTo(x)));
            }

            if (meaning.ExternalLinks != null && meaning.ExternalLinks.Any())
            {
                meaning.ExternalLinks.ToList().ForEach(x => model.ExternalLinks.Add(ConvertTo(x)));
            }

            if (meaning.Constraints != null && meaning.Constraints.Any())
            {
                meaning.Constraints.ToList().ForEach(x => model.Constraints.Add(ConvertTo(x)));
            }

            return model;
        }

        public static Meaning ConvertTo(MeaningModel model)
        {
            Meaning meaning = new Meaning();
            meaning.Id = model.Id;
            meaning.Name = model.Name;
            meaning.Description = model.Description;
            meaning.Approved = model.Approved;
            meaning.Selectable = model.Selectable;

            if (meaning.Related_meaning != null && meaning.Related_meaning.Any())
            {
                using (var meaningManager = new MeaningManager())
                {
                    var ids = model.Related_meaning.Select(m => m.Id);
                    meaning.Related_meaning = meaningManager.GetMeanings().Where(m => ids.Contains(m.Id)).ToList();
                }
            }

            if (model.ExternalLinks != null && model.ExternalLinks.Any())
            {
                foreach (MeaningEntryModel l in model.ExternalLinks)
                {
                    if (l.MappingRelation.Id > 0) // add only if there is a selected mapping relation
                    { 
                        meaning.ExternalLinks.Add(ConvertTo(l));
                    }
                }

            }

            if (model.Constraints != null && model.Constraints.Any())
            {
                using (var constraintManager = new ConstraintManager())
                {
                    var ids = model.Constraints.Select(m => m.Id);
                    meaning.Constraints = constraintManager.ConstraintRepository.Query(c => ids.Contains(c.Id)).ToList<Constraint>();
                }
            }

            return meaning;
        }

        public static MeaningEntry ConvertTo(MeaningEntryModel model)
        {
            MeaningEntry entry = new MeaningEntry();

            using (var meaningManager = new MeaningManager())
            {
                if (model.MappingRelation != null)
                {
                    entry.MappingRelation = meaningManager.GetExternalLink(model.MappingRelation.Id);

                    if (model.MappedLinks.Any())
                    {
                        var ids = model.MappedLinks.Select(m => m.Id);

                        entry.MappedLinks = meaningManager.GetExternalLinks().Where(e => ids.Contains(e.Id)).ToList();
                    }
                }
            }
            return entry;
        }

        public static MeaningEntryModel ConvertTo(MeaningEntry entry)
        {
            MeaningEntryModel model = new MeaningEntryModel();
            model.MappingRelation = ConvertToListItem(entry.MappingRelation);

            if (entry.MappedLinks.Any())
            {
                entry.MappedLinks.ToList().ForEach(m => model.MappedLinks.Add(ConvertToListItem(m)));
            }

            return model;
        }

        public static ListItem ConvertTo(Constraint constraint)
        {
            ListItem item = new ListItem();
            item.Id = constraint.Id;
            item.Text = constraint.Name;
            item.Group = getConstraintType(constraint);
            item.Description = constraint.FormalDescription;
            return item;
        }

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

            if (link.Type != ExternalLinkType.prefix && link.Prefix != null)
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
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (model.Type.Id.Equals(ExternalLinkType.prefix) && model.PrefixCategory == null) throw new ArgumentNullException(nameof(model.PrefixCategory));

            ExternalLink link = new ExternalLink();
            using (var meaningManager = new MeaningManager())
            {
                PrefixCategory prefixCategory = null;
                ExternalLink prefix = null;

                if (model.PrefixCategory != null)
                {
                    prefixCategory = meaningManager.GetPrefixCategory(model.PrefixCategory.Id);
                }

                if (model.Prefix != null)
                {
                    prefix = meaningManager.GetExternalLink(model.Prefix.Id);
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

        private static string getConstraintType(Constraint c)
        {
            if (c is DomainConstraint) return "Domain";
            if (c is RangeConstraint) return "Range";
            if (c is PatternConstraint) return "Pattern";

            return string.Empty;
        }
    }
}