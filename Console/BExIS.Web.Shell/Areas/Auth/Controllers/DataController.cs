using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Web.Shell.Areas.Auth.Models;
using Telerik.Web.Mvc;

namespace BExIS.Web.Shell.Areas.Auth.Controllers
{
    public class DataController : Controller
    {
        #region Grid - DataPermissions

        public ActionResult DataPermissions()
        {
            return View();
        }

        [GridAction]
        public ActionResult DataPermissions_Select()
        {
            throw new NotImplementedException();
        }

        //
        // Create
        public ActionResult Create()
        {
            return PartialView("_CreatePartial");
        }

        [HttpPost]
        public ActionResult Create(DataPermissionCreationModel model)
        {
            throw new NotImplementedException();
        }

        // Delete


        // Edit

        #endregion
    }
}
