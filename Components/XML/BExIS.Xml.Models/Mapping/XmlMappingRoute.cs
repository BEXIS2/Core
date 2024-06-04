using System.Linq;
using System.Xml;

namespace BExIS.Xml.Models.Mapping
{
    public class XmlMappingRoute
    {
        public Source Source { get; set; }
        public Destination Destination { get; set; }

        public static XmlMappingRoute Convert(XmlNode xmlNode)
        {
            XmlMappingRoute route = new XmlMappingRoute();

            if (xmlNode.Name.Equals(XmlMapperTags.route.ToString()))
            {
                foreach (XmlNode childNode in xmlNode.ChildNodes)
                {
                    if (childNode.Name.Equals(XmlMapperTags.destination.ToString()))
                        route.Destination = Destination.Convert(childNode);

                    if (childNode.Name.Equals(XmlMapperTags.source.ToString()))
                        route.Source = Source.Convert(childNode);
                }

                return route;
            }

            return null;
        }

        public string GetDestinationTagNames()
        {
            string destinationXPath = Destination.XPath;
            destinationXPath = destinationXPath.TrimEnd('/');

            return destinationXPath.Split('/').Last();
        }

        public string GetDestinationXPath()
        {
            return Destination.XPath;
        }

        public string GetDestinationParentXPath()
        {
            string destinationXPath = Destination.XPath;
            destinationXPath = destinationXPath.TrimEnd('/');

            string[] temp = destinationXPath.Split('/');

            //lenght wihtout last element
            int lengthWithoutLastElement = temp.Length - 1;

            string parentXPath = "";

            for (int i = 0; i < lengthWithoutLastElement; i++)
            {
                if (i == 0) parentXPath = temp[i];
                else
                {
                    parentXPath += "/" + temp[i];
                }
            }

            return parentXPath;
        }
    }
}