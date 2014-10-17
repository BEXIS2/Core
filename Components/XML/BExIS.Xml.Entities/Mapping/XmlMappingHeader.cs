using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BExIS.Xml.Entities.Mapping
{
    public class XmlMappingHeader
    {
        public Destination Destination { get; set; }
        public Dictionary<string, string> Packages { get; set; }
        public Dictionary<string, string> Schemas { get; set; }
        public Dictionary<string, string> Attributes { get; set; }

        public XmlMappingHeader()
        {
            Packages = new Dictionary<string,string>();
            Schemas = new Dictionary<string, string>();
            Attributes = new Dictionary<string, string>();
        }

        public void AddToPackages(XmlNode xmlNode)
        {
            string abbr = xmlNode.Attributes[XmlMapperAttributes.abbreviation.ToString()].Value;
            string url = xmlNode.Attributes[XmlMapperAttributes.url.ToString()].Value;

            this.Packages.Add(abbr, url);
        }

        public void AddToSchemas(XmlNode xmlNode)
        {
            string abbr = xmlNode.Attributes[XmlMapperAttributes.abbreviation.ToString()].Value;
            string url = xmlNode.Attributes[XmlMapperAttributes.url.ToString()].Value;

            this.Schemas.Add(abbr, url);
        }

        public void AddToAttributes(XmlNode xmlNode)
        {
            string name = xmlNode.Attributes[XmlMapperAttributes.name.ToString()].Value;
            string value = xmlNode.Attributes[XmlMapperAttributes.value.ToString()].Value;

            this.Attributes.Add(name, value);
        }
    }
}
