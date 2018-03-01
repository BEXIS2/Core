using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;

namespace BExIS.Modules.Rpm.UI.Models
{
    public class DataAttributeManagerModel
    {
        private DataContainerManager dataContainerManager = null;
        public DataAttributeModel DataAttributeModel = new DataAttributeModel();
        public List<DataAttributeStruct> DataAttributeStructs = new List<DataAttributeStruct>();


        public DataAttributeManagerModel()
        {
            try
            {
                dataContainerManager = new DataContainerManager();
                DataAttributeModel = new DataAttributeModel();
                dataContainerManager.DataAttributeRepo.Get().ToList().ForEach(da => DataAttributeStructs.Add(new DataAttributeStruct() { Id = da.Id, Name = da.Name, ShortName = da.ShortName, Description = da.Description, DataType = da.DataType.Name, Unit = da.Unit.Name, InUse = inUse(da), FormalDescriptions = getFormalDescriptions(da) }));
            }
            finally
            {
                dataContainerManager.Dispose();
            }
        }

        public DataAttributeManagerModel(bool showConstraints)
        {
            try
            {
                dataContainerManager = new DataContainerManager();
                DataAttributeModel = new DataAttributeModel(showConstraints);
                dataContainerManager.DataAttributeRepo.Get().ToList().ForEach(da => DataAttributeStructs.Add(new DataAttributeStruct() { Id = da.Id, Name = da.Name, ShortName = da.ShortName, Description = da.Description, DataType = da.DataType.Name, Unit = da.Unit.Name, InUse = inUse(da), FormalDescriptions = getFormalDescriptions(da) }));
            }
            finally
            {
                dataContainerManager.Dispose();
            }
        }

        public DataAttributeManagerModel(long dataAttributeId, bool showConstraints = false)
        {
            try
            {
                dataContainerManager = new DataContainerManager();
                DataAttributeModel = new DataAttributeModel(dataContainerManager.DataAttributeRepo.Get(dataAttributeId), showConstraints);
                dataContainerManager.DataAttributeRepo.Get().ToList().ForEach(da => DataAttributeStructs.Add(new DataAttributeStruct() { Id = da.Id, Name = da.Name, ShortName = da.ShortName, Description = da.Description, DataType = da.DataType.Name, Unit = da.Unit.Name, InUse = inUse(da), FormalDescriptions = getFormalDescriptions(da) }));
            }
            finally
            {
                dataContainerManager.Dispose();
            }
        }

        public DataAttributeManagerModel(DataAttributeModel dataAttributeModel)
        {
            try
            {
                dataContainerManager = new DataContainerManager();
                DataAttributeModel = dataAttributeModel;
                dataContainerManager.DataAttributeRepo.Get().ToList().ForEach(da => DataAttributeStructs.Add(new DataAttributeStruct() { Id = da.Id, Name = da.Name, ShortName = da.ShortName, Description = da.Description, DataType = da.DataType.Name, Unit = da.Unit.Name, InUse = inUse(da), FormalDescriptions = getFormalDescriptions(da) }));
            }
            finally
            {
                dataContainerManager.Dispose();
            }
        }

        private bool inUse(DataAttribute dataAttribute)
        {
            if (dataAttribute.UsagesAsVariable.Count() > 0 || dataAttribute.UsagesAsParameter.Count() > 0)
                return true;
            else
                return false;
        }

        private string getFormalDescriptions(DataAttribute dataAttribute)
        {
            string FormalDescriptions = "";
            BExIS.Dlm.Entities.DataStructure.Constraint temp = null;

            if (dataAttribute.Constraints != null && dataAttribute.Constraints.Count() > 0)
            {
                foreach (BExIS.Dlm.Entities.DataStructure.Constraint c in dataAttribute.Constraints)
                {
                    temp = c;
                    temp.Materialize();

                    if (FormalDescriptions == "")
                        FormalDescriptions = temp.FormalDescription;
                    else
                        FormalDescriptions = FormalDescriptions + "\r\n" + temp.FormalDescription;
                }
            }
            
            return FormalDescriptions;
        }

    }

    public class DataAttributeStruct
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public string DataType { get; set; }
        public string Unit { get; set; }
        public bool InUse { get; set; }
        public string FormalDescriptions { get; set; }
    }

    public class DataAttributeModel
    {
        private DataTypeManager dataTypeManager = null;
        private UnitManager unitManager = null;

        public long Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public string Description { get; set; }
        public bool InUse { get; set; }
        public bool ShowConstraints { get; set; }

        public DataTypeItemModel DataType { get; set; }
        public List<DataTypeItemModel> DataTypes = new List<DataTypeItemModel>();

        public UnitItemModel Unit { get; set; }
        public List<UnitItemModel> Units = new List<UnitItemModel>();

        public List<RangeConstraintModel> RangeConstraints { get; set; }
        public List<DomainConstraintModel> DomainConstraints { get; set; }
        public List<PatternConstraintModel> PatternConstraints { get; set; }

        public DataAttributeModel()
        {
            Id = 0;
            Name = "";
            ShortName = "";
            Description = "";
            InUse = false;
            ShowConstraints = false;

            DataType = new DataTypeItemModel();
            Unit = new UnitItemModel();

            try
            {
                dataTypeManager = new DataTypeManager();
                unitManager = new UnitManager();
                dataTypeManager.Repo.Get().OrderBy(dt => dt.Name).ToList().ForEach(dt => DataTypes.Add(new DataTypeItemModel(dt)));
                unitManager.Repo.Get().OrderBy(u => u.Name).ToList().ForEach(u => Units.Add(new UnitItemModel(u)));
            }
            finally
            {
                dataTypeManager.Dispose();
                unitManager.Dispose();
            }

            RangeConstraints = new List<RangeConstraintModel>();
            DomainConstraints = new List<DomainConstraintModel>();
            PatternConstraints = new List<PatternConstraintModel>();
        }

        public DataAttributeModel(bool showConstraints)
        {
            Id = 0;
            Name = "";
            ShortName = "";
            Description = "";
            InUse = false;
            ShowConstraints = showConstraints;

            DataType = new DataTypeItemModel();
            Unit = new UnitItemModel();

            try
            {
                dataTypeManager = new DataTypeManager();
                unitManager = new UnitManager();
                dataTypeManager.Repo.Get().OrderBy(dt => dt.Name).ToList().ForEach(dt => DataTypes.Add(new DataTypeItemModel(dt)));
                unitManager.Repo.Get().OrderBy(u => u.Name).ToList().ForEach(u => Units.Add(new UnitItemModel(u)));
            }
            finally
            {
                dataTypeManager.Dispose();
                unitManager.Dispose();
            }

            RangeConstraints = new List<RangeConstraintModel>();
            DomainConstraints = new List<DomainConstraintModel>();
            PatternConstraints = new List<PatternConstraintModel>();
        }

        public DataAttributeModel(DataAttribute dataAttribute, bool showConstraints = false)
        {
            Id = dataAttribute.Id;
            Name = dataAttribute.Name;
            ShortName = dataAttribute.ShortName;
            Description = dataAttribute.Description;
            ShowConstraints = showConstraints;

            DataType = new DataTypeItemModel(dataAttribute.DataType);
            Unit = new UnitItemModel(dataAttribute.Unit);

            try
            {
                dataTypeManager = new DataTypeManager();
                unitManager = new UnitManager();
                dataTypeManager.Repo.Get().OrderBy(dt => dt.Name).ToList().ForEach(dt => DataTypes.Add(new DataTypeItemModel(dt)));
                unitManager.Repo.Get().OrderBy(u => u.Name).ToList().ForEach(u => Units.Add(new UnitItemModel(u)));
            }
            finally
            {
                dataTypeManager.Dispose();
                unitManager.Dispose();
            }

            RangeConstraints = new List<RangeConstraintModel>();
            DomainConstraints = new List<DomainConstraintModel>();
            PatternConstraints = new List<PatternConstraintModel>();

            dataAttribute.Constraints.ToList().Where(c => c.GetType().Equals(typeof(RangeConstraint))).ToList().ForEach(rc => RangeConstraints.Add(RangeConstraintModel.Convert((RangeConstraint)rc, Id)));
            dataAttribute.Constraints.ToList().Where(c => c.GetType().Equals(typeof(PatternConstraint))).ToList().ForEach(pc => PatternConstraints.Add(PatternConstraintModel.Convert((PatternConstraint)pc, Id)));
            dataAttribute.Constraints.ToList().Where(c => c.GetType().Equals(typeof(DomainConstraint))).ToList().ForEach(dc => DomainConstraints.Add(DomainConstraintModel.Convert((DomainConstraint)dc, Id)));

            if (dataAttribute.UsagesAsVariable != null && dataAttribute.UsagesAsVariable.Count > 0)
                InUse = true;

            if (dataAttribute.Constraints.Count > 0)
            {
                foreach (Constraint c in dataAttribute.Constraints)
                {
                    if (c is RangeConstraint)
                    {
                        RangeConstraints.Add(RangeConstraintModel.Convert((RangeConstraint)c, Id));
                    }
                    if (c is PatternConstraint)
                    {
                        PatternConstraints.Add(PatternConstraintModel.Convert((PatternConstraint)c, Id));
                    }

                    if (c is DomainConstraint)
                    {
                        DomainConstraints.Add(DomainConstraintModel.Convert((DomainConstraint)c, Id));
                    }
                }
            }

        }
    }


    public class ItemModel
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public ItemModel()
        {
            Id = 0;
            Name = "";
        }
    }

    public class DataTypeItemModel : ItemModel
    {
        public DataTypeItemModel()
        {
            Id = 0;
            Name = "";
        }

        public DataTypeItemModel(DataType dataType)
        {
            Id = dataType.Id;
            Name = dataType.Name;
        }

    }

    public class UnitItemModel : ItemModel
    {
        public UnitItemModel()
        {
            Id = 0;
            Name = "";
        }

        public UnitItemModel(Unit unit)
        {
            Id = unit.Id;
            Name = unit.Name;
        }
    }
}