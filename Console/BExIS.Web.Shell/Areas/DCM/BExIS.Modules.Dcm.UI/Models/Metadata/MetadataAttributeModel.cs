using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.IO.Transform.Validation.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.Dcm.UI.Models.Metadata
{
    public class MetadataAttributeModel
    {
        public long Id { get; set; }

        public long Number { get; set; }
        public long ParentModelNumber { get; set; }
        public long ParentStepId { get; set; }
        public long ParentPartyId { get; set; }
        public long MetadataStructureId { get; set; }
        public BaseUsage Source { get; set; }
        public BaseUsage Parent { get; set; }
        public long MetadataAttributeId { get; set; }
        public string MetadataAttributeName { get; set; }
        public String DisplayName { get; set; }
        public String Discription { get; set; }
        public int MinCardinality { get; set; }
        public int MaxCardinality { get; set; }
        public String DataType { get; set; }
        public String SystemType { get; set; }
        public string DisplayPattern { get; set; }
        public int NumberOfSourceInPackage { get; set; }
        public List<object> DomainList { get; set; }
        public List<Error> Errors { get; set; }
        public bool Locked { get; set; }

        public double LowerBoundary { get; set; }
        public double UpperBoundary { get; set; }

        public List<MetadataParameterModel> Parameters { get; set; }

        #region Mapping Variables

        public bool MappingSelectionField { get; set; }
        public bool PartyMappingExist { get; set; }

        public bool PartyComplexMappingExist { get; set; }

        public bool PartySimpleMappingExist { get; set; }

        public bool EntityMappingExist { get; set; }

        //url to the show view of the entity
        public string EntityUrl { get; set; }

        public long PartyId { get; set; }

        #endregion Mapping Variables

        public string ConstraintDescription { get; set; }

        public bool last = false;
        public bool first = false;

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

        public MetadataAttribute GetMetadataAttribute()
        {
            return this.GetUnitOfWork().GetReadOnlyRepository<MetadataAttribute>().Get(MetadataAttributeId);
        }

        public MetadataAttributeModel Copy(long number, int numberOfSourceInPackage)
        {
            var copy = new MetadataAttributeModel
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
                first = false,
                DomainList = this.DomainList,
                last = false,
                MetadataAttributeId = this.MetadataAttributeId,
                ParentStepId = this.ParentStepId,
                Errors = null,
                Locked = false,
                EntityMappingExist = this.EntityMappingExist,
                PartyMappingExist = this.PartyMappingExist,
                PartySimpleMappingExist = this.PartySimpleMappingExist,
                PartyComplexMappingExist = this.PartyComplexMappingExist,
                EntityUrl = "",
                UpperBoundary = this.UpperBoundary,
                LowerBoundary = this.LowerBoundary,
                PartyId = 0,
                MappingSelectionField = false,
                MetadataAttributeName = this.MetadataAttributeName,
                Parameters = new List<MetadataParameterModel>()
            };

            foreach (var p in this.Parameters)
            {
                copy.Parameters.Add(p.Copy(number, numberOfSourceInPackage));
            }


            return copy;
        }

        public void Update(XElement xelement)
        { 
            this.Value = xelement?.Value;

            if (xelement.HasAttributes)
            {
                foreach (var attr in xelement.Attributes())
                {
                    var parameter = Parameters.FirstOrDefault(p => p.DisplayName.ToLower().Equals(attr.Name.LocalName.ToLower()));
                    if(parameter != null)
                    {
                        parameter.Value = attr.Value;
                    }
                }
            }
        }
    }
}