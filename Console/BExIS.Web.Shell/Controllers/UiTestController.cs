using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Web.Shell.Models;

namespace BExIS.Web.Shell.Controllers
{
    public class UiTestController : Controller
    {
        //
        // GET: /UiTest/

        public ActionResult Index()
        {

            return View(new UiTestModel());
        }

        public ActionResult sendForm( UiTestModel model) 
        {

            return View("Index", model);
        }
    }
}
