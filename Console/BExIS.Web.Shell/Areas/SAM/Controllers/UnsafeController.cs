using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Web.Shell.Areas.SAM.Controllers
{
    public class UnsafeController : Controller
    {
        //
        // GET: /SAM/Unsafe/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult PurgeDataset(long id)
        {
            DatasetManager dm = new DatasetManager();
            bool b = dm.PurgeDataset(id);
            return View();
        }

        public ActionResult ListDatasets()
        {
            DatasetManager dm = new DatasetManager();
            List<Dataset> datasets = dm.DatasetRepo.Query().ToList();
            return View(datasets);
        }

        public ActionResult ListDatasetVersions(int id)
        {
            DatasetManager dm = new DatasetManager();
            List<DatasetVersion> versions = dm.DatasetVersionRepo.Query(p=>p.Dataset.Id == id).OrderBy(p=>p.Id).ToList();
            ViewBag.VersionId = id;
            return View(versions);
        }

        public ActionResult DetailDatasetVersion(int id)
        {
            DatasetManager dm = new DatasetManager();
            DatasetVersion version = dm.DatasetVersionRepo.Get(p => p.Id == id).First();
            var tuples = dm.GetDatasetVersionEffectiveTuples(version);
            ViewBag.VersionId = id;
            ViewBag.DatasetId = version.Dataset.Id;
            ViewBag.Variables = ((StructuredDataStructure)version.Dataset.DataStructure.Self).Variables.ToList();
            return View(tuples);
        }
    }
}
