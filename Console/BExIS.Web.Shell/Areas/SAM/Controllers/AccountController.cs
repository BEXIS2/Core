using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Security;
using BExIS.Security.Entities.Authentication;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authentication;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.Web.Shell.Areas.SAM.Models;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;
using Vaiona.Logging;

namespace BExIS.Web.Shell.Areas.SAM.Controllers
{
    public class AccountController : Controller
    {
        public ActionResult Error()
        {
            return View();
        }

        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            return View();
        }

        public ActionResult MyAccount()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("My Account", this.Session.GetTenant());

            SubjectManager subjectManager = new SubjectManager();
            User user = subjectManager.GetUserByName(HttpContext.User.Identity.Name);

            if (user != null)
            {
                return View("MyAccount", MyAccountModel.Convert(user));
            }
            else
            {
                return View("MyAccount");
            }
            
        }

        [HttpPost]
        public ActionResult MyAccount(MyAccountModel model)
        {
            if (!ModelState.IsValid) return View("MyAccount", model);

            SubjectManager subjectManager = new SubjectManager();

            User user = subjectManager.GetUserById(model.UserId);

            if (model.Password == model.ConfirmPassword && model.Password != null)
            {
                subjectManager.ChangePassword(user.Id, model.Password);
            }

            if (model.SecurityAnswer != null)
            {
                subjectManager.ChangeSecurityQuestionAndSecurityAnswer(user.Id, model.SecurityQuestionId, model.SecurityAnswer);
            }

            user.Email = model.Email;
            user.FullName = model.FullName;

            subjectManager.UpdateUser(user);

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        [ChildActionOnly]
        public ActionResult LogOnStatusPartial()
        {
            return PartialView("_LogOnStatusPartial");
        }

        public ActionResult LogOn(string returnUrl)
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Home", this.Session.GetTenant());

            if (Request.IsAuthenticated)
            {
                return View("Error");
            }
            else
            {
                if (returnUrl != null)
                {
                    Session["ReturnUrl"] = returnUrl;
                    return View("LogOn");
                }
                else
                {
                    Session["ReturnUrl"] = "";
                    return PartialView("_LogOnPartial", new AccountLogOnModel());
                }
            }
        }

        [HttpPost]
        public ActionResult LogOn(AccountLogOnModel model)
        {
            if (ModelState.IsValid)
            {
                #region Authenticator

                AuthenticatorManager authenticatorManager = new AuthenticatorManager();
                Authenticator authenticator = authenticatorManager.GetAuthenticatorById(model.AuthenticatorList.Id);
                Assembly assembly = Assembly.Load(authenticator.AssemblyPath);
                Type type = assembly.GetType(authenticator.ClassPath);

                #endregion Authenticator

                #region AuthenticationManager

                IAuthenticationManager authenticationManager = (IAuthenticationManager)Activator.CreateInstance(type, authenticator.ConnectionString);

                #endregion AuthenticationManager

                if (authenticationManager.ValidateUser(model.Username, model.Password))
                {
                    SubjectManager subjectManager = new SubjectManager();

                    if (!subjectManager.ExistsUsernameWithAuthenticatorId(model.Username, model.AuthenticatorList.Id))
                    {
                        subjectManager.CreateUser(model.Username, model.AuthenticatorList.Id);
                    }

                    FormsAuthentication.SetAuthCookie(model.Username, false);

                    return Json(new { success = true });
                }
                else
                {
                    ModelState.AddModelError("", "The Username or Password provided is incorrect.");
                }
            }

            return PartialView("_LogOnPartial", model);
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home", new { area = "" });
        }

        public ActionResult Register()
        {
            return PartialView("_RegisterPartial", new AccountRegisterModel());
        }

        [HttpPost]
        public ActionResult Register(AccountRegisterModel model)
        {
            if (ModelState.IsValid)
            {
                SubjectManager subjectManager = new SubjectManager();

                User user = subjectManager.CreateUser(model.Username, model.Password, model.FullName, model.Email, model.SecurityQuestion, model.SecurityAnswer, model.AuthenticatorList.Id);
                LoggerFactory.LogData(user.Id.ToString(), typeof(User).Name, Vaiona.Entities.Logging.CrudState.Created);

                // Feature
                FeatureManager featureManager = new FeatureManager();
                Feature feature = featureManager.FeaturesRepo.Get(f => f.Name == "Search").FirstOrDefault();

                // Permissions
                PermissionManager permissionManager = new PermissionManager();
                permissionManager.CreateFeaturePermission(user.Id, feature.Id);

                FormsAuthentication.SetAuthCookie(model.Username, false);
                return Json(new { success = true });
            }

            return PartialView("_RegisterPartial", model);
        }

        #region Validation

        public JsonResult ValidateEmail(string email, long userId = 0)
        {
            SubjectManager subjectManager = new SubjectManager();

            User user = subjectManager.GetUserByEmail(email);

            if (user == null)
            {
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (user.Id == userId)
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

        public JsonResult ValidateTermsAndConditions(bool accepted)
        {
            return accepted ? Json(true, JsonRequestBehavior.AllowGet) : Json(false, JsonRequestBehavior.AllowGet);
        }

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
                    string error = String.Format(CultureInfo.InvariantCulture, "User name already exists.", username);

                    return Json(error, JsonRequestBehavior.AllowGet);
                }
            }
        }

        #endregion Validation

        public string RenderRazorViewToString()
        {
            Controller controller = this;
            string viewName = "_LogOnPartial";
            AccountLogOnModel model = new AccountLogOnModel();

            controller.ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controller.ControllerContext, viewName);
                var viewContext = new ViewContext(controller.ControllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}