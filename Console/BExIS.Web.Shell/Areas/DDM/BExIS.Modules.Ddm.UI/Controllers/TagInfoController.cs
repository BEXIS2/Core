using BExIS.App.Bootstrap.Attributes;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Dlm.Entities.Curation;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.UI.Helpers;
using BExIS.Utils.Config;
using BExIS.Utils.Route;
using NHibernate.Linq;
using NHibernate.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Telerik.Web.Mvc.Extensions;
using Vaiona.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;


namespace BExIS.Modules.Ddm.UI.Controllers
{
    public class TagInfoController : BaseController
    {
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

            using (var userManager = new UserManager())
            {
                var userWithGroups = userManager.Users
                    .Where(u => u.Name == userName)
                    .Fetch(u => u.Groups)
                    .SingleOrDefault();

                var userIsCurator = CurationEntry.GetCurationUserType(userWithGroups, GetCurationGroupName()).Equals(CurationUserType.Curator);
                return Json(userIsCurator, JsonRequestBehavior.AllowGet);

            }
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


            using (var emailService = new EmailService())
            {
                var header = $"Relaese Tag Request for ID {datasetID} by {userName}";

                emailService.Send(header, message,
                    new List<string>() { GeneralSettings.SystemEmail }
                );
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