using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Web.Shell.Areas.SAM.Models;
using Telerik.Web.Mvc;

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
                subjectManager.CreateUser(model.UserName, model.Password, model.FullName, model.Email, model.SecurityQuestionList.Id, model.SecurityAnswer, model.AuthenticatorList.Id);

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
        public ActionResult Edit(UserEditModel model, long[] selectedGroups)
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

                long[] groups = user.Groups.Select(g => g.Id).ToArray();

                if (groups != null)
                {
                    foreach (long groupId in groups)
                    {
                        subjectManager.RemoveUserFromGroup(user.Id, groupId);
                    }
                }

                if (selectedGroups != null)
                {
                    foreach (long groupId in selectedGroups)
                    {
                        subjectManager.AddUserToGroup(user.Id, groupId);
                    }
                }

                subjectManager.UpdateUser(user);

                return Json(new { success = true });
            }
            else
            {
                ViewData["SelectedGroups"] = selectedGroups;

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

        // --------------------------------------------------
        // USERS
        // --------------------------------------------------

        #region Users

        public ActionResult Users()
        {
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

        public JsonResult ValidateUserName(string userName, long id = 0)
        {
            SubjectManager subjectManager = new SubjectManager();

            User user = subjectManager.GetUserByName(userName);

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
                    string error = String.Format(CultureInfo.InvariantCulture, "The user name already exists.", userName);

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
                    string error = String.Format(CultureInfo.InvariantCulture, "The e-mail address already exists.", email);

                    return Json(error, JsonRequestBehavior.AllowGet);
                }
            }
        }

        #endregion Remote Validation
    }
}