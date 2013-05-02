using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Core.UI
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
