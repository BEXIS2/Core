using BExIS.App.Bootstrap.Attributes;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Dlm.Entities.Curation;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.UI.Helpers;
using BExIS.Utils.Config;
using Microsoft.AspNet.Identity;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using Telerik.Web.Mvc.Extensions;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;


namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class TagInfoController : BaseController
    {
        private readonly UserManager _userManager;

        public TagInfoController(UserManager userManager)
        {
            _userManager = userManager;
        }

        // GET: TagInfo
        public ActionResult Index(long id)
        {
            string module = "DDM";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);
            ViewData["id"] = id;

            var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
            ViewData["use_minor"] = moduleSettings.GetValueByKey("use_minor");

            return View();
        }


        [BExISApiAuthorize]
        [JsonNetFilter]
        [System.Web.Http.HttpGet]
        public JsonResult GetUserRole()
        {
           
            string userName = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);

            var userWithGroups = _userManager.Users
                                .Where(u => u.Name == userName)
                                .Fetch(u => u.Groups)
                                .SingleOrDefault();

            var userIsCurator = CurationEntry.GetCurationUserType(userWithGroups, GetCurationGroupName()).Equals(CurationUserType.Curator);
            return Json(userIsCurator, JsonRequestBehavior.AllowGet);
        }

        [BExISApiAuthorize]
        [JsonNetFilter]
        [System.Web.Http.HttpGet]
        public JsonResult GetCuratorRequired()
        {
            bool isCuratorRequired = false;
            var moduleSettings = ModuleManager.GetModuleSettings("Ddm");

            isCuratorRequired = (bool)moduleSettings.GetValueByKey("curator_required_for_tags");

            return Json(isCuratorRequired, JsonRequestBehavior.AllowGet);
        }


        // Temporary solution: Send only Email instead of store requests
        [BExISApiAuthorize]
        [JsonNetFilter]
        [System.Web.Http.HttpPost]
        public JsonResult SendTagRequest(Data data)
        {
            var datasetID = data.Id;
            string userName = BExISAuthorizeHelper.GetAuthorizedUserName(HttpContext);

            var message = data.Message;

            using (DatasetManager dm = new DatasetManager())
            using (var emailService = new EmailService())
            {
                {
                    // Convert datasetID to long
                    if (!long.TryParse(datasetID, out long datasetIdLong))
                    {
                        // Handle the error
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }

                    // var dataset = dm.GetDataset(datasetIdLong);
                    var latestVersion = dm.GetDatasetLatestVersion(datasetIdLong);

                    emailService.Send(MessageHelper.GetReleaseTagHeader(datasetIdLong, typeof(Dataset).Name), MessageHelper.GetReleaseTagMessage(GetDisplayName(), datasetIdLong, typeof(Dataset).Name, latestVersion.Title, message),
                        new List<string>() { GeneralSettings.SystemEmail }
                    );
                }
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        private static String GetCurationGroupName()
        {
            var groupName = ModuleManager.GetModuleSettings("DDM").GetValueByKey("curatorsGroupName").ToString();
            if (string.IsNullOrEmpty(groupName))
            {
                return "curator";
            }
            return groupName;
        }

        public ActionResult UpdateSearch(long id)
        {
            if (this.IsAccessible("DDM", "SearchIndex", "ReIndexSingle"))
            {
                return this.Run("DDM", "SearchIndex", "ReIndexSingle", new RouteValueDictionary() { { "id", id } });
            }

            return null;
        }

        public string GetDisplayName()
        {
            string username = string.Empty;
            try
            {
                username = HttpContext.User.Identity.Name;
                User user = _userManager.FindByNameAsync(username).Result;

                return user.DisplayName;
            }
            catch
            {
                return "DEFAULT";
            }
        }

    }

    public class Data
    {
        public string Id { get; set; }
        public string Message { get; set; }

        public Data()
        {
            Id = "";
            Message = "";
        }
    }

}