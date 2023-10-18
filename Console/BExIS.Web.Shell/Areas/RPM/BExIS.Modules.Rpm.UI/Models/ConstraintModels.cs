using BExIS.Dlm.Entities.DataStructure;
using BExIS.UI.Models;
using System;
using System.Collections.Generic;

namespace BExIS.Modules.Rpm.UI.Models
{
    public class ConstraitListItem
    {
        public ConstraitListItem()
        {
            Id = 0;
            Name = string.Empty;
            Description = string.Empty;
            Specification = string.Empty;
            InUse = false;
        }

        /// <summary>
        /// Description of the Dimension
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Name of the Dimension
        /// </summary>
        public long Id { get; set; }

        public bool InUse { get; set; }

        /// <summary>
        /// Name of the Dimension
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Specification of the Dimension
        /// </summary>
        public string Specification { get; set; }
    }

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
        public string Description { get; set; }
        public long Id { get; set; }

        //public DateTime LastModificationDate { get; set; }
        public string Name { get; set; }

        public int Version { get; set; }

        public static ReadConstraintModel Convert(Constraint constraint)
        {
            return new ReadConstraintModel()
            {
                Id = constraint.Id,
                Name = constraint.Name,
                Description = constraint.Description,
                Version = constraint.VersionNo
            };
        }
    }

    public class ReadDomainConstraintModel : ReadConstraintModel
    {
        public List<DomainItem> DomainItems { get; set; }

        public static ReadDomainConstraintModel Convert(DomainConstraint constraint)
        {
            return new ReadDomainConstraintModel()
            {
                Id = constraint.Id,
                Name = constraint.Name,
                Description = constraint.Description,
                Version = constraint.VersionNo,
                DomainItems = constraint.Items
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
}