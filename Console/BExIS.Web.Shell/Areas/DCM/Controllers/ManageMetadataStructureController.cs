using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using BExIS.Ddm.Model;
using BExIS.Ddm.Providers.LuceneProvider;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Security.Services.Objects;
using BExIS.Web.Shell.Areas.DCM.Models;
using BExIS.Xml.Helpers;
using BExIS.Xml.Helpers.Mapping;
using BExIS.Xml.Services;
using Telerik.Web.Mvc;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class ManageMetadataStructureController : Controller
    {
        // GET: DCM/ManageMetadataStructure
        public ActionResult Index()
        {
            return View(GetDefaultModel());
        }

        private MetadataStructureManagerModel GetDefaultModel()
        {
            MetadataStructureManagerModel tmp = new MetadataStructureManagerModel();

           //load all metadatastructure
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            IEnumerable<MetadataStructure> metadataStructures = metadataStructureManager.Repo.Get();

            foreach (var metadataStructure in metadataStructures)
            {
                tmp.MetadataStructureModels.Add(convertToMetadataStructureModel(metadataStructure));
            }


            return tmp;
        }

        #region helper

        private MetadataStructureModel convertToMetadataStructureModel(MetadataStructure metadataStructure)
        {
            MetadataStructureModel metadataStructureModel = new MetadataStructureModel();
            metadataStructureModel.Id = metadataStructure.Id;
            metadataStructureModel.Name = metadataStructure.Name;
            metadataStructureModel.MetadataNodes = GetAllXPath(metadataStructure.Id);
            //get all informaions from xml
            metadataStructureModel.EntityClassPath = XmlDatasetHelper.GetEntityTypeFromMetadatStructure(metadataStructure.Id);

            string xpath = XmlDatasetHelper.GetInformationPath(metadataStructure,NameAttributeValues.title);

            var searchMetadataNode = metadataStructureModel.MetadataNodes.Where(e => e.XPath.Equals(xpath)).FirstOrDefault();
            if (searchMetadataNode != null)
                metadataStructureModel.TitleNode =
                    searchMetadataNode.DisplayName;

            xpath = XmlDatasetHelper.GetInformationPath(metadataStructure,
                NameAttributeValues.description);

            var firstOrDefault = metadataStructureModel.MetadataNodes.Where(e => e.XPath.Equals(xpath)).FirstOrDefault();
            if (firstOrDefault != null)
                metadataStructureModel.DescriptionNode =
                    firstOrDefault.DisplayName;

            metadataStructureModel.MetadataNodes = GetAllXPath(metadataStructureModel.Id);
            metadataStructureModel.EnitiesClassPaths = GetEntityClassPathList();
            metadataStructureModel.Active = XmlDatasetHelper.IsActive(metadataStructure.Id);

            return metadataStructureModel;
        }

        private MetadataStructure updateMetadataStructure(MetadataStructure metadataStructure, MetadataStructureModel metadataStructureModel)
        {

            if (metadataStructure.Id.Equals(metadataStructureModel.Id))
            {
                metadataStructure.Name = metadataStructureModel.Name;

                if (metadataStructure.Extra != null)
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    if (metadataStructure.Extra as XmlDocument!=null)
                        xmlDocument = metadataStructure.Extra as XmlDocument;
                    else
                    {
                        xmlDocument.AppendChild(metadataStructure.Extra);
                    }

                    metadataStructureModel.MetadataNodes = GetAllXPath(metadataStructure.Id);

                    //set title & description
                    string titleXPath =
                        metadataStructureModel.MetadataNodes
                            .Where(e => e.DisplayName.Equals(metadataStructureModel.TitleNode))
                            .FirstOrDefault()
                            .XPath;

                    XmlNode tmp =  XmlUtility.GetXmlNodeByAttribute(xmlDocument.DocumentElement, nodeNames.nodeRef.ToString(), AttributeNames.name.ToString(), NameAttributeValues.title.ToString());

                    tmp.Attributes[AttributeNames.value.ToString()].Value = titleXPath;

                    string descriptionXPath =
                        metadataStructureModel.MetadataNodes
                            .Where(e => e.DisplayName.Equals(metadataStructureModel.DescriptionNode))
                            .FirstOrDefault()
                            .XPath;

                    tmp = XmlUtility.GetXmlNodeByAttribute(xmlDocument.DocumentElement, nodeNames.nodeRef.ToString(), AttributeNames.name.ToString(), NameAttributeValues.title.ToString());
                    tmp.Attributes[AttributeNames.value.ToString()].Value = descriptionXPath;

                    //set entity
                    tmp = XmlUtility.GetXmlNodeByName(xmlDocument.DocumentElement, nodeNames.entity.ToString());
                    if (tmp != null)
                        tmp.Attributes[AttributeNames.value.ToString()].Value = metadataStructureModel.EntityClassPath;
                    else
                    {
                        xmlDocument = XmlDatasetHelper.AddReferenceToXml(xmlDocument, nodeNames.entity.ToString(),
                            metadataStructureModel.EntityClassPath, AttributeType.entity.ToString(), "extra/entity");
                    }

                    //set active
                    tmp = XmlUtility.GetXmlNodeByAttribute(xmlDocument.DocumentElement, nodeNames.parameter.ToString(), AttributeNames.name.ToString(), NameAttributeValues.active.ToString());
                    if (tmp != null)
                        tmp.Attributes[AttributeNames.value.ToString()].Value = metadataStructureModel.Active.ToString();
                    else
                    {
                        xmlDocument = XmlDatasetHelper.AddReferenceToXml(xmlDocument, NameAttributeValues.active.ToString(),
                            metadataStructureModel.Active.ToString(), AttributeType.parameter.ToString(), "extra/parameters/parameter");

                    }



                    metadataStructure.Extra = xmlDocument;
                }

                
            }
            return metadataStructure;
        }

        /// <summary>
        /// Get all simple attributes of a metadata Structure
        /// </summary>
        /// <param name="metadatastructureId"></param>
        /// <returns></returns>
        private List<SearchMetadataNode> GetAllXPath(long metadatastructureId)
        {
            SearchDesigner searchDesigner = new SearchDesigner();
            return searchDesigner.GetAllXPathsOfSimpleAttributes(metadatastructureId);
        }

        private List<string> GetEntityClassPathList()
        {
            EntityManager entityManager = new EntityManager();

            IEnumerable<string> tmp = entityManager.GetAllEntities().Select(e => e.ClassPath);

            return tmp.ToList();
        }

        #endregion

        public ActionResult Save(MetadataStructureModel metadataStructureModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
                    MetadataStructure metadataStructure = metadataStructureManager.Repo.Get(metadataStructureModel.Id);
                    metadataStructure = updateMetadataStructure(metadataStructure, metadataStructureModel);
                    metadataStructureManager.Update(metadataStructure);

                }
                catch (Exception ex)
                {

                    return Json(ex.Message);
                }
              
                return Json(true);
            }

            return Json(false);
        }

        public ActionResult Edit(long id)
        {
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            MetadataStructure metadataStructure = metadataStructureManager.Repo.Get(id);

            MetadataStructureModel model = convertToMetadataStructureModel(metadataStructure);

            return PartialView("_editMetadataStructureView", model);
        }

        public ActionResult Delete(long id)
        {
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            MetadataStructure metadataStructure = metadataStructureManager.Repo.Get(id);

            try
            {
                // delete local files
                if (XmlSchemaManager.Delete(metadataStructure))
                {
                    metadataStructureManager.Delete(metadataStructure);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }

            

            // delete links from search index

            

            if (metadataStructureManager.Repo.Get(id) == null) return Json(true);

            return Json(false);
        }
    }
}