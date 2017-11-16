using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BExIS.Dlm.Services.TypeSystem;
using System.Xml;
using BExIS.Xml.Helpers;

namespace BExIS.IO.DataType.DisplayPattern
{
    public class DataTypeDisplayPattern
    {
        private static List<DataTypeDisplayPattern> displayPattern = new List<DataTypeDisplayPattern>()
        {
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "DateTimeIso",   StringPattern = "yyyy-MM-ddThh:mm:ss",      RegexPattern = null},
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "DateIso",       StringPattern = "yyyy-MM-dd",               RegexPattern = null},
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "DateUs",        StringPattern = "MM/dd/yyyy",               RegexPattern = null},
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "DateUk",        StringPattern = "dd/MM/yyyy",               RegexPattern = null},
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "DateEu",        StringPattern = "dd.MM.yyyy",               RegexPattern = null},
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "Time",          StringPattern = "HH:mm:ss",                 RegexPattern = null},
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "Time 12h",      StringPattern = "hh:mm:ss tt",              RegexPattern = null},
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "Year",          StringPattern = "yyyy",                     RegexPattern = null},
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "Month",         StringPattern = "MMMMMM",                   RegexPattern = null}
        };

        public DataTypeCode Systemtype { get; set; }
        public string Name { get; set; }
        public string StringPattern { get; set; }
        public string RegexPattern { get; set; }

        /// <summary>
        /// use this property in the form of DataTypeInfo.Types to access all the types and filter them using LINQ if required
        /// </summary>
        public static List<DataTypeDisplayPattern> Pattern { get { return displayPattern; } }

        public static XmlNode Dematerialize(DataTypeDisplayPattern dataTypeDisplayPattern)
        {
            string StringPattern;
            string RegexPattern;

            if (dataTypeDisplayPattern.StringPattern == null)
                StringPattern = "null";
            else
                StringPattern = dataTypeDisplayPattern.StringPattern;

            if (dataTypeDisplayPattern.RegexPattern == null)
                RegexPattern = "null";
            else
                RegexPattern = dataTypeDisplayPattern.RegexPattern;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml( "<DisplayPattern>" +
                            "   <Systemtype>"+ dataTypeDisplayPattern.Systemtype.ToString() + "</Systemtype>" +
                            "   <Name>" + dataTypeDisplayPattern.Name + "</Name>" +
                            "   <StringPattern>" + StringPattern + "</StringPattern>" +
                            "   <RegexPattern>" + RegexPattern + "</RegexPattern>" +
                            "</DisplayPattern>");
            return xmlDoc.DocumentElement;
        }

        public static DataTypeDisplayPattern Materialize(XmlNode extra)
        {
            if (extra != null)
            {
                XmlDocument xmlDoc = extra as XmlDocument;
                XmlNode xmlNode;

                DataTypeDisplayPattern displayPattern = new DataTypeDisplayPattern();
                if (xmlDoc.GetElementsByTagName("DisplayPattern").Count > 0)
                {
                    xmlNode = XmlUtility.GetXmlNodeByName(xmlDoc.GetElementsByTagName("DisplayPattern").Item(0), "Systemtype");
                    if (xmlNode != null)
                    {
                        foreach (DataTypeCode dtc in Enum.GetValues(typeof(DataTypeCode)))
                        {
                            if (dtc.ToString() == xmlNode.InnerText)
                            {
                                displayPattern.Systemtype = dtc;
                                break;
                            }
                        }
                    }
                    else
                    {
                        return null;
                    }
                    xmlNode = XmlUtility.GetXmlNodeByName(xmlDoc.GetElementsByTagName("DisplayPattern").Item(0), "Name");
                    if(xmlNode != null && xmlNode.InnerText != "null")
                        displayPattern.Name = xmlNode.InnerText;
                    else
                        displayPattern.Name = null;
                    xmlNode = XmlUtility.GetXmlNodeByName(xmlDoc.GetElementsByTagName("DisplayPattern").Item(0), "StringPattern");
                    if (xmlNode != null && xmlNode.InnerText != "null")
                        displayPattern.StringPattern = xmlNode.InnerText;
                    else
                        displayPattern.StringPattern = null;
                    xmlNode = XmlUtility.GetXmlNodeByName(xmlDoc.GetElementsByTagName("RegexPattern").Item(0), "RegexPattern");
                    if (xmlNode != null && xmlNode.InnerText != "null")
                        displayPattern.RegexPattern = xmlNode.InnerText;
                    else
                        displayPattern.RegexPattern = null;
                    return displayPattern;
                }
            }
            return null;
        }
    }
}
