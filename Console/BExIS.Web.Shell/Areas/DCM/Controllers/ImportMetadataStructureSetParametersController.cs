using BExIS.Dcm.ImportMetadataStructureWizard;
using BExIS.Dcm.Wizard;
using BExIS.Ddm.Model;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Security.Services.Objects;
using BExIS.Web.Shell.Areas.DCM.Models.ImportMetadata;
using BExIS.Web.Shell.Helpers;
using BExIS.Web.Shell.Models;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;

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

                model.MetadataNodes = GetMetadataNodes();
                model.Entities = GetEntityList();
                model.SystemNodes = GetSystemNodes();
            }
            else
            {
                ModelState.AddModelError("", "MetadataStructure not exist");
            }

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.TITLE_NODE))
                model.TitleNode = GetDisplayName((string)TaskManager.Bus[ImportMetadataStructureTaskManager.TITLE_NODE]);

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.DESCRIPTION_NODE))
                model.DescriptionNode =
                    GetDisplayName((string)TaskManager.Bus[ImportMetadataStructureTaskManager.DESCRIPTION_NODE]);
            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.ENTITY_TYPE_NODE))
                model.EntityType = TaskManager.Bus[ImportMetadataStructureTaskManager.ENTITY_TYPE_NODE].ToString();

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.SYSTEM_NODES))
                model.SystemNodes = (Dictionary<string, string>)TaskManager.Bus[ImportMetadataStructureTaskManager.SYSTEM_NODES];
            else model.SystemNodes = GetSystemNodes();

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
                && TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.DESCRIPTION_NODE)
                && TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.ENTITY_TYPE_NODE))
            {
                long id = Convert.ToInt64((TaskManager.Bus[ImportMetadataStructureTaskManager.METADATASTRUCTURE_ID]));

                string mappingFilePathImport = TaskManager.Bus[ImportMetadataStructureTaskManager.MAPPING_FILE_NAME_IMPORT].ToString();
                string mappingFilePathExport = TaskManager.Bus[ImportMetadataStructureTaskManager.MAPPING_FILE_NAME_EXPORT].ToString();
                string titleXpath = TaskManager.Bus[ImportMetadataStructureTaskManager.TITLE_NODE].ToString();
                string descriptionXpath = TaskManager.Bus[ImportMetadataStructureTaskManager.DESCRIPTION_NODE].ToString();
                string entity = TaskManager.Bus[ImportMetadataStructureTaskManager.ENTITY_TYPE_NODE].ToString();
                model.TitleNode = GetMetadataNodes().First(p => p.XPath.Equals(titleXpath)).DisplayName;
                model.DescriptionNode = GetMetadataNodes().First(p => p.XPath.Equals(descriptionXpath)).DisplayName;
                model.EntityType = TaskManager.Bus[ImportMetadataStructureTaskManager.ENTITY_TYPE_NODE].ToString();


                if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.SYSTEM_NODES))
                    model.SystemNodes = (Dictionary<string, string>)TaskManager.Bus[ImportMetadataStructureTaskManager.SYSTEM_NODES];
                else
                {
                    model.SystemNodes = GetSystemNodes();
                }

                TaskManager.Current().SetValid(true);

                try
                {
                    StoreParametersToMetadataStruture(id, titleXpath, descriptionXpath, entity, mappingFilePathImport, mappingFilePathExport, model.SystemNodes);
                }
                catch (Exception ex)
                {
                    TaskManager.Current().SetValid(false);
                    ModelState.AddModelError("", ex.Message);
                }
            }
            else
            {
                //set existing parameter
                if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.TITLE_NODE))
                    model.TitleNode = GetMetadataNodes().First(p => p.XPath.Equals(TaskManager.Bus[ImportMetadataStructureTaskManager.TITLE_NODE].ToString())).DisplayName; ;
                if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.DESCRIPTION_NODE))
                    model.DescriptionNode = GetMetadataNodes().First(p => p.XPath.Equals(TaskManager.Bus[ImportMetadataStructureTaskManager.DESCRIPTION_NODE].ToString())).DisplayName; ;

                if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.SYSTEM_NODES))
                    model.SystemNodes = (Dictionary<string, string>)TaskManager.Bus[ImportMetadataStructureTaskManager.SYSTEM_NODES];
                else model.SystemNodes = GetSystemNodes();

                TaskManager.Current().SetValid(false);
                ModelState.AddModelError("", "Please select the missing field");
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
                model.MetadataNodes = GetMetadataNodes();
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

                List<XElement> elements = metadataXml.Root.Descendants().Where(e => e.HasElements.Equals(false)).ToList();

                foreach (XElement element in elements)
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

            model.MetadataNodes = GetMetadataNodes();

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

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.ENTITY_TYPE_NODE))
                TaskManager.Bus[ImportMetadataStructureTaskManager.ENTITY_TYPE_NODE] = model.EntityType;
            else
                TaskManager.Bus.Add(ImportMetadataStructureTaskManager.ENTITY_TYPE_NODE, model.EntityType);

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.SYSTEM_NODES))
                model.SystemNodes = (Dictionary<string, string>)TaskManager.Bus[ImportMetadataStructureTaskManager.SYSTEM_NODES];
            else model.SystemNodes = GetSystemNodes();


            string mappingFilePathImport = TaskManager.Bus[ImportMetadataStructureTaskManager.MAPPING_FILE_NAME_IMPORT].ToString();
            string mappingFilePathExport = TaskManager.Bus[ImportMetadataStructureTaskManager.MAPPING_FILE_NAME_EXPORT].ToString();

            try
            {
                StoreParametersToMetadataStruture(metadatstructureId, model.TitleNode, model.EntityType, model.DescriptionNode, mappingFilePathImport, mappingFilePathExport, model.SystemNodes);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return PartialView("SetParameters", model);
        }

        [HttpGet]
        public ActionResult ShowListOfMetadataNodesTitle()
        {
            List<SearchMetadataNode> SearchMetadataNode = GetMetadataNodes();

            string recieverActionName = "";
            string modelTitle = "";

            recieverActionName = "ShowListOfMetadataNodesForTitleReciever";
            modelTitle = "Select a node for the title";

            EntitySelectorModel model = BexisModelManager.LoadEntitySelectorModel(
                SearchMetadataNode,
                new List<string>() { "DisplayName" },
                new EntitySelectorModelAction(recieverActionName, "ImportMetadataStructureSetParameters", "DCM"),
                "DisplayName",
                "ImportMetadataStructureSetParameters"
                );

            model.Title = modelTitle;

            return PartialView("_EntitySelectorInWindowView", model);
        }

        public ActionResult ShowListOfMetadataNodesForTitleReciever(string Id)
        {
            SearchMetadataNode SelectedNode = GetMetadataNodes().Where(s => s.DisplayName.Equals(Id)).FirstOrDefault();

            ParametersModel model = new ParametersModel();
            model.StepInfo = TaskManager.Current();

            model.TitleNode = SelectedNode.DisplayName;

            model.Entities = GetEntityList();

            TaskManager.AddToBus(ImportMetadataStructureTaskManager.TITLE_NODE, SelectedNode.XPath);

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.DESCRIPTION_NODE))
            {
                model.DescriptionNode = GetDisplayName((string)TaskManager.Bus[ImportMetadataStructureTaskManager.DESCRIPTION_NODE]);
            }

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.ENTITY_TYPE_NODE))
            {
                model.EntityType = TaskManager.Bus[ImportMetadataStructureTaskManager.ENTITY_TYPE_NODE].ToString();
            }

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.SYSTEM_NODES))
                model.SystemNodes = (Dictionary<string, string>)TaskManager.Bus[ImportMetadataStructureTaskManager.SYSTEM_NODES];
            else model.SystemNodes = GetSystemNodes();

            return PartialView("SetParameters", model);
        }

        [HttpGet]
        public ActionResult ShowListOfMetadataNodesDescription()
        {
            List<SearchMetadataNode> SearchMetadataNode = GetMetadataNodes();

            string recieverActionName = "";
            string modelTitle = "";

            recieverActionName = "ShowListOfMetadataNodesForDescriptionReciever";
            modelTitle = "Select a node for the description";

            EntitySelectorModel model = BexisModelManager.LoadEntitySelectorModel(
                SearchMetadataNode,
                new List<string>() { "DisplayName" },
                new EntitySelectorModelAction(recieverActionName, "ImportMetadataStructureSetParameters", "DCM"),
                "DisplayName",
                "ImportMetadataStructureSetParameters"
                );

            model.Title = modelTitle;

            return PartialView("_EntitySelectorInWindowView", model);
        }

        public ActionResult ShowListOfMetadataNodesForDescriptionReciever(string Id)
        {
            SearchMetadataNode SelectedNode = GetMetadataNodes().Where(s => s.DisplayName.Equals(Id)).FirstOrDefault();

            ParametersModel model = new ParametersModel();
            model.StepInfo = TaskManager.Current();

            model.DescriptionNode = SelectedNode.DisplayName;

            model.Entities = GetEntityList();

            TaskManager.AddToBus(ImportMetadataStructureTaskManager.DESCRIPTION_NODE, SelectedNode.XPath);

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.TITLE_NODE))
            {
                model.TitleNode = GetDisplayName((string)TaskManager.Bus[ImportMetadataStructureTaskManager.TITLE_NODE]);
            }

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.ENTITY_TYPE_NODE))
            {
                model.EntityType = TaskManager.Bus[ImportMetadataStructureTaskManager.ENTITY_TYPE_NODE].ToString();
            }

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.SYSTEM_NODES))
                model.SystemNodes = (Dictionary<string, string>)TaskManager.Bus[ImportMetadataStructureTaskManager.SYSTEM_NODES];
            else model.SystemNodes = GetSystemNodes();

            return PartialView("SetParameters", model);
        }

        public ActionResult SetEntityName(string name)
        {
            TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.ENTITY_TYPE_NODE))
                TaskManager.Bus[ImportMetadataStructureTaskManager.ENTITY_TYPE_NODE] = name;
            else
                TaskManager.Bus.Add(ImportMetadataStructureTaskManager.ENTITY_TYPE_NODE, name);

            return null;
        }

        #region system informations

        public ActionResult ShowListOfMetadataNodes(string key)
        {
            List<SearchMetadataNode> SearchMetadataNode = GetMetadataNodes();

            string recieverActionName = "";
            string modelTitle = "";

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("name", key);

            recieverActionName = "ShowListOfMetadataNodesReciever";
            modelTitle = "Select a node for the " + key;

            EntitySelectorModel model = BexisModelManager.LoadEntitySelectorModel(
                SearchMetadataNode,
                new List<string>() { "DisplayName" },
                new EntitySelectorModelAction(recieverActionName, "ImportMetadataStructureSetParameters", "DCM"),
                "DisplayName",
                "ImportMetadataStructureSetParameters",
                parameters
                );

            model.Title = modelTitle;

            return PartialView("_EntitySelectorInWindowView", model);
        }

        public ActionResult ShowListOfMetadataNodesReciever(string Id, string parameters)
        {
            Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(parameters);

            SearchMetadataNode SelectedNode = GetMetadataNodes().Where(s => s.DisplayName.Equals(Id)).FirstOrDefault();

            ParametersModel model = new ParametersModel();
            model.StepInfo = TaskManager.Current();

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.SYSTEM_NODES))
                model.SystemNodes = (Dictionary<string, string>)TaskManager.Bus[ImportMetadataStructureTaskManager.SYSTEM_NODES];
            else model.SystemNodes = GetSystemNodes();

            if (values.Keys.Contains("name"))
            {
                if (model.SystemNodes.ContainsKey(values["name"]))
                {
                    model.SystemNodes[values["name"]] = SelectedNode.DisplayName;
                }
            }

            if (model.SystemNodes != null)
            {
                TaskManager.Bus[ImportMetadataStructureTaskManager.SYSTEM_NODES] = model.SystemNodes;
            }

            model.Entities = GetEntityList();

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.TITLE_NODE))
            {
                model.TitleNode = GetDisplayName((string)TaskManager.Bus[ImportMetadataStructureTaskManager.TITLE_NODE]);
            }

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.DESCRIPTION_NODE))
            {
                model.DescriptionNode = GetDisplayName((string)TaskManager.Bus[ImportMetadataStructureTaskManager.DESCRIPTION_NODE]);
            }

            if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.ENTITY_TYPE_NODE))
            {
                model.EntityType = TaskManager.Bus[ImportMetadataStructureTaskManager.ENTITY_TYPE_NODE].ToString();
            }

            return PartialView("SetParameters", model);
        }

        private Dictionary<string, string> GetSystemNodes()
        {
            Dictionary<string, string> tmpDic = new Dictionary<string, string>();

            var tmp = Enum.GetValues(typeof(SystemNameAttributeValues))
                    .Cast<SystemNameAttributeValues>()
                    .Select(v => v.ToString())
                    .ToList();

            foreach (var node in tmp)
            {
                tmpDic.Add(node, "");
            }

            return tmpDic;
        }

        #endregion

        private List<SearchMetadataNode> GetMetadataNodes()
        {
            TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];

            if (TaskManager != null && !TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.ALL_METADATA_NODES))
            {
                TaskManager.AddToBus(ImportMetadataStructureTaskManager.ALL_METADATA_NODES, GetAllXPathsOfSimpleAttributes());
            }

            return (List<SearchMetadataNode>)TaskManager.Bus[ImportMetadataStructureTaskManager.ALL_METADATA_NODES];
        }

        private List<string> GetEntityList()
        {
            EntityManager entityManager = new EntityManager();

            IEnumerable<string> tmp = entityManager.GetAllEntities().Select(e => e.ClassPath);

            return tmp.ToList();
        }

        private string GetDisplayName(string xpath)
        {
            var searchMetadataNode = GetMetadataNodes().Where(n => n.XPath == xpath).FirstOrDefault();
            if (searchMetadataNode != null)
                return searchMetadataNode.DisplayName;

            return "";
        }

        #region extra xdoc
        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="titlePath"></param>
        /// <param name="descriptionPath"></param>
        /// <param name="mappingFilePath"></param>
        /// <param name="direction"></param>
        private void StoreParametersToMetadataStruture(long id, string titlePath, string descriptionPath, string entity, string mappingFilePathImport, string mappingFilePathExport, Dictionary<string, string> systemParameters)
        {
            MetadataStructureManager mdsManager = new MetadataStructureManager();
            MetadataStructure metadataStructure = mdsManager.Repo.Get(id);

            XmlDocument xmlDoc = new XmlDocument();

            if (metadataStructure.Extra != null)
            {
                xmlDoc = (XmlDocument)metadataStructure.Extra;
            }

            // add title Node
            xmlDoc = AddReferenceToMetadatStructure("title", titlePath, AttributeType.xpath.ToString(), "extra/nodeReferences/nodeRef", xmlDoc);
            // add Description
            xmlDoc = AddReferenceToMetadatStructure("description", descriptionPath, AttributeType.xpath.ToString(), "extra/nodeReferences/nodeRef", xmlDoc);

            xmlDoc = AddReferenceToMetadatStructure("entity", entity, AttributeType.entity.ToString(), "extra/entity", xmlDoc);

            // add mappingFilePath
            xmlDoc = AddReferenceToMetadatStructure(metadataStructure.Name, mappingFilePathImport, "mappingFileImport", "extra/convertReferences/convertRef", xmlDoc);
            xmlDoc = AddReferenceToMetadatStructure(metadataStructure.Name, mappingFilePathExport, "mappingFileExport", "extra/convertReferences/convertRef", xmlDoc);

            //set active
            xmlDoc = AddReferenceToMetadatStructure(NameAttributeValues.active.ToString(), true.ToString(), AttributeType.parameter.ToString(), "extra/parameters/parameter", xmlDoc);

            foreach (KeyValuePair<string, string> kvp in systemParameters)
            {

                SystemNameAttributeValues name = (SystemNameAttributeValues)Enum.Parse(typeof(SystemNameAttributeValues), kvp.Key);
                xmlDoc = XmlDatasetHelper.AddSystemReferenceToXml(xmlDoc, name, kvp.Value);

            }



            metadataStructure.Extra = xmlDoc;
            mdsManager.Update(metadataStructure);

        }

        private XmlDocument AddReferenceToMetadatStructure(string nodeName, string nodePath, string nodeType, string destinationPath, XmlDocument xmlDoc)
        {

            XmlDocument doc = XmlDatasetHelper.AddReferenceToXml(xmlDoc, nodeName, nodePath, nodeType, destinationPath);

            return doc;

        }

        #endregion

    }
}