using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using BExIS.Security.Entities;
using BExIS.Security.Services;
using BExIS.Web.Shell.Areas.Auth.Models;
using Telerik.Web.Mvc;

namespace BExIS.Web.Shell.Areas.Auth.Controllers
{
    public class RolesController : Controller
    {
        #region Grid View

        public ActionResult Roles()
        {
            return View();
        }

        //
        //
        // C
        public ActionResult Create()
        {
            return PartialView("_CreatePartial");
        }

        [HttpPost]
        public ActionResult Create(RoleCreationModel model)
        {
            if (ModelState.IsValid)
            {
                RoleCreateStatus createStatus;

                RoleManager roleManager = new RoleManager();

                roleManager.Create(model.RoleName, model.Description, model.Comment, out createStatus);

                if (createStatus == RoleCreateStatus.Success)
                {
                    //return Json(new { message = "The role was created successfully." });
                    return PartialView("_CreatePartial", model);
                }
                else
                {
                    ModelState.AddModelError("RoleName", ErrorCodeToErrorMessage(createStatus));
                }
            }

            return PartialView("_CreatePartial", model);
        }

        //
        //
        // D
        public PartialViewResult Delete(long id)
        {
            RoleManager roleManager = new RoleManager();

            return PartialView("_DeletePartial", RoleModel.Convert(roleManager.GetRoleById(id)));
        }

        [HttpPost]
        public PartialViewResult Delete(RoleModel model)
        {
            return null;
        }


        public ActionResult Details(long id)
        {
            RoleManager roleManager = new RoleManager();

            return PartialView("_DetailsPartial", id);
        }

        public ActionResult RoleInfo(long id)
        {
            RoleManager roleManager = new RoleManager();

            return PartialView("_RoleInfoPartial", RoleModel.Convert(roleManager.GetRoleById(id)));
        }

        public ActionResult RoleEdit(long id)
        {
            RoleManager roleManager = new RoleManager();

            return PartialView("_RoleEditPartial", RoleModel.Convert(roleManager.GetRoleById(id)));
        }

        [HttpPost]
        public ActionResult RoleEdit(RoleModel model)
        {
            RoleManager roleManager = new RoleManager();

            return null;
        }

        //
        //
        // M
        public ActionResult Membership(long id)
        {
            ViewData["RoleID"] = id;

            return PartialView("_MembershipPartial");
        }

        [GridAction(EnableCustomBinding = true)]
        public ActionResult RoleMembership_Select(long id)
        {
            List<RoleUserModel> members = new List<RoleUserModel>();

            return View(new GridModel<RoleUserModel> { Data = members });
        }

        // R
        [GridAction(EnableCustomBinding = true)]
        public ActionResult Roles_Select()
        {
            RoleManager roleManager = new RoleManager();

            // DATA
            IQueryable<Role> data = roleManager.GetAllRoles();

            List<RoleModel> roles = new List<RoleModel>();
            data.ToList().ForEach(r => roles.Add(RoleModel.Convert(r)));

            return View(new GridModel<RoleModel> { Data = roles });
        }

        #endregion


        #region Validation

        public JsonResult ValidateRoleName(string roleName, long id = 0)
        {
            RoleManager roleManager = new RoleManager();

            Role role = roleManager.GetRoleByName(roleName);

            if (role == null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (role.Id == id)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string error = String.Format(CultureInfo.InvariantCulture, "The role name already exists.", roleName);

                    return Json(error, JsonRequestBehavior.AllowGet);
                }
            }
        }

        private static string ErrorCodeToErrorMessage(RoleCreateStatus createStatus)
        {
            switch (createStatus)
            {
                case RoleCreateStatus.DuplicateRoleName:
                    return "The role name already exists.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        #endregion
    }
}
