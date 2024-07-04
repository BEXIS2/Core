using System.Collections.Generic;
using System.Xml;

namespace BExIS.Xml.Models.Mapping
{
    public class XmlMappingHeader
    {
        public Destination Destination { get; set; }
        public Dictionary<string, string> Packages { get; set; }
        public Dictionary<string, string> Schemas { get; set; }
        public Dictionary<string, string> Attributes { get; set; }

        public XmlMappingHeader()
        {
            Packages = new Dictionary<string, string>();
            Schemas = new Dictionary<string, string>();
            Attributes = new Dictionary<string, string>();
        }

        public void AddToDestination(string xpath, string prefix = "", string parentSequence = "")
        {
            Destination = new Destination(xpath, parentSequence);
            Destination.Prefix = prefix;
        }

        public void AddToPackages(XmlNode xmlNode)
        {
            string abbr = xmlNode.Attributes[XmlMapperAttributes.abbreviation.ToString()].Value;
            string url = xmlNode.Attributes[XmlMapperAttributes.url.ToString()].Value;

            this.Packages.Add(abbr, url);
        }

        public void AddToPackages(string abbr, string url)
        {
            this.Packages.Add(abbr, url);
        }

        public void AddToSchemas(XmlNode xmlNode)
        {
            string abbr = xmlNode.Attributes[XmlMapperAttributes.abbreviation.ToString()].Value;
            string url = xmlNode.Attributes[XmlMapperAttributes.url.ToString()].Value;

            this.Schemas.Add(abbr, url);
        }

        public void AddToSchemas(string abbr, string url)
        {
            this.Schemas.Add(abbr, url);
        }

        public void AddToAttributes(XmlNode xmlNode)
        {
            string name = xmlNode.Attributes[XmlMapperAttributes.name.ToString()].Value;
            string value = xmlNode.Attributes[XmlMapperAttributes.value.ToString()].Value;

            this.Attributes.Add(name, value);
        }

        public void AddToAttributes(string name, string value)
        {
            this.Attributes.Add(name, value);
        }
    }
}