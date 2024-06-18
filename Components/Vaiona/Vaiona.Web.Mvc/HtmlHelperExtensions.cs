using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Xsl;
using Vaiona.Model.MTnt;
using Vaiona.Web.Extensions;
using Vaiona.Web.Helpers;

namespace Vaiona.Web.Mvc
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

                // Attaches an action to the validation event handler. This will write out error messages in the Output pane.
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

                // Attaches an action to the validation event handler. This will write out error messages in the Output pane.
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

        public static System.Globalization.CultureInfo GetCurrentCulture(this HtmlHelper helper)
        {
            return (GlobalizationHelper.GetCurrentCulture());
        }

        public static string GetCultureDirection(this HtmlHelper helper)
        {
            return (GlobalizationHelper.GetCurrentCulture().TextInfo.IsRightToLeft ? "right" : "left");
        }

        public static Tenant GetTenant(this HtmlHelper helper)
        {
            return (HttpContext.Current.Session.GetTenant());
        }

        public static string GetCultureReverseDirection(this HtmlHelper helper)
        {
            return (GlobalizationHelper.GetCurrentCulture().TextInfo.IsRightToLeft ? "left" : "right");
        }

        public static string GetCultureDirectionShort(this HtmlHelper helper)
        {
            return (GlobalizationHelper.GetCurrentCulture().TextInfo.IsRightToLeft ? "rtl" : "ltr");
        }

        public static string GetCultureReverseDirectionShort(this HtmlHelper helper)
        {
            return (GlobalizationHelper.GetCurrentCulture().TextInfo.IsRightToLeft ? "ltr" : "rtl");
        }

        public static void SetCulture(this HtmlHelper helper)
        {
            helper.SetCulture(helper.GetCurrentCulture());
        }

        public static void SetCulture(this HtmlHelper helper, string cultureId)
        {
            helper.SetCulture(new CultureInfo(cultureId, true));
        }

        public static void SetCulture(this HtmlHelper helper, CultureInfo culture)
        {
            GlobalizationHelper.SetSessionCulture(culture); //??
        }

        public static void SetTheme(this HtmlHelper helper, string themeName)
        {
            //return (GlobalizationHelper.GetCurrentCulture());
        }

        public static string GetTheme(this HtmlHelper helper)
        {
            string themeName = "default";
            //var theme = ccPeople.Model.PublicQueries.MessageQueries.GetMessage("Theme");
            //if(theme != null)
            //{
            //    themeName = theme.Body;
            //}
            return (themeName);
        }
    }
}