using BExIS.Dlm.Entities.DataStructure;
using BExIS.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BExIS.Modules.Rpm.UI.Models
{
    public class ReadConstraintModel
    {
        public long Id { get; set; }
        public int Version { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FormalDescription { get; set; }
        public string Type { get; set; }
        public bool Negated { get; set; }
        public bool InUse { get; set; }
        public List<long> VariableIDs { get; set; }
        public string CreationDate { get; set; }
        public string LastModified {get; set;}

        public static ReadConstraintModel Convert(Constraint constraint, string type = "")
        {
            return new ReadConstraintModel()
            {
                Id = constraint.Id,
                Version = constraint.VersionNo,
                Name = string.IsNullOrEmpty(constraint.Name) ? "Constraint No. " + constraint.Id : constraint.Name,
                Description = string.IsNullOrEmpty(constraint.Description) ? "Constraint " + constraint.Id : constraint.Description,
                FormalDescription = constraint.FormalDescription,
                Type = type,
                Negated = constraint.Negated,
                InUse = constraint.DataContainer != null && constraint.DataContainer.Id > 0 || constraint.VariableConstraints.Any(),
                VariableIDs = constraint.VariableConstraints.Select(v => v.Id).ToList(),
                CreationDate = constraint.CreationDate.ToString("f"),
                LastModified = constraint.LastModified.ToString("f")

            };
        }

        public static ReadConstraintModel Convert(DomainConstraint constraint)
        {
            return Convert(constraint, ConstraintType.Domain.ToString());
        }

        public static ReadConstraintModel Convert(PatternConstraint constraint)
        {
            return Convert(constraint, ConstraintType.Pattern.ToString());
        }

        public static ReadConstraintModel Convert(RangeConstraint constraint)
        {
            return Convert(constraint, ConstraintType.Range.ToString());
        }
    }

    public class ReadDomainConstraintModel : ReadConstraintModel
    {
        public string Domain { get; set; }

        public static ReadDomainConstraintModel Convert(DomainConstraint constraint)
        {
            return new ReadDomainConstraintModel()
            {
                Id = constraint.Id,
                Version = constraint.VersionNo,
                Name = string.IsNullOrEmpty(constraint.Name) ? "Constraint No. " + constraint.Id : constraint.Name,
                Description = string.IsNullOrEmpty(constraint.Description) ? "Constraint " + constraint.Id : constraint.Description,
                FormalDescription = constraint.FormalDescription,
                Negated = constraint.Negated,
                InUse = constraint.DataContainer != null && constraint.DataContainer.Id > 0 || constraint.VariableConstraints.Any(),
                VariableIDs = constraint.VariableConstraints.Select(v => v.Id).ToList(),
                Domain = DomainConverter.convertDomainItemsToDomain(constraint.Items)
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
                Version = constraint.VersionNo,
                Name = string.IsNullOrEmpty(constraint.Name) ? "Constraint No. " + constraint.Id : constraint.Name,
                Description = string.IsNullOrEmpty(constraint.Description) ? "Constraint " + constraint.Id : constraint.Description,
                FormalDescription = constraint.FormalDescription,
                Negated = constraint.Negated,
                InUse = constraint.DataContainer != null && constraint.DataContainer.Id > 0 || constraint.VariableConstraints.Any(),
                VariableIDs = constraint.VariableConstraints.Select(v => v.Id).ToList(),
                Pattern = constraint.MatchingPhrase
            };
        }
    }

    public class ReadRangeConstraintModel : ReadConstraintModel
    {

        public double Lowerbound { get; set; }
        public double Upperbound { get; set; }
        public bool LowerboundIncluded { get; set; }
        public bool UpperboundIncluded { get; set; }

        public static ReadRangeConstraintModel Convert(RangeConstraint constraint)
        {
            return new ReadRangeConstraintModel()
            {
                Id = constraint.Id,
                Version = constraint.VersionNo,
                Name = string.IsNullOrEmpty(constraint.Name) ? "Constraint No. " + constraint.Id : constraint.Name,
                Description = string.IsNullOrEmpty(constraint.Description) ? "Constraint " + constraint.Id : constraint.Description,
                FormalDescription = constraint.FormalDescription,
                Negated = constraint.Negated,
                InUse = constraint.DataContainer != null && constraint.DataContainer.Id > 0 || constraint.VariableConstraints.Any(),
                VariableIDs = constraint.VariableConstraints.Select(v => v.Id).ToList(),
                Lowerbound = constraint.Lowerbound,
                Upperbound = constraint.Upperbound,
                LowerboundIncluded = constraint.LowerboundIncluded,
                UpperboundIncluded = constraint.UpperboundIncluded
            };
        }
    }

    public enum ConstraintType
    {
        Domain,
        Range,
        Pattern
    }

    public class EditConstraintModel
    {
        public long Id { get; set; }
        public int Version { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Negated { get; set; }
        public bool InUse { get; set; }
    }
    public class EditDomainConstraintModel : EditConstraintModel
    {
        public string Domain { get; set; }
    }

    public class EditRangeConstraintModel : EditConstraintModel
    {
        public double Lowerbound { get; set; }
        public double Upperbound { get; set; }
        public bool LowerboundIncluded { get; set; }
        public bool UpperboundIncluded { get; set; }
    }

    public class EditPatternConstraintModel : EditConstraintModel
    {
        public string pattern { get; set; }
    }

    public static class DomainConverter
    {
        public static List<DomainItem> convertDomainToDomainItems(string domain)
        {
            List<DomainItem> domainItems = new List<DomainItem>();
            List<string> rows = domain.Split(Environment.NewLine.ToCharArray()).ToList();

            foreach (string row in rows)
            {
                domainItems.Add(new DomainItem()
                {
                    Key = row.Trim(),
                    Value = row.Trim()
                });
            }
            return domainItems;
        }

        public static List<DomainItem> convertDomainToDomainItemsKVP(string domain)
        {
            List<DomainItem> domainItems = new List<DomainItem>();
            List<string> rows = domain.Split(Environment.NewLine.ToCharArray()).ToList();

            foreach (string row in rows)
            {
                List<string> columns = row.Split(',').ToList();
                if (columns.Count == 1)
                {
                    domainItems.Add(new DomainItem()
                    {
                        Key = columns[0].Trim(),
                        Value = columns[0].Trim()
                    });
                }
                else if (columns.Count == 2)
                {
                    domainItems.Add(new DomainItem()
                    {
                        Key = columns[0].Trim(),
                        Value = columns[1].Trim()
                    });
                }
            }
            return domainItems;
        }

        public static string convertDomainItemsToDomain(List<DomainItem> domainItems)
        {
            string domain = "";
            foreach (DomainItem domainItem in domainItems)
            {
            
                if (domainItem.Key == domainItem.Value)
                {
                    if(domainItem != domainItems.Last())
                        domain += domainItem.Value + "\n";
                    else 
                        domain += domainItem.Value;
                }
                else
                {
                    if (domainItem.Key != domainItem.Value)
                        domain += domainItem.Key + "," + domainItem.Value + "\n";
                    else
                        domain += domainItem.Key + "," + domainItem.Value;
                }
            }
            return domain;
        }
    }
}