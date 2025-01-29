using BExIS.App.Bootstrap.Attributes;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Versions;
using BExIS.Utils.Config;
using BExIS.Web.Shell.Models;
using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Data;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Web.Shell.Controllers
{
    public class HomeController : Controller
    {
        [DoesNotNeedDataAccess]
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Home", this.Session.GetTenant());

            // here there are 3 cases to consider.
            // 1. no user->landingpage
            // 2. user logged into landingpage for users
            // 3. user logged in -> no permission landing page for user -> forward to extra site "noPermission" or to defined page
            Tuple<string, string, string> landingPage = null;
            //check if user exist
            if (!string.IsNullOrEmpty(HttpContext.User?.Identity?.Name)) //user
            {
                // User exist : load ladingpage for users

                var landingPageForUsers = GeneralSettings.LandingPageForUsers;

                if (landingPageForUsers.Split(',').Length == 3)//check wheter 3 values exist for teh action
                {
                    landingPage = new Tuple<string, string, string>(
                        landingPageForUsers.Split(',')[0].Trim(), //module id
                        landingPageForUsers.Split(',')[1].Trim(), //controller
                        landingPageForUsers.Split(',')[2].Trim());//action
                }

                //if the landingPage not null and the action is accessable
                if (landingPage == null || !this.IsAccessible(landingPage.Item1, landingPage.Item2, landingPage.Item3) || !checkPermission(landingPage))
                {
                    landingPageForUsers = GeneralSettings.LandingPageForUsersNoPermission;

                    if (landingPageForUsers.Split(',').Length == 3)//check wheter 3 values exist for teh action
                    {
                        landingPage = new Tuple<string, string, string>(
                            landingPageForUsers.Split(',')[0].Trim(), //module id
                            landingPageForUsers.Split(',')[1].Trim(), //controller
                            landingPageForUsers.Split(',')[2].Trim());//action
                    }

                    // fallback if not exists
                    // this.IsAccessible not possible for shell
                    if (checkPermission(landingPage) == false)
                    {
                        landingPage = new Tuple<string, string, string>(
                            GeneralSettings.LandingPage.Split(',')[0].Trim(), //module id
                            GeneralSettings.LandingPage.Split(',')[1].Trim(), //controller
                            GeneralSettings.LandingPage.Split(',')[2].Trim());//action
                    }

                    // Default forward, if no other path given for no permission page
                    if (landingPage.Item1.ToLower() == "shell" && landingPage.Item2.ToLower() == "home")
                    {
                        return View(landingPage.Item3);
                    }
                }
            }
            // use defined landing page without login
            else
            {
                // load langding page from tenants (custom ) if settings
                if (string.IsNullOrEmpty(GeneralSettings.LandingPage))
                    return RedirectToAction("Start");

                landingPage = new Tuple<string, string, string>(
                            GeneralSettings.LandingPage.Split(',')[0].Trim(), //module id
                            GeneralSettings.LandingPage.Split(',')[1].Trim(), //controller
                            GeneralSettings.LandingPage.Split(',')[2].Trim());//action

                // Default forward, if no other path given for no permission page
                if (landingPage.Item1.ToLower() == "shell" && landingPage.Item2.ToLower() == "home")
                {
                    return View(landingPage.Item3);
                }
            }

            //if the landingPage is null and the action is not accessible forward to shell/home/index
            if (landingPage == null || !this.IsAccessible(landingPage.Item1, landingPage.Item2, landingPage.Item3))
                return View(); // open shell/home/index

            // return result of defined landing page
            string action = landingPage.Item3.ToLower().Equals("index")?"": landingPage.Item3;
            var result = this.Render(landingPage.Item1.ToLower(), landingPage.Item2.ToLower(), action);

            return RedirectToAction(action, landingPage.Item2.ToLower(), new { area = landingPage.Item1.ToLower() });

            //return Content(result.ToHtmlString(), "text/html");
        }

        /// <summary>
        /// use this action to load the landingpage from the tenants folder
        /// </summary>
        /// <returns></returns>
        [DoesNotNeedDataAccess]
        public ActionResult Start()
        {
            ViewData["ShowMenu"] = GeneralSettings.GetValueByKey("showMenuOnLandingPage");
            ViewData["ShowHeader"] = GeneralSettings.GetValueByKey("showHeaderOnLandingPage");
            ViewData["ShowFooter"] = GeneralSettings.GetValueByKey("showFooterOnLandingPage");

            return View("Start", null, Session.GetTenant().LandingPageFileNamePath);
        }

        public ActionResult GetImage(string image)
        {
            string path = Path.Combine(Session.GetTenant().PathProvider.GetImagePath(Session.GetTenant().Id, image, "bexis2"));
            byte[] imageData = System.IO.File.ReadAllBytes(path);

            return File(imageData, MimeMapping.GetMimeMapping(path));
        }

        [DoesNotNeedDataAccess]
        public ActionResult Nopermission()
        {
            return View("NoPermission");
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetApplicationName()
        {
            try
            {
                var generalSettings = new GeneralSettings();
                var applicationName = generalSettings.GetApplicationName();

                return Json(applicationName, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json("BEXIS2", JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        [HttpGet]
        public ActionResult Breadcrumb()
        {
            return View("");
        }

        [DoesNotNeedDataAccess]
        public ActionResult SessionTimeout()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Session Timeout", this.Session.GetTenant());

            return RedirectToAction("Index");
        }

        [DoesNotNeedDataAccess]
        public ActionResult Version()
        {
            // Site
            var site = ConfigurationManager.AppSettings["ApplicationVersion"];

            // Database
            using (var versionManager = new VersionManager())
            {
                var database = versionManager.GetLatestVersion().Value;

                // load version from workspace in settings file of general

                string workspace = GeneralSettings.ApplicationVersion;

                var model = new ReadVersionsModel()
                {
                    Site = site,
                    Database = database,
                    Workspace = workspace
                };

                ViewBag.Title = PresentationModel.GetViewTitleForTenant("Session Timeout", this.Session.GetTenant());

                return View(model);
            }
        }

        protected bool checkPermission(Tuple<string, string, string> LandingPage)
        {
            var featurePermissionManager = new FeaturePermissionManager();
            var operationManager = new OperationManager();
            var userManager = new UserManager();

            try
            {
                var areaName = LandingPage.Item1;
                if (areaName == "")
                {
                    areaName = "shell";
                }
                var controllerName = LandingPage.Item2;
                var actionName = LandingPage.Item3;

                var userName = HttpContext.User?.Identity?.Name;
                var operation = operationManager.Find(areaName, controllerName, "*");

                var feature = operation?.Feature;
                if (feature == null) return true;

                var result = userManager.FindByNameAsync(userName);

                if (featurePermissionManager.HasAccessAsync(result.Result?.Id, feature.Id).Result)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                featurePermissionManager.Dispose();
                operationManager.Dispose();
                userManager.Dispose();
            }
        }
    }
}