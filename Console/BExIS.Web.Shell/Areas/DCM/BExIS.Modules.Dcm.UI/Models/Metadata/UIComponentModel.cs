using System;
using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models.Metadata
{
    public enum MetadataAttrType
    {
        Attribute = 0,
        Parameter = 1
    }

    public class UIComponentModel
    {
        public MetadataAttrType Type { get; set; }
        public string IdInput { get; set; }
        public string IdByXpath { get; set; }

        public long ParentId { get; set; }

        public string DisplayName { get; set; }
        public string DisplayPattern { get; set; }
        public string Discription { get; set; }

        public object Value { get; set; }

        public bool Locked { get; set; }

        public List<object> DomainList { get; set; }

        public string SystemType { get; set; }

        public string ErrorMessage { get; set; }
        public string ErrorClass { get; set; }
        public string LockedClass { get; set; }

        public string OnChangeAction { get; set; }

        public UIComponentModel(MetadataAttrType type)
        {
            Type = type;
            IdInput = String.Empty;
            IdByXpath = String.Empty;
            ParentId = 0;
            DisplayName = String.Empty;
            DisplayPattern = String.Empty;
            Value = null;
            DomainList = new List<object>();
            SystemType = String.Empty;
            ErrorMessage = String.Empty;
            LockedClass = String.Empty;
            OnChangeAction = String.Empty;
        }
    }
}