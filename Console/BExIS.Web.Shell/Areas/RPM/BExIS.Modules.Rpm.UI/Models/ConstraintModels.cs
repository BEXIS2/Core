using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Meanings;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.Meanings;
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
        public bool InUseByVariable { get; set; }
        public bool InUseByMeaning { get; set; }
        public List<long> VariableIDs { get; set; }
        public string CreationDate { get; set; }
        public string LastModified { get; set; }

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
                InUseByVariable = inUseChecker.isConstrainInUseByVariable(constraint),
                InUseByMeaning = inUseChecker.isConstrainInUseByMeaning(constraint),
                VariableIDs = constraint.VariableConstraints.Select(v => v.Id).ToList(),
                CreationDate = constraint.CreationDate != null ? constraint.CreationDate.ToString("MMMM d, HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) : "",
                LastModified = constraint.LastModified != null ? constraint.LastModified.ToString("MMMM d, HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture) : "",
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
        public string Provider { get; set; }
        public ConstraintSelectionPredicate SelectionPredicate { get; set; }

        public static ReadDomainConstraintModel Convert(DomainConstraint constraint)
        {
            ConstraintSelectionPredicate selectionPredicate = new ConstraintSelectionPredicate();

            return new ReadDomainConstraintModel()
            {
                Id = constraint.Id,
                Version = constraint.VersionNo,
                Name = string.IsNullOrEmpty(constraint.Name) ? "Constraint No. " + constraint.Id : constraint.Name,
                Description = string.IsNullOrEmpty(constraint.Description) ? "Constraint " + constraint.Id : constraint.Description,
                FormalDescription = constraint.FormalDescription,
                Negated = constraint.Negated,
                InUseByVariable = inUseChecker.isConstrainInUseByVariable(constraint),
                InUseByMeaning = inUseChecker.isConstrainInUseByMeaning(constraint),
                VariableIDs = constraint.VariableConstraints.Select(v => v.Id).ToList(),
                Domain = DomainConverter.convertDomainItemsToDomain(constraint.Items),
                Provider = constraint.Provider != null ? constraint.Provider.ToString() : null,
                SelectionPredicate = constraint.ConstraintSelectionPredicate != null ? selectionPredicate.Materialise(constraint.ConstraintSelectionPredicate) : null,
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
                InUseByVariable = inUseChecker.isConstrainInUseByVariable(constraint),
                InUseByMeaning = inUseChecker.isConstrainInUseByMeaning(constraint),
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
                InUseByVariable = inUseChecker.isConstrainInUseByVariable(constraint),
                InUseByMeaning = inUseChecker.isConstrainInUseByMeaning(constraint),
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
        public string Provider { get; set; }
        public ConstraintSelectionPredicate SelectionPredicate { get; set; }
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

    
    public static class inUseChecker
    {
        private static List<StructuredDataStructure> structuredDataStructures = null;

        public static void reset()
        {
            structuredDataStructures = null;
        }
        public static bool isConstrainInUseByVariable(Constraint constraint)
        {
            bool inUse = false;
            if(constraint != null) 
            {
                List<long> variableIds = constraint.VariableConstraints.Select(v => v.Id).ToList();
                using (DataStructureManager dataStructureManager = new DataStructureManager())
                {
                    if(structuredDataStructures == null)
                        structuredDataStructures = dataStructureManager.StructuredDataStructureRepo.Get().ToList();

                    //List<StructuredDataStructure> structuredDataStructures =
                    inUse = structuredDataStructures.Where(s => s.Variables.Any(v => variableIds.Contains(v.Id))).Any(s => s.Datasets.Count != 0);

                    //if (structuredDataStructures != null && structuredDataStructures.Count > 0)
                    //{
                    //    foreach (StructuredDataStructure structuredDataStructure in structuredDataStructures)
                    //    {
                    //        inUse = structuredDataStructure.Datasets.Any();
                    //        if (inUse)
                    //            break;
                    //    }
                    //}
                }
            }                    
            return inUse;
        }

        public static bool isConstrainInUseByMeaning(Constraint constraint)
        {
            bool inUse = false;
            using (MeaningManager meaningManager = new MeaningManager())
            {
                if (constraint != null)
                {
                    inUse = meaningManager.GetMeanings().Where(m => m.Constraints.Any(c => c.Id.Equals(constraint.Id))).Any();
                }
            }
            return inUse;
        }
    }

    public static class DomainConverter
    {
        public static List<DomainItem> convertDomainToDomainItems(string domain)
        {
            List<DomainItem> domainItems = new List<DomainItem>();
            List<string> rows = domain.Split(Environment.NewLine.ToCharArray()).ToList();
            DomainItem domainItem = new DomainItem();

            foreach (string row in rows)
            {
                if (!String.IsNullOrEmpty(row))
                {
                    domainItem = new DomainItem()
                    {
                        Key = row.Trim(),
                        Value = row.Trim()
                    };
                    if (domainItems.Where(di => di.Key.Equals(domainItem.Key)).ToList().Count == 0)
                        domainItems.Add(domainItem);
                }
            }
            return domainItems.Distinct().ToList();
        }

        public static List<DomainItem> convertDomainToDomainItemsKVP(string domain)
        {
            List<DomainItem> domainItems = new List<DomainItem>();
            List<string> rows = domain.Split(Environment.NewLine.ToCharArray()).ToList();
            DomainItem domainItem = new DomainItem();

            foreach (string row in rows)
            {
                List<string> columns = row.Split(',').ToList();
                if (columns.Count >= 1 && !String.IsNullOrEmpty(columns[0]))
                {
                    if (columns.Count == 1)
                    {
                        domainItem = new DomainItem()
                        {
                            Key = columns[0].Trim(),
                            Value = columns[0].Trim()
                        };
                    }
                    else if (columns.Count == 2)
                    {
                        domainItem = new DomainItem()
                        {
                            Key = columns[0].Trim(),
                            Value = columns[1].Trim()
                        };
                    }

                    if (domainItems.Where(di => di.Key.Equals(domainItem.Key)).ToList().Count == 0)
                        domainItems.Add(domainItem);
                }
            }
            return domainItems.Distinct().ToList();
        }

        public static string convertDomainItemsToDomain(List<DomainItem> domainItems)
        {
            string domain = "";
            foreach (DomainItem domainItem in domainItems)
            {
                if (domainItem.Key == domainItem.Value)
                {
                    if (domainItem != domainItems.Last())
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

    public class Info
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Info()
        {
            Id = 0;
            Name = string.Empty;
            Description = string.Empty;
        }
    }

    public class DatasetInfo : Info
    {
        public long DatasetVersionId { get; set; }
        public long DatasetVersionNumber { get; set; }
        public long DatastructureId { get; set; }

        public DatasetInfo()
        {
            DatasetVersionId = 0;
            DatasetVersionNumber = 0;
            DatastructureId = 0;
        }
    }

    public class DatastructureInfo : Info
    {
        public List<ColumnInfo> ColumnInfos = new List<ColumnInfo>();

        public DatastructureInfo()
        {
            ColumnInfos = new List<ColumnInfo>();
        }
    }

    public class ColumnInfo : Info
    {
        public long VariableId { get; set; }
        public string Unit { get; set; }
        public string DataType { get; set; }
        public int OrderNo { get; set; }

        public ColumnInfo()
        {
            VariableId = 0;
            Unit = string.Empty;
            DataType = string.Empty;
            OrderNo = 0;
        }
    }
}