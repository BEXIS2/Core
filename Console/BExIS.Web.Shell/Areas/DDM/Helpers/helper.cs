using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace BExIS.Web.Shell.Areas.DDM.Helpers
{
    public class helper
    {

        /// <summary>
        /// Create a list of Parents nodes from an XML document
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref="BExIS.Web.Shell\Areas\DDM\Controllers\DataController.cs"/>
        /// <param name="xDoc">An XML Document</param>
        /// <param name="attribute">Description of a node</param>
        /// <param name="value">Value that is searching </param>
        /// <returns list>An IEnumerable of XElements</returns>
        public static IEnumerable<XElement> GetElementsByAttribute(XDocument xDoc, string attribute, string value)
        {
            IEnumerable<XElement> list;
            list = xDoc.Root.Descendants().Where(e => e.Attribute(attribute).Value.Equals(value));

            return list;
        }


        /// <summary>
        /// Create a list of childs from an XML document
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="element">an XElement</param>
        /// <param name="attribute">an Attribute name</param>
        /// <param name="value">value of the attribute</param>
        /// <returns>An IEnumerable of XElements</returns>
        public static IEnumerable<XElement> GetElementsByAttribute(XElement element, string attribute, string value)
        {
            IEnumerable<XElement> list;
            list = element.Descendants().Where(e => e.Attribute(attribute).Value.Equals(value));

            return list;
        }
     
    }

}