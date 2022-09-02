﻿using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Versions;
using BExIS.Utils.Config;
using BExIS.Web.Shell.Models;
using System;
using System.Configuration;
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
                        landingPage = this.Session.GetTenant().LandingPageTuple;
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
                landingPage = this.Session.GetTenant().LandingPageTuple;
            }

            //if the landingPage is null and the action is not accessible forward to shell/home/index
            if (landingPage == null || !this.IsAccessible(landingPage.Item1, landingPage.Item2, landingPage.Item3))
                return View(); // open shell/home/index

            // return result of defined landing page
            var result = this.Render(landingPage.Item1, landingPage.Item2, landingPage.Item3);
            return Content(result.ToHtmlString(), "text/html");
        }

        [DoesNotNeedDataAccess]
        public ActionResult Nopermission()
        {
            return View("NoPermission");
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

                var model = new VersionModel()
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

                var feature = operation.Feature;
                if (feature == null) return true;

                var result = userManager.FindByNameAsync(userName);

                if (featurePermissionManager.HasAccess(result.Result?.Id, feature.Id))
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