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
        public ActionResult Index()
        {
            return View();
        }

        #region Grid View

        // A
        public bool AddUserToGroup(long userId, long groupId)
        {
            SubjectManager subjectManager = new SubjectManager();

            return subjectManager.AddUserToGroup(userId, groupId);
        }

        // C
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

        // E
        public ActionResult Edit(long id)
        {
            SubjectManager subjectManager = new SubjectManager();

            User user = subjectManager.GetUserById(id);

            if (user != null)
            {
                return PartialView("_EditPartial", UserUpdateModel.Convert(user));
            }
            else
            {
                return PartialView("_InfoPartial", new InfoModel("Window_Details", "The user does not exist!"));
            }
        }

        [HttpPost]
        public ActionResult Edit(UserUpdateModel model)
        {
            if (ModelState.IsValid)
            {
                SubjectManager subjectManager = new SubjectManager();

                User user = subjectManager.GetUserById(model.Id);

                if (user != null)
                {
                    user.FullName = model.FullName;
                    user.Email = model.Email;

                    subjectManager.UpdateUser(user);

                    return PartialView("_ShowPartial", UserReadModel.Convert(user));
                }
                else
                {
                    return PartialView("_InfoPartial", new InfoModel("Window_Details", "The user does not exist!"));
                }
            }

            return PartialView("_EditPartial", model);
        }

        // G
        public ActionResult Users()
        {
            return View();
        }

        [GridAction]
        public ActionResult Users_Select()
        {
            SubjectManager subjectManager = new SubjectManager();

            // DATA
            IQueryable<User> data = subjectManager.GetAllUsers();

            List<UserGridRowModel> groups = new List<UserGridRowModel>();
            data.ToList().ForEach(u => groups.Add(UserGridRowModel.Convert(u)));

            return View(new GridModel<UserGridRowModel> { Data = groups });
        }

        // M
        public ActionResult Membership(long id)
        {
            SubjectManager subjectManager = new SubjectManager();

            User user = subjectManager.GetUserById(id); ;

            if (user != null)
            {
                ViewData["UserId"] = id;

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

            List<UserMembershipGridRowModel> groups = new List<UserMembershipGridRowModel>();

            if (user != null)
            {
                IQueryable<Group> data = subjectManager.GetAllGroups();

                data.ToList().ForEach(g => groups.Add(UserMembershipGridRowModel.Convert(user.Id, g, subjectManager.IsUserInGroup(user.Id, g.Id))));
            }

            return View(new GridModel<UserMembershipGridRowModel> { Data = groups });
        }

        // R
        public bool RemoveUserFromGroup(long userId, long groupId)
        {
            SubjectManager subjectManager = new SubjectManager();

            return subjectManager.RemoveUserFromGroup(userId, groupId);
        }

        // S
        public ActionResult Show(long id)
        {
            SubjectManager subjectManager = new SubjectManager();

            User user = subjectManager.GetUserById(id);

            if (user != null)
            {
                return PartialView("_ShowPartial", UserReadModel.Convert(user));
            }
            else
            {
                return PartialView("_InfoPartial", new InfoModel("Window_Details", "The user does not exist!"));
            }
        }

        // U
        public ActionResult Update(long id)
        {
            return PartialView("_UpdatePartial", id);
        }

        #endregion

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
    }
}
