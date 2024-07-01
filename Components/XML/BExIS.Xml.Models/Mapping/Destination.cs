using System.Xml;

namespace BExIS.Xml.Models.Mapping
{
    public class Destination : XmlMappingPath
    {
        public string ParentSequence { get; set; }
        public string Prefix { get; set; }
        public string NamepsaceURI { get; set; }

        public Destination(string xPath, string parentSequence)
        {
            this.XPath = xPath;
            this.ParentSequence = parentSequence;
            this.Prefix = "";
            this.NamepsaceURI = "";
        }

        public Destination(string xPath, string parentSequence, string prefix, string namespaceUri)
        {
            this.XPath = xPath;
            this.ParentSequence = parentSequence;
            this.Prefix = prefix;
            this.NamepsaceURI = namespaceUri;
        }

        public static Destination Convert(XmlNode xmlNode)
        {
            string xPath = "";
            string sequenceName = "";
            string prefix = "";
            string namespaceUri = "";
            if (xmlNode.Attributes.Count > 0)
            {
                xPath = xmlNode.Attributes[XmlMapperAttributes.xPath.ToString()].Value;

                if (xmlNode.Attributes[XmlMapperAttributes.sequence.ToString()] != null)
                {
                    sequenceName = xmlNode.Attributes[XmlMapperAttributes.sequence.ToString()].Value;
                }

                if (xmlNode.Attributes[XmlMapperAttributes.prefix.ToString()] != null)
                {
                    prefix = xmlNode.Attributes[XmlMapperAttributes.prefix.ToString()].Value;
                }

                if (xmlNode.Attributes[XmlMapperAttributes.namespaceUri.ToString()] != null)
                {
                    namespaceUri = xmlNode.Attributes[XmlMapperAttributes.namespaceUri.ToString()].Value;
                }
            }

            return new Destination(xPath, sequenceName, prefix, namespaceUri);
        }
    }
}