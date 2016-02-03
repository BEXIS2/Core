using Newtonsoft.Json;
using Saxon.Api;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using Telerik.Web.Mvc.UI;

namespace BExIS.Web.Shell.Areas.MSM.Models
{
    public class TreePartialModel
    {
        public string Name { get; set; }
        public int PartialId { get; set; }
        public TreeViewItemModel xmlTree { get; set; }
        public string Xsd { get; set; }
        public string JsonTreeView { get; set; }
        public string XsdTreeView { get; set; }


        public TreePartialModel(int partialId, string name, string xsd) {
            Name = name;
            Xsd = xsd;
            PartialId = partialId;
            //XDocument xmldoc = XDocument.Parse(xsd);
            //this.xsd = formatXml(xmldoc);
            //xmlTree = xml2treeViewItem(xmldoc.Root);
            XsdTreeView = xml2HtmlTree();
            JsonTreeView=xsd2jsonTree();
            //JsonTree = JsonConvert.SerializeXNode(xmldoc.Root, Newtonsoft.Json.Formatting.Indented);
            
        }

        private string xml2HtmlTree(){
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(this.Xsd);
            StreamReader xsl = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "/Areas/MSM/Resources/Stylesheets/xsd2htmlTree.xslt");
            
            Processor processor = new Processor();
            XdmNode input = processor.NewDocumentBuilder().Build(xml);
            XsltTransformer transformer = processor.NewXsltCompiler().Compile(xsl).Load();
            transformer.InitialContextNode = input;
            StringWriter writer = new StringWriter();
            Serializer serializer = new Serializer();
            serializer.SetOutputWriter(writer);
            transformer.Run(serializer);
            return writer.ToString();
        }
        private string xsd2jsonTree() {
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(this.Xsd);
            StreamReader xsl = new StreamReader(AppDomain.CurrentDomain.BaseDirectory + "/Areas/MSM/Resources/Stylesheets/xsd2jsonTree.xslt");

            Processor processor = new Processor();
            XdmNode input = processor.NewDocumentBuilder().Build(xml);
            XsltTransformer transformer = processor.NewXsltCompiler().Compile(xsl).Load();
            transformer.InitialContextNode = input;
            StringWriter writer = new StringWriter();
            Serializer serializer = new Serializer();
            serializer.SetOutputWriter(writer);
            transformer.Run(serializer);
            return writer.ToString();
        }



        #region deprecated
        /*private TreeViewItemModel xml2treeViewItem(XElement element) {
            TreeViewItemModel root = new TreeViewItemModel();
            root.Text = element.Name.ToString();
            foreach (XAttribute attr in element.Attributes()) {
                root.Text = root.Text + " " + attr.Name + " = \"" + attr.Value + "\"";
                UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);
                root.NavigateUrl = url.Action("addTextField", null, new { name=root.Text, xPath=element.BaseUri});
                 
            }
            foreach (XElement node in element.Nodes()) {
                root.Items.Add(xml2treeViewItem(node));
            }
            return root;
        }*/

        /*private string formatXml(XDocument xml) {
            StringWriter sw = new StringWriter();
            XmlTextWriter tx = new XmlTextWriter(sw);
            xml.WriteTo(tx);
            string formatXml = XElement.Parse(sw.ToString()).ToString();
            return formatXml;
        }*/
        #endregion
    }
}