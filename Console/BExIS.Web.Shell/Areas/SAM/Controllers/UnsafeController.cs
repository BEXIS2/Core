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
            List<Int64> ids = dm.DatasetRepo.Query().Select(p => p.Id).ToList();
            return View(ids);
        }


    }
}
