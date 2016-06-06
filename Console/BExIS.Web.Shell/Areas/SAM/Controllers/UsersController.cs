using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Web.Shell.Areas.SAM.Models;
using Telerik.Web.Mvc;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;

namespace BExIS.Web.Shell.Areas.SAM.Controllers
{
    public class UsersController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return PartialView("_CreatePartial", new UserCreateModel());
        }

        [HttpPost]
        public ActionResult Create(UserCreateModel model)
        {
            if (ModelState.IsValid)
            {
                SubjectManager subjectManager = new SubjectManager();
                User user = subjectManager.CreateUser(model.Username, model.Password, model.FullName, model.Email, model.SecurityQuestion, model.SecurityAnswer, model.AuthenticatorList.Id);

                // Feature
                FeatureManager featureManager = new FeatureManager();
                Feature feature = featureManager.FeaturesRepo.Get(f => f.Name == "Search").FirstOrDefault();

                // Permissions
                PermissionManager permissionManager = new PermissionManager();
                permissionManager.CreateFeaturePermission(user.Id, feature.Id);

                return Json(new { success = true });
            }

            return PartialView("_CreatePartial", model);
        }

        [HttpPost]
        public void Delete(long id)
        {
            SubjectManager subjectManager = new SubjectManager();
            subjectManager.DeleteUserById(id);
        }

        public ActionResult Edit(long id)
        {
            SubjectManager subjectManager = new SubjectManager();

            User user = subjectManager.GetUserById(id);

            return PartialView("_EditPartial", UserEditModel.Convert(user));
        }

        [HttpPost]
        public ActionResult Edit(UserEditModel model)
        {
            if (ModelState.IsValid)
            {
                SubjectManager subjectManager = new SubjectManager();

                User user = subjectManager.GetUserById(model.UserId);

                if (model.Password == model.ConfirmPassword && model.Password != null)
                {
                    subjectManager.ChangePassword(user.Id, model.Password);
                }

                user.FullName = model.FullName;
                user.Email = model.Email;

                user.IsApproved = model.IsApproved;
                user.IsBlocked = model.IsBlocked;
                user.IsLockedOut = model.IsLockedOut;

                // removing
                long[] groups = user.Groups.Select(g => g.Id).ToArray();

                foreach (long groupId in groups)
                {
                    subjectManager.RemoveUserFromGroup(user.Id, groupId);
                }

                //adding
                if (Session["Groups"] != null)
                {
                    foreach (UserMembershipGridRowModel group in (UserMembershipGridRowModel[]) Session["Groups"])
                    {
                        if (group.IsUserInGroup)
                        {
                            subjectManager.AddUserToGroup(user.Id, group.Id);
                        }
                    }
                }

                subjectManager.UpdateUser(user);

                return Json(new { success = true });
            }
            else
            {
                return PartialView("_EditPartial", model);
            }
        }

        [GridAction]
        public ActionResult Membership_Select(long id, long[] selectedGroups)
        {
            SubjectManager subjectManager = new SubjectManager();

            List<UserMembershipGridRowModel> groups = new List<UserMembershipGridRowModel>();

            if (selectedGroups != null)
            {
                groups = subjectManager.GetAllGroups().Select(g => UserMembershipGridRowModel.Convert(g, selectedGroups.Contains(g.Id))).ToList();
            }
            else
            {
                User user = subjectManager.GetUserById(id);

                groups = subjectManager.GetAllGroups().Select(g => UserMembershipGridRowModel.Convert(g, g.Users.Any(u => u.Id == id))).ToList();
            }

            return View(new GridModel<UserMembershipGridRowModel> { Data = groups });
        }

        public void SetMembership(UserMembershipGridRowModel[] groups)
        {
            Session["Groups"] = groups;
        }

        // --------------------------------------------------
        // USERS
        // --------------------------------------------------

        #region Users

        public ActionResult Users()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Users", this.Session.GetTenant());
            return View();
        }

        [GridAction]
        public ActionResult Users_Select()
        {
            SubjectManager subjectManager = new SubjectManager();
            List<UserGridRowModel> users = subjectManager.GetAllUsers().Select(u => UserGridRowModel.Convert(u)).ToList();

            return View(new GridModel<UserGridRowModel> { Data = users });
        }

        #endregion Users

        // --------------------------------------------------
        // REMOTE VALIDATION
        // --------------------------------------------------

        #region Remote Validation

        public JsonResult ValidateUsername(string username, long id = 0)
        {
            SubjectManager subjectManager = new SubjectManager();

            User user = subjectManager.GetUserByName(username);

            if (user == null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (user.Id == id)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string error = String.Format(CultureInfo.InvariantCulture, "The Username exists already.", username);

                    return Json(error, JsonRequestBehavior.AllowGet);
                }
            }
        }

        public JsonResult ValidateEmail(string email, long id = 0)
        {
            SubjectManager subjectManager = new SubjectManager();

            User user = subjectManager.GetUserByEmail(email);

            if (user == null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (user.Id == id)
                {
                    return Json(true, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    string error = String.Format(CultureInfo.InvariantCulture, "The Email Address exists already.", email);

                    return Json(error, JsonRequestBehavior.AllowGet);
                }
            }
        }

        #endregion Remote Validation
    }
}