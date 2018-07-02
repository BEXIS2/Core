using BExIS.Dlm.Services.Data;
using BExIS.Security.Services.Authorization;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using BExIS.Modules.Vim.UI.Models;
using BExIS.Dlm.Entities.Data;

namespace BExIS.Modules.Vim.UI.Controllers
{
    public class VisualizationController : Controller
    {
        // GET: Visualization
        public ActionResult Index()
        {
            //ViewBag.Title = PresentationModel.GetViewTitleForTenant("Maintain Datasets", Session.GetTenant());
            VisualizationModels visModel = new VisualizationModels();
            Dictionary<string, int> allDatasets = new Dictionary<string, int>();
            Dictionary<string, int> checkedInDatasets = new Dictionary<string, int>();
            Dictionary<string, int> checkedOutDatasets = new Dictionary<string, int>();
            Dictionary<string, int> deletedDatasets = new Dictionary<string, int>();

            DatasetManager dm = new DatasetManager();
            var entityPermissionManager = new EntityPermissionManager();

            List<Dataset> datasets = dm.DatasetRepo.Query().OrderBy(p => p.Id).ToList();

            var groupsByYear = datasets.GroupBy(p => p.LastCheckIOTimestamp.Year);
            foreach (var year in groupsByYear)
            {
                var groupsByMonth = year.GroupBy(p => p.LastCheckIOTimestamp.Month);

                foreach (var month in groupsByMonth)
                {
                    var countGroup = month.Count();
                    var monthOfGroup = month.First().LastCheckIOTimestamp.Month;
                    var yearOfGroup = month.First().LastCheckIOTimestamp.Year;
                    string dateOfGroup = monthOfGroup + "/" + yearOfGroup;
                    allDatasets.Add(dateOfGroup, countGroup);

                    var groupsByStatus = month.GroupBy(p => p.Status);
                    foreach (var status in groupsByStatus)
                    {
                        countGroup = status.Count();
                        string statusofGroup = status.First().Status.ToString();
                        if (statusofGroup == "CheckedIn")
                        { checkedInDatasets.Add(dateOfGroup, countGroup); }
                        if (statusofGroup == "CheckedOut")
                        { checkedOutDatasets.Add(dateOfGroup, countGroup); }
                        if (statusofGroup == "Deleted")
                        { deletedDatasets.Add(dateOfGroup, countGroup); }
                    }

                }
            }

            visModel.allDatasets = allDatasets;
            visModel.checkedInDatasets = checkedInDatasets;
            visModel.checkedOutDatasets = checkedOutDatasets;
            visModel.deletedDatasets = deletedDatasets;

            return View(visModel);
        }
    }
}