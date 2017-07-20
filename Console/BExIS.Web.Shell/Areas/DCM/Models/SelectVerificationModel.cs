using BExIS.Dcm.Wizard;
using BExIS.IO.Transform.Validation.Exceptions;
using System;
using System.Collections.Generic;
using BExIS.Modules.Dcm.UI.Helpers;

namespace BExIS.Modules.Dcm.UI.Models
{
    public class SelectVerificationModel
    {
        public StepInfo StepInfo { get; set; }
        public String[] HeaderFields { get; set; }
        public Dictionary<int, List<EasyUploadSuggestion>> Suggestions { get; set; } //Item1 = Attribute Name, Item2 = UnitID, Item3 = DataTypeID, Item4 = Unit Name, Item5 = DataType
        public List<UnitInfo> AvailableUnits { get; set; }
        public List<Tuple<int, string, UnitInfo>> AssignedHeaderUnits { get; set; }

        public List<Error> ErrorList { get; set; }

        public SelectVerificationModel()
        {
            ErrorList = new List<Error>();
            AvailableUnits = new List<UnitInfo>();
            AssignedHeaderUnits = new List<Tuple<int, string, UnitInfo>>();
        }
    }

    public class UnitInfo : ICloneable
    {
        public long UnitId { get; set; }
        public String Description { get; set; }
        public String Name { get; set; }
        public String Abbreviation { get; set; }
        public long SelectedDataTypeId { get; set; }
        public List<DataTypeInfo> DataTypeInfos { get; set; }

        public UnitInfo()
        {
            this.SelectedDataTypeId = -1;
            this.DataTypeInfos = new List<DataTypeInfo>();
        }

        public UnitInfo(long UnitId, String Description, String Name, String Abbreviation)
        {
            this.UnitId = UnitId;
            this.Description = Description;
            this.Name = Name;
            this.Abbreviation = Abbreviation;
            this.DataTypeInfos = new List<DataTypeInfo>();
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

        public DataTypeInfo() { }

        public DataTypeInfo(long UnitId, long DataTypeId, String Description, String Name)
        {
            this.UnitId = UnitId;
            this.DataTypeId = DataTypeId;
            this.Description = Description;
            this.Name = Name;
        }
    }

    public class ErrorInfo
    {
        public string VariableName { get; set; }
        public string Value { get; set; }
        public int Row { get; set; }
        public string DataType { get; set; }

        public ErrorInfo() { }

        public ErrorInfo(Error error)
        {
            this.VariableName = error.getName();
            this.Value = error.getValue();
            this.Row = error.getRow();
            this.DataType = error.getDataType();
        }
    }
}