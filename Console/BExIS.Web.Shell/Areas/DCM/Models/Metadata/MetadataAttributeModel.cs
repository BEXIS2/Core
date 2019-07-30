using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.IO.Transform.Validation.Exceptions;
using System;
using System.Collections.Generic;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.Dcm.UI.Models.Metadata
{
    public class MetadataAttributeModel
    {
        public long Id { get; set; }
        public long Number { get; set; }
        public long ParentModelNumber { get; set; }
        public long ParentStepId { get; set; }
        public long MetadataStructureId { get; set; }
        public BaseUsage Source { get; set; }
        public BaseUsage Parent { get; set; }
        public long MetadataAttributeId { get; set; }
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

        #region Mapping Variables

        public bool PartyMappingExist { get; set; }
        public bool EntityMappingExist { get; set; }

        //url to the show view of the entity
        public string EntityUrl { get; set; }

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

        public MetadataAttributeModel Kopie(long number, int numberOfSourceInPackage)
        {
            return new MetadataAttributeModel
            {
                Id = this.Id,
                Number = number,
                ParentModelNumber = this.ParentModelNumber,
                MetadataStructureId = this.MetadataStructureId,
                Parent = this.Parent,
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
                PartyMappingExist = this.PartyMappingExist,
                EntityMappingExist = this.EntityMappingExist,
                EntityUrl = ""
            };
        }
    }
}