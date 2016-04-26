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
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "DateTime",      StringPattern = "MM-dd-yyyyThh:mm:ss",      RegexPattern = null},
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "Date",          StringPattern = "MM.dd.yyyy",               RegexPattern = null},
            new DataTypeDisplayPattern() {Systemtype = DataTypeCode.DateTime,   Name = "Time",          StringPattern = "hh:mm:ss",                 RegexPattern = null}
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

        public static DataTypeDisplayPattern Materialize(XmlNode dataTypeDisplayPattern)
        {
            if (dataTypeDisplayPattern != null)
            {
                XmlDocument xmlDoc = dataTypeDisplayPattern as XmlDocument;
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
