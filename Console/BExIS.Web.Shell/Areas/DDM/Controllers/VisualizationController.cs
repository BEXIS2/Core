using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;

using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Web.Extensions;

using System.IO;
using Vaiona.Web.Mvc;


namespace BExIS.Modules.Ddm.UI.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public class VisualizationController : Controller
    {
        // GET: Visualization
        public ActionResult Index()
        {
            //ViewBag.Title = PresentationModel.GetViewTitleForTenant("Maintain Datasets", Session.GetTenant());

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

        //public ActionResult vis()
        //{
            //ViewBag.Title = PresentationModel.GetViewTitleForTenant("Visualization", this.Session.GetTenant());

            //var visModel = new VisualizationModel();


            //visModel.title = "Diagram";

            //Dictionary<string, int> dic = new Dictionary<string, int>();
            //dic.Add("Venezuella", 100);
            //dic.Add("Iran", 200);
            //visModel.values = dic;

        //    return View();
        //}

    }
}

