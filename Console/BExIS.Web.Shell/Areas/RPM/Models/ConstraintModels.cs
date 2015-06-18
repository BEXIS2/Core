using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dlm.Entities.DataStructure;

namespace BExIS.Web.Shell.Areas.RPM.Models
{
    public class ConstraintModel
    {
        public long Id { get; set; }
        public string Description { get; set; }
        public bool Negated { get; set; }

    }

    public class RangeConstraintModel:ConstraintModel
    {
        public double Min { get; set; }
        public double Max { get; set; }

        public static RangeConstraintModel Convert(RangeConstraint rangeConstraint)
        {
            return new RangeConstraintModel()
            {
                Id = rangeConstraint.Id,
                Description = rangeConstraint.Description,
                Min = rangeConstraint.Lowerbound,
                Max = rangeConstraint.Upperbound
            };
        }
    }

    public class PatternConstraintModel:ConstraintModel
    {
        public string MatchingPhrase { get; set; }

        public static PatternConstraintModel Convert(PatternConstraint patternConstraint)
        {
            return new PatternConstraintModel()
            {
                Id = patternConstraint.Id,
                Description = patternConstraint.Description,
                MatchingPhrase = patternConstraint.MatchingPhrase
            };
        }
    }

    public class DomainConstraintModel:ConstraintModel
    {
        public  List<DomainConstraintItemModel> DomainItems { get; set; }

        public DomainConstraintModel()
        {
            DomainItems = new List<DomainConstraintItemModel>();
        }

        public static DomainConstraintModel Convert(DomainConstraint domainConstraint)
        {
            domainConstraint.Materialize();

            List<DomainConstraintItemModel> domainItemList = new List<DomainConstraintItemModel>();
            if (domainConstraint.Items!=null)
                domainConstraint.Items.ForEach(d => domainItemList.Add(new DomainConstraintItemModel(d.Key, d.Value)));
         
            return new DomainConstraintModel()
            {
                Id = domainConstraint.Id,
                Description = domainConstraint.Description,
                DomainItems = domainItemList
            };
        }

    }

    public class DomainConstraintItemModel
    {
        public string Key { get; set; }
        public string Value { get; set; }

        public DomainConstraintItemModel()
        {
            Key = "";
            Value = "";
        }

        public DomainConstraintItemModel(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}