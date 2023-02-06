using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.MetadataStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.Dcm.UI.Models.Metadata
{
    public class MetadataParameterModel
    {
        public long Id { get; set; }
        public long Number { get; set; }
        public long ParentModelNumber { get; set; }
        public long ParentStepId { get; set; }
        public long ParentPartyId { get; set; }
        public long MetadataStructureId { get; set; }
        public BaseUsage Source { get; set; }
        public BaseUsage Parent { get; set; }
        public long MetadataParameterId { get; set; }
        public string MetadataParameterName { get; set; }
        public String DisplayName { get; set; }
        public String Discription { get; set; }
        public int MinCardinality { get; set; }
        public int MaxCardinality { get; set; }
        public String DataType { get; set; }
        public String SystemType { get; set; }
        public string DisplayPattern { get; set; }
        public int NumberOfSourceInPackage { get; set; }
        public List<object> DomainList { get; set; }

        public double LowerBoundary { get; set; }
        public double UpperBoundary { get; set; }

        public string ConstraintDescription { get; set; }

        public bool IsEmpty = true;

        private object _value;

        public object Value
        {
            get { return _value; }
            set
            {
                _value = value;
                IsEmpty = global::System.Convert.ChangeType(_value, _value.GetType()) == null || String.IsNullOrEmpty(_value.ToString());
            }
        }

        public MetadataParameter GetMetadataParameter()
        {
            return this.GetUnitOfWork().GetReadOnlyRepository<MetadataParameter>().Get(MetadataParameterId);
        }

        public MetadataParameterModel Copy(long number, int numberOfSourceInPackage)
        {
            return new MetadataParameterModel
            {
                Id = this.Id,
                Number = number,
                ParentModelNumber = this.ParentModelNumber,
                MetadataStructureId = this.MetadataStructureId,
                Parent = this.Parent,
                ParentPartyId = 0,
                Source = this.Source,
                DisplayName = this.DisplayName,
                Discription = this.Discription,
                ConstraintDescription = this.ConstraintDescription,
                DataType = this.DataType,
                SystemType = this.SystemType,
                MinCardinality = this.MinCardinality,
                MaxCardinality = this.MaxCardinality,
                NumberOfSourceInPackage = numberOfSourceInPackage,
                DomainList = this.DomainList,
                ParentStepId = this.ParentStepId,
                UpperBoundary = this.UpperBoundary,
                LowerBoundary = this.LowerBoundary,

            };
        }
    
    }
}