using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using BExIS.Security.Entities;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services;
using BExIS.Security.Services.Subjects;
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
        // Creation
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

                roleManager.Create(model.RoleName, model.Description, out createStatus);

                if (createStatus == RoleCreateStatus.Success)
                {
                    return PartialView("_InfoPartial", new InfoModel("windowCreation", "The role was successfully created."));
                }
                else
                {
                    ModelState.AddModelError(ErrorCodeToErrorKey(createStatus), ErrorCodeToErrorMessage(createStatus));
                }
            }

            return PartialView("_CreatePartial", model);
        }

        //
        // Deletion
        public PartialViewResult Delete(long id)
        {
            RoleManager roleManager = new RoleManager();

            Role role = roleManager.GetRoleById(id);

            if (role != null)
            {
                return PartialView("_DeletePartial", RoleModel.Convert(role));
            }
            else
            {
                return PartialView("_InfoPartial", new InfoModel("windowDeletion", "The role does not exist!"));
            }
        }

        [HttpPost]
        public PartialViewResult Delete(RoleModel model)
        {
            RoleManager roleManager = new RoleManager();

            Role role = roleManager.GetRoleById(model.Id);

            if (role != null)
            {
                roleManager.Delete(role);
            }

            return PartialView("_InfoPartial", new InfoModel("windowDeletion", "The role was successfully deleted."));
        }

        //
        // Details
        public ActionResult Details(long id)
        {
            return PartialView("_DetailsPartial", id);
        }

        public ActionResult RoleInfo(long id)
        {
            RoleManager roleManager = new RoleManager();

            Role role = roleManager.GetRoleById(id);

            if (role != null)
            {
                return PartialView("_RoleInfoPartial", RoleModel.Convert(role));
            }
            else
            {
                return PartialView("_InfoPartial", new InfoModel("windowDetails", "The role does not exist!"));
            }
        }

        public ActionResult RoleEdit(long id)
        {
            RoleManager roleManager = new RoleManager();

            Role role = roleManager.GetRoleById(id);

            if (role != null)
            {
                return PartialView("_RoleEditPartial", RoleModel.Convert(role));
            }
            else
            {
                return PartialView("_InfoPartial", new InfoModel("windowDetails", "The role does not exist!"));
            }
        }

        [HttpPost]
        public ActionResult RoleEdit(RoleModel model)
        {
            if (ModelState.IsValid)
            {
                RoleManager roleManager = new RoleManager();

                Role role = roleManager.GetRoleById(model.Id);

                if (role != null)
                {
                    role.Name = model.RoleName;
                    role.Description = model.Description;

                    roleManager.Update(role);

                    return PartialView("_RoleInfoPartial", model);
                }
                else
                {
                    return PartialView("_InfoPartial", new InfoModel("windowDetails", "The role does not exist!"));
                }
            }

            return PartialView("_RoleEditPartial", model);
        }

        //
        // M
        public ActionResult Membership(long id)
        {
            RoleManager roleManager = new RoleManager();

            Role role = roleManager.GetRoleById(id); ;

            if (role != null)
            {
                ViewData["roleID"] = id;

                return PartialView("_MembershipPartial");
            }
            else
            {
                return PartialView("_InfoPartial", new InfoModel("windowMembership", "The role does not exist!"));
            }
        }

        [GridAction]
        public ActionResult RoleMembership_Select(long id)
        {
            RoleManager roleManager = new RoleManager();
            UserManager userManager = new UserManager();

            // DATA
            Role role = roleManager.GetRoleById(id);

            List<RoleUserModel> users = new List<RoleUserModel>();

            if (role != null)
            {
                IQueryable<User> data = userManager.GetAllUsers();

                data.ToList().ForEach(u => users.Add(RoleUserModel.Convert(role.Id, u, roleManager.IsUserInRole(u, role))));
            }

            return View(new GridModel<RoleUserModel> { Data = users });
        }

        public void AddUserToRole(long userId, long roleId)
        {
            RoleManager roleManager = new RoleManager();
            UserManager userManager = new UserManager();

            Role role = roleManager.GetRoleById(roleId);
            User user = userManager.GetUserById(userId);

            if (user != null && role != null)
            {
                roleManager.AddUserToRole(user, role);
            }

        }

        public void RemoveUserFromRole(long userId, long roleId)
        {
            RoleManager roleManager = new RoleManager();
            UserManager userManager = new UserManager();

            Role role = roleManager.GetRoleById(roleId);
            User user = userManager.GetUserById(userId);

            if (user != null && role != null)
            {
                roleManager.RemoveUserFromRole(user, role);
            }
        }

        // R
        [GridAction]
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

        private static string ErrorCodeToErrorKey(RoleCreateStatus createStatus)
        {
            switch (createStatus)
            {
                case RoleCreateStatus.DuplicateRoleName:
                    return "RoleName";

                default:
                    return "";
            }
        }

        #endregion
    }
}
