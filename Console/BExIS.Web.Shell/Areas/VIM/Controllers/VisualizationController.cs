using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Modules.Vim.UI.Models;
using BExIS.Security.Services.Authorization;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace BExIS.Modules.Vim.UI.Controllers
{
    public class VisualizationController : Controller
    {
        // GET: Visualization
        public ActionResult Index()
        {
            //ViewBag.Title = PresentationModel.GetViewTitleForTenant("Maintain Datasets", Session.GetTenant());
            VisualizationModels visModel = new VisualizationModels();
            Dictionary<string, int> allActivities = new Dictionary<string, int>();
            Dictionary<string, int> createdDatasets = new Dictionary<string, int>();
            Dictionary<string, int> deletedDatasets = new Dictionary<string, int>();
            List<string> helpCountCreated = new List<string>(); //It is later used to save date of creation of a dataset (or similar for other types). It helps to save dates to count them later.
            List<string> helpCountDeleted = new List<string>();
            List<string> helpCountActivity = new List<string>();

            //--------

            using (DatasetManager dm = new DatasetManager())
            using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
            {
                List<Dataset> datasets = dm.DatasetRepo.Query().OrderBy(p => p.Id).ToList();
                List<long> datasetIds = datasets.Select(p => p.Id).ToList();

                foreach (var id in datasetIds)
                {
                    Dataset dataset = dm.GetDataset(id);

                    List<DatasetVersion> versions = dm.DatasetVersionRepo.Query(p => p.Dataset.Id == id).OrderBy(p => p.Id).ToList();

                    List<long> datasetVersionId = versions.Select(p => p.Id).ToList();

                    //extract the timestamp of version id = 1. This is the time of the creation of a dataset.
                    var createTime = versions.First().Timestamp.Month + "/" + versions.First().Timestamp.Year;
                    helpCountCreated.Add(createTime);

                    //extract the timestamp of the last version. This is the time of the deletion of a dataset.
                    // if(dataset.Status == DatasetStatus.Deleted) -- BY DAVID
                    if (dataset.Status == DatasetStatus.Deleted)
                    {
                        var deleteTime = versions.Last().Timestamp.Month + "/" + versions.Last().Timestamp.Year;
                        helpCountDeleted.Add(deleteTime);
                    }

                    foreach (var version in versions)
                    {
                        var activityTime = version.Timestamp.Month + "/" + version.Timestamp.Year;
                        helpCountActivity.Add(activityTime);
                    }
                }

                ///Create the list of created datasets
                var createdItems = from tt in helpCountCreated
                                   group tt by tt into g
                                   let count = g.Count()
                                   orderby count ascending
                                   select new { Value = g.Key, Count = count };

                foreach (var t in createdItems)
                {
                    createdDatasets.Add(t.Value, t.Count);
                }

                //createdDatasets.OrderBy(order => order.Value).ToList();

                ///Create the list of deleted datasets
                var deletedItems = from tt in helpCountDeleted
                                   group tt by tt into g
                                   let count = g.Count()
                                   orderby count ascending
                                   select new { Value = g.Key, Count = count };

                foreach (var t in deletedItems)
                {
                    deletedDatasets.Add(t.Value, t.Count);
                }

                ///Create the list of all activities includes create, update and delete
                var activityItems = from tt in helpCountActivity
                                    group tt by tt into g
                                    let count = g.Count()
                                    orderby count ascending
                                    select new { Value = g.Key, Count = count };

                foreach (var t in activityItems)
                {
                    allActivities.Add(t.Value, t.Count);
                }

                visModel.allActivities = allActivities;
                visModel.allDatasets = allActivities;
                visModel.createdDatasets = createdDatasets;
                visModel.deletedDatasets = deletedDatasets;

                return View(visModel);
            }
        }
    }
}