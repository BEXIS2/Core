using BExIS.IO;
using System;
using System.Collections.Generic;

namespace BExIS.Modules.Dcm.UI.Models
{
    public class FileInfoModel
    {
        public string Extention { get; set; }

        /// <summary>
        /// representation of decimal
        /// </summary>
        public DecimalCharacter Decimal { get; set; }

        /// <summary>
        ///
        /// </summary>
        public Orientation Orientation { get; set; }

        /// <summary>
        /// Number of empty columns (columnwise) or rows (rowwise) before the variables are specified.
        /// </summary>
        public int Offset { get; set; }

        /// <summary>
        /// Row/Column in which the variables are.
        /// </summary>
        public int Variables { get; set; }

        /// <summary>
        /// Row/Column in which the Data are.
        /// </summary>
        public int Data { get; set; }

        /// <summary>
        /// Format of the Date
        /// </summary>
        public string Dateformat { get; set; }

        public TextSeperator Separator { get; set; }

        public TextMarker TextMarker { get; set; }

        //select area for excel
        public List<string> DataArea { get; set; }

        public string HeaderArea { get; set; }
        public Dictionary<Uri, String> SheetUriDictionary { get; set; }
        public string activeSheetUri { get; set; }

        public FileInfoModel()
        {
            Extention = "";
            Decimal = DecimalCharacter.point;
            Orientation = Orientation.columnwise;
            Offset = 0;
            Variables = 1;
            Data = 2;
            Dateformat = "";
            TextMarker = TextMarker.quotes;
            DataArea = new List<string>();
            HeaderArea = "";
            SheetUriDictionary = new Dictionary<Uri, string>();
            activeSheetUri = "";
        }
    }
}