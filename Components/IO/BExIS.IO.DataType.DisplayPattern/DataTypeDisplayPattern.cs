using BExIS.Dlm.Services.TypeSystem;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace BExIS.IO.DataType.DisplayPattern
{
    public class DataTypeDisplayPattern
    {
        private static List<DataTypeDisplayPattern> displayPatterns = new List<DataTypeDisplayPattern>()
        {
            new DataTypeDisplayPattern() {Id=0,Systemtype = DataTypeCode.DateTime,   Name = "DateTimeIso",       ExcelPattern=@"yyyy-MM-dd\Thh:mm:ss", DisplayPattern = "yyyy-MM-ddThh:mm:ss",     StringPattern = "yyyy-MM-ddTHH:mm:ss",      RegexPattern = null},
            new DataTypeDisplayPattern() {Id=1,Systemtype = DataTypeCode.DateTime,   Name = "DateIso",           ExcelPattern="yyyy-MM-dd",          DisplayPattern= "yyyy-MM-dd",               StringPattern = "yyyy-MM-dd",               RegexPattern = null},
            new DataTypeDisplayPattern() {Id=2,Systemtype = DataTypeCode.DateTime,   Name = "DateUs",            ExcelPattern=@"MM\/dd\/yyyy",         DisplayPattern="MM/dd/yyyy",                StringPattern = "MM/dd/yyyy",               RegexPattern = null},
            new DataTypeDisplayPattern() {Id=3,Systemtype = DataTypeCode.DateTime,   Name = "DateUs yy",         ExcelPattern=@"MM\/dd\/yy",           DisplayPattern="MM/dd/yy",                  StringPattern = "MM/dd/yy",                 RegexPattern = null},
            new DataTypeDisplayPattern() {Id=4,Systemtype = DataTypeCode.DateTime,   Name = "DateUs M/d/yyyy",   ExcelPattern=@"M\/d\/yyyy",           DisplayPattern="M/d/yyyy",                  StringPattern = "M/d/yyyy",                 RegexPattern = null},
            new DataTypeDisplayPattern() {Id=5,Systemtype = DataTypeCode.DateTime,   Name = "DateTimeUs h:m",    ExcelPattern=@"M\/d\/yyyy h:m",       DisplayPattern="M/d/yyyy h:m",              StringPattern = "M/d/yyyy h:m",             RegexPattern = null},
            new DataTypeDisplayPattern() {Id=6,Systemtype = DataTypeCode.DateTime,   Name = "DateUk",            ExcelPattern=@"dd\/MM\/yyyy",         DisplayPattern="dd/MM/yyyy",                StringPattern = "dd/MM/yyyy",               RegexPattern = null},
            new DataTypeDisplayPattern() {Id=7,Systemtype = DataTypeCode.DateTime,   Name = "DateUk yy",         ExcelPattern=@"dd\/MM\/yy",           DisplayPattern="dd/MM/yy",                  StringPattern = "dd/MM/yy",                 RegexPattern = null},
            new DataTypeDisplayPattern() {Id=8,Systemtype = DataTypeCode.DateTime,   Name = "DateEu",            ExcelPattern=@"dd\.MM\.yyyy",          DisplayPattern="dd.MM.yyyy",                StringPattern = "dd.MM.yyyy",              RegexPattern = null},
            new DataTypeDisplayPattern() {Id=9,Systemtype = DataTypeCode.DateTime,   Name = "DateEu yy",         ExcelPattern=@"dd\.MM.yy",            DisplayPattern="dd.MM.yy",                  StringPattern = "dd.MM.yy",                 RegexPattern = null},
            new DataTypeDisplayPattern() {Id=10,Systemtype = DataTypeCode.DateTime,   Name = "Time",              ExcelPattern="hh:mm:ss",            DisplayPattern="HH:mm:ss",                  StringPattern = "HH:mm:ss",                 RegexPattern = null},
            new DataTypeDisplayPattern() {Id=11,Systemtype = DataTypeCode.DateTime,   Name = "Time mm:ss",        ExcelPattern="mm:ss",               DisplayPattern="mm:ss",                     StringPattern = "mm:ss",                    RegexPattern = null},
            new DataTypeDisplayPattern() {Id=12,Systemtype = DataTypeCode.DateTime,   Name = "Time hh:mm",        ExcelPattern="hh:mm",               DisplayPattern="HH:mm",                     StringPattern = "HH:mm",                    RegexPattern = null},
            new DataTypeDisplayPattern() {Id=13,Systemtype = DataTypeCode.DateTime,   Name = "Time 12h",          ExcelPattern=@"hh:mm:ss AM/PM",     DisplayPattern="hh:mm:ss tt",               StringPattern = "hh:mm:ss tt",              RegexPattern = null},
            new DataTypeDisplayPattern() {Id=14,Systemtype = DataTypeCode.DateTime,   Name = "Time 12h hh:mm",    ExcelPattern=@"hh:mm AM/PM",        DisplayPattern="hh:mm tt",                  StringPattern = "hh:mm tt",                 RegexPattern = null}
            //new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "Year",              ExcelPattern="yyyy",                DisplayPattern="yyyy",                      StringPattern = "yyyy",                     RegexPattern = null},
            //new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "Month",             ExcelPattern="MM",                  DisplayPattern="MM",                        StringPattern = "MM",                       RegexPattern = null}
        };

        public DataTypeCode Systemtype { get; set; }

        public int Id { get; set; }
        public string Name { get; set; }

        public string DisplayPattern { get; set; }
        public string StringPattern { get; set; }
        public string ExcelPattern { get; set; }
        public string RegexPattern { get; set; }

        /// <summary>
        /// use this property in the form of DataTypeInfo.Types to access all the types and filter them using LINQ if required
        /// </summary>
        public static List<DataTypeDisplayPattern> Pattern { get { return displayPatterns; } }
        public static DataTypeDisplayPattern Get(int id ) {

            if(id <=0 ) throw new ArgumentException("id is missing");

            return displayPatterns.Where(p => p.Id.Equals(id)).FirstOrDefault();
        }


        public static DataTypeDisplayPattern GetByExcelPattern(string excelPattern)
        {
            excelPattern = excelPattern.Replace("\\/", @"\/");
            excelPattern = excelPattern.Replace(@"\-", "-");
            excelPattern = excelPattern.Replace(@"\.", ".");
            excelPattern = excelPattern.Replace(@"\ ", " ");
            excelPattern = excelPattern.Replace(@"\T", "T");

            foreach (var p in Pattern)
            {
                if (p.ExcelPattern.ToLower().Equals(excelPattern.ToLower())) return p;
            }

            return null;
        }


    }
}
