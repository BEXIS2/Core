﻿using System.Web.Mvc;
using Vaiona.Web.Mvc.Data;
using Vaiona.Web.Mvc.Modularity;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;
using BExIS.Web.Shell.Helpers;
using Vaiona.IoC;
using BExIS.App.Bootstrap;
using System;
using System.Configuration;
using System.IO;
using System.Xml.Linq;
using BExIS.Security.Services.Versions;
using BExIS.Web.Shell.Models;
using BExIS.Xml.Helpers;
using Vaiona.Utils.Cfg;

namespace BExIS.Web.Shell.Controllers
{
    public class HomeController : Controller
    {
        [DoesNotNeedDataAccess]
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Home", this.Session.GetTenant());

            // here there are 2 cases to consider.
            // 1.no user->landingpage
            // 2.user logged into landingpage for users
            Tuple<string, string, string> landingPage = null;
            //check if user exist
            if (!string.IsNullOrEmpty(HttpContext.User?.Identity?.Name)) //user
            {
                // User exist : load ladingpage for users
                GeneralSettings generalSettings = IoCFactory.Container.Resolve<GeneralSettings>();
                var landingPageForUsers = generalSettings.GetEntryValue("landingPageForUsers").ToString();

                if (landingPageForUsers.Split(',').Length == 3)//check wheter 3 values exist for teh action
                {
                    landingPage = new Tuple<string, string, string>(
                        landingPageForUsers.Split(',')[0].Trim(), //module id
                        landingPageForUsers.Split(',')[1].Trim(), //controller
                        landingPageForUsers.Split(',')[2].Trim());//action
                }
            }
            else
            {
                landingPage = this.Session.GetTenant().LandingPageTuple;
            }

            //if the landingPage not null and the action is accessable
            if (landingPage == null || !this.IsAccessible(landingPage.Item1, landingPage.Item2, landingPage.Item3))
                return View();

            var result = this.Render(landingPage.Item1, landingPage.Item2, landingPage.Item3);



            return Content(result.ToHtmlString(), "text/html");
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
            var versionManager = new VersionManager();
            var database = versionManager.GetLatestVersion().Value;

            // Workspace
            string filePath = Path.Combine(AppConfiguration.WorkspaceGeneralRoot, "General.Settings.xml");
            XDocument settings = XDocument.Load(filePath);
            XElement entry = XmlUtility.GetXElementByAttribute("entry", "key", "version", settings);
            var workspace = entry.Attribute("value")?.Value;


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
}