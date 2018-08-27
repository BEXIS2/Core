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
            Dictionary<string, int> createdDatasets = new Dictionary<string, int>();
            //Dictionary<string, int> checkedInDatasets = new Dictionary<string, int>();
            //Dictionary<string, int> checkedOutDatasets = new Dictionary<string, int>();
            Dictionary<string, int> deletedDatasets = new Dictionary<string, int>();
            List<string> helpCount = new List<string>(); //It is later used to save date of creation of a dataset (or similar for other types). It helps to save dates to count them later.

            DatasetManager dm = new DatasetManager();
            var entityPermissionManager = new EntityPermissionManager();

            List<Dataset> datasets = dm.DatasetRepo.Query().OrderBy(p => p.Id).ToList();
            List<long> datasetIds = datasets.Select(p => p.Id).ToList();
            
            ///Find deleted datasets
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
                        //if (statusofGroup == "CheckedIn")
                        //{ checkedInDatasets.Add(dateOfGroup, countGroup); }
                        //if (statusofGroup == "CheckedOut")
                        //{ checkedOutDatasets.Add(dateOfGroup, countGroup); }
                        //if (statusofGroup == "Deleted")
                        { deletedDatasets.Add(dateOfGroup, countGroup); }
                    }

                }
            }

            ///Find Created Datasets
            foreach(var id in datasetIds)
            {
                List<DatasetVersion> versions = dm.DatasetVersionRepo.Query(p => p.Dataset.Id == id).OrderBy(p => p.Id).ToList();                
                //extract the timestamp of version id = 1. This is the time of the creation of a dataset.
                var createTime = versions.First().Timestamp.Month + "/" + versions.First().Timestamp.Year;
                helpCount.Add(createTime);
            }

            var items = from tt in helpCount
                        group tt by tt into g
                        let count = g.Count()
                        orderby count descending
                        select new { Value = g.Key, Count = count };
            
            foreach (var t in items)
            {
                createdDatasets.Add(t.Value, t.Count);
            }

            
            visModel.allDatasets = allDatasets;
            //visModel.checkedInDatasets = checkedInDatasets;
            //visModel.checkedOutDatasets = checkedOutDatasets;
            visModel.deletedDatasets = deletedDatasets;

            return View(visModel);
        }
    }
}