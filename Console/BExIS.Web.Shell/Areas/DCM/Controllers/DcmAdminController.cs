using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Xml.Helpers;


namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class DcmAdminController : Controller
    {
        //
        // GET: /DCM/DcmAdmin/

        public ActionResult Index()
        {
            return View();
        }

        #region metadata

        //public ActionResult XsdRead()
        //{
        //    XsdSchemaReader xsdSchemaReader = new XsdSchemaReader();

        //    xsdSchemaReader.Read();

        //    return View("Index");
        //}

        //public ActionResult CreateEMLMetadataStructure()
        //{

        //    #region eml

        //    //CreateEmlDatasetAdv();

        //    #endregion

        //    return View("CreateMetadataStructure");
        //}


        #endregion

        #region helper

        private XmlDocument AddNodeReferncesToMetadatStructure(MetadataStructure metadataStructure, string nodeName, string nodePath, XmlDocument xmlDoc)
        {

            string destinationPath = "extra/nodeReferences/nodeRef";

            XmlDocument doc = xmlDoc;
            XmlNode extra;

            if(metadataStructure.Extra !=null)
            {

                extra = ((XmlDocument)metadataStructure.Extra).DocumentElement;
            }
            else
            {
                extra = doc.CreateElement("extra","");
            }

            doc.AppendChild(extra);

            XmlNode x = createMissingNodes(destinationPath, doc.DocumentElement, doc);

            if (x.Attributes.Count > 0)
            {
                foreach (XmlAttribute attr in x.Attributes)
                {
                    if (attr.Name == "name") attr.Value = nodeName;
                    if (attr.Name == "value") attr.Value = nodePath;
                }
            }
            else
            {
                XmlAttribute name = doc.CreateAttribute("name");
                name.Value = nodeName;
                XmlAttribute value = doc.CreateAttribute("value");
                value.Value = nodePath;

                x.Attributes.Append(name);
                x.Attributes.Append(value);

            }

            return doc;
           
        }

        /// <summary>
        /// Add missing node to the desitnation document
        /// </summary>
        /// <param name="destinationParentXPath"></param>
        /// <param name="currentParentXPath"></param>
        /// <param name="parentNode"></param>
        /// <param name="doc"></param>
        /// <returns></returns>
        private XmlNode createMissingNodes(string destinationParentXPath, XmlNode parentNode, XmlDocument doc)
        {
            string dif = destinationParentXPath;

            List<string> temp = dif.Split('/').ToList();
            temp.RemoveAt(0);

            XmlNode parentTemp = parentNode;

            foreach (string s in temp)
            {
                if (XmlUtility.GetXmlNodeByName(parentTemp, s) == null)
                {
                    XmlNode t = XmlUtility.CreateNode(s, doc);

                    parentTemp.AppendChild(t);
                    parentTemp = t;
                }
                else
                {
                    XmlNode t = XmlUtility.GetXmlNodeByName(parentTemp, s);
                    parentTemp = t;
                }
            }

            return parentTemp;
        }

        #endregion
    }
}
