using BExIS.Modules.Sam.UI.Models;
using System;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class SubjectController : Controller
    {
        public ActionResult CreateGroup()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CreateGroup(CreateGroupModel model)
        {
            return View(model);
        }

        public ActionResult CreateUser()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public ActionResult CreateUser(CreateUserModel model)
        {
            return View(model);
        }

        public ActionResult Index()
        {
            return View();
        }

        [GridAction]
        public ActionResult Subjects_Select()
        {
            throw new NotImplementedException();
        }
    }
}