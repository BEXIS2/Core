using BExIS.IO.Transform.Input;
using BExIS.UI.Models;
using System.Collections.Generic;

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
        public int FileEncoding { get; set; }

        public List<ListItem> Delimeters { get; set; }
        public List<ListItem> Decimals { get; set; }
        public List<ListItem> TextMarkers { get; set; }
        public List<ListItem> Encodings { get; set; }

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

        public List<VariableInstanceModel> Variables { get; set; }

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
            Variables = new List<VariableInstanceModel>();
            FileEncoding = (int)EncodingType.UTF8;
        }
    }

    public class DataStructureEditModel
    {
        public long Id { get; set; }

        /// <summary>
        /// title of the data structure
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// description of the data structure
        /// </summary>
        public string Description { get; set; }

        public List<string> Preview { get; set; }

        public List<VariableInstanceModel> Variables { get; set; }

        public List<MissingValueModel> MissingValues { get; set; }

        public DataStructureEditModel()
        {
            Id = 0;
            Title = "";
            Description = "";
            Preview = new List<string>();
            MissingValues = new List<MissingValueModel>();
            Variables = new List<VariableInstanceModel>();
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
}