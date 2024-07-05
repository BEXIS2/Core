using BExIS.Dcm.Wizard;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Dcm.UI.Helpers;
using System;
using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models
{
    public class SelectVerificationModel
    {
        public StepInfo StepInfo { get; set; }
        public String[] HeaderFields { get; set; }
        //public Dictionary<int, List<EasyUploadSuggestion>> Suggestions { get; set; } //Item1 = Attribute Name, Item2 = UnitID, Item3 = DataTypeID, Item4 = Unit Name, Item5 = DataType
        //public List<UnitInfo> AvailableUnits { get; set; }
        //public List<DataAttrInfo> AvailableDataAttributes { get; set; }
        //public List<Tuple<int, string, UnitInfo>> AssignedHeaderUnits { get; set; }

        public List<RowModel> Rows { get; set; }

        public List<Error> ErrorList { get; set; }

        public SelectVerificationModel()
        {
            ErrorList = new List<Error>();
            Rows = new List<RowModel>();
        }
    }

    public class RowModel
    {
        public long Index { get; set; }
        public string Name { get; set; }
        public DataAttrInfo SelectedDataAttribute { get; set; }
        public UnitInfo SelectedUnit { get; set; }
        public DataTypeInfo SelectedDataType { get; set; }

        public List<EasyUploadSuggestion> Suggestions { get; set; }
        public List<UnitInfo> AvailableUnits { get; set; }
        public List<DataAttrInfo> AvailableDataAttributes { get; set; }
        public List<DataTypeInfo> AvailableDataTypes { get; set; }

        public RowModel()
        {
            AvailableUnits = new List<UnitInfo>();
            AvailableDataAttributes = new List<DataAttrInfo>();
        }

        public RowModel(int index, string name, DataAttrInfo selectedDataAttribute, UnitInfo selectedUnit, DataTypeInfo selectedDataType, List<EasyUploadSuggestion> suggestions, List<UnitInfo> availableUnits, List<DataAttrInfo> availableDataAttributes, List<DataTypeInfo> availableDataTypes)
        {
            Index = index;
            Name = name;
            SelectedDataAttribute = selectedDataAttribute;
            SelectedUnit = selectedUnit;
            SelectedDataType = selectedDataType;
            Suggestions = suggestions;
            AvailableUnits = availableUnits;
            AvailableDataAttributes = availableDataAttributes;
            AvailableDataTypes = availableDataTypes;
        }
    }

    public class UnitInfo : ICloneable
    {
        public long UnitId { get; set; }
        public String Description { get; set; }
        public String Name { get; set; }
        public String Abbreviation { get; set; }
        public long SelectedDataTypeId { get; set; }
        public long DimensionId { get; set; }
        public List<DataTypeInfo> DataTypeInfos { get; set; }

        public UnitInfo()
        {
            this.SelectedDataTypeId = -1;
            this.DataTypeInfos = new List<DataTypeInfo>();
        }

        public UnitInfo(long UnitId, String Description, String Name, String Abbreviation, long dimensionId)
        {
            this.UnitId = UnitId;
            this.Description = Description;
            this.Name = Name;
            this.Abbreviation = Abbreviation;
            this.DataTypeInfos = new List<DataTypeInfo>();
            this.DimensionId = dimensionId;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class DataTypeInfo
    {
        public long UnitId { get; set; }
        public long DataTypeId { get; set; }
        public String Description { get; set; }
        public String Name { get; set; }

        public DataTypeInfo()
        { }

        public DataTypeInfo(long UnitId, long DataTypeId, String Description, String Name)
        {
            this.UnitId = UnitId;
            this.DataTypeId = DataTypeId;
            this.Description = Description;
            this.Name = Name;
        }
    }

    public class DataAttrInfo
    {
        public long Id { get; set; }
        public long UnitId { get; set; }
        public long DataTypeId { get; set; }
        public long DimensionId { get; set; }
        public String Description { get; set; }
        public String Name { get; set; }

        public DataAttrInfo()
        { }

        public DataAttrInfo(long id, long UnitId, long DataTypeId, String Description, String Name, long dimensionId)
        {
            this.Id = id;
            this.UnitId = UnitId;
            this.DataTypeId = DataTypeId;
            this.Description = Description;
            this.Name = Name;
            this.DimensionId = dimensionId;
        }
    }

    public class ErrorInfo
    {
        public string VariableName { get; set; }
        public string Value { get; set; }
        public int Row { get; set; }
        public string DataType { get; set; }

        public ErrorInfo()
        { }

        public ErrorInfo(Error error)
        {
            this.VariableName = error.getName();
            this.Value = error.getValue();
            this.Row = error.getRow();
            this.DataType = error.getDataType();
        }
    }
}