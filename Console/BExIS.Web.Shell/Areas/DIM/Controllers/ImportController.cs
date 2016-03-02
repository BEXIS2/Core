using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Administration;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.Xml.Helpers;
using BExIS.Xml.Helpers.Mapping;
using BExIS.Xml.Services;
using Vaiona.Utils.Cfg;

namespace BExIS.Web.Shell.Areas.DIM.Controllers
{
    public class ImportController : Controller
    {
        //
        // GET: /DIM/Import/
        public ActionResult Index()
        {

            //xml metadata for import
            string metadataForImportPath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), "MetadataIDIV_EXAMPLE.xml");

            XmlDocument metadataForImport = new XmlDocument();
            metadataForImport.Load(metadataForImportPath);

            // metadataStructure DI
            long metadataStructureId = 3;

            // loadMapping file
            string path_mappingFile = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), getMappingFileName(metadataStructureId));

            // XML mapper + mapping file
            XmlMapperManager xmlMapperManager = new XmlMapperManager();
            xmlMapperManager.Load(path_mappingFile, "IDIV");
            
            // generate intern metadata 
            XmlDocument metadataResult = xmlMapperManager.Generate(metadataForImport,1);

            // generate intern template
            XmlMetadataWriter xmlMetadatWriter = new XmlMetadataWriter(BExIS.Xml.Helpers.XmlNodeMode.xPath);
            XDocument metadataXml = xmlMetadatWriter.CreateMetadataXml(metadataStructureId);
            XmlDocument metadataXmlTemplate = XmlMetadataWriter.ToXmlDocument(metadataXml);

            XmlDocument completeMetadata = fillInXmlAttributes(metadataResult, metadataXmlTemplate);

            // create Dataset

            //load datastructure

            DataStructureManager dsm = new DataStructureManager();
            ResearchPlanManager rpm = new ResearchPlanManager();
            MetadataStructureManager msm = new MetadataStructureManager();
                 
            DatasetManager dm = new DatasetManager();
            Dataset dataset = dm.CreateEmptyDataset(dsm.UnStructuredDataStructureRepo.Get(1), rpm.Repo.Get(1), msm.Repo.Get(3));

            if (dm.IsDatasetCheckedOutFor(dataset.Id, GetUserNameOrDefault()) || dm.CheckOutDataset(dataset.Id, GetUserNameOrDefault()))
            {
                DatasetVersion workingCopy = dm.GetDatasetWorkingCopy(dataset.Id);
                workingCopy.Metadata = completeMetadata;

                string title = XmlDatasetHelper.GetInformation(workingCopy, AttributeNames.title);
                if (String.IsNullOrEmpty(title)) title = "No Title available.";

                dm.EditDatasetVersion(workingCopy, null, null, null);
                dm.CheckInDataset(dataset.Id, "Metadata was submited.", GetUserNameOrDefault());

                // add security
                if (GetUserNameOrDefault() != "DEFAULT")
                {
                    PermissionManager pm = new PermissionManager();
                    SubjectManager sm = new SubjectManager();

                    BExIS.Security.Entities.Subjects.User user = sm.GetUserByName(GetUserNameOrDefault());

                    foreach (RightType rightType in Enum.GetValues(typeof(RightType)).Cast<RightType>())
                    {
                        pm.CreateDataPermission(user.Id, 1, dataset.Id, rightType);
                    }
                }
            }

            

            return View();
        }



        private string getMappingFileName(long id)
        {
            MetadataStructureManager msm = new MetadataStructureManager();
            MetadataStructure metadataStructure = msm.Repo.Get(id);

            // get MetadataStructure 
            XDocument xDoc = XmlUtility.ToXDocument((XmlDocument)metadataStructure.Extra);
            XElement temp = XmlUtility.GetXElementByAttribute(nodeNames.convertRef.ToString(), "name", "mappingFileImport", xDoc);

            return temp.Attribute("value").Value.ToString();
        }

        #region update new xml metadata with a base template

        public XmlDocument fillInXmlAttributes(XmlDocument metadataXml, XmlDocument metadataXmlTemplate)
        {
            // add missing nodes
            //doc = manipulate(doc);

            // add the xml attributes
            handle(metadataXml, metadataXml, metadataXmlTemplate);

            return metadataXml;
        }

        // rekursive Funktion
        private void handle(XmlNode root, XmlDocument doc, XmlDocument temp)
        {
            foreach (XmlNode node in root.ChildNodes)
            {
                Debug.WriteLine(node.Name);///////////////////////////////////////////////////////////////////////////
                if (node.HasChildNodes)
                {
                    string xpath = XmlUtility.FindXPath(node); // xpath in doc
                    long number = 1;
                    List<xpathProp> xpathDict = dismantle(xpath); // divide xpath
                    string xpathTemp = "";
                    for (int i = 1; i < xpathDict.Count; i++)
                        xpathTemp += "/" + xpathDict[i].nodeName + "[1]"; // atapt xpath to template
                    for (int j = xpathDict.Count - 1; j >= 0; j--)
                    {
                        xpathProp xp = xpathDict[j];
                        if (xp.nodeIndex > number)
                            number = xp.nodeIndex; // get node index "number"
                    }

                    XmlNode tempNode = temp.SelectSingleNode(xpathTemp); // get node from template

                    if (tempNode != null && tempNode.Attributes.Count > 0)
                    {
                        foreach (XmlAttribute a in tempNode.Attributes)
                        {
                            try // transfer all attributes from tamplate node to doc node
                            {
                                XmlAttribute b = doc.CreateAttribute(a.Name);
                                if (a.Name == "number") // handle node index "number"
                                    b.Value = number.ToString();
                                else
                                    b.Value = a.Value;
                                node.Attributes.Append(b);
                            }
                            catch
                            {
                            }
                        }
                    }

                    handle(node, doc, temp); // next level recursively
                }
            }
        }

        private List<xpathProp> dismantle(string xpath)
        {
            String[] xpathArray = xpath.Split('/');
            List<xpathProp> xpathDict = new List<xpathProp>();
            foreach (string s in xpathArray)
            {
                xpathProp xp = new xpathProp();
                if (s.Length > 0)
                {
                    xp.nodeName = s.Substring(0, s.IndexOf('['));
                    string subs = s.Substring(s.IndexOf('[') + 1, s.IndexOf(']') - s.IndexOf('[') - 1);
                    xp.nodeIndex = long.Parse(subs);
                }
                else
                {
                    xp.nodeName = s;
                    xp.nodeIndex = 1;
                }
                xpathDict.Add(xp);
            }
            return xpathDict;
        }

        private XmlDocument manipulate(XmlDocument doc)
        {
            XmlNode root = doc.SelectSingleNode("/Metadata");
            for (int i = 0; i < root.ChildNodes.Count; i++)
            {
                XmlNode oldNode = root.ChildNodes[i].Clone();
                XmlNode newNode = doc.CreateNode(oldNode.NodeType, oldNode.Name, oldNode.NamespaceURI);
                newNode.AppendChild(oldNode);
                root.ReplaceChild(newNode, root.ChildNodes[i]);
            }
            return doc;
        }


        #endregion

        #region helper

        // chekc if user exist
        // if true return usernamem otherwise "DEFAULT"
        public string GetUserNameOrDefault()
        {
            string userName = string.Empty;
            try
            {
                userName = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(userName) ? userName : "DEFAULT";
        }

        #endregion
    }

    class xpathProp
    {
        public string nodeName { get; set; }
        public long nodeIndex { get; set; }
    }

}