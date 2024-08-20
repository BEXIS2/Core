using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Utils.Models;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Xml.Helpers
{
    public class XmlMetadataHelper
    {
        /// <summary>
        /// Returns a List of SearchMetadataNode
        /// for each Metadata Simple Attribute
        /// </summary>
        /// <param name="id">Metadata Structure Id</param>
        /// <returns> List<SearchMetadataNode> </returns>
        public List<SearchMetadataNode> GetAllXPathsOfSimpleAttributes(long id)
        {
            List<SearchMetadataNode> list = new List<SearchMetadataNode>();

            // load metadatastructure with all packages and attributes
            using (var uow = this.GetUnitOfWork())
            {
                string title = uow.GetReadOnlyRepository<MetadataStructure>().Get(id).Name;

                XmlMetadataWriter xmlMetadatWriter = new XmlMetadataWriter(XmlNodeMode.xPath);
                XDocument metadataXml = xmlMetadatWriter.CreateMetadataXml(id);

                List<XElement> elements = metadataXml.Root.Descendants().Where(e => e.HasElements.Equals(false)).ToList();

                foreach (XElement element in elements)
                {
                    list.Add(
                        new SearchMetadataNode(title, XExtentsions.GetAbsoluteXPath(element).Substring(1))
                        );
                }

                return list;
            }
        }
    }
}