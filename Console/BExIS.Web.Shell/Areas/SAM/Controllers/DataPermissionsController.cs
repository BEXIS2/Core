using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Web.Shell.Areas.SAM.Models;
using Telerik.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;

namespace BExIS.Web.Shell.Areas.SAM.Controllers
{
    public class DataPermissionsController : Controller
    {
        //
        // GET: /Auth/DataPermissions/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Data()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Data Permissions", this.Session.GetTenant());

            return View(new EntitySelectListModel());
        }

        public ActionResult Entities_OnSelect(long entityId)
        {
            return PartialView("_DatasetsPartial");
        }

        [GridAction]
        public ActionResult Datasets_Select()
        {
            DatasetManager datasetManager = new DatasetManager();
            PermissionManager permissionManager = new PermissionManager();
            SubjectManager subjectManager = new SubjectManager();

            // DATA
            //var ids = datasetManager.GetDatasetLatestIds();
            List<DatasetVersion> data = datasetManager.GetDatasetLatestVersions(); // .GetDatasetLatestVersions(ids);

            List<DatasetGridRowModel> datasets = new List<DatasetGridRowModel>();
            data.ForEach(d => datasets.Add(DatasetGridRowModel.Convert(d, permissionManager.ExistsDataPermission(subjectManager.GetGroupByName("everyone").Id, 1, d.Id, RightType.View))));

            return View(new GridModel<DatasetGridRowModel> { Data = datasets });
        }

        public ActionResult Subjects(long dataId, long entityId)
        {
            ViewData["DataId"] = dataId;
            ViewData["EntityId"] = entityId;

            return PartialView("_SubjectsPartial");
        }

        [GridAction]
        public ActionResult Subjects_Select(long dataId, long entityId)
        {
            EntityManager entityManager = new EntityManager();
            PermissionManager permissionManager = new PermissionManager();
            SubjectManager subjectManager = new SubjectManager();

            List<DataPermissionGridRowModel> subjects = new List<DataPermissionGridRowModel>();

            IQueryable<Subject> data = subjectManager.GetAllSubjects();
            data.ToList().ForEach(s => subjects.Add(DataPermissionGridRowModel.Convert(dataId, entityManager.GetEntityById(entityId), s, permissionManager.GetAllRights(s.Id, entityId, dataId).ToList())));

            return View(new GridModel<DataPermissionGridRowModel> { Data = subjects });
        }

        public DataPermission CreateDataPermission(long subjectId, long entityId, long dataId, int rightType)
        {
            PermissionManager permissionManager = new PermissionManager();

            return permissionManager.CreateDataPermission(subjectId, entityId, dataId, (RightType)rightType);
        }

        public bool DeleteDataPermission(long subjectId, long entityId, long dataId, int rightType)
        {
            PermissionManager permissionManager = new PermissionManager();

            permissionManager.DeleteDataPermission(subjectId, entityId, dataId, (RightType)rightType);

            return true;
        }

        public void PublishDataset(long entityId, long datasetId)
        {
            PermissionManager permissionManager = new PermissionManager();
            SubjectManager subjectManager = new SubjectManager();

            Group group = subjectManager.GetGroupByName("everyone");

            permissionManager.CreateDataPermission(group.Id, entityId, datasetId, RightType.View);
        }

        public void ConcealDataset(long entityId, long datasetId)
        {
            PermissionManager permissionManager = new PermissionManager();
            SubjectManager subjectManager = new SubjectManager();

            Group group = subjectManager.GetGroupByName("everyone");

            permissionManager.DeleteDataPermission(group.Id, entityId, datasetId, RightType.View);
        }
    }
}