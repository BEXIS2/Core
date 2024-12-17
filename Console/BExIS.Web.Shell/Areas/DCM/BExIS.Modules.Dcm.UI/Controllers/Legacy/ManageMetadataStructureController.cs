using BExIS.Dim.Entities.Export.GBIF;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Dcm.UI.Models;
using BExIS.Security.Services.Objects;
using BExIS.Utils.Extensions;
using BExIS.Utils.Models;
using BExIS.Xml.Helpers;
using BExIS.Xml.Helpers.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using Telerik.Web.Mvc.Extensions;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class ManageMetadataStructureController : Controller
    {
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        public ActionResult Delete(long id)
        {
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();

            try
            {
                MetadataStructure metadataStructure = metadataStructureManager.Repo.Get(id);
                // delete local files
                if (XmlSchemaManager.Delete(metadataStructure))
                {
                    metadataStructureManager.Delete(metadataStructure);

                    return Json(true);
                }

                return Json(false);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
            finally
            {
                metadataStructureManager.Dispose();
            }
        }

        // [2024-12-10][Sven]: The function will return a zip, even though the directory does not exist? 
        // [2024-12-10][Sven]: The function will return a zip, even though the schema does not exist? 
        public ActionResult DownloadSchema(long id)
        {
            try
            {
                using (var metadataStructureManager = new MetadataStructureManager())
                {
                    // [2024-12-10][Sven]: Why does no function exists to get a certain metadata structure?
                    var metadataStructure = metadataStructureManager.GetMetadataStructureById(id);

                    if (metadataStructure == null)
                        return HttpNotFound();

                    string path = OutputMetadataManager.GetSchemaDirectoryPathFromMetadataStructure(id, metadataStructureManager);

                    if(!Directory.Exists(path))
                        return HttpNotFound();

                    
                    var memoryStream = new MemoryStream();

                    using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
                    {
                        // Add each file from the folder to the archive
                        archive.AddAllFilesFromDirectory(path);
                    }

                    memoryStream.Position = 0;

                    return File(memoryStream, "application/zip", $"{metadataStructure.Name}.zip"); ;
                }
            }
            catch (Exception ex)
            {
                return HttpNotFound();
            }
        }

        public ActionResult Edit(long id)
        {
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();

            try
            {
                MetadataStructure metadataStructure = metadataStructureManager.Repo.Get(id);

                MetadataStructureModel model = convertToMetadataStructureModel(metadataStructure, metadataStructureManager);

                return PartialView("_editMetadataStructureView", model);
            }
            finally
            {
                metadataStructureManager.Dispose();
            }
        }

        // GET: DCM/ManageMetadataStructure
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Manage Metadata Structure", this.Session.GetTenant());

            return View(GetDefaultModel());
        }

        public ActionResult Save(MetadataStructureModel metadataStructureModel)
        {
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            DatasetManager datasetManager = new DatasetManager();

            try
            {
                if (ModelState.IsValid)
                {
                    MetadataStructure metadataStructure = metadataStructureManager.Repo.Get(metadataStructureModel.Id);

                    metadataStructure = updateMetadataStructure(metadataStructure, metadataStructureModel);
                    metadataStructureManager.Update(metadataStructure);

                    //update dsv title and description if there is a change
                    //ToDo check if there is a change in the xpaths
                    // update datasetversion

                    // get all datasetIds which using the metadata structure
                    var datasetIds = datasetManager.DatasetRepo.Query().Where(d => d.MetadataStructure.Id.Equals(metadataStructure.Id)).Select(d => d.Id);

                    if (datasetIds.Any())
                    {
                        //get all datasetversions of the dataset ids
                        var datasetVersionIds = datasetManager.DatasetVersionRepo.Query().Where(dsv => datasetIds.Contains(dsv.Dataset.Id)).Select(dsv => dsv.Id).ToList();

                        //load all titles & descriptions from versions
                        var allTitles = xmlDatasetHelper.GetInformationFromVersions(datasetVersionIds, metadataStructure.Id, NameAttributeValues.title);
                        var allDescriptions = xmlDatasetHelper.GetInformationFromVersions(datasetVersionIds, metadataStructure.Id, NameAttributeValues.description);

                        // update each datasetversion
                        foreach (var datasetVersionId in datasetVersionIds)
                        {
                            // load dataset version
                            var datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);

                            datasetVersion.Title = allTitles.ContainsKey(datasetVersion.Id) ? allTitles[datasetVersion.Id] : string.Empty;
                            datasetVersion.Description = allDescriptions.ContainsKey(datasetVersion.Id) ? allDescriptions[datasetVersion.Id] : string.Empty;

                            datasetManager.UpdateDatasetVersion(datasetVersion);
                        }
                    }

                    return Json(true);
                }

                return Json(false);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
            finally
            {
                metadataStructureManager.Dispose();
                datasetManager.Dispose();
            }
        }

        private MetadataStructureManagerModel GetDefaultModel()
        {
            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();

            try
            {
                MetadataStructureManagerModel tmp = new MetadataStructureManagerModel();

                //load all metadatastructure
                IEnumerable<MetadataStructure> metadataStructures = metadataStructureManager.Repo.Get();

                foreach (var metadataStructure in metadataStructures)
                {
                    tmp.MetadataStructureModels.Add(convertToMetadataStructureModel(metadataStructure, metadataStructureManager));
                }

                if (tmp.MetadataStructureModels.Any())
                {
                    tmp.MetadataStructureModels = tmp.MetadataStructureModels.OrderBy(m => m.Id).ToList();
                }

                return tmp;
            }
            finally
            {
                metadataStructureManager.Dispose();
            }
        }

        #region helper

        private MetadataStructureModel convertToMetadataStructureModel(MetadataStructure metadataStructure, MetadataStructureManager metadataStructureManager)
        {
            MetadataStructureModel metadataStructureModel = new MetadataStructureModel();
            metadataStructureModel.Id = metadataStructure.Id;
            metadataStructureModel.Name = metadataStructure.Name;

            try
            {
                metadataStructureModel.MetadataNodes = GetAllXPath(metadataStructure.Id);

                //get all informaions from xml
                metadataStructureModel.EntityClasses = GetEntityModelList();
                string EntityClassName = xmlDatasetHelper.GetEntityNameFromMetadatStructure(metadataStructure.Id, metadataStructureManager);

                var entityModel =
                    metadataStructureModel.EntityClasses.Where(e => e.Name.Equals(EntityClassName))
                        .FirstOrDefault();
                if (entityModel != null) metadataStructureModel.Entity = entityModel;

                string xpath = xmlDatasetHelper.GetInformationPath(metadataStructure.Id, NameAttributeValues.title);

                var searchMetadataNode =
                    metadataStructureModel.MetadataNodes.Where(e => e.XPath.Equals(xpath)).FirstOrDefault();
                if (searchMetadataNode != null)
                    metadataStructureModel.TitleNode =
                        searchMetadataNode.DisplayName;

                xpath = xmlDatasetHelper.GetInformationPath(metadataStructure.Id,
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

                metadataStructureModel.Active = xmlDatasetHelper.IsActive(metadataStructure.Id);
            }
            catch (Exception exception)
            {
                metadataStructureModel = new MetadataStructureModel();
                metadataStructureModel.Id = metadataStructure.Id;
                metadataStructureModel.Name = metadataStructure.Name;
            }

            return metadataStructureModel;
        }

        /// <summary>
        /// Get all simple attributes of a metadata Structure
        /// </summary>
        /// <param name="metadatastructureId"></param>
        /// <returns></returns>
        private List<SearchMetadataNode> GetAllXPath(long metadatastructureId)
        {
            //XmlMetadataHelper.GetAllXPathsOfSimpleAttributes(metadatastructureId);
            XmlMetadataHelper xmlMetadataHelper = new XmlMetadataHelper();

            return xmlMetadataHelper.GetAllXPathsOfSimpleAttributes(metadatastructureId);
        }

        // Improvement: [Sven] Vereinfachung der Abfrage, ggfs. muss alte Version wiederhergestellt werden, falls es nicht korrekt funktioniert.
        private List<EntityModel> GetEntityModelList()
        {
            using (EntityManager entityManager = new EntityManager())
            {
                return entityManager.Entities.Where(e => e.UseMetadata).ToList().Select(e =>
                          new EntityModel()
                          {
                              Name = e.Name,
                              ClassPath = e.EntityType.FullName
                          }
                      ).ToList();
            }
        }

        private MetadataStructure updateMetadataStructure(MetadataStructure metadataStructure,
                            MetadataStructureModel metadataStructureModel)
        {
            if (metadataStructure.Id.Equals(metadataStructureModel.Id))
            {
                metadataStructure.Name = metadataStructureModel.Name;
                XmlDocument xmlDocument = new XmlDocument();
                if (metadataStructure.Extra != null)
                {
                    if (metadataStructure.Extra as XmlDocument != null)
                        xmlDocument = metadataStructure.Extra as XmlDocument;
                    else
                    {
                        xmlDocument.AppendChild(metadataStructure.Extra);
                    }
                }
                else
                {
                    xmlDocument = new XmlDocument();
                }

                metadataStructureModel.MetadataNodes = GetAllXPath(metadataStructure.Id);

                //set title & description
                string titleXPath =
                    metadataStructureModel.MetadataNodes
                        .Where(e => e.DisplayName.Equals(metadataStructureModel.TitleNode))
                        .FirstOrDefault()
                        .XPath;

                XmlNode tmp = null;
                try
                {
                    tmp = XmlUtility.GetXmlNodeByAttribute(xmlDocument.DocumentElement,
                    nodeNames.nodeRef.ToString(), AttributeNames.name.ToString(),
                    NameAttributeValues.title.ToString());
                    tmp.Attributes[AttributeNames.value.ToString()].Value = titleXPath;
                }
                catch
                {
                    xmlDocument = xmlDatasetHelper.AddReferenceToXml(xmlDocument, NameAttributeValues.title.ToString(),
                        titleXPath, AttributeType.xpath.ToString(), "extra/nodeReferences/nodeRef");
                }

                string descriptionXPath =
                    metadataStructureModel.MetadataNodes
                        .Where(e => e.DisplayName.Equals(metadataStructureModel.DescriptionNode))
                        .FirstOrDefault()
                        .XPath;

                try
                {
                    tmp = XmlUtility.GetXmlNodeByAttribute(xmlDocument.DocumentElement, nodeNames.nodeRef.ToString(),
                    AttributeNames.name.ToString(), NameAttributeValues.description.ToString());
                    tmp.Attributes[AttributeNames.value.ToString()].Value = descriptionXPath;
                }
                catch
                {
                    xmlDocument = xmlDatasetHelper.AddReferenceToXml(xmlDocument, NameAttributeValues.description.ToString(),
                        descriptionXPath, AttributeType.xpath.ToString(), "extra/nodeReferences/nodeRef");
                }

                //set entity
                tmp = XmlUtility.GetXmlNodeByName(xmlDocument.DocumentElement, nodeNames.entity.ToString());
                if (tmp != null)
                {
                    tmp.Attributes[AttributeNames.value.ToString()].Value = metadataStructureModel.Entity.ClassPath;
                    tmp.Attributes[AttributeNames.name.ToString()].Value = metadataStructureModel.Entity.Name;
                }
                else
                {
                    xmlDocument = xmlDatasetHelper.AddReferenceToXml(xmlDocument, nodeNames.entity.ToString(),
                        metadataStructureModel.Entity.ClassPath, AttributeType.entity.ToString(), "extra/entity");
                }

                //set active
                tmp = XmlUtility.GetXmlNodeByAttribute(xmlDocument.DocumentElement, nodeNames.parameter.ToString(),
                    AttributeNames.name.ToString(), NameAttributeValues.active.ToString());
                if (tmp != null)
                    tmp.Attributes[AttributeNames.value.ToString()].Value = metadataStructureModel.Active.ToString();
                else
                {
                    xmlDocument = xmlDatasetHelper.AddReferenceToXml(xmlDocument,
                        NameAttributeValues.active.ToString(),
                        metadataStructureModel.Active.ToString(), AttributeType.parameter.ToString(),
                        "extra/parameters/parameter");
                }

                metadataStructure.Extra = xmlDocument;
            }
            return metadataStructure;
        }

        #endregion helper
    }
}