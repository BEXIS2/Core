using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Security;
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

                UserManager userManager = new UserManager();

                userManager.Create(model.UserName, model.Email, model.Password, model.SecurityQuestion, model.SecurityAnswer, out createStatus);

                if (createStatus == UserCreateStatus.Success)
                {
                    return PartialView("_InfoPartial", new InfoModel("windowCreation", "The user was successfully created."));
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
            UserManager userManager = new UserManager();

            User user = userManager.GetUserById(id);

            if (user != null)
            {
                return PartialView("_DeletePartial", UserModel.Convert(user));         
            }
            else
            {
                return PartialView("_InfoPartial", new InfoModel("windowDeletion", "The user does not exist!"));
            }
        }

        [HttpPost]
        public ActionResult Delete(UserModel model)
        {
            UserManager userManager = new UserManager();

            User user = userManager.GetUserById(model.Id);

            if (user != null)
            {
                userManager.Delete(user);
            }

            return PartialView("_InfoPartial", new InfoModel("windowDeletion", "The user was successfully deleted."));
        }

        //
        // Details
        public ActionResult Details(long id)
        {
            return PartialView("_DetailsPartial", id);
        }

        public ActionResult UserInfo(long id)
        {
            UserManager userManager = new UserManager();

            User user = userManager.GetUserById(id);

            if (user != null)
            {
                return PartialView("_UserInfoPartial", UserModel.Convert(user));
            }
            else
            {
                return PartialView("_InfoPartial", new InfoModel("windowDetails", "The user does not exist!"));
            }
        }

        public ActionResult UserEdit(long id)
        {
            UserManager userManager = new UserManager();

            User user = userManager.GetUserById(id);

            if (user != null)
            {
                return PartialView("_UserEditPartial", UserModel.Convert(user));
            }
            else
            {
                return PartialView("_InfoPartial", new InfoModel("windowDetails", "The user does not exist!"));
            }
        }

        [HttpPost]
        public ActionResult UserEdit(UserModel model)
        {
            if (ModelState.IsValid)
            {
                UserManager userManager = new UserManager();

                User user = userManager.GetUserById(model.Id);

                if (user != null)
                {
                    user.Email = model.Email;

                    userManager.Update(user);

                    return PartialView("_UserInfoPartial", model);
                }
                else
                {
                    return PartialView("_InfoPartial", new InfoModel("windowDetails", "The user does not exist!"));
                }
            }

            return PartialView("_UserEditPartial", model);
        }

        // Membership
        public ActionResult Membership(long id)
        {
            UserManager userManager = new UserManager();

            User user = userManager.GetUserById(id);;

            if (user != null)
            {
                ViewData["UserID"] = id;

                return PartialView("_MembershipPartial");
            }
            else
            {
                return PartialView("_InfoPartial", new InfoModel("windowMembership", "The user does not exist!"));
            }
        }

        [GridAction]
        public ActionResult UserMembership_Select(long id)
        {
            RoleManager roleManager = new RoleManager();
            UserManager userManager = new UserManager();

            // DATA
            User user = userManager.GetUserById(id);

            List<UserRoleModel> roles = new List<UserRoleModel>();

            if (user != null)
            {
                IQueryable<Role> data = roleManager.GetAllRoles();

                data.ToList().ForEach(r => roles.Add(UserRoleModel.Convert(user.Id, r, roleManager.IsUserInRole(user, r))));
            }

            return View(new GridModel<UserRoleModel> { Data = roles });
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

        // S
        [GridAction]
        public ActionResult Users_Select()
        {
            UserManager userManager = new UserManager();

            // DATA
            IQueryable<User> data = userManager.GetAllUsers();

            List<UserModel> users = new List<UserModel>();
            data.ToList().ForEach(u => users.Add(UserModel.Convert(u)));

            return View(new GridModel<UserModel> { Data = users });
        }

        #endregion


        #region Validation

        public JsonResult ValidateUserName(string userName, long id = 0)
        {
            UserManager userManager = new UserManager();

            User user = userManager.GetUserByName(userName);

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
            UserManager userManager = new UserManager();

            User user = userManager.GetUserByEmail(email);

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
