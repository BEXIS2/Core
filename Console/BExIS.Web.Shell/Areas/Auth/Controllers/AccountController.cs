using System;
using System.Web.Mvc;
using System.Web.Security;
using BExIS.Security.Services;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Entities.Subjects;
using BExIS.Web.Shell.Areas.Auth.Models;
using System.Globalization;
using BExIS.Security.Services.Security;
using BExIS.Security.Entities.Security;
using System.Reflection;
using BExIS.Security.Providers.Authentication;
using System.Linq.Expressions;
using System.Linq;
using BExIS.Dlm.Entities.Data;

namespace BExIS.Web.Shell.Areas.Auth.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Error()
        {
            Func<Dataset, bool> x;

            x = d => d.Versions.AsQueryable().FirstOrDefault().Metadata.GetElementsByTagName("Title").Item(0).Value == "hallo";

            return View();
        }

        [ChildActionOnly]
        public ActionResult LogOnStatusPartial()
        {
            return PartialView("_LogOnStatusPartial");
        }

        //
        // GET: /Account/LogOn
        public ActionResult LogOn()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Error", "Account", new { area = "Auth" });
            }
            else
            {
                return View(new LogOnModel());
            }
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                AuthenticatorManager authenticatorManager = new AuthenticatorManager();

                Authenticator authenticator = authenticatorManager.GetAuthenticatorById(model.AuthenticatorList.Id);

                Assembly assembly = Assembly.Load(authenticator.ProjectPath);
                Type type = assembly.GetType(authenticator.ClassPath);

                IAuthenticationProvider authenticationProvider = (IAuthenticationProvider)Activator.CreateInstance(type, authenticator.ConnectionString);

                if (authenticationProvider.IsUserAuthenticated(model.UserName, model.Password))
                {
                    SubjectManager subjectManager = new SubjectManager();

                    if(!subjectManager.ExistsUserWithUserNameAndAuthenticatorId(model.UserName, model.AuthenticatorList.Id))
                    {
                        subjectManager.CreateUser(model.UserName, model.AuthenticatorList.Id);
                    }

                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home", new { area = "" });
                    }
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
                }
            }

            return View(model);
        }

        //
        // GET: /Account/LogOff

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        //
        // GET: /Account/Register

        public ActionResult Register()
        {
            if (Request.IsAuthenticated)
            {
                return RedirectToAction("Error", "Account", new { area = "Auth" });
            }
            else
            {
                return View();
            }
        }

        //
        // POST: /Account/Register

        [HttpPost]
        public ActionResult Register(AccountRegistrationModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                UserCreateStatus createStatus;

                SubjectManager subjectManager = new SubjectManager();

                try
                {
                    User user = subjectManager.CreateUser(model.UserName, model.Email, model.Password, model.SecurityQuestion, model.SecurityAnswer);

                    FormsAuthentication.SetAuthCookie(model.UserName, false /* createPersistentCookie */);
                    return RedirectToAction("RegisterSummary", UserModel.Convert(user));
                }
                catch (Exception e)
                {

                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult RegisterSummary(UserModel model)
        {
            return View(model);
        }

        //
        // GET: /Account/ChangePassword

        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Account/ChangePassword

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            if (ModelState.IsValid)
            {

                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return RedirectToAction("ChangePasswordSuccess");
                }
                else
                {
                    ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ChangePasswordSuccess

        public ActionResult ChangePasswordSuccess()
        {
            return View();
        }

        #region Validation

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
