using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Resolvers;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Diagnostics;

namespace BExIS.Core.UI
{
    /// <summary>
    /// A HTMLExtension method to render XML using XSL
    /// </summary>
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Accepts a reference to the XML and XSL files and applies a transformation with optional
        /// parameters.
        /// </summary>
        /// <param name="helper">A reference to the HtmlHelper object</param>
        /// <param name="xsltPath">The path to the XSL file</param>
        /// <param name="xmlPath">The path to the XML file</param>
        /// <param name="parameters">Optional: A list  of arguments to pass into the XSL file</param>
        /// <returns>MvcHtmlString representing the transform</returns>
        public static MvcHtmlString RenderXslt(this HtmlHelper helper, string xslPath, string xmlPath, Dictionary<string, object> parameters = null)
        {
            string xsltResult = string.Empty;

            try
            {
                // XML Settings
                XmlReaderSettings xmlSettings = new XmlReaderSettings();
                xmlSettings.XmlResolver = null;
                xmlSettings.IgnoreComments = true;
                xmlSettings.DtdProcessing = DtdProcessing.Ignore;
                xmlSettings.ValidationType = ValidationType.None;

                // Attaches an action to the valiation event handler. This will write out error messages in the Output pane.
#if DEBUG
                xmlSettings.ValidationEventHandler += (sender, e) =>
                {
                    Debug.WriteLine(string.Format("{0}({1},{2}): {3} - {4}", e.Exception.SourceUri, e.Exception.LineNumber, e.Exception.LinePosition, e.Severity, e.Message));
                };
#endif

                // XSLT Settings
                XmlReaderSettings xsltSettings = new XmlReaderSettings();
                xsltSettings.XmlResolver = null;
                xsltSettings.DtdProcessing = DtdProcessing.Ignore;
                xsltSettings.ValidationType = ValidationType.None;

                // Attaches an action to the valiation event handler. This will write out error messages in the Output pane.
#if DEBUG
                xsltSettings.ValidationEventHandler += (sender, e) =>
                {
                    Debug.WriteLine(string.Format("{0}({1},{2}): {3} - {4}", e.Exception.SourceUri, e.Exception.LineNumber, e.Exception.LinePosition, e.Severity, e.Message));
                };
#endif

                // Init params
                XsltArgumentList xslArgs = new XsltArgumentList();
                if (parameters != null)
                {
                    foreach (KeyValuePair<string, object> param in parameters)
                        xslArgs.AddParam(param.Key, string.Empty, param.Value);
                }

                //XsltArgumentList arguments = new XsltArgumentList();
                //arguments.AddExtensionObject("pda:MyUtils", new MyXslExtension());

                // Load XML
                using (XmlReader reader = XmlReader.Create(xmlPath, xmlSettings))
                {
                    // Load XSL
                    XsltSettings xslSettings = new XsltSettings(true, true); // Need to enable the document() fucntion

                    //also there is a need to load xsd!!! all the information about the fields are there

                    using (XmlReader xslSource = XmlReader.Create(xslPath, xsltSettings))
                    {
                        XslCompiledTransform xsltDoc = new XslCompiledTransform();
                        xsltDoc.Load(xslSource, xslSettings, new XmlUrlResolver());

                        // Transform
                        using (var sw = new StringWriter()) // Utf8
                        {
                            XmlWriterSettings settings = new XmlWriterSettings();
                            settings.Encoding = Encoding.UTF8;
                            settings.OmitXmlDeclaration = true;

                            using (var xw = XmlWriter.Create(sw, settings))
                            {
                                xsltDoc.Transform(reader, xslArgs, sw);
                            }

                            xsltResult = sw.ToString();
                        }
                    }
                }
            }
            catch { } // custom error handling here

            // Return result
            return MvcHtmlString.Create(xsltResult);
        }
    }
}
