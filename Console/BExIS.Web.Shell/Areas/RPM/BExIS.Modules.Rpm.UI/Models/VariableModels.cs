﻿using BExIS.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Rpm.UI.Models
{
    public abstract class VariableModel
    { 
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SystemType { get; set; }
        public ListItem DataType { get; set; }
        public ListItem Unit { get; set; }


        public List<MissingValueItem> MissingValues { get; set; }

        public VariableModel()
        {
            Id = 0;
            Name = "";
            Description = "";
            SystemType = "";
            DataType = new ListItem();
            Unit = new ListItem();
            MissingValues = new List<MissingValueItem>();
        }
    }

    public class VariableInstanceModel : VariableModel
    {
        // this variable is part of the primary key 
        public bool IsKey { get; set; }
        public bool IsOptional { get; set; }

        public ListItem Template { get; set; }
        public ListItem DisplayPattern { get; set; }

        public List<ListItem> PossibleUnits { get; set; }
        public List<ListItem> PossibleTemplates { get; set; }
        public List<ListItem> PossibleDisplayPattern { get; set; }


        public VariableInstanceModel()
        {
            Template = new ListItem();
            DisplayPattern = new ListItem(-1, "", "");
            PossibleUnits = new List<ListItem>();
            PossibleTemplates = new List<ListItem>();
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
}