using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.MetadataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.MetadataStructure;
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
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class AdminController : Controller
    {

        private List<long> datasetVersionIds = new List<long>();
        private XmlMapperManager xmlMapperManager = new XmlMapperManager(TransactionDirection.InternToExtern);

        //
        // GET: /DIM/Admin/

        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Export Metadata", this.Session.GetTenant());

            AdminModel model = new AdminModel();

            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            IList<MetadataStructure> metadataStructures = metadataStructureManager.Repo.Get();

            foreach (MetadataStructure metadataStructure in metadataStructures)
            {
                model.Add(metadataStructure);
            }

            return View(model);
        }

        public ActionResult LoadMetadataStructureTab(long Id)
        {
            #region load Model

            DatasetManager datasetManager = new DatasetManager();
            // retrieves all the dataset version IDs which are in the checked-in state
            datasetVersionIds = datasetManager.GetDatasetVersionLatestIds();

            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            MetadataStructure metadataStructure = metadataStructureManager.Repo.Get(Id);

            MetadataStructureModel model = new MetadataStructureModel(
                    metadataStructure.Id,
                    metadataStructure.Name,
                    metadataStructure.Description,
                    getDatasetVersionsDic(metadataStructure, datasetVersionIds),
                     IsExportAvailable(metadataStructure)

                );

            #endregion

            return PartialView("_metadataStructureView", model);
        }

        public ActionResult ConvertSelectedDatasetVersion(string Id, string SelectedDatasetIds)
        {

            #region load Model

            DatasetManager datasetManager = new DatasetManager();
            datasetVersionIds = datasetManager.GetDatasetVersionLatestIds();

            MetadataStructureManager metadataStructureManager = new MetadataStructureManager();
            MetadataStructure metadataStructure = metadataStructureManager.Repo.Get(Convert.ToInt64(Id));

            MetadataStructureModel model = new MetadataStructureModel(
                    metadataStructure.Id,
                    metadataStructure.Name,
                    metadataStructure.Description,
                    getDatasetVersionsDic(metadataStructure, datasetVersionIds),
                    IsExportAvailable(metadataStructure)

                );

            #endregion

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

            #endregion

            return PartialView("_metadataStructureView", model);
        }

        public ActionResult Download(string path)
        {
            return File(path, "text/xml");
        }

        private string Export(long datasetVersionId)
        {
            DatasetManager datasetManager = new DatasetManager();
            DatasetVersion datasetVersion = datasetManager.GetDatasetVersion(datasetVersionId);

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

            return "";
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

            // gets all the dataset versions that their Id is in the datasetVersionIds and they are using a specific metadata structure as indicated by metadataStructure parameter
            var q = datasetManager.DatasetVersionRepo.Get(p => datasetVersionIds.Contains(p.Id) &&
                                                          p.Dataset.MetadataStructure.Id.Equals(metadataStructure.Id)).Distinct();


            foreach (DatasetVersion datasetVersion in q)
            {
                if (datasetManager.IsDatasetCheckedIn(datasetVersion.Dataset.Id))
                {
                    datasetVersions.Add(
                        new DatasetVersionModel
                        {
                            DatasetVersionId = datasetVersion.Id,
                            DatasetId = datasetVersion.Dataset.Id,
                            Title = XmlDatasetHelper.GetInformation(datasetVersion, NameAttributeValues.title),
                            MetadataDownloadPath = OutputMetadataManager.GetMetadataPath(datasetVersion.Id)
                        });
                }
            }
            return datasetVersions;
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



        #endregion


    }
}
