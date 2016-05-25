using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dlm.Entities.DataStructure;
using DataAnnotationsExtensions;
using System.ComponentModel.DataAnnotations;
using Foolproof;

namespace BExIS.Web.Shell.Areas.RPM.Models
{
    public class ConstraintModel
    {
        public long Id { get; set; }
        public long AttributeId { get; set; }
        public string Description { get; set; }
        public bool Negated { get; set; }
        public string FormalDescription { get; set; }
    }

    public class RangeConstraintModel:ConstraintModel
    {
        public double Min { get; set; }
        //[GreaterThan("Min")]
        public double Max { get; set; }
        public bool MinInclude { get; set; }
        public bool MaxInclude { get; set; }


        public RangeConstraintModel()
        {
            MinInclude = true;
            AttributeId = 0;
            FormalDescription = (new RangeConstraint() { LowerboundIncluded = MinInclude }).FormalDescription;
        }
        public RangeConstraintModel(long attributeId)
        {
            MinInclude = true;
            AttributeId = attributeId;
            FormalDescription = (new RangeConstraint() {LowerboundIncluded = MinInclude}).FormalDescription;
        }
        public static RangeConstraintModel Convert(RangeConstraint rangeConstraint, long attributeId)
        {
            return new RangeConstraintModel(attributeId)
            {
                Id = rangeConstraint.Id,
                Negated = rangeConstraint.Negated,
                Description = rangeConstraint.Description,
                Min = rangeConstraint.Lowerbound,
                Max = rangeConstraint.Upperbound,
                MinInclude = rangeConstraint.LowerboundIncluded,
                MaxInclude = rangeConstraint.UpperboundIncluded,
                FormalDescription = rangeConstraint.FormalDescription
            };
        }
    }

    public class PatternConstraintModel:ConstraintModel
    {
        public string MatchingPhrase { get; set; }

        public PatternConstraintModel()
        {
            AttributeId = 0;
            FormalDescription = (new PatternConstraint()).FormalDescription;
        }

        public PatternConstraintModel(long attributeId)
        {
            AttributeId = attributeId;
            FormalDescription = (new PatternConstraint()).FormalDescription;
        }

        public static PatternConstraintModel Convert(PatternConstraint patternConstraint, long attributeId)
        {
            return new PatternConstraintModel(attributeId)
            {
                Id = patternConstraint.Id,
                Negated = patternConstraint.Negated,
                Description = patternConstraint.Description,
                MatchingPhrase = patternConstraint.MatchingPhrase,
                FormalDescription = patternConstraint.FormalDescription
            };
        }
    }

    public class DomainConstraintModel:ConstraintModel
    {
        public string Terms { get; set; }

        public DomainConstraintModel()
        {
            Terms= "";
            AttributeId = 0;
            List<DomainItem> ldi = new List<DomainItem>();
            ldi.Add(new DomainItem());
            FormalDescription = (new DomainConstraint() {Items = ldi}).FormalDescription;
        }

        public DomainConstraintModel(long attributeId)
        {
            Terms = "";
            AttributeId = attributeId;
            List<DomainItem> ldi = new List<DomainItem>();
            ldi.Add(new DomainItem());
            FormalDescription = (new DomainConstraint() { Items = ldi }).FormalDescription;
        }

        public static DomainConstraintModel Convert(DomainConstraint domainConstraint, long attributeId)
        {
            domainConstraint.Materialize();

            string terms = "";
            if (domainConstraint.Items != null)
            {
                foreach(DomainItem i in domainConstraint.Items)
                {
                    if (String.IsNullOrEmpty(i.Value))
                    {
                        if (terms == "")
                            terms = i.Key;
                        else
                            terms = terms + ", " + i.Key;
                    }
                    else
                    {
                        if (terms == "")
                            terms = i.Key + ", " + i.Value;
                        else
                            terms = terms + "; " + i.Key + ", " + i.Value;
                    }
                }
            }
         
            return new DomainConstraintModel(attributeId)
            {
                Id = domainConstraint.Id,
                Negated = domainConstraint.Negated,
                Description = domainConstraint.Description,
                Terms = terms,
                FormalDescription = domainConstraint.FormalDescription
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