using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Services.Authorization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Sam.UI.Controllers
{


    /// <summary>
    /// Manages all funactions an authorized user can do with datasets and their versions
    /// </summary>
    public class DatasetsController : BaseController
    {
        public ActionResult Checkin(int id)
        {
            return View();
        }

        public ActionResult Checkout(int id)
        {
            return View();
        }

        public ActionResult SyncAll()
        {
            var datasetManager = new DatasetManager();
            var datasetIds = datasetManager.GetDatasetLatestIds();
            try
            {
                datasetManager.SyncView(datasetIds, ViewCreationBehavior.Create | ViewCreationBehavior.Refresh);
                // if the viewData has a model error, the redirect forgets about it.
                return RedirectToAction("Index", new { area = "Sam" });
            }
            catch (Exception ex)
            {
                ViewData.ModelState.AddModelError("", $@"'{ex.Message}'");
                return View("Sync");
            }
        }

        public ActionResult Sync(long id)
        {
            var datasetManager = new DatasetManager();

            try
            {
                datasetManager.SyncView(id, ViewCreationBehavior.Create | ViewCreationBehavior.Refresh);
                // if the viewData has a model error, the redirect forgets about it.
                return RedirectToAction("Index", new { area = "Sam" });
            }
            catch (Exception ex)
            {
                ViewData.ModelState.AddModelError("", $@"'{ex.Message}'");
                return View();
            }
        }

        /// <summary>
        /// Deletes a dataset, which means the dataset is marked as deleted, but is not physically removed from the database.
        /// </summary>
        /// <param name="id">the identifier of the dataset to be purged.</param>
        /// <remarks>When a dataset is deleted, it is consodered as non-exisiting, but for the sake or provenance, citation, history, etc, it is not removed froom the database.
        /// The function to recover a deleted dataset, will not be provided.</remarks>
        /// <returns></returns>
        public ActionResult Delete(long id)
        {
            var datasetManager = new DatasetManager();
            var entityPermissionManager = new EntityPermissionManager();

            try
            {
                if (datasetManager.DeleteDataset(id, ControllerContext.HttpContext.User.Identity.Name, true))
                {
                    //entityPermissionManager.Delete(typeof(Dataset), id); // This is not needed here.

                    if (this.IsAccessible("DDM", "SearchIndex", "ReIndexUpdateSingle"))
                    {
                        var x = this.Run("DDM", "SearchIndex", "ReIndexUpdateSingle", new RouteValueDictionary() { { "id", id }, { "actionType", "DELETE" } });
                    }
                }
            }
            catch (Exception e)
            {
                ViewData.ModelState.AddModelError("", $@"Dataset {id} could not be deleted.");
            }
            return View();
            //return RedirectToAction("List");
        }

        /// <summary>
        /// Shows a berif intro about the functions available as well as some warnings that inofrom the user about non recoverability of some of the operations
        /// such as purge.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Maintain Datasets", Session.GetTenant());

            DatasetManager dm = new DatasetManager();
            var entityPermissionManager = new EntityPermissionManager();

            List<Dataset> datasets = new List<Dataset>();
            List<long> datasetIds = new List<long>();
            //if (!string.IsNullOrWhiteSpace(HttpContext.User.Identity.Name))
            //{
            //    datasetIds.AddRange(entityPermissionManager.GetKeys(HttpContext.User.Identity.Name, "Dataset", typeof(Dataset), RightType.Delete));
            //    if(datasetIds.Count() > 0)
            //        datasets = dm.DatasetRepo.Query().OrderBy(p => p.Id).ToList();
            //}

            datasets = dm.DatasetRepo.Query().OrderBy(p => p.Id).ToList();
            datasetIds = datasets.Select(p => p.Id).ToList();

            // dataset id, dataset status, number of data tuples of the latest version, number of variables in the dataset's structure
            List<DatasetStatModel> datasetStat = new List<DatasetStatModel>();
            foreach (Dataset ds in datasets)
            {
                long noColumns = ds.DataStructure.Self is StructuredDataStructure ? (ds.DataStructure.Self as StructuredDataStructure).Variables.Count() : 0L;
                long noRows = ds.DataStructure.Self is StructuredDataStructure ? dm.GetDatasetLatestVersionEffectiveTupleCount(ds) : 0; // It would save time to calc the row count for all the datasets at once!
                bool synced = false;
                if (string.Compare(ds.StateInfo?.State, "Synced", true) == 0
                        && ds.StateInfo?.Timestamp != null
                        && ds.StateInfo?.Timestamp > DateTime.MinValue
                        && ds.StateInfo?.Timestamp < DateTime.MaxValue)
                    synced = ds.StateInfo?.Timestamp >= ds.LastCheckIOTimestamp;
                datasetStat.Add(new DatasetStatModel { Id = ds.Id, Status = ds.Status, NoOfRows = noRows, NoOfCols = noColumns, IsSynced = synced });
            }
            ViewData["DatasetIds"] = datasetIds;
            return View(datasetStat);
        }

        /// <summary>
        /// Purges a dataset, which means the dataset and all its versions will be physically removed from the database.
        /// </summary>
        /// <param name="id">the identifier of the dataset to be purged.</param>
        /// <remarks>This operation is not revocerable.</remarks>
        /// <returns></returns>
        public ActionResult Purge(long id)
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Purge", Session.GetTenant());

            DatasetManager dm = new DatasetManager();
            var entityPermissionManager = new EntityPermissionManager();

            try
            {
                if (dm.PurgeDataset(id))
                {
                    entityPermissionManager.Delete(typeof(Dataset), id);

                    if (this.IsAccessible("DDM", "SearchIndex", "ReIndexUpdateSingle"))
                    {
                        var x = this.Run("DDM", "SearchIndex", "ReIndexUpdateSingle", new RouteValueDictionary() { { "id", id }, { "actionType", "DELETE" } });
                    }
                }
            }
            catch (Exception e)
            {
                ViewData.ModelState.AddModelError("", string.Format("Dataset {0} could not be purged.", id));
            }
            return View();
        }

        public ActionResult Rollback(int id)
        {
            return View();
        }

        /// <summary>
        /// Shows the content of a specific dataset version
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Version(int id)
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Version", Session.GetTenant());

            DatasetManager dm = new DatasetManager();
            DatasetVersion version = dm.DatasetVersionRepo.Get(p => p.Id == id).First();
            var tuples = dm.GetDatasetVersionEffectiveTuples(version);
            ViewBag.VersionId = id;
            ViewBag.DatasetId = version.Dataset.Id;

            if (version.Dataset.DataStructure is StructuredDataStructure)
            {
                ViewBag.Variables = ((StructuredDataStructure)version.Dataset.DataStructure.Self).Variables.ToList();
            }
            else
            {
                ViewBag.Variables = new List<Variable>();
            }

            return View(tuples);
        }

        /// <summary>
        /// Having the identifier of a dataset, lists all its versions.
        /// </summary>
        /// <param name="id">the identifier of the dataset.</param>
        /// <returns></returns>
        public ActionResult Versions(int id)
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Versions", Session.GetTenant());
            DatasetManager dm = new DatasetManager();
            List<DatasetVersion> versions = dm.DatasetVersionRepo.Query(p => p.Dataset.Id == id).OrderBy(p => p.Id).ToList();
            ViewBag.VersionId = id;
            return View(versions);
        }

        public ActionResult FlipDateTime(long id, long variableid)
        {
            DatasetManager datasetManager = new DatasetManager();
            
            try
            {
                DatasetVersion dsv = datasetManager.GetDatasetLatestVersion(id);
                IEnumerable<long> datatupleIds = datasetManager.GetDatasetVersionEffectiveTupleIds(dsv);

                foreach (var tid in datatupleIds)
                {
                    DataTuple dataTuple = datasetManager.DataTupleRepo.Get(tid);
                    dataTuple.Materialize();
                    bool needUpdate = false;

                    foreach (var vv in dataTuple.VariableValues)
                    {
                        string systemType = vv.DataAttribute.DataType.SystemType;
                        if (systemType.Equals(typeof(DateTime).Name) && vv.VariableId.Equals(variableid))
                        {
                            string value = vv.Value.ToString();
                            vv.Value = flip(value,out needUpdate);


                        }
                    }

                    if (needUpdate)
                    {
                        dataTuple.Dematerialize();
                        datasetManager.UpdateDataTuple(dataTuple);
                    }

                }

            }
            catch (Exception ex)
            {

            }
            finally
            {
                datasetManager.Dispose();
            }

            return RedirectToAction("Index");
        }


        private string flip(string dateTime, out bool needUpdate)
        {
            string newDt = "";

            DateTime dt;

            if(DateTime.TryParse(dateTime, new CultureInfo("en-us"),DateTimeStyles.NoCurrentDateDefault,out dt))
            {
                int day = dt.Day;
                int month = dt.Month;

                if (day < 13)
                {
                    needUpdate = true;
                    //1/1/2017 12:00:00 AM
                    return day + "/" + month + "/" + dt.Year + " " + dt.TimeOfDay;

                }
            }

            needUpdate = false;

            return dateTime;
        }
        
    }
}