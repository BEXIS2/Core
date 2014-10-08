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
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Web.Shell.Areas.Auth.Models;
using Telerik.Web.Mvc;

namespace BExIS.Web.Shell.Areas.Auth.Controllers
{
    public class RolesController : Controller
    {
        //#region Grid View

        //public ActionResult Roles()
        //{
        //    return View();
        //}

        ////
        //// Creation
        //public ActionResult Create()
        //{
        //    return PartialView("_CreatePartial");
        //}

        //[HttpPost]
        //public ActionResult Create(RoleCreationModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        RoleCreateStatus createStatus;

        //        SubjectManager subjectManager = new SubjectManager();

        //        subjectManager.CreateRole(model.RoleName, model.Description, out createStatus);

        //        if (createStatus == RoleCreateStatus.Success)
        //        {
        //            return PartialView("_InfoPartial", new InfoModel("Window_Creation", "The role was successfully created."));
        //        }
        //        else
        //        {
        //            ModelState.AddModelError(ErrorCodeToErrorKey(createStatus), ErrorCodeToErrorMessage(createStatus));
        //        }
        //    }

        //    return PartialView("_CreatePartial", model);
        //}

        ////
        //// Deletion
        //public ActionResult Delete(long id)
        //{
        //    SubjectManager subjectManager = new SubjectManager();

        //    Role role = subjectManager.GetRoleById(id);

        //    if (role != null)
        //    {
        //        return PartialView("_DeletePartial", RoleModel.Convert(role));
        //    }
        //    else
        //    {
        //        return PartialView("_InfoPartial", new InfoModel("Window_Deletion", "The role does not exist!"));
        //    }
        //}

        //[HttpPost]
        //public ActionResult Delete(RoleModel model)
        //{
        //    SubjectManager subjectManager = new SubjectManager();

        //    subjectManager.DeleteRoleById(model.Id);

        //    return PartialView("_InfoPartial", new InfoModel("Window_Deletion", "The role was successfully deleted."));
        //}

        ////
        //// Details
        //public ActionResult Details(long id)
        //{
        //    return PartialView("_DetailsPartial", id);
        //}

        //public ActionResult RoleInfo(long id)
        //{
        //    SubjectManager subjectManager = new SubjectManager();

        //    Role role = subjectManager.GetRoleById(id);

        //    if (role != null)
        //    {
        //        return PartialView("_RoleInfoPartial", RoleModel.Convert(role));
        //    }
        //    else
        //    {
        //        return PartialView("_InfoPartial", new InfoModel("Window_Details", "The role does not exist!"));
        //    }
        //}

        //public ActionResult RoleEdit(long id)
        //{
        //    SubjectManager subjectManager = new SubjectManager();

        //    Role role = subjectManager.GetRoleById(id);

        //    if (role != null)
        //    {
        //        return PartialView("_RoleEditPartial", RoleModel.Convert(role));
        //    }
        //    else
        //    {
        //        return PartialView("_InfoPartial", new InfoModel("Window_Details", "The role does not exist!"));
        //    }
        //}

        //[HttpPost]
        //public ActionResult RoleEdit(RoleModel model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        SubjectManager subjectManager = new SubjectManager();

        //        Role role = subjectManager.GetRoleById(model.Id);

        //        if (role != null)
        //        {
        //            role.Name = model.RoleName;
        //            role.Description = model.Description;

        //            subjectManager.UpdateRole(role);

        //            return PartialView("_RoleInfoPartial", model);
        //        }
        //        else
        //        {
        //            return PartialView("_InfoPartial", new InfoModel("Window_Details", "The role does not exist!"));
        //        }
        //    }

        //    return PartialView("_RoleEditPartial", model);
        //}

        ////
        //// M
        //public ActionResult Membership(long id)
        //{
        //    SubjectManager subjectManager = new SubjectManager();

        //    Role role = subjectManager.GetRoleById(id); ;

        //    if (role != null)
        //    {
        //        ViewData["roleID"] = id;

        //        return PartialView("_MembershipPartial");
        //    }
        //    else
        //    {
        //        return PartialView("_InfoPartial", new InfoModel("Window_Details", "The role does not exist!"));
        //    }
        //}

        //[GridAction]
        //public ActionResult Membership_Select(long id)
        //{
        //    SubjectManager subjectManager = new SubjectManager();

        //    // DATA
        //    Role role = subjectManager.GetRoleById(id);

        //    List<RoleUserModel> users = new List<RoleUserModel>();

        //    if (role != null)
        //    {
        //        IQueryable<User> data = subjectManager.GetAllUsers();

        //        data.ToList().ForEach(u => users.Add(RoleUserModel.Convert(role.Id, u, subjectManager.IsUserInRole(u.Name, role.Name))));
        //    }

        //    return View(new GridModel<RoleUserModel> { Data = users });
        //}

        //public int AddUserToRole(long userId, long roleId)
        //{
        //    SubjectManager subjectManager = new SubjectManager();

        //    return subjectManager.AddUserToRole(userId, roleId);
        //}

        //public int RemoveUserFromRole(long userId, long roleId)
        //{
        //    SubjectManager subjectManager = new SubjectManager();

        //    return subjectManager.RemoveUserFromRole(userId, roleId);
        //}

        //// Feature Permissions
        //public ActionResult FeaturePermissions(long id)
        //{
        //    SubjectManager subjectManager = new SubjectManager();

        //    Role role = subjectManager.GetRoleById(id); ;

        //    if (role != null)
        //    {
        //        ViewData["RoleID"] = id;

        //        return PartialView("_FeaturePermissionsPartial");
        //    }
        //    else
        //    {
        //        return PartialView("_InfoPartial", new InfoModel("Window_Details", "The role does not exist!"));
        //    }
        //}

        //// R
        //[GridAction]
        //public ActionResult Roles_Select()
        //{
        //    SubjectManager subjectManager = new SubjectManager();

        //    // DATA
        //    IQueryable<Role> data = subjectManager.GetAllRoles();

        //    List<RoleModel> roles = new List<RoleModel>();
        //    data.ToList().ForEach(r => roles.Add(RoleModel.Convert(r)));

        //    return View(new GridModel<RoleModel> { Data = roles });
        //}

        //#endregion


        //#region Validation

        //public JsonResult ValidateRoleName(string roleName, long id = 0)
        //{
        //    SubjectManager subjectManager = new SubjectManager();

        //    Role role = subjectManager.GetRoleByName(roleName);

        //    if (role == null)
        //    {
        //        return Json(true, JsonRequestBehavior.AllowGet);
        //    }
        //    else
        //    {
        //        if (role.Id == id)
        //        {
        //            return Json(true, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            string error = String.Format(CultureInfo.InvariantCulture, "The role name already exists.", roleName);

        //            return Json(error, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}

        //private static string ErrorCodeToErrorMessage(RoleCreateStatus createStatus)
        //{
        //    switch (createStatus)
        //    {
        //        case RoleCreateStatus.DuplicateRoleName:
        //            return "The role name already exists.";

        //        case RoleCreateStatus.InvalidRoleName:
        //            return "The role name is not valid.";

        //        default:
        //            return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
        //    }
        //}

        //private static string ErrorCodeToErrorKey(RoleCreateStatus createStatus)
        //{
        //    switch (createStatus)
        //    {
        //        case RoleCreateStatus.DuplicateRoleName:
        //            return "RoleName";

        //        case RoleCreateStatus.InvalidRoleName:
        //            return "RoleName";

        //        default:
        //            return "";
        //    }
        //}

        //#endregion
    }
}
