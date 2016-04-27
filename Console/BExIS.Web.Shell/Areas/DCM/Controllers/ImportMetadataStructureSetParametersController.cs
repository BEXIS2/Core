using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;
using BExIS.Dcm.ImportMetadataStructureWizard;
using BExIS.Dcm.Wizard;
using BExIS.Ddm.Model;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Web.Shell.Areas.DCM.Models.ImportMetadata;
using BExIS.Xml.Helpers;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class ImportMetadataStructureSetParametersController : Controller
    {
        private ImportMetadataStructureTaskManager TaskManager;
        private List<SearchMetadataNode> _metadataNodes = new List<SearchMetadataNode>();

        [HttpGet]
        public ActionResult SetParameters(int index)
        {
            TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];
            long metadatstructureId = 0;

            ParametersModel model = new ParametersModel();

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.METADATASTRUCTURE_ID))
            {
                metadatstructureId = Convert.ToInt64(TaskManager.Bus[ImportMetadataStructureTaskManager.METADATASTRUCTURE_ID]);

                //set current stepinfo based on index
                if (TaskManager != null)
                    TaskManager.SetCurrent(index);

                model.MetadataNodes = GetAllXPathsOfSimpleAttributes();
            }
            else
            {
                ModelState.AddModelError("", "MetadataStructure not exist");
            }

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.TITLE_NODE))
                model.TitleNode = TaskManager.Bus[ImportMetadataStructureTaskManager.TITLE_NODE].ToString();

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.DESCRIPTION_NODE))
                model.DescriptionNode = TaskManager.Bus[ImportMetadataStructureTaskManager.DESCRIPTION_NODE].ToString();


            model.StepInfo = TaskManager.Current();
            model.StepInfo.notExecuted = true;

           return PartialView(model);
        }

        [HttpPost]
        public ActionResult SetParameters(int? index, string name = null)
        {
            TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];

            ParametersModel model = new ParametersModel();
            model.StepInfo = TaskManager.Current();

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.TITLE_NODE)
                && TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.DESCRIPTION_NODE))
            {
                TaskManager.Current().SetValid(true);
            }
            else
            {

                TaskManager.Current().SetValid(false);
                ModelState.AddModelError("", "Please save the configurations.");
            }


            if (TaskManager.Current().IsValid())
            {
                TaskManager.Current().SetStatus(StepStatus.success);
                TaskManager.AddExecutedStep(TaskManager.Current());
                TaskManager.GoToNext();
                Session["TaskManager"] = TaskManager;
                ActionInfo actionInfo = TaskManager.Current().GetActionInfo;
                return RedirectToAction(actionInfo.ActionName, actionInfo.ControllerName, new RouteValueDictionary { { "area", actionInfo.AreaName }, { "index", TaskManager.GetCurrentStepInfoIndex() } });
            }

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.METADATASTRUCTURE_ID))
            {
                long metadatstructureId = Convert.ToInt64(TaskManager.Bus[ImportMetadataStructureTaskManager.METADATASTRUCTURE_ID]);
                model.MetadataNodes = GetAllXPathsOfSimpleAttributes();
                model.StepInfo.notExecuted = true;
            }

            return PartialView(model);

        }

        public List<SearchMetadataNode> GetAllXPathsOfSimpleAttributes()
        {
            List<SearchMetadataNode> list = new List<SearchMetadataNode>();

           TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];

            // load metadatastructure with all packages and attributes

           if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.METADATASTRUCTURE_ID))
           {
               long metadataStrutureId = Convert.ToInt64(TaskManager.Bus[ImportMetadataStructureTaskManager.METADATASTRUCTURE_ID]);
               MetadataStructureManager msd = new MetadataStructureManager();
               string title = msd.Repo.Get(metadataStrutureId).Name;

               XmlMetadataWriter xmlMetadatWriter = new XmlMetadataWriter(XmlNodeMode.xPath);
               XDocument metadataXml = xmlMetadatWriter.CreateMetadataXml(metadataStrutureId);

               List<XElement> elements = metadataXml.Root.Descendants().Where(e=>e.HasElements.Equals(false)).ToList();

               foreach(XElement element in elements )
               {
                   list.Add(
                     new SearchMetadataNode(title, XExtentsions.GetAbsoluteXPath(element).Substring(1))
                     );

               }
           }

           return list;
        }

        public ActionResult Save(ParametersModel model)
        {
            TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];

            long metadatstructureId = Convert.ToInt64(TaskManager.Bus[ImportMetadataStructureTaskManager.METADATASTRUCTURE_ID]);

            model.MetadataNodes = GetAllXPathsOfSimpleAttributes();
            model.StepInfo = TaskManager.Current();
            model.StepInfo.notExecuted = false;

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.TITLE_NODE))
                TaskManager.Bus[ImportMetadataStructureTaskManager.TITLE_NODE] = model.TitleNode;
            else
                TaskManager.Bus.Add(ImportMetadataStructureTaskManager.TITLE_NODE, model.TitleNode);

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.DESCRIPTION_NODE))
                TaskManager.Bus[ImportMetadataStructureTaskManager.DESCRIPTION_NODE] = model.DescriptionNode;
            else
                TaskManager.Bus.Add(ImportMetadataStructureTaskManager.DESCRIPTION_NODE, model.DescriptionNode);

            string mappingFilePathImport = TaskManager.Bus[ImportMetadataStructureTaskManager.MAPPING_FILE_NAME_IMPORT].ToString();
            string mappingFilePathExport = TaskManager.Bus[ImportMetadataStructureTaskManager.MAPPING_FILE_NAME_EXPORT].ToString();

            try
            {
                StoreParametersToMetadataStruture(metadatstructureId, model.TitleNode, model.DescriptionNode, mappingFilePathImport, mappingFilePathExport);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return PartialView("SetParameters",model);
        }

      
        #region extra xdoc
        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="titlePath"></param>
        /// <param name="descriptionPath"></param>
        /// <param name="mappingFilePath"></param>
        /// <param name="direction"></param>
        private void StoreParametersToMetadataStruture(long id, string titlePath, string descriptionPath, string mappingFilePathImport, string mappingFilePathExport)
        {
            MetadataStructureManager mdsManager = new MetadataStructureManager();
            MetadataStructure metadataStructure = mdsManager.Repo.Get(id);

            XmlDocument xmlDoc = new XmlDocument();

            if (metadataStructure.Extra != null)
            {
                xmlDoc = (XmlDocument)metadataStructure.Extra;
            }

            // add title Node
            xmlDoc = AddReferenceToMetadatStructure(metadataStructure, "title", titlePath, "extra/nodeReferences/nodeRef", xmlDoc);
            // add Description
            xmlDoc = AddReferenceToMetadatStructure(metadataStructure, "description", descriptionPath, "extra/nodeReferences/nodeRef", xmlDoc);

            // add mappingFilePath
            xmlDoc = AddReferenceToMetadatStructure(metadataStructure, "mappingFileImport", mappingFilePathImport, "extra/convertReferences/convertRef", xmlDoc);
            xmlDoc = AddReferenceToMetadatStructure(metadataStructure, "mappingFileExport", mappingFilePathExport, "extra/convertReferences/convertRef", xmlDoc);


            metadataStructure.Extra = xmlDoc;
            mdsManager.Update(metadataStructure);

        }

        private XmlDocument AddReferenceToMetadatStructure(MetadataStructure metadataStructure, string nodeName, string nodePath, string destinationPath, XmlDocument xmlDoc)
        {

            XmlDocument doc = xmlDoc;
            XmlNode extra;

            if (doc.DocumentElement == null)
            {
                if (metadataStructure.Extra != null)
                {

                    extra = ((XmlDocument)metadataStructure.Extra).DocumentElement;
                }
                else
                {
                    extra = doc.CreateElement("extra", "");
                }

                doc.AppendChild(extra);
            }

            XmlNode x = createMissingNodes(destinationPath, doc.DocumentElement, doc, nodeName);

            //check attrviute of the xmlnode
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
        private XmlNode createMissingNodes(string destinationParentXPath, XmlNode parentNode, XmlDocument doc, string name)
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

                    if (temp.Last().Equals(s))
                    {
                        if (!t.Attributes["name"].Equals(name))
                        {
                            t = XmlUtility.CreateNode(s, doc);
                            parentTemp.AppendChild(t);
                        }

                    }

                    parentTemp = t;
                }
            }

            return parentTemp;
        }

        #endregion

    }
}