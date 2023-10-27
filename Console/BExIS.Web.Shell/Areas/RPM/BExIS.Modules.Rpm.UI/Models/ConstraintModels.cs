using BExIS.Dlm.Entities.DataStructure;
using BExIS.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Modules.Rpm.UI.Models
{

    public class CreateConstraintModel
    {
        public string Description { get; set; }
        public string Name { get; set; }
    }

    public class CreateDomainConstraintModel : CreateConstraintModel
    {
    }

    public class CreatePatternConstraintModel : CreateConstraintModel
    {
    }

    public class CreateRangeConstraintModel : CreateConstraintModel
    {
    }

    public class ReadConstraintModel
    {
        public long Id { get; set; }
        public int Version { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FormalDescription { get; set; }
        public string Type { get; set; }
        public bool InUse { get; set; }

        public static ReadConstraintModel Convert(Constraint constraint)
        {
            return new ReadConstraintModel()
            {
                Id = constraint.Id,
                Version = constraint.VersionNo,
                Name = constraint.Name ?? "Constraint No. " + constraint.Id,
                Description = constraint.Description,
                FormalDescription = constraint.FormalDescription,
                Type = "",
                InUse = constraint.DataContainer != null && constraint.DataContainer.Id > 0
            };
        }
        public static ReadConstraintModel Convert(DomainConstraint constraint)
        {
            return new ReadConstraintModel()
            {
                Id = constraint.Id,
                Version = constraint.VersionNo,
                Name = constraint.Name ?? "Constraint No. " + constraint.Id,
                Description = constraint.Description,
                FormalDescription = constraint.FormalDescription,
                Type = ConstraintType.Domain.ToString(),
                InUse = constraint.DataContainer != null && constraint.DataContainer.Id > 0
            };
        }
        public static ReadConstraintModel Convert(PatternConstraint constraint)
        {
            return new ReadConstraintModel()
            {
                Id = constraint.Id,
                Version = constraint.VersionNo,
                Name = constraint.Name ?? "Constraint No. " + constraint.Id,
                Description = constraint.Description,
                FormalDescription = constraint.FormalDescription,
                Type = ConstraintType.Pattern.ToString(),
                InUse = constraint.DataContainer != null && constraint.DataContainer.Id > 0
            };
        }
        public static ReadConstraintModel Convert(RangeConstraint constraint)
        {
            return new ReadConstraintModel()
            {
                Id = constraint.Id,
                Version = constraint.VersionNo,
                Name = constraint.Name ?? "Constraint No. " + constraint.Id,
                Description = constraint.Description,
                FormalDescription = constraint.FormalDescription,
                Type = ConstraintType.Range.ToString(),
                InUse = constraint.DataContainer != null && constraint.DataContainer.Id > 0
            };
        }
    }

    public class ReadDomainConstraintItemModel
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public static ReadDomainConstraintItemModel Convert(DomainItem item)
        {
            return new ReadDomainConstraintItemModel()
            {
                Key = item.Key,
                Value = item.Value
            };
        }
    }

    public class ReadDomainConstraintModel : ReadConstraintModel
    {
        public List<ReadDomainConstraintItemModel> Items { get; set; }

        public static ReadDomainConstraintModel Convert(DomainConstraint constraint)
        {
            return new ReadDomainConstraintModel()
            {
                Id = constraint.Id,
                Name = constraint.Name,
                Description = constraint.Description,
                Version = constraint.VersionNo,
                Items = constraint.Items.Select(i => ReadDomainConstraintItemModel.Convert(i)).ToList()
            };
        }
    }

    public class ReadPatternConstraintModel : ReadConstraintModel
    {
        public string Pattern { get; set; }

        public static ReadPatternConstraintModel Convert(PatternConstraint constraint)
        {
            return new ReadPatternConstraintModel()
            {
                Id = constraint.Id,
                Name = constraint.Name,
                Description = constraint.Description,
                Version = constraint.VersionNo,
                Pattern = constraint.MatchingPhrase
            };
        }
    }

    public class ReadRangeConstraintModel : ReadConstraintModel
    {
        public static ReadRangeConstraintModel Convert(RangeConstraint constraint)
        {
            return new ReadRangeConstraintModel()
            {
                Id = constraint.Id,
                Name = constraint.Name,
                Description = constraint.Description,
                Version = constraint.VersionNo
            };
        }
    }

    public class UpdateConstraintModel
    {
        public string Description { get; set; }
        public string Name { get; set; }
    }

    public class UpdateDomainConstraintModel : UpdateConstraintModel
    {
        public List<DomainItem> DomainItems { get; set; }
    }

    public class UpdatePatternConstraintModel : UpdateConstraintModel
    {
        public string Pattern { get; set; }
    }

    public class UpdateRangeConstraintModel
    {
    }

    public enum ConstraintType
    {
        Domain,
        Range,
        Pattern
    }
}