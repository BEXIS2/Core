using BExIS.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Windows.Forms.VisualStyles;

namespace BExIS.Modules.Rpm.UI.Models
{
    public abstract class VariableModel
    { 
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SystemType { get; set; }
        public ListItem DataType { get; set; }
        public UnitItem Unit { get; set; }


        public List<MissingValueItem> MissingValues { get; set; }
        public List<MeaningItem> Meanings { get; set; }
        public List<ListItem> Constraints { get; set; }

        public VariableModel()
        {
            Id = 0;
            Name = "";
            Description = "";
            SystemType = "";
            DataType = new ListItem();
            Unit = new UnitItem();
            MissingValues = new List<MissingValueItem>();
            Meanings = new List<MeaningItem>();
            Constraints = new List<ListItem>();
        }
    }

    public class VariableInstanceModel : VariableModel
    {
        // this variable is part of the primary key 
        public bool IsKey { get; set; }
        public bool IsOptional { get; set; }

        public VariableTemplateItem Template { get; set; }
        public ListItem DisplayPattern { get; set; }

        public List<UnitItem> PossibleUnits { get; set; }
        public List<VariableTemplateItem> PossibleTemplates { get; set; }
        public List<ListItem> PossibleDisplayPattern { get; set; }


        public VariableInstanceModel()
        {
            Template = new VariableTemplateItem();
            DisplayPattern = new ListItem(-1, "", "", "");
            PossibleUnits = new List<UnitItem>();
            PossibleTemplates = new List<VariableTemplateItem>();
            PossibleDisplayPattern = new List<ListItem>();
        }
    }

    public class VariableTemplateModel: VariableModel
    {

        public Boolean InUse { get; set; }
        public Boolean Approved { get; set; }

        public VariableTemplateModel()
        {
            Approved = false;
            InUse = false;
        }
    }

    public class UnitItem
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public string Group { get; set; }

        public List<string> DataTypes { get; set; }

        public UnitItem()
        {
            Id = 0;
            Text = "";
            Group = "";
            DataTypes = new List<string>();
        }

        public UnitItem(long key, string value, List<string> dataTypes, string group = "")
        {
            Id = key;
            Text = value;
            Group = group;
            DataTypes = dataTypes;
        }

    }

    public class VariableTemplateItem
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public string Group { get; set; }
        public string Description { get; set; }

        public List<string> DataTypes { get; set; }
        public List<string> Units { get; set; }
        public List<string> Meanings { get; set; }
        public List<string> Constraints { get; set; }

        public VariableTemplateItem()
        {
            Id = 0;
            Text = "";
            Group = "";
            Description = "";
            DataTypes = new List<string>();
            Units = new List<string>();
            Meanings = new List<string>();
            Constraints = new List<string>();
        }

        public VariableTemplateItem(long key, string value,  List<string> units, List<string> dataTypes,List<string> meanings, List<string> constraints = null, string group = "", string description="")
        {
            Id = key;
            Text = value;
            Group = group;
            DataTypes = dataTypes;
            Units = units;
            Meanings = meanings;
            Constraints = constraints;
            Description = description;
        }

    }

    public class MissingValueItem
    {
        public long Id { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }

        public MissingValueItem()
        {
            Id = 0;
            DisplayName = "";
            Description = "";
        }

        public MissingValueItem(long _id,  string _displayName, string _description)
        {
            Id = _id;
            DisplayName = _displayName;
            Description = _description;
        }

    }

    public class MeaningItem
    {
        public long Id { get; set; }
        public string Text { get; set; }
        public string Group { get; set; }

        public List<string> Constraints { get; set; }
        public List<MeaningEntryItem> Links { get; set; }

        public MeaningItem()
        {
            Id = 0;
            Text = "";
            Group = "";
            Constraints = new List<string>();
            Links = new List<MeaningEntryItem>();

        }

        public MeaningItem(long _id, string _name, string _group = "", List<string> _constraints = null, List<MeaningEntryItem> _links = null)
        {
            Id = _id;
            Text = _name;
            Group = _group;
            Constraints = _constraints==null?new List<string>():_constraints;
            Links = _links==null?new List<MeaningEntryItem>():_links;

        }

    }

    public class MeaningEntryItem
    {
        public string label { get; set; }
        public string prefix { get; set; }
        public string releation { get; set; }
        public string link { get; set; }
    }

}