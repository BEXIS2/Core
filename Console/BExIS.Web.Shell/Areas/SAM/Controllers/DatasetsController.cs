using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Sam.UI.Controllers
{
    /// <summary>
    /// Manages all funactions an authorized user can do with datasets and their versions
    /// </summary>
    public class DatasetsController : Controller
    {
        public ActionResult Checkin(int id)
        {
            return View();
        }

        public ActionResult Checkout(int id)
        {
            return View();
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
                    entityPermissionManager.Delete(typeof(Dataset), id);

                    // ToDo: refactor the indexing after "delete dataset" process
                    //ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;
                    //provider?.UpdateSingleDatasetIndex(id, IndexingAction.DELETE);
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

            List<Dataset> datasets = dm.DatasetRepo.Query().OrderBy(p => p.Id).ToList();

            List<long> datasetIds = new List<long>();
            if (HttpContext.User.Identity.Name != null)
            {
                datasetIds.AddRange(entityPermissionManager.GetKeys<User>(HttpContext.User.Identity.Name, "Dataset", typeof(Dataset), RightType.Delete));
            }

            ViewData["DatasetIds"] = datasetIds;
            return View(datasets);
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

                    // ToDo: refactor the indexing after "delete dataset" process
                    //ISearchProvider provider = IoCFactory.Container.ResolveForSession<ISearchProvider>() as ISearchProvider;
                    //provider?.UpdateSingleDatasetIndex(id, IndexingAction.DELETE);
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
    }
}