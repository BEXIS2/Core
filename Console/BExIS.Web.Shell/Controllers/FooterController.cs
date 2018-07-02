﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Web.Shell.Controllers
{
    public class FooterController : Controller
    {
        // GET: footer
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ContactUs()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Contact Us", this.Session.GetTenant());
            return View("Content", null, Session.GetTenant().ContactUsFileNamePath);
        }

        public ActionResult Imprint()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Imprint", this.Session.GetTenant());
            return View("Content", null, Session.GetTenant().ImprintFileNamePath);
        }

        public ActionResult Policy()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Policy", this.Session.GetTenant());
            return View("Content", null, this.Session.GetTenant().PolicyFileNamePath);
        }
    }
}