using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Dim.UI.Models;
using BExIS.Xml.Helpers;
using BExIS.Xml.Helpers.Mapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using System.Xml;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class AdminController : BaseController
    {
        private List<long> datasetVersionIds = new List<long>();
        private XmlMapperManager xmlMapperManager = new XmlMapperManager(TransactionDirection.InternToExtern);
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        //
        // GET: /DIM/Admin/

        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Export Metadata", this.Session.GetTenant());

            AdminModel model = new AdminModel();

            IList<MetadataStructure> metadataStructures = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get();

            foreach (MetadataStructure metadataStructure in metadataStructures)
            {
                model.Add(metadataStructure.Id, metadataStructure.Name);
            }

            return View(model);
        }

        public ActionResult LoadMetadataStructureTab(long Id)
        {
            #region load Model

            DatasetManager datasetManager = new DatasetManager();
            try
            {
                // retrieves all the dataset version IDs which are in the checked-in state
                datasetVersionIds = datasetManager.GetDatasetVersionLatestIds();

                MetadataStructure metadataStructure = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(Id);

                MetadataStructureModel model = new MetadataStructureModel(
                        metadataStructure.Id,
                        metadataStructure.Name,
                        metadataStructure.Description,
                        getDatasetVersionsDic(metadataStructure, datasetVersionIds),
                         IsExportAvailable(metadataStructure)
                    );

                #endregion load Model

                return PartialView("_metadataStructureView", model);
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        public ActionResult ConvertSelectedDatasetVersion(string Id, string SelectedDatasetIds)
        {
            #region load Model

            DatasetManager datasetManager = new DatasetManager();

            try
            {
                datasetVersionIds = datasetManager.GetDatasetVersionLatestIds();

                MetadataStructure metadataStructure = this.GetUnitOfWork().GetReadOnlyRepository<MetadataStructure>().Get(Convert.ToInt64(Id));

                MetadataStructureModel model = new MetadataStructureModel(
                        metadataStructure.Id,
                        metadataStructure.Name,
                        metadataStructure.Description,
                        getDatasetVersionsDic(metadataStructure, datasetVersionIds),
                        IsExportAvailable(metadataStructure)

                    );

                #endregion load Model

                #region convert

                if (SelectedDatasetIds != null && SelectedDatasetIds != "")
                {
                    string[] ids = SelectedDatasetIds.Split(',');

                    foreach (string id in ids)
                    {
                        string path = Export(Convert.ToInt64(id));
                        model.AddMetadataPath(Convert.ToInt64(id), path);
                    }
                }

                #endregion convert

                return PartialView("_metadataStructureView", model);
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        public ActionResult Download(string path)
        {
            return File(path, "text/xml");
        }

        private string Export(long datasetVersionId)
        {
            DatasetVersion datasetVersion = this.GetUnitOfWork().GetReadOnlyRepository<DatasetVersion>().Get(datasetVersionId);

            string path_mapping_file = "";
            try
            {
                string path = OutputMetadataManager.CreateConvertedMetadata(datasetVersion.Dataset.Id,
                    TransmissionType.mappingFileExport);

                path = Path.Combine(AppConfiguration.DataPath, path);

                return path;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        #region helper

        public string GetUsernameOrDefault()
        {
            string username = string.Empty;
            try
            {
                username = HttpContext.User.Identity.Name;
            }
            catch { }

            return !string.IsNullOrWhiteSpace(username) ? username : "DEFAULT";
        }

        private List<DatasetVersionModel> getDatasetVersionsDic(MetadataStructure metadataStructure, List<long> datasetVersionIds)
        {
            List<DatasetVersionModel> datasetVersions = new List<DatasetVersionModel>();
            DatasetManager datasetManager = new DatasetManager();
            try
            {
                using (var uow = this.GetUnitOfWork())
                {
                    // gets all the dataset versions that their Id is in the datasetVersionIds and they are using a specific metadata structure as indicated by metadataStructure parameter
                    var q = uow.GetReadOnlyRepository<DatasetVersion>().Get(p => datasetVersionIds.Contains(p.Id) &&
                                                                  p.Dataset.MetadataStructure.Id.Equals(metadataStructure.Id)).Distinct();

                    foreach (DatasetVersion datasetVersion in q)
                    {
                        if (datasetManager.IsDatasetCheckedIn(datasetVersion.Dataset.Id))
                        {
                            uow.GetReadOnlyRepository<DatasetVersion>().Load(datasetVersion.ContentDescriptors);
                            datasetVersions.Add(
                            new DatasetVersionModel
                            {
                                DatasetVersionId = datasetVersion.Id,
                                DatasetId = datasetVersion.Dataset.Id,
                                Title = datasetVersion.Title,
                                MetadataDownloadPath = OutputMetadataManager.GetMetadataPath(datasetVersion.ContentDescriptors)
                            });
                        }
                    }
                    return datasetVersions;
                }
            }
            finally
            {
                datasetManager.Dispose();
            }
        }

        private bool IsExportAvailable(MetadataStructure metadataStructure)
        {
            bool hasMappingFile = false;
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(metadataStructure.Extra.OuterXml);

            if (XmlUtility.GetXElementByNodeName("convertRef", XmlUtility.ToXDocument(doc)).Count() > 0)
            {
                hasMappingFile = true;
            }

            return hasMappingFile;
        }

        #endregion helper
    }
}