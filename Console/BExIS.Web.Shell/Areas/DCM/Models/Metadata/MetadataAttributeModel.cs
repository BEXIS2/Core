using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Dlm.Entities.Common;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.IO.DataType.DisplayPattern;
using BExIS.IO.Transform.Validation.Exceptions;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace BExIS.Web.Shell.Areas.DCM.Models
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

        public string ConstraintDescription { get; set; }

        public bool last = false;
        public bool first = false;

        public bool IsEmpty = true;

        private object _value;

        public object Value {
                get { return _value; }
                set
                {
                    _value = value;
                    IsEmpty = global::System.Convert.ChangeType(_value, _value.GetType()) == null || String.IsNullOrEmpty(_value.ToString());
                }
        }

        

        public static MetadataAttributeModel Convert(BaseUsage current , BaseUsage parent, long metadataStructureId, int packageModelNumber, long parentStepId)
        {
            MetadataAttribute metadataAttribute;
            List<object> domainConstraintList = new List<object>();
            string constraintsDescription="";

            if (current is MetadataNestedAttributeUsage)
            {
                MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)current;
                metadataAttribute = mnau.Member;
            }
            else
            { 
                MetadataAttributeUsage mau = (MetadataAttributeUsage)current;
                metadataAttribute = mau.MetadataAttribute;
            }

            if (metadataAttribute.Constraints.Where(c => (c is DomainConstraint)).Count() > 0)
                domainConstraintList = createDomainContraintList(metadataAttribute);

            if (metadataAttribute.Constraints.Count > 0)
            {
                foreach (Constraint c in metadataAttribute.Constraints)
                {
                    if (string.IsNullOrEmpty(constraintsDescription)) constraintsDescription = c.FormalDescription;
                    else constraintsDescription = String.Format("{0}\n{1}", constraintsDescription, c.FormalDescription);
                }
            }
            //load displayPattern
            DataTypeDisplayPattern dtdp = DataTypeDisplayPattern.Materialize(metadataAttribute.DataType.Extra);
            string displayPattern="";
            if(dtdp !=null) displayPattern = dtdp.StringPattern;

            return new MetadataAttributeModel
            {
                Id = current.Id,
                Number = 1,
                ParentModelNumber = packageModelNumber,
                MetadataStructureId = metadataStructureId,
                Parent = parent,
                Source = current,
                DisplayName = current.Label,
                Discription = current.Description,
                ConstraintDescription = constraintsDescription,
                DataType = metadataAttribute.DataType.Name,
                SystemType = metadataAttribute.DataType.SystemType,
                DisplayPattern = displayPattern,
                MinCardinality = current.MinCardinality,
                MaxCardinality = current.MaxCardinality,
                NumberOfSourceInPackage = 1,
                first = true,
                DomainList = domainConstraintList,
                last = true,
                MetadataAttributeId = metadataAttribute.Id,
                ParentStepId = parentStepId,
                Errors = null
            };
        }

        private static List<object> createDomainContraintList(MetadataAttribute attribute)
        {
            List<object> list = new List<object>();

            foreach (Constraint constraint in attribute.Constraints)
            {
                if (constraint is DomainConstraint)
                {
                    DomainConstraint domainConstraint = (DomainConstraint)constraint;
                    domainConstraint.Materialize();
                    domainConstraint.Items.ForEach(i => list.Add(i.Value));
                }
            }

            return list;
        }

        public MetadataAttribute GetMetadataAttribute()
        {
            MetadataAttributeManager mam = new MetadataAttributeManager();
            return mam.MetadataAttributeRepo.Get().Where(m => m.Id.Equals(MetadataAttributeId)).FirstOrDefault();

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
                    Errors = null
            };
        }

    }
}