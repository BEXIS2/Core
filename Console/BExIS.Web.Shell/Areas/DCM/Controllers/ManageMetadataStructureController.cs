using BExIS.Ddm.Model;
using BExIS.Ddm.Providers.LuceneProvider;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.IO.Transform.Output;
using BExIS.Security.Services.Objects;
using BExIS.Web.Shell.Areas.DCM.Models;
using BExIS.Xml.Helpers;
using BExIS.Xml.Helpers.Mapping;
using Ionic.Zip;
using NHibernate.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Web.Shell.Areas.DCM.Controllers
{
    public class ManageMetadataStructureController : Controller
    {
        // GET: DCM/ManageMetadataStructure
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Metadata Structure", this.Session.GetTenant());


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

            if (tmp.MetadataStructureModels.Any())
            {
                tmp.MetadataStructureModels = tmp.MetadataStructureModels.OrderBy(m => m.Id).ToList();
            }


            return tmp;
        }

        #region helper

        private MetadataStructureModel convertToMetadataStructureModel(MetadataStructure metadataStructure)
        {
            MetadataStructureModel metadataStructureModel = new MetadataStructureModel();
            metadataStructureModel.Id = metadataStructure.Id;
            metadataStructureModel.Name = metadataStructure.Name;

            try
            {
                metadataStructureModel.MetadataNodes = GetAllXPath(metadataStructure.Id);

                //get all informaions from xml
                metadataStructureModel.EntityClasses = GetEntityModelList();
                string EntityClassPath = XmlDatasetHelper.GetEntityTypeFromMetadatStructure(metadataStructure.Id);
                var entityModel =
                    metadataStructureModel.EntityClasses.Where(e => e.ClassPath.Equals(EntityClassPath))
                        .FirstOrDefault();
                if (entityModel != null) metadataStructureModel.Entity = entityModel;

                string xpath = XmlDatasetHelper.GetInformationPath(metadataStructure, NameAttributeValues.title);

                var searchMetadataNode =
                    metadataStructureModel.MetadataNodes.Where(e => e.XPath.Equals(xpath)).FirstOrDefault();
                if (searchMetadataNode != null)
                    metadataStructureModel.TitleNode =
                        searchMetadataNode.DisplayName;

                xpath = XmlDatasetHelper.GetInformationPath(metadataStructure,
                    NameAttributeValues.description);

                //check if xsd exist
                string schemapath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DCM"), "Metadata",
                    metadataStructure.Name);

                if (Directory.Exists(schemapath) && Directory.GetFiles(schemapath).Length > 0)
                    metadataStructureModel.HasSchema = true;

                var firstOrDefault =
                    metadataStructureModel.MetadataNodes.Where(e => e.XPath.Equals(xpath)).FirstOrDefault();
                if (firstOrDefault != null)
                    metadataStructureModel.DescriptionNode =
                        firstOrDefault.DisplayName;

                metadataStructureModel.MetadataNodes = GetAllXPath(metadataStructureModel.Id);

                metadataStructureModel.Active = XmlDatasetHelper.IsActive(metadataStructure.Id);

                //load system nodes
                metadataStructureModel.SystemNodes = GetSystemNodes(metadataStructureModel.Id, metadataStructureModel.MetadataNodes);

            }
            catch (Exception exception)
            {
                metadataStructureModel = new MetadataStructureModel();
                metadataStructureModel.Id = metadataStructure.Id;
                metadataStructureModel.Name = metadataStructure.Name;
            }



            return metadataStructureModel;
        }

        private MetadataStructure updateMetadataStructure(MetadataStructure metadataStructure,
            MetadataStructureModel metadataStructureModel)
        {

            if (metadataStructure.Id.Equals(metadataStructureModel.Id))
            {
                metadataStructure.Name = metadataStructureModel.Name;

                if (metadataStructure.Extra != null)
                {
                    XmlDocument xmlDocument = new XmlDocument();
                    if (metadataStructure.Extra as XmlDocument != null)
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

                    XmlNode tmp = XmlUtility.GetXmlNodeByAttribute(xmlDocument.DocumentElement,
                        nodeNames.nodeRef.ToString(), AttributeNames.name.ToString(),
                        NameAttributeValues.title.ToString());

                    tmp.Attributes[AttributeNames.value.ToString()].Value = titleXPath;

                    string descriptionXPath =
                        metadataStructureModel.MetadataNodes
                            .Where(e => e.DisplayName.Equals(metadataStructureModel.DescriptionNode))
                            .FirstOrDefault()
                            .XPath;

                    tmp = XmlUtility.GetXmlNodeByAttribute(xmlDocument.DocumentElement, nodeNames.nodeRef.ToString(),
                        AttributeNames.name.ToString(), NameAttributeValues.description.ToString());
                    tmp.Attributes[AttributeNames.value.ToString()].Value = descriptionXPath;

                    //set entity
                    tmp = XmlUtility.GetXmlNodeByName(xmlDocument.DocumentElement, nodeNames.entity.ToString());
                    if (tmp != null)
                        tmp.Attributes[AttributeNames.value.ToString()].Value = metadataStructureModel.Entity.ClassPath;
                    else
                    {
                        xmlDocument = XmlDatasetHelper.AddReferenceToXml(xmlDocument, nodeNames.entity.ToString(),
                            metadataStructureModel.Entity.ClassPath, AttributeType.entity.ToString(), "extra/entity");
                    }

                    //set active
                    tmp = XmlUtility.GetXmlNodeByAttribute(xmlDocument.DocumentElement, nodeNames.parameter.ToString(),
                        AttributeNames.name.ToString(), NameAttributeValues.active.ToString());
                    if (tmp != null)
                        tmp.Attributes[AttributeNames.value.ToString()].Value = metadataStructureModel.Active.ToString();
                    else
                    {
                        xmlDocument = XmlDatasetHelper.AddReferenceToXml(xmlDocument,
                            NameAttributeValues.active.ToString(),
                            metadataStructureModel.Active.ToString(), AttributeType.parameter.ToString(),
                            "extra/parameters/parameter");

                    }


                    //set systemNodes
                    if (metadataStructureModel.SystemNodes.Count > 0)
                    {
                        foreach (var systemNode in metadataStructureModel.SystemNodes)
                        {
                            string key = systemNode.Key;

                            tmp = XmlUtility.GetXmlNodeByAttribute(xmlDocument.DocumentElement, nodeNames.systemRef.ToString(),
                            AttributeNames.name.ToString(), key);

                            string xpath = metadataStructureModel.MetadataNodes
                                .Where(e => e.DisplayName.Equals(systemNode.Value))
                                .FirstOrDefault()
                                .XPath;

                            if (tmp != null)
                            {
                                tmp.Attributes[AttributeNames.value.ToString()].Value = xpath;
                            }
                            else
                            {

                                SystemNameAttributeValues systemName = (SystemNameAttributeValues)Enum.Parse(typeof(SystemNameAttributeValues), systemNode.Key);

                                xmlDocument = XmlDatasetHelper.AddSystemReferenceToXml(xmlDocument,
                                    systemName, xpath);
                            }

                        }




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

        private List<EntityModel> GetEntityModelList()
        {
            EntityManager entityManager = new EntityManager();

            List<EntityModel> tmp = new List<EntityModel>();
            entityManager.GetAllEntities().Where(e => e.UseMetadata).ForEach(e => tmp.Add(
                      new EntityModel()
                      {
                          Name = e.Name,
                          ClassPath = e.ClassPath
                      }
                  )
            );

            return tmp.ToList();
        }

        private Dictionary<string, string> GetSystemNodes(long metadatastructureId, List<SearchMetadataNode> MetadataNodes)
        {
            Dictionary<string, string> tmp = new Dictionary<string, string>();

            foreach (SystemNameAttributeValues value in Enum.GetValues(typeof(SystemNameAttributeValues)))
            {
                string xpath = XmlDatasetHelper.GetSystemInformationPath(metadatastructureId, value);

                var searchMetadataNode = MetadataNodes.Where(e => e.XPath.Equals(xpath)).FirstOrDefault();
                if (searchMetadataNode != null)
                    tmp.Add(value.ToString(), searchMetadataNode.DisplayName);
                else tmp.Add(value.ToString(), "");
            }

            return tmp;
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

        public ActionResult DownloadSchema(long id)
        {
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            MetadataStructure metadataStructure = metadataStructureManager.Repo.Get(id);
            string name = metadataStructure.Name;

            string path = OutputMetadataManager.GetSchemaDirectoryPathFromMetadataStructure(id);

            ZipFile zip = new ZipFile();
            if (Directory.Exists(path))
                zip.AddDirectory(path);

            MemoryStream stream = new MemoryStream();
            zip.Save(stream);
            stream.Position = 0;
            var result = new FileStreamResult(stream, "application/zip")
            { FileDownloadName = name + ".zip" };

            return result;
        }

    }
}