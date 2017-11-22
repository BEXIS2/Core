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

            #region read csv
            //var reader = new StreamReader(@"C:\test.csv");                        
            ////List<string> listA = new List<string>();
            ////List<string> listB = new List<string>();
            
            //var line = reader.ReadLine(); //first line: Title
            //visModel.title = line;
            //line = reader.ReadLine();  // second line: title of Y-Axis
            //visModel.yAxis = line;

            //while (!reader.EndOfStream)
            //{
            //    line = reader.ReadLine();
            //    var values = line.Split(';');

            //    visModel.values.Add(values[0], Convert.ToInt32(values[1]));
            //    //listA.Add(values[0]);
            //    //listB.Add(values[1]);
            //}
            #endregion

            //visModel.title = "Population from 1978 until 2017";
            //visModel.type = "Line Chart";

            //var entityManager = new EntityManager();

            //try
            //{                
            //    var entities = entityManager.Entities.Select(e => EntityTreeViewItemModel.Convert(e, e.Parent.Id)).ToList();

            //    foreach (var entity in entities)
            //    {
            //        entity.Children = entities.Where(e => e.ParentId == entity.Id).ToList();
            //    }

            //    visModel.entities = entities.AsEnumerable();
            //    return View(visModel);
            //}
            //finally
            //{
            //    entityManager.Dispose();
            //}

            return View (visModel);
        }

    }
}


//DatasetManager dm = new DatasetManager();
//EntityPermissionManager entityPermissionManager = new EntityPermissionManager();


//            try
//            {
//                DatasetVersion dsv;
//ShowDataModel model = new ShowDataModel();

//string title = "";
//long metadataStructureId = -1;
//long dataStructureId = -1;
//long researchPlanId = 1;
//XmlDocument metadata = new XmlDocument();

//                if (dm.IsDatasetCheckedIn(id))
//                {
//                    long versionId = dm.GetDatasetLatestVersionId(id); // check for zero value
//dsv = dm.DatasetVersionRepo.Get(versionId); // this is needed to allow dsv to access to an open session that is available via the repo

//                    metadataStructureId = dsv.Dataset.MetadataStructure.Id;

//                    //MetadataStructureManager msm = new MetadataStructureManager();
//                    //dsv.Dataset.MetadataStructure = msm.Repo.Get(dsv.Dataset.MetadataStructure.Id);

//                    title = xmlDatasetHelper.GetInformationFromVersion(dsv.Id, NameAttributeValues.title); // this function only needs metadata and extra fields, there is no need to pass the version to it.
//                    dataStructureId = dsv.Dataset.DataStructure.Id;
//                    researchPlanId = dsv.Dataset.ResearchPlan.Id;
//                    metadata = dsv.Metadata;

//                    ViewBag.Title = PresentationModel.GetViewTitleForTenant("Show Data : " + title, this.Session.GetTenant());
//}
//                else
//                {
//                    ModelState.AddModelError(string.Empty, "Dataset is just in processing.");
//                }

//                model = new ShowDataModel()
//{
//    Id = id,
//                    Title = title,
//                    MetadataStructureId = metadataStructureId,
//                    DataStructureId = dataStructureId,
//                    ResearchPlanId = researchPlanId,
//                    ViewAccess = entityPermissionManager.HasRight<User>(HttpContext.User.Identity.Name, "Dataset", typeof(Dataset), id, RightType.Read),
//                    GrantAccess = entityPermissionManager.HasRight<User>(HttpContext.User.Identity.Name, "Dataset", typeof(Dataset), id, RightType.Grant)
//                };

////set metadata in session
//Session["ShowDataMetadata"] = metadata;

//                return View(model);
//            }
//            finally
//            {
//                dm.Dispose();
//                entityPermissionManager.Dispose();
//            }
//        }