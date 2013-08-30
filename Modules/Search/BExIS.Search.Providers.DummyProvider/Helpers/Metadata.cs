using System;
using System.Collections.Generic;
using System.Xml;

namespace BExIS.Search.Providers.DummyProvider.Helpers
{
    public class Metadata
    {
        private XmlReader _xmlReader;
        private string _url = "";

        public Metadata() {

            metadataXml = new XmlDocument();
        
        }

        // source from db
        public XmlDocument metadataXml
        {
            get;
            set;
        }

        /// <summary>
        /// Load xml file using url
        /// </summary>
        /// <param name="url">Path of the xmlfile</param>
        public void LoadXml(string url)
        {
            _url = url;
            XmlReader _XmlReader = XmlReader.Create(url);
            metadataXml.Load(_XmlReader);

        }

        /// <summary>
        /// Get a value from a node
        /// </summary>
        /// <param name="node"></param>
        /// <returns>XmlNodeList of values</returns>
        public XmlNodeList GetValueFromNode(string node)
        {
            XmlNodeList list = metadataXml.GetElementsByTagName(node);
            return list;
        }

        /// <summary>
        /// Get all values as a list of strings
        /// </summary>
        /// <returns>List of string</returns>
        public List<string> GetAllValuesAsList()
        {
            List<string> l = new List<string>();

            _xmlReader = XmlReader.Create(_url);
            

            while (_xmlReader.Read())
            {

                switch (_xmlReader.NodeType)
                {

                    case XmlNodeType.Text: //Anzeigen des Textes in jedem Element.
                        Console.WriteLine(_xmlReader.Value);
                        l.Add(_xmlReader.Value);
                        break;
                }
            }

            return l;
        }

        /// <summary>
        /// Check if value inside the metadata
        /// </summary>
        /// <param name="value">value to search for</param>
        /// <returns>true/false</returns>
        public bool IsValueInMetadata(string value) {

            _xmlReader = XmlReader.Create(_url);
            while (_xmlReader.Read())
            {

                if (_xmlReader.Value.ContainsExact(value) || _xmlReader.Value.ToLower() == value.ToLower()) return true; 
            }

            return false;
        }

        /// <summary>
        /// Check if value in node
        /// </summary>
        /// <param name="node">node name</param>
        /// <param name="value">value name</param>
        /// <returns>true/false</returns>
        public bool IsValueInNode(string node, string value)
        {

            XmlNodeList list = metadataXml.GetElementsByTagName(node);
            foreach (XmlNode x in list)
            {
                if (x.InnerText.ContainsExact(value) || x.InnerText.ToLower() == value.ToLower()) return true; 
               
            }

            return false;
        }

        public bool IsDateInRangeOf(int date, string node, bool desc) {

            XmlNodeList list = metadataXml.GetElementsByTagName(node);
            bool isInRange = false;
     

            if (!desc)
            {
                foreach (XmlNode x in list)
                {
                    int test = Convert.ToDateTime(x.InnerText).Year;
                    //startdate innerhalb start && end
                    if (date <= test) isInRange = true;
                }
            }
            else
            {
                foreach (XmlNode x in list)
                {
                    int test = Convert.ToDateTime(x.InnerText).Year;
                    //enddate innhalb start && end
                    if (date >= test) isInRange = true;
                }
            }

            if (isInRange == true) return true;
            else return false;
        
        }

    }
}