using BExIS.Dlm.Entities.DataStructure;
using BExIS.IO;
using BExIS.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BExIS.Modules.Dcm.UI.Models.StructureSuggestion
{
    public class StructureSuggestionModel
    {

        public long Id { get; set; }
        public string File { get; set; }
        public int Delimeter { get; set; }
        public int Decimal { get; set; }
        public int TextMarker { get; set; }
        public List<KvP> Delimeters { get; set; }
        public List<KvP> Decimals { get; set; }
        public List<KvP> TextMarkers { get; set; }
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

        public StructureSuggestionModel()
        {
            Id = 0;
            File = "";
            Delimeter = ' ';
            Decimal = ' ';
            Preview = new List<string>();
            MissingValues = new List<MissingValueModel>();
            Delimeters = new List<KvP>();
            Decimals = new List<KvP>();
            TextMarkers = new List<KvP>();

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

        public KvP DataType { get; set; }
        public KvP Unit { get; set; }
        public KvP Template { get; set; }

        ///// <summary>
        ///// List of possible Datatypes based on the result of the strutcure Analyzer 
        ///// </summary>
        //public List<KvP> PossibleDataTypes { get; set; }

        /// <summary>
        /// List of possible units based on the result of the strutcure Analyzer 
        /// </summary>
        public List<KvP> PossibleUnits { get; set; }

        /// <summary>
        /// List of possible Templates based on the result of the strutcure Analyzer 
        /// </summary>
        public List<KvP> PossibleTemplates { get; set; }

        public VariableModel()
        {
            Id = 0;
            Name = "";
            Description = "";
            SystemType = "";
            DataType = new KvP();
            Unit = new KvP();
            Template = new KvP();

            //PossibleDataTypes = new List<KvP>();
            PossibleUnits = new List<KvP>();
            PossibleTemplates = new List<KvP>();
       }
    }
}