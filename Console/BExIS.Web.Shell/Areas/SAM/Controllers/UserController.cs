using BExIS.Modules.Sam.UI.Models;
using System;
using System.Web.Mvc;
using Telerik.Web.Mvc;

namespace BExIS.Modules.Sam.UI.Controllers
{
    public class UserController : Controller
    {
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreateUserModel model)
        {
            //if (ModelState.IsValid)
            //{
            //    SubjectManager subjectManager = new SubjectManager();
            //    User user = subjectManager.CreateUser(model.Username, model.Password, model.FullName, model.Email, model.SecurityQuestion, model.SecurityAnswer, model.AuthenticatorList.Id);

            //    // Feature
            //    FeatureManager featureManager = new FeatureManager();
            //    Feature feature = featureManager.FeaturesRepo.Get(f => f.Name == "Search").FirstOrDefault();

            //    // Permissions
            //    PermissionManager permissionManager = new PermissionManager();
            //    permissionManager.CreateFeaturePermission(user.Id, feature.Id);

            //    return Json(new { success = true });
            //}

            //return PartialView("_CreatePartial", model);

            return View(model);
        }

        [HttpPost]
        public void Delete(long id)
        {
            throw new NotImplementedException();
        }

        public ActionResult Edit(long id)
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(EditUserModel model)
        {
            //if (ModelState.IsValid)
            //{
            //    SubjectManager subjectManager = new SubjectManager();

            //    User user = subjectManager.GetUserById(model.UserId);

            //    if (model.Password == model.ConfirmPassword && model.Password != null)
            //    {
            //        subjectManager.ChangePassword(user.Id, model.Password);
            //    }

            //    user.FullName = model.FullName;
            //    user.Email = model.Email;

            //    user.IsApproved = model.IsApproved;
            //    user.IsBlocked = model.IsBlocked;
            //    user.IsLockedOut = model.IsLockedOut;

            //    // removing
            //    long[] groups = user.Groups.Select(g => g.Id).ToArray();

            //    foreach (long groupId in groups)
            //    {
            //        subjectManager.RemoveUserFromGroup(user.Id, groupId);
            //    }

            //    //adding
            //    if (Session["Groups"] != null)
            //    {
            //        foreach (UserMembershipGridRowModel group in (UserMembershipGridRowModel[])Session["Groups"])
            //        {
            //            if (group.IsUserInGroup)
            //            {
            //                subjectManager.AddUserToGroup(user.Id, group.Id);
            //            }
            //        }
            //    }

            //    subjectManager.UpdateUser(user);

            //    return Json(new { success = true });
            //}
            //else
            //{
            //    return PartialView("_EditPartial", model);
            //}

            return View(model);
        }

        public ActionResult Index()
        {
            return View();
        }

        [GridAction]
        public ActionResult Users_Select()
        {
            throw new NotImplementedException();
        }

        public JsonResult ValidateEmail(string email, long id = 0)
        {
            //SubjectManager subjectManager = new SubjectManager();

            //User user = subjectManager.GetUserByEmail(email);

            //if (user == null)
            //{
            //    return Json(true, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
            //    if (user.Id == id)
            //    {
            //        return Json(true, JsonRequestBehavior.AllowGet);
            //    }
            //    else
            //    {
            //        string error = String.Format(CultureInfo.InvariantCulture, "The Email Address exists already.", email);

            //        return Json(error, JsonRequestBehavior.AllowGet);
            //    }
            //}

            throw new NotImplementedException();
        }

        public JsonResult ValidateUsername(string username, long id = 0)
        {
            //SubjectManager subjectManager = new SubjectManager();

            //User user = subjectManager.GetUserByName(username);

            //if (user == null)
            //{
            //    return Json(true, JsonRequestBehavior.AllowGet);
            //}
            //else
            //{
            //    if (user.Id == id)
            //    {
            //        return Json(true, JsonRequestBehavior.AllowGet);
            //    }
            //    else
            //    {
            //        string error = String.Format(CultureInfo.InvariantCulture, "The Username exists already.", username);

            //        return Json(error, JsonRequestBehavior.AllowGet);
            //    }
            //}

            throw new NotImplementedException();
        }
    }
}