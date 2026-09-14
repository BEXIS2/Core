using BExIS.App.Bootstrap.Attributes;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Ddm.UI.Models;
using BExIS.Modules.DDM.UI.Models;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Requests;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Requests;
using BExIS.Security.Services.Subjects;
using BExIS.UI.Helpers;
using BExIS.Utils.Models;
using BExIS.Xml.Helpers;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using Vaiona.Persistence.Api;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class DashboardController : Controller
    {
        private readonly UserManager<User, long> _userManager;
        private XmlDatasetHelper xmlDatasetHelper = new XmlDatasetHelper();

        public DashboardController(UserManager<User, long> userManager)
        {
            _userManager = userManager;
        }

        public ActionResult Index()
        {
            string module = "DDM";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            // load settings
            var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
            ViewData["use_tags"] = moduleSettings.GetValueByKey("use_tags");

            return View();
        }

        #region Dashboard API

        [JsonNetFilter]
        [System.Web.Mvc.HttpGet]
        public JsonResult GetEntities()
        {
            try
            {
                var entities = new List<object>();
                using (var entityManager = new EntityManager())
                {
                    foreach (var entity in entityManager.Entities)
                    {
                        if(!entity.Name.ToLower().Contains("extension"))
                            entities.Add(new { id = entity.Id, name = entity.Name });
                    }
                }
                return Json(entities, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        [System.Web.Mvc.HttpGet]
        public JsonResult GetMyDatasets(string rightType = "grant", string entityName = "Dataset")
        {
            try
            {
                RightType rt = (RightType)Enum.Parse(typeof(RightType), rightType, true);
                List<MyDatasetsModel> model = new List<MyDatasetsModel>();

                using (var datasetManager = new DatasetManager())
                using (var entityManager = new EntityManager())
                {
                    EntityPermissionManager entityPermissionManager = new EntityPermissionManager();
                    var entity = entityManager.FindByName(entityName);

                    List<long> datasetIds = entityPermissionManager.GetKeys(BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext), entityName, typeof(Dataset), rt).Result;

                    List<DatasetVersion> datasetVersions = datasetManager.GetDatasetLatestVersions(datasetIds, true);

                    // batch query: get all dataset IDs with their latest released tag number (single DB query)
                    var datasetTags = datasetManager.GetDatasetIdsWithLatestTagNr(true);

                    // batch query: get all dataset IDs that have data
                    var datasetIdsWithData = datasetManager.GetDatasetIdsWithData(datasetIds);

                    foreach (var dsv in datasetVersions)
                    {
                        string isValid = "no";
                        bool isOwn = rt == RightType.Grant || rt == RightType.Write;
                        string type = dsv.Dataset.DataStructure != null ? "tabular" : "file";
                        double tagNr = datasetTags.TryGetValue(dsv.Dataset.Id, out var tn) ? tn : 0;
                        bool hasTag = tagNr > 0;
                        bool hasData = datasetIdsWithData.Contains(dsv.Dataset.Id);

                        if (dsv.Dataset.Status == DatasetStatus.CheckedIn)
                        {
                            string title = dsv.Title;
                            string description = dsv.Description;

                            if (dsv.StateInfo != null)
                            {
                                isValid = DatasetStateInfo.Valid.ToString().Equals(dsv.StateInfo.State) ? "yes" : "no";
                            }

                            model.Add(new MyDatasetsModel(dsv.Dataset.Id, title, description, isOwn, isValid, type, hasTag, tagNr, hasData));
                        }
                        else
                        {
                            model.Add(new MyDatasetsModel(dsv.Dataset.Id, "", "Dataset is just in processing.", isOwn, "no", type, hasTag, tagNr, hasData));
                        }
                    }
                }

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        [System.Web.Mvc.HttpGet]
        public JsonResult GetMyRequests()
        {
            try
            {
                var model = new List<RequestGridRowModel>();
                string username = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);

                using (var entityManager = new EntityManager())
                using (var requestManager = new RequestManager())
                {
                    var entityPermissionManager = new EntityPermissionManager();
                    var entity = entityManager.FindByName("Dataset");
                    var entityStore = (IEntityStore)Activator.CreateInstance(entity.EntityStoreType);

                    var requests = requestManager.Requests.Where(r => r.Entity.Id == entity.Id && r.Applicant.Name == username);

                    foreach (var m in requests)
                    {
                        if (entityStore.Exist(m.Key))
                        {
                            model.Add(new RequestGridRowModel()
                            {
                                Id = m.Id,
                                InstanceId = m.Key,
                                Title = entityStore.GetTitleById(m.Key),
                                Rights = string.Join(", ", entityPermissionManager.GetRightsAsync(m.Rights).Result),
                                RequestStatus = Enum.GetName(typeof(RequestStatus), m.Status),
                                Intention = m.Intention,
                                RequestDate = m.RequestDate
                            });
                        }
                    }
                }

                return Json(model, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        [System.Web.Mvc.HttpGet]
        public JsonResult GetDecisions()
        {
            try
            {
                var model = new List<DecisionGridRowModel>();
                string username = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);

                using (var entityManager = new EntityManager())
                using (var decisionManager = new DecisionManager())
                {
                    var entityPermissionManager = new EntityPermissionManager();
                    var entity = entityManager.FindByName("Dataset");
                    var entityStore = (IEntityStore)Activator.CreateInstance(entity.EntityStoreType);

                    var decisions = decisionManager.Decisions.Where(d => d.Request.Entity.Id == entity.Id && d.DecisionMaker.Name == username);

                    foreach (var m in decisions)
                    {
                        if (entityStore.Exist(m.Request.Key))
                        {
                            model.Add(new DecisionGridRowModel()
                            {
                                Id = m.Id,
                                RequestId = m.Request.Id,
                                Rights = string.Join(", ", entityPermissionManager.GetRightsAsync(m.Request.Rights).Result),
                                Status = m.Status,
                                StatusAsText = Enum.GetName(typeof(DecisionStatus), m.Status),
                                InstanceId = m.Request.Key,
                                Title = entityStore.GetTitleById(m.Request.Key),
                                Applicant = m.Request.Applicant.DisplayName,
                                Intention = m.Request.Intention,
                                RequestDate = m.Request.RequestDate
                            });
                        }
                    }
                }

                return Json(model.OrderBy(x => x.Status).ThenBy(n => n.Id), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        [System.Web.Mvc.HttpPost]
        public JsonResult WithdrawRequest(long requestId)
        {
            try
            {
                using (var decisionManager = new DecisionManager())
                {
                    decisionManager.Withdraw(requestId);
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        [System.Web.Mvc.HttpPost]
        public JsonResult AcceptDecision(long decisionId)
        {
            try
            {
                using (var decisionManager = new DecisionManager())
                {
                    decisionManager.Accept(decisionId, "");
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        [System.Web.Mvc.HttpPost]
        public JsonResult RejectDecision(long requestId)
        {
            try
            {
                using (var decisionManager = new DecisionManager())
                {
                    decisionManager.Reject(requestId, "");
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        [System.Web.Mvc.HttpGet]
        public JsonResult GetUseTags()
        {
            try
            {
                var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
                bool useTags = (Boolean)moduleSettings.GetValueByKey("use_tags");
                return Json(useTags, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion Dashboard API
    }
         
}