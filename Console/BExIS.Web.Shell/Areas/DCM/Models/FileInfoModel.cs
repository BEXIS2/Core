using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BExIS.DCM.Transform.Input;

namespace BExIS.Web.Shell.Areas.DCM.Models
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

        public FileInfoModel()
        {
            Extention = "";
            Decimal = DecimalCharacter.comma;
            Orientation = Orientation.columnwise;
            Offset = 0;
            Variables = 1;
            Data = 2;
            Dateformat = "";
        }
  
    }
}