using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Security;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Web.Shell.Areas.Auth.Models;
using Telerik.Web.Mvc;

namespace BExIS.Web.Shell.Areas.Auth.Controllers
{
    public class UsersController : Controller
    {
        #region Grid View

        public ActionResult Users()
        {
            return View();
        }

        //
        // Create
        public ActionResult Create()
        {
            return PartialView("_CreatePartial");
        }

        [HttpPost]
        public ActionResult Create(UserCreationModel model)
        {
            if (ModelState.IsValid)
            {
                UserCreateStatus createStatus;

                SubjectManager subjectManager = new SubjectManager();

                subjectManager.CreateUser(model.UserName, model.Email, model.Password, model.SecurityQuestion, model.SecurityAnswer, out createStatus);

                if (createStatus == UserCreateStatus.Success)
                {
                    return PartialView("_InfoPartial", new InfoModel("Window_Creation", "The user was successfully created."));
                }
                else
                {
                    ModelState.AddModelError(ErrorCodeToErrorKey(createStatus), ErrorCodeToErrorMessage(createStatus));
                }
            }

            return PartialView("_CreatePartial", model);
        }

        //
        // Delete
        public ActionResult Delete(long id)
        {
            SubjectManager subjectManager = new SubjectManager();

            User user = subjectManager.GetUserById(id);

            if (user != null)
            {
                return PartialView("_DeletePartial", UserModel.Convert(user));         
            }
            else
            {
                return PartialView("_InfoPartial", new InfoModel("Window_Deletion", "The user does not exist!"));
            }
        }

        [HttpPost]
        public ActionResult Delete(UserModel model)
        {
            SubjectManager subjectManager = new SubjectManager();

            subjectManager.DeleteUserById(model.Id);

            return PartialView("_InfoPartial", new InfoModel("Window_Deletion", "The user was successfully deleted."));
        }

        //
        // Details
        public ActionResult Details(long id)
        {
            return PartialView("_DetailsPartial", id);
        }

        public ActionResult UserInfo(long id)
        {
            SubjectManager subjectManager = new SubjectManager();

            User user = subjectManager.GetUserById(id);

            if (user != null)
            {
                return PartialView("_UserInfoPartial", UserModel.Convert(user));
            }
            else
            {
                return PartialView("_InfoPartial", new InfoModel("Window_Details", "The user does not exist!"));
            }
        }

        public ActionResult UserEdit(long id)
        {
            SubjectManager subjectManager = new SubjectManager();

            User user = subjectManager.GetUserById(id);

            if (user != null)
            {
                return PartialView("_UserEditPartial", UserModel.Convert(user));
            }
            else
            {
                return PartialView("_InfoPartial", new InfoModel("Window_Details", "The user does not exist!"));
            }
        }

        [HttpPost]
        public ActionResult UserEdit(UserModel model)
        {
            if (ModelState.IsValid)
            {
                SubjectManager subjectManager = new SubjectManager();

                User user = subjectManager.GetUserById(model.Id);

                if (user != null)
                {
                    user.Email = model.Email;

                    subjectManager.UpdateUser(user);

                    return PartialView("_UserInfoPartial", model);
                }
                else
                {
                    return PartialView("_InfoPartial", new InfoModel("Window_Details", "The user does not exist!"));
                }
            }

            return PartialView("_UserEditPartial", model);
        }

        // Membership
        public ActionResult Membership(long id)
        {
            SubjectManager subjectManager = new SubjectManager();

            User user = subjectManager.GetUserById(id);;

            if (user != null)
            {
                ViewData["UserID"] = id;

                return PartialView("_MembershipPartial");
            }
            else
            {
                return PartialView("_InfoPartial", new InfoModel("Window_Details", "The user does not exist!"));
            }
        }

        [GridAction]
        public ActionResult Membership_Select(long id)
        {
            SubjectManager subjectManager = new SubjectManager();

            // DATA
            User user = subjectManager.GetUserById(id);

            List<UserRoleModel> roles = new List<UserRoleModel>();

            if (user != null)
            {
                IQueryable<Role> data = subjectManager.GetAllRoles();

                data.ToList().ForEach(r => roles.Add(UserRoleModel.Convert(user.Id, r, subjectManager.IsUserInRole(user.Name, r.Name))));
            }

            return View(new GridModel<UserRoleModel> { Data = roles });
        }

        public int AddUserToRole(long userId, long roleId)
        {
            SubjectManager subjectManager = new SubjectManager();

            return subjectManager.AddUserToRole(userId, roleId);
        }

        public int RemoveUserFromRole(long userId, long roleId)
        {
            SubjectManager subjectManager = new SubjectManager();

            return subjectManager.RemoveUserFromRole(userId, roleId);
        }

        // Feature Permissions
        public ActionResult FeaturePermissions(long id)
        {
            SubjectManager subjectManager = new SubjectManager();

            User user = subjectManager.GetUserById(id); ;

            if (user != null)
            {
                ViewData["UserID"] = id;

                return PartialView("_FeaturePermissionsPartial");
            }
            else
            {
                return PartialView("_InfoPartial", new InfoModel("Window_Details", "The user does not exist!"));
            }
        }

        // S
        [GridAction]
        public ActionResult Users_Select()
        {
            SubjectManager subjectManager = new SubjectManager();

            // DATA
            IQueryable<User> data = subjectManager.GetAllUsers();

            List<UserModel> users = new List<UserModel>();
            data.ToList().ForEach(u => users.Add(UserModel.Convert(u)));

            return View(new GridModel<UserModel> { Data = users });
        }

        #endregion


        #region Validation

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
                    string error = String.Format(CultureInfo.InvariantCulture, "User name already exists.", userName);

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
                    string error = String.Format(CultureInfo.InvariantCulture, "Email address already exists.", email);

                    return Json(error, JsonRequestBehavior.AllowGet);
                }
            }
        }

        private static string ErrorCodeToErrorMessage(UserCreateStatus createStatus)
        {
            switch (createStatus)
            {
                case UserCreateStatus.DuplicateUserName:
                    return "The user name already exists.";

                case UserCreateStatus.InvalidUserName:
                    return "The user name is not valid.";

                case UserCreateStatus.DuplicateEmail:
                    return "The email address already exists.";

                case UserCreateStatus.InvalidPassword:
                    return "The password is invalid.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }

        private static string ErrorCodeToErrorKey(UserCreateStatus createStatus)
        {
            switch (createStatus)
            {
                case UserCreateStatus.DuplicateUserName:
                    return "UserName";

                case UserCreateStatus.InvalidUserName:
                    return "UserName";

                case UserCreateStatus.DuplicateEmail:
                    return "Email";

                case UserCreateStatus.InvalidPassword:
                    return "Password";

                default:
                    return "";
            }
        }

        #endregion
    }
}
