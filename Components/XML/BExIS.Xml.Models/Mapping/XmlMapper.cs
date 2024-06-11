using System.Collections.Generic;
using System.Linq;

namespace BExIS.Xml.Models.Mapping
{
    public class XmlMapper
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public XmlMappingHeader Header { get; set; }
        public List<XmlMappingRoute> Routes { get; set; }

        public XmlMapper()
        {
            Header = new XmlMappingHeader();
            Routes = new List<XmlMappingRoute>();
        }

        public XmlMappingRoute GetRoute(string sourceXPath)
        {
            if (Routes.Where(x => x.Source.XPath.Equals(sourceXPath)).Count() > 0)
            {
                return Routes.Where(x => x.Source.XPath.Equals(sourceXPath)).First();
            }

            return null;
        }

        public XmlMappingRoute GetRouteFromDestination(string desitinationXPath)
        {
            if (Routes.Where(x => x.Destination.XPath.Equals(desitinationXPath)).Count() > 0)
            {
                return Routes.Where(x => x.Destination.XPath.Equals(desitinationXPath)).First();
            }

            return null;
        }

        public bool SourceExist(string sourceXPath)
        {
            if (Routes.Where(x => x.Source.XPath.Equals(sourceXPath)).Count() > 0)
                return true;
            else
                return false;
        }

        public bool IsSourceElement(string sourceXPath)
        {
            if (Routes.Where(x => x.Source.XPath.Equals(sourceXPath)).Count() > 0)
            {
                return true;
            }

            return false;
        }
    }

    public enum XmlMapperTags
    {
        attributes,
        attribute,
        destination,
        element,
        header,
        mapping,
        package,
        packages,
        route,
        routes,
        schema,
        source
    }

    public enum XmlMapperAttributes
    {
        abbreviation,
        id,
        name,
        namespaceUri,
        sequence,
        type,
        prefix,
        url,
        value,
        xPath
    }
}