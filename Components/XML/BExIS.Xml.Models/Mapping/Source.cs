using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BExIS.Xml.Models.Mapping
{
    public class Source : XmlMappingPath
    {

        public Source(string xPath)
        {
            this.XPath = xPath;
        }

        
        public static Source Convert(XmlNode xmlNode)
        {
            string xPath = xmlNode.Attributes[XmlMapperAttributes.xPath.ToString()].Value;

            return new Source(xPath);
        }
    
    }

    public enum SourceType
    { 
        MetadataStructure,
        MetadataPackage,
        MetadataPackageUsage,
        MetadataAttribute,
        MetadataAttributeUsage
    }
}
