using System.Xml;
using Vaiona.Util.Cfg;


namespace BExIS.Ddm.Providers.DummyProvider.Helpers
{
    public static class MetadataWriter
    {

        public static string  ShowMetadataAsHtml(string m ){

            // this code changes again later when the extensibility fraework is ready! all the modules and components should provide a class to introduce their manifests!!
            // Javad: 04.03.13
            string url = AppConfiguration.GetModuleWorkspacePath("DDM") + "\\UI\\HtmlShowMetadata.xsl";

            if (m != null)
            {
                    System.IO.StringReader stringReader = new System.IO.StringReader(m);
                    XmlReader xmlReader = XmlReader.Create(stringReader);

                    System.Xml.Xsl.XslCompiledTransform xslt = new System.Xml.Xsl.XslCompiledTransform(true);
                    System.Xml.Xsl.XsltSettings xsltSettings = new System.Xml.Xsl.XsltSettings(true, false);
                    xslt.Load(url, xsltSettings, new XmlUrlResolver());

                    System.Xml.Xsl.XsltArgumentList xsltArgumentList = new System.Xml.Xsl.XsltArgumentList();

                    System.IO.StringWriter stringWriter = new System.IO.StringWriter();
                    xslt.Transform(xmlReader, xsltArgumentList, stringWriter);
                    return stringWriter.ToString();
            }

            return "";

        }


    }
}