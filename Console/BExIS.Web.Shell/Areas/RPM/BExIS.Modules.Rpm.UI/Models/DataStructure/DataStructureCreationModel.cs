using BExIS.Dlm.Entities.DataStructure;
using BExIS.IO;
using BExIS.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Rpm.UI.Models.DataStructure
{
    public class DataStructureCreationModel
    {

        /// <summary>
        ///  entity id
        /// </summary>
        public long EntityId { get; set; }
        /// <summary>
        /// title of the data structure
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// description of the data structure
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// file that come in
        /// </summary>
        public string File { get; set; }
        public int Delimeter { get; set; }
        public int Decimal { get; set; }
        public int TextMarker { get; set; }

        public List<ListItem> Delimeters { get; set; }
        public List<ListItem> Decimals { get; set; }
        public List<ListItem> TextMarkers { get; set; }
        public List<string> Preview { get; set; }

        /// <summary>
        /// total numbe of detect rows
        /// </summary>
        public long Total { get; set; }

        /// <summary>
        /// skipped rows
        /// </summary>
        public int Skipped { get; set; }
        public List<Marker> Markers { get; set; }

        public List<VariableModel> Variables { get; set; }

        public List<MissingValueModel> MissingValues { get; set; }

        public DataStructureCreationModel()
        {
            EntityId = 0;
            File = "";
            Title = "";
            Description = "";
            Delimeter = ' ';
            Decimal = ' ';
            Preview = new List<string>();
            MissingValues = new List<MissingValueModel>();
            Delimeters = new List<ListItem>();
            Decimals = new List<ListItem>();
            TextMarkers = new List<ListItem>();
            Markers = new List<Marker>();
            Variables = new List<VariableModel>();
        }
    }

    public class MissingValueModel
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }

        public MissingValueModel()
        {
            DisplayName = "";
            Description = "";
        }
    }

    public class Marker
    {
        public string Type { get; set; }
        public int Row { get; set; }
        public List<bool> Cells { get; set; }

        public Marker()
        {
            Type = "";
            Row = 0;
            Cells = new List<bool>();
        }
    }

    public class VariableModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SystemType { get; set; }

        // this variable is part of the primary key 
        public bool IsKey { get; set; }
        public bool IsOptional { get; set; }

        public ListItem DataType { get; set; }
        public ListItem Unit { get; set; }
        public ListItem Template { get; set; }
        public ListItem DisplayPattern { get; set; }

        ///// <summary>
        ///// List of possible Datatypes based on the result of the strutcure Analyzer 
        ///// </summary>
        //public List<KvP> PossibleDataTypes { get; set; }

        /// <summary>
        /// List of possible units based on the result of the strutcure Analyzer 
        /// </summary>
        public List<ListItem> PossibleUnits { get; set; }

        /// <summary>
        /// List of possible Templates based on the result of the strutcure Analyzer 
        /// </summary>
        public List<ListItem> PossibleTemplates { get; set; }

        /// <summary>
        /// List of possible Templates based on the result of the strutcure Analyzer 
        /// </summary>
        public List<ListItem> PossibleDisplayPattern { get; set; }

        public VariableModel()
        {
            Id = 0;
            Name = "";
            Description = "";
            SystemType = "";
            DataType = new ListItem();
            Unit = new ListItem();
            Template = new ListItem();
            DisplayPattern = new ListItem(-1,"","");

            IsKey = false;
            IsOptional = false;

            //PossibleDataTypes = new List<KvP>();
            PossibleUnits = new List<ListItem>();
            PossibleTemplates = new List<ListItem>();
            PossibleDisplayPattern = new List<ListItem>();
       }
    }
}