using System.Collections.Generic;

namespace Vaiona.Web.Mvc.Models
{
    public class XsltViewModel
    {
        /// <summary>
        /// Holds a reference to the XSLT path
        /// </summary>
        public string XsltPath { get; set; }

        /// <summary>
        /// Holds a reference to the XML path
        /// </summary>
        public string XmlPath { get; set; }

        /// <summary>
        /// Optional parameters to pass into the XSL transformer.
        /// </summary>
        public Dictionary<string, object> Params { get; set; }
    }
}