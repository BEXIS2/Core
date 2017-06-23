using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Security;
using System.Linq;
using BExIS.Security.Entities.Authentication;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authentication;
using BExIS.Security.Services.Subjects;
using BExIS.Web.Shell.Areas.SAM.Models;
using Vaiona.Web.Mvc.Models;
using BExIS.Dlm.Services.Party;
using System.Configuration;
using System.Collections.Generic;
using BExIS.Dlm.Entities.Party;

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
            ViewBag.Title = PresentationModel.GetViewTitle("My Account");

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

        public ActionResult Registeration()
        {

            //Select all the parties which are defined in web.config
            //Defined AccountPartyTypes vallue in web config format is like PartyType1:PartyTypePairTitle1-PartyTypePairTitle2,PartyType2
            var accountPartyTypes = new List<string>();
            var partyTypeAccountModel = new List<BAM.Models.PartyTypeAccountModel>();
            var pm = new PartyTypeManager();
            var pr = new PartyRelationshipTypeManager();
            //Split them by "," and split each one by ":"
            foreach (string partyTypeAndRelationsStr in ConfigurationManager.AppSettings["AccountPartyTypes"].Split(','))
            {
                var partyTypeAndRelations = partyTypeAndRelationsStr.Split(':');
                var partyType = pm.Repo.Get(item => item.Title == partyTypeAndRelations[0]).FirstOrDefault();
                if (partyType == null)
                    throw new Exception("accountPartyType format in app setting is not correct or this 'partyType' doesn't exist.");
                var allowedPartyTypePairs = new Dictionary<string, PartyTypePair>();
                if (partyTypeAndRelations.Length > 1)
                {
                    var partyRelationshipsTypeStr = partyTypeAndRelations[1].Split('-');
                    var partyRelationshipsType = pr.Repo.Get(item => partyRelationshipsTypeStr.Contains(item.Title));

                    foreach (var partyRelationshipType in partyRelationshipsType)
                    {
                        //filter AssociatedPairs to allowed pairs
                        partyRelationshipType.AssociatedPairs = partyRelationshipType.AssociatedPairs.Where(item => partyType.Id == item.AllowedSource.Id && item.AllowedTarget.Parties.Any()).ToList();

                        //try to find first type pair witch has PartyRelationShipTypeDefault otherwise the first one 
                        var defaultPartyTypePair = partyRelationshipType.AssociatedPairs.FirstOrDefault(item => item.PartyRelationShipTypeDefault);
                        if (defaultPartyTypePair == null)
                            defaultPartyTypePair = partyRelationshipType.AssociatedPairs.FirstOrDefault();
                        if (defaultPartyTypePair != null)
                            allowedPartyTypePairs.Add(partyRelationshipType.DisplayName, defaultPartyTypePair);
                    }
                }
                partyTypeAccountModel.Add(new BAM.Models.PartyTypeAccountModel()
                {
                    PartyType = partyType,
                    PartyRelationshipTypes = allowedPartyTypePairs
                });

            }
            return View(partyTypeAccountModel);
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

                if (authenticationManager.ValidateUser(model.UserName, model.Password))
                {
                    SubjectManager subjectManager = new SubjectManager();

                    if (!subjectManager.ExistsUserNameWithAuthenticatorId(model.UserName, model.AuthenticatorList.Id))
                    {
                        subjectManager.CreateUser(model.UserName, model.AuthenticatorList.Id);
                    }

                    FormsAuthentication.SetAuthCookie(model.UserName, false);

                    return Json(new { success = true });
                }
                else
                {
                    ModelState.AddModelError("", "The user name or password provided is incorrect.");
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

                User user = subjectManager.CreateUser(model.UserName, model.Password, model.FullName, model.Email, model.SecurityQuestionList.Id, model.SecurityAnswer, model.AuthenticatorList.Id);

                FormsAuthentication.SetAuthCookie(model.UserName, false);
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