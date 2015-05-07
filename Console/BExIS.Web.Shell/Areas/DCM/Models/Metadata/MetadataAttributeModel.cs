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

namespace BExIS.Web.Shell.Areas.DCM.Models
{
    public class MetadataAttributeModel
    {
        public long Id { get; set; }
        public long Number { get; set; }
        public long ParentModelNumber { get; set; }
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
        public int NumberOfSourceInPackage { get; set; }
        public List<object> DomainList { get; set; }

        public string ConstraintDescription { get; set; }

        public bool last = false;
        public bool first = false;

        public object Value { get; set; }

        public static MetadataAttributeModel Convert(BaseUsage current , BaseUsage parent, long metadataStructureId, int packageModelNumber)
        {
            MetadataAttribute metadataAttribute;
            List<object> domainConstraintList = new List<object>();
            string constraintsDescription="";

            if (current is MetadataNestedAttributeUsage)
            {
                MetadataNestedAttributeUsage mnau = (MetadataNestedAttributeUsage)current;
                metadataAttribute = mnau.Member;

                if (metadataAttribute.Constraints.Where(c => (c is DomainConstraint)).Count() > 0)
                    domainConstraintList = createDomainContraintList(metadataAttribute);

                if (metadataAttribute.Constraints.Count > 0)
                { 
                    foreach(Constraint c in  metadataAttribute.Constraints)
                    {
                        constraintsDescription = Path.Combine("{0} , {1}", constraintsDescription, c.FormalDescription);
                    }
                }

            }
            else
            { 
                MetadataAttributeUsage mau = (MetadataAttributeUsage)current;
                metadataAttribute = mau.MetadataAttribute;
            }

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
                MinCardinality = current.MinCardinality,
                MaxCardinality = current.MaxCardinality,
                NumberOfSourceInPackage = 1,
                first = true,
                DomainList = domainConstraintList,
                last = true,
                MetadataAttributeId = metadataAttribute.Id
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

    }
}