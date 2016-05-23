using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Web.Shell.Areas.Sam.Controllers
{
    /// <summary>
    /// Manages all funactions an authorized user can do with datasets and their versions
    /// </summary>
    public class DatasetController : Controller
    {
        /// <summary>
        /// Shows a berif intro about the functions available as well as some warnings that inofrom the user about non recoverability of some of the operations
        /// such as purge.
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitle("Maintain Datasets");
            return View();
        }

        /// <summary>
        /// Lists all exisiting datasets alongside with their current status
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            ViewBag.Title = PresentationModel.GetViewTitle("Maintain Datasets");

            DatasetManager dm = new DatasetManager();
            PermissionManager permissionManager = new PermissionManager();
            SubjectManager subjectManager = new SubjectManager();

            User user = subjectManager.GetUserByName(HttpContext.User.Identity.Name);

            List<Dataset> datasets = dm.DatasetRepo.Query().ToList();

            List<long> datasetIds = new List<long>();
            if (user != null)
            {
                datasetIds.AddRange(permissionManager.GetAllDataIds(user.Id, 1, RightType.Delete));
            }

            ViewData["DatasetIds"] = datasetIds;

            return View(datasets);
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
            DatasetManager dm = new DatasetManager();
            PermissionManager pm = new PermissionManager();

            pm.DeleteDataPermissionsByEntity(1, id);
            bool b = dm.DeleteDataset(id, this.ControllerContext.HttpContext.User.Identity.Name, true);
            return RedirectToAction("List");
        }

        /// <summary>
        /// Purges a dataset, which means the dataset and all its versions will be physically removed from the database.
        /// </summary>
        /// <param name="id">the identifier of the dataset to be purged.</param>
        /// <remarks>This operation is not revocerable.</remarks>
        /// <returns></returns>
        public ActionResult Purge(long id)
        {
            ViewBag.Title = PresentationModel.GetViewTitle("Purge");

            DatasetManager dm = new DatasetManager();
            PermissionManager pm = new PermissionManager();

            try
            {
                if (dm.PurgeDataset(id))
                {
                    pm.DeleteDataPermissionsByEntity(1, id);
                }
            }
            catch (Exception e)
            {
                
            }
            
            return View();
        }

        /// <summary>
        /// Having the identifier of a dataset, lists all its versions.
        /// </summary>
        /// <param name="id">the identifier of the dataset.</param>
        /// <returns></returns>
        public ActionResult Versions(int id)
        {
            ViewBag.Title = PresentationModel.GetViewTitle("Versions");
            DatasetManager dm = new DatasetManager();
            List<DatasetVersion> versions = dm.DatasetVersionRepo.Query(p => p.Dataset.Id == id).OrderBy(p => p.Id).ToList();
            ViewBag.VersionId = id;
            return View(versions);
        }

        /// <summary>
        /// Shows the content of a specific dataset version
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Version(int id)
        {
            ViewBag.Title = PresentationModel.GetViewTitle("Version");

            DatasetManager dm = new DatasetManager();
            DatasetVersion version = dm.DatasetVersionRepo.Get(p => p.Id == id).First();
            var tuples = dm.GetDatasetVersionEffectiveTuples(version);
            ViewBag.VersionId = id;
            ViewBag.DatasetId = version.Dataset.Id;

            if (version.Dataset.DataStructure is StructuredDataStructure)
            {
                ViewBag.Variables = ((StructuredDataStructure) version.Dataset.DataStructure.Self).Variables.ToList();
            }
            else
            {
                ViewBag.Variables = new List<Variable>();
            }


            return View(tuples);
        }

        public ActionResult Checkout(int id)
        {
            return View();
        }

        public ActionResult Checkin(int id)
        {
            return View();
        }

        public ActionResult Rollback(int id)
        {
            return View();
        }
    }
}