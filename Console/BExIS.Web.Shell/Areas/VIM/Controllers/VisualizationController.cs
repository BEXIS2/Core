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
            List<string> helpCountCreated = new List<string>(); //It is later used to save date of creation of a dataset (or similar for other types). It helps to save dates to count them later.
            List<string> helpCountDeleted = new List<string>();

            DatasetManager dm = new DatasetManager();
            var entityPermissionManager = new EntityPermissionManager();

            List<Dataset> datasets = dm.DatasetRepo.Query().OrderBy(p => p.Id).ToList();
            List<long> datasetIds = datasets.Select(p => p.Id).ToList();            

            foreach(var id in datasetIds)
            {
                List<DatasetVersion> versions = dm.DatasetVersionRepo.Query(p => p.Dataset.Id == id).OrderBy(p => p.Id).ToList();                
                //extract the timestamp of version id = 1. This is the time of the creation of a dataset.
                var createTime = versions.First().Timestamp.Month + "/" + versions.First().Timestamp.Year;
                helpCountCreated.Add(createTime);
                //extract the timestamp of the last version. This is the time of the deletion of a dataset.
                var deleteTime = versions.Last().Timestamp.Month + "/" + versions.First().Timestamp.Year;
                helpCountDeleted.Add(deleteTime);
            }

            var createdItems = from tt in helpCountCreated
                                group tt by tt into g
                                let count = g.Count()
                                orderby count ascending
                                select new { Value = g.Key, Count = count };

            foreach (var t in createdItems)
            {
                createdDatasets.Add(t.Value, t.Count);
            }

            var deletedItems = from tt in helpCountDeleted
                                group tt by tt into g
                                let count = g.Count()
                                orderby count ascending
                                select new { Value = g.Key, Count = count };

            foreach (var t in deletedItems)
            {
                deletedDatasets.Add(t.Value, t.Count);
            }


            visModel.allDatasets = allDatasets;
            visModel.createdDatasets = createdDatasets;
            //visModel.checkedInDatasets = checkedInDatasets;
            //visModel.checkedOutDatasets = checkedOutDatasets;
            visModel.deletedDatasets = deletedDatasets;

            return View(visModel);
        }
    }
}