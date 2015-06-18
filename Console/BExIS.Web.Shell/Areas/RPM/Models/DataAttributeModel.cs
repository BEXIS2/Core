using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dlm.Entities.DataStructure;

namespace BExIS.Web.Shell.Areas.RPM.Models
{
    public class DataAttributeModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public bool InUse { get; set; }

        public DataTypeItemModel DataType { get; set; }
        public List<DataTypeItemModel> DataTypes { get; set; }

        public UnitItemModel Unit { get; set; }
        public List<UnitItemModel> Units { get; set; }

        public List<RangeConstraintModel> RangeConstraints { get; set; }
        public List<DomainConstraintModel> DomainConstraints { get; set; }
        public List<PatternConstraintModel> PatternConstraints { get; set; }
        public List<DomainConstraintItemModel> DomainItems { get; set; }


        public DataAttributeModel()
        {
            Id = 0;
            Name = "";
            ShortName = "";
            Description = "";
            InUse = false;

            DataType = new DataTypeItemModel();
            Unit = new UnitItemModel();

            DataTypes = new List<DataTypeItemModel>();
            Units = new List<UnitItemModel>();

            RangeConstraints = new List<RangeConstraintModel>();
            DomainConstraints = new List<DomainConstraintModel>();
            PatternConstraints = new List<PatternConstraintModel>();
            DomainItems = new List<DomainConstraintItemModel>();
            
        }

        public DataAttributeModel(DataAttribute dataAttribute)
        {
            Id = dataAttribute.Id;
            Name = dataAttribute.Name;
            Description = dataAttribute.Description;

            DataType = new DataTypeItemModel(dataAttribute.DataType);
            Unit = new UnitItemModel(dataAttribute.Unit);


            RangeConstraints = new List<RangeConstraintModel>();
            DomainConstraints = new List<DomainConstraintModel>();
            PatternConstraints = new List<PatternConstraintModel>();
            DomainItems = new List<DomainConstraintItemModel>();

            if (dataAttribute.UsagesAsVariable != null && dataAttribute.UsagesAsVariable.Count > 0)
                InUse = true;

            if (dataAttribute.Constraints.Count > 0)
            {
                foreach (Constraint c in dataAttribute.Constraints)
                {
                    if (c is RangeConstraint)
                    { 
                        RangeConstraints.Add(RangeConstraintModel.Convert((RangeConstraint)c));
                    }
                    if (c is PatternConstraint)
                    { 
                        PatternConstraints.Add(PatternConstraintModel.Convert((PatternConstraint)c));
                    }

                    if (c is DomainConstraint)
                    {
                        DomainConstraints.Add(DomainConstraintModel.Convert((DomainConstraint)c));
                    }
                }
            }

        }
    }

    
    public class ItemModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class DataTypeItemModel:ItemModel
    {
        public DataTypeItemModel()
        { 
        
        }

        public DataTypeItemModel(DataType dataType)
        {
            Id = dataType.Id;
            Name = dataType.Name;
        }

    }

    public class UnitItemModel:ItemModel
    {
        public UnitItemModel()
        { 
        
        }

        public UnitItemModel(Unit unit)
        {
            Id = unit.Id;
            Name = unit.Name;
        }
    }
}