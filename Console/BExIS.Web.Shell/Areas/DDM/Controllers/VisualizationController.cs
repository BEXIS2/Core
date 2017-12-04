using BExIS.Modules.Ddm.UI.Models;
using BExIS.Modules.Sam.UI.Models;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;
using System.IO;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class VisualizationController : Controller
    {
        // GET: Visualization
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Visualization", this.Session.GetTenant());

            var visModel = new VisualizationModel();


            visModel.title = "Diagram";

            Dictionary<string, int> dic = new Dictionary<string, int>();
            dic.Add("a", 100);
            dic.Add("b", 200);
            visModel.values = dic;

            string[] xArray = { "a", "b" };
            int[] yArray = { 100, 200 };
            visModel.axisX = xArray;
            visModel.axisY = yArray;

            return View (visModel);
        }

    }
}

