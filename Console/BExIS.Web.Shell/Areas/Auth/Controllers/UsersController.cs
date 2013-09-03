using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using BExIS.Security.Entities;
using BExIS.Security.Services;
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

                userManager.Create(model.UserName, model.Email, model.Password, model.SecurityQuestion, model.SecurityAnswer, model.Comment, true, 6, "qwertzuioplkjhgfdsayxcvbqwertztu", out createStatus);


                if (createStatus == UserCreateStatus.Success)
                {
                    return PartialView("_CreatePartial", model);
                }
                else
                {
                    ModelState.AddModelError(ErrorCodeToErrorKey(createStatus), ErrorCodeToErrorMessage(createStatus));
                }
            }

            return PartialView("_CreatePartial", model);
        }

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
                return PartialView("_InfoPartial", new InfoModel("windowDelete", "The user does not exist!"));
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

            return PartialView("_DeletePartial", model);
        }

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
                    user.Comment = model.Comment;

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

            User user = userManager.GetUserById(id);

            if (user != null)
            {
                ViewData["UserID"] = id;

                return PartialView("_MembershipPartial");
            }
            else
            {
                return PartialView("_InfoPartial", new InfoModel("windowDelete", "The user does not exist!"));
            }

            
        }

        [GridAction(EnableCustomBinding = true)]
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

                data.ToList().ForEach(r => roles.Add(UserRoleModel.Convert(r, roleManager.IsUserInRole(user.Name, r.Name))));
            }

            return View(new GridModel<UserRoleModel> { Data = roles });
        }

        // S
        [GridAction(EnableCustomBinding = true)]
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

                default:
                    return "";
            }
        }

        #endregion
    }
}
