using BExIS.Dcm.ImportMetadataStructureWizard;
using BExIS.Dcm.Wizard;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Modules.Dcm.UI.Models.ImportMetadata;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Services.Objects;
using BExIS.UI.Helpers;
using BExIS.UI.Models;
using BExIS.Utils.Models;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class ImportMetadataStructureSetParametersController : Controller
    {
        private List<SearchMetadataNode> _metadataNodes = new List<SearchMetadataNode>();
        private ImportMetadataStructureTaskManager TaskManager;
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        public List<SearchMetadataNode> GetAllXPathsOfSimpleAttributes()
        {
            using (IUnitOfWork unitOfWork = this.GetUnitOfWork())
            {
                List<SearchMetadataNode> list = new List<SearchMetadataNode>();

                TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];

                // load metadatastructure with all packages and attributes

                if (TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.METADATASTRUCTURE_ID))
                {
                    long metadataStrutureId = Convert.ToInt64(TaskManager.Bus[ImportMetadataStructureTaskManager.METADATASTRUCTURE_ID]);
                    string title = unitOfWork.GetReadOnlyRepository<MetadataStructure>().Get(metadataStrutureId).Name;

                    XmlMetadataWriter xmlMetadatWriter = new XmlMetadataWriter(XmlNodeMode.xPath);
                    XDocument metadataXml = xmlMetadatWriter.CreateTempMetadataXmlWithChoiceChildrens(metadataStrutureId);

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

            string mappingFilePathImport = TaskManager.Bus[ImportMetadataStructureTaskManager.MAPPING_FILE_NAME_IMPORT].ToString();
            string mappingFilePathExport = TaskManager.Bus[ImportMetadataStructureTaskManager.MAPPING_FILE_NAME_EXPORT].ToString();

            try
            {
                StoreParametersToMetadataStruture(metadatstructureId, model.TitleNode, model.EntityType, model.DescriptionNode, mappingFilePathImport, mappingFilePathExport);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

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

                TaskManager.Current().SetValid(true);

                try
                {
                    StoreParametersToMetadataStruture(id, titleXpath, descriptionXpath, entity, mappingFilePathImport, mappingFilePathExport);
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
                model.Entities = GetEntityList();
                model.StepInfo.notExecuted = true;
            }

            return PartialView(model);
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

            return PartialView("SetParameters", model);
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

        private string GetDisplayName(string xpath)
        {
            var searchMetadataNode = GetMetadataNodes().Where(n => n.XPath == xpath).FirstOrDefault();
            if (searchMetadataNode != null)
                return searchMetadataNode.DisplayName;

            return "";
        }

        private List<string> GetEntityList()
        {
            EntityManager entityManager = new EntityManager();

            try
            {
                List<string> tmp = new List<string>();

                foreach (var entity in entityManager.Entities)
                {
                    tmp.Add(entity.Name);
                }

                //IEnumerable<string> tmp = entityManager.Entities.Select(e => e.EntityType.FullName);

                return tmp.ToList();
            }
            finally
            {
                entityManager.Dispose();
            }
        }

        private List<SearchMetadataNode> GetMetadataNodes()
        {
            TaskManager = (ImportMetadataStructureTaskManager)Session["TaskManager"];

            if (TaskManager != null && !TaskManager.Bus.ContainsKey(ImportMetadataStructureTaskManager.ALL_METADATA_NODES))
            {
                TaskManager.AddToBus(ImportMetadataStructureTaskManager.ALL_METADATA_NODES, GetAllXPathsOfSimpleAttributes());
            }

            return (List<SearchMetadataNode>)TaskManager.Bus[ImportMetadataStructureTaskManager.ALL_METADATA_NODES];
        }

        #region extra xdoc

        private XmlDocument AddReferenceToMetadatStructure(string nodeName, string nodePath, string nodeType, string destinationPath, XmlDocument xmlDoc)
        {
            XmlDocument doc = xmlDatasetHelper.AddReferenceToXml(xmlDoc, nodeName, nodePath, nodeType, destinationPath);

            return doc;
        }

        /// <summary>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="titlePath"></param>
        /// <param name="descriptionPath"></param>
        /// <param name="mappingFilePath"></param>
        /// <param name="direction"></param>
        private void StoreParametersToMetadataStruture(long id, string titlePath, string descriptionPath, string entity, string mappingFilePathImport, string mappingFilePathExport)
        {
            MetadataStructureManager mdsManager = new MetadataStructureManager();
            EntityManager entityManager = new EntityManager();

            try
            {
                MetadataStructure metadataStructure = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(id);

                XmlDocument xmlDoc = new XmlDocument();

                if (metadataStructure.Extra != null)
                {
                    xmlDoc = (XmlDocument)metadataStructure.Extra;
                }

                // add title Node
                xmlDoc = AddReferenceToMetadatStructure("title", titlePath, AttributeType.xpath.ToString(), "extra/nodeReferences/nodeRef", xmlDoc);
                // add Description
                xmlDoc = AddReferenceToMetadatStructure("description", descriptionPath, AttributeType.xpath.ToString(), "extra/nodeReferences/nodeRef", xmlDoc);

                if (entityManager.EntityRepository.Get().Any(e => { return e.Name != null && e.Name.Equals(entity); }))
                {
                    Entity e = entityManager.EntityRepository.Get().FirstOrDefault(x => x.Name != null && x.Name.Equals(entity));
                    if (e != null)
                        xmlDoc = AddReferenceToMetadatStructure(e.Name, e.EntityType.FullName, AttributeType.entity.ToString(), "extra/entity", xmlDoc);
                }

                // add mappingFilePath
                xmlDoc = AddReferenceToMetadatStructure(metadataStructure.Name, mappingFilePathImport, "mappingFileImport", "extra/convertReferences/convertRef", xmlDoc);
                xmlDoc = AddReferenceToMetadatStructure(metadataStructure.Name, mappingFilePathExport, "mappingFileExport", "extra/convertReferences/convertRef", xmlDoc);

                //set active
                xmlDoc = AddReferenceToMetadatStructure(NameAttributeValues.active.ToString(), true.ToString(), AttributeType.parameter.ToString(), "extra/parameters/parameter", xmlDoc);

                metadataStructure.Extra = xmlDoc;
                mdsManager.Update(metadataStructure);
            }
            finally
            {
                mdsManager.Dispose();
                entityManager.Dispose();
            }
        }

        #endregion extra xdoc
    }
}