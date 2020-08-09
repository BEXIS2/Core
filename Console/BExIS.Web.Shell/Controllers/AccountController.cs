﻿using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authentication;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.Web.Shell.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Configuration;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.SessionState;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Web.Shell.Controllers
{
    public class AccountController : Controller
    {
        //
        // GET: /Account/ConfirmEmail
        public async Task<ActionResult> ConfirmEmail(long userId, string code)
        {
            var identityUserService = new IdentityUserService();
            var signInManager = new SignInManager(AuthenticationManager);

            try
            {
                if (code == null)
                {
                    return View("Error");
                }

                var result = await identityUserService.ConfirmEmailAsync(userId, code);
                if (!result.Succeeded) return View("Error");
                var user = await identityUserService.FindByIdAsync(userId);
                await signInManager.SignInAsync(user, false, false);

                var es = new EmailService();
                es.Send(MessageHelper.GetRegisterUserHeader(),
                    MessageHelper.GetRegisterUserMessage(user.Id, user.Name, user.Email),
                    ConfigurationManager.AppSettings["SystemEmail"]
                    );


                return this.IsAccessible("bam", "PartyService", "UserRegistration")
                    ? RedirectToAction("UserRegistration", "PartyService", new { area = "bam" })
                    : RedirectToAction("Index", "Home");
            }
            finally
            {
                identityUserService.Dispose();
                signInManager.Dispose();
            }

        }

        //
        // POST: /Account/ExternalLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            //ControllerContext.HttpContext.Session.RemoveAll();
            // Umleitung an den externen Anmeldeanbieter anfordern
            return new ChallengeResult(provider, Url.Action("ExternalLogin", "Account", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var signInManager = new SignInManager(AuthenticationManager);

            try
            {
                var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (loginInfo == null)
                {
                    return RedirectToAction("Login");
                }

                // Benutzer mit diesem externen Anmeldeanbieter anmelden, wenn der Benutzer bereits eine Anmeldung besitzt
                var result = await signInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
                switch (result)
                {
                    case SignInStatus.Success:
                        return RedirectToLocal(returnUrl);

                    case SignInStatus.LockedOut:
                        return View("Lockout");

                    case SignInStatus.RequiresVerification:
                        return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });

                    case SignInStatus.Failure:
                    default:
                        // Benutzer auffordern, ein Konto zu erstellen, wenn er kein Konto besitzt
                        ViewBag.ReturnUrl = returnUrl;
                        ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                        return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel() { Email = loginInfo.Email });
                }
            }
            finally
            {
                signInManager.Dispose();
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model,
            string returnUrl)
        {
            var identityUserService = new IdentityUserService();

            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Index", "Manage");
                }

                if (ModelState.IsValid)
                {
                    // Informationen zum Benutzer aus dem externen Anmeldeanbieter abrufen
                    var info = await AuthenticationManager.GetExternalLoginInfoAsync();
                    if (info == null)
                    {
                        return View("ExternalLoginFailure");
                    }
                    var user = new User { UserName = model.Email, Email = model.Email };

                    var result = await identityUserService.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        result = await identityUserService.AddLoginAsync(user.Id, info.Login);
                        if (result.Succeeded)
                        {
                            var signInManager = new SignInManager(AuthenticationManager);
                            await signInManager.SignInAsync(user, false, false);
                            return RedirectToLocal(returnUrl);
                        }
                    }
                    AddErrors(result);
                }

                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }
            finally
            {
                identityUserService.Dispose();
            }


        }

        //
        // GET: /Account/ExternalLoginFailure

        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        //
        // GET: /Account/ForgotPassword

        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            var identityUserService = new IdentityUserService();

            try
            {
                if (!ModelState.IsValid) return View(model);
                var user = await identityUserService.FindByEmailAsync(model.Email);
                if (user == null || !(await identityUserService.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                var code = await identityUserService.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code }, Request.Url.Scheme);
                await identityUserService.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }
            finally
            {
                identityUserService.Dispose();
            }
        }

        //
        // GET: /Account/ForgotPasswordConfirmation

        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/Login
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        public ActionResult ClearSession()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            return RedirectToAction("SessionTimeout", "Home", new { area = "" });
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            var identityUserService = new IdentityUserService();

            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                

                // Search for user by email, if not found search by user name
                var user = await identityUserService.FindByEmailAsync(model.UserName);

                if (user != null)
                {
                    model.UserName = user.UserName;
                }
                else
                { 
                    user = await identityUserService.FindByNameAsync(model.UserName);
                }
                
                // Require the user to have a confirmed email before they can log on.
                if (user != null)
                {
                    if (!await identityUserService.IsEmailConfirmedAsync(user.Id))
                    {
                        ViewBag.errorMessage = "You must have a confirmed email address to log in. Please check your email and verify your email address. If you did not receive an email, please also check your spam folder.";
                        return View("Error");
                    }
                }

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, change to shouldLockout: true
                var signInManager = new SignInManager(AuthenticationManager);

                var result =
                    await signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);
                switch (result)
                {
                    case SignInStatus.Success:
                        return RedirectToLocal(returnUrl);

                    case SignInStatus.LockedOut:
                        return View("Lockout");

                    default:
                        ModelState.AddModelError("", "Invalid login attempt.");
                        return View(model);
                }
            }
            finally
            {
                identityUserService.Dispose();
            }
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            //System.Web.HttpContext.Current.Session["menu_permission"] = null; // Remove permissions for menu
            System.Web.HttpContext.Current.Session.Abandon(); // Remove permissions for menu
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/Register

        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            var identityUserService = new IdentityUserService();

            try
            {
                if (!ModelState.IsValid) return View(model);


                var user = new User { UserName = model.UserName,FullName = model.UserName, Email = model.Email };

                var result = await identityUserService.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //var signInManager = new SignInManager(userManager, AuthenticationManager);
                    //await signInManager.SignInAsync(user, false, false);

                    // Weitere Informationen zum Aktivieren der Kontobestätigung und Kennwortzurücksetzung finden Sie unter "http://go.microsoft.com/fwlink/?LinkID=320771".
                    // E-Mail-Nachricht mit diesem Link senden
                    var code = await identityUserService.GenerateEmailConfirmationTokenAsync(user.Id);
                    await SendEmailConfirmationTokenAsync(user.Id, "Account registration - Verify your email address");

                    var es = new EmailService();
                    es.Send(MessageHelper.GetTryToRegisterUserHeader(),
                        MessageHelper.GetTryToRegisterUserMessage(user.Id, user.Name, user.Email),
                        ConfigurationManager.AppSettings["SystemEmail"]
                        );


                    ViewBag.Message = "Before you can log in to complete your registration, please check your email and verify your email address. If you did not receive an email, please also check your spam folder.";

                    return View("Info");
                }

                AddErrors(result);

                return View(model);
            }
            finally
            {
                identityUserService.Dispose();
            }



        }

        //
        // GET: /Account/ResetPassword

        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            var identityUserService = new IdentityUserService();

            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await identityUserService.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Nicht anzeigen, dass der Benutzer nicht vorhanden ist.
                    return RedirectToAction("ResetPasswordConfirmation", "Account");
                }

                var result = await identityUserService.ResetPasswordAsync(user.Id, model.Code, model.Password);
                if (result.Succeeded)
                {
                    user = await identityUserService.FindByEmailAsync(model.Email);
                    user.IsEmailConfirmed = true;
                    await identityUserService.UpdateAsync(user);
                    return RedirectToAction("ResetPasswordConfirmation", "Account");
                }

                AddErrors(result);
                return View();
            }
            finally
            {
                identityUserService.Dispose();
            }
        }

        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        private async Task<string> SendEmailConfirmationTokenAsync(long userId, string subject)
        {
            var identityUserService = new IdentityUserService();

            try
            {
                var code = await identityUserService.GenerateEmailConfirmationTokenAsync(userId);
                var callbackUrl = Url.Action("ConfirmEmail", "Account",
                   new { userId, code }, Request.Url.Scheme);

                var policyUrl = Url.Action("Index", "PrivacyPolicy", null, Request.Url.Scheme);
                var termsUrl = Url.Action("Index", "TermsAndConditions", null, Request.Url.Scheme);
                
                var applicationName = AppConfiguration.ApplicationName;

                await identityUserService.SendEmailAsync(userId, subject,
                    $"<p>Dear user,</p>" +
                    $"<p>please confirm your email address and complete your registration by clicking <a href=\"{callbackUrl}\">here</a>.</p>" +
                    $"<p>Once you finished the registration, a system administrator will decide based on your provided information about your assigned permissions. " +
                    $"This process can take up to 3 days.</p>" +
                    $"<p>You agreed on our <a href=\"{policyUrl}\">data policy</a> and <a href=\"{termsUrl}\">terms and conditions</a>.</p>" +
                    $"<br><p>Sincerely your {applicationName} administration team");

                return callbackUrl;
            }
            finally
            {
                identityUserService.Dispose();
            }
        }

        public async Task<ActionResult> Profile()
        {
            var identityUserService = new IdentityUserService();
            var userManager = new UserManager();

            try
            {
                long userId = 0;
                long.TryParse(this.User.Identity.GetUserId(), out userId);

                var user = identityUserService.FindById(userId);

                if (string.IsNullOrEmpty(user.Token))
                {
                    await userManager.SetTokenAsync(user);
                }

                user = identityUserService.FindById(userId);
                var token = await userManager.GetTokenAsync(user);

                return View(model: token);
            }
            finally
            {
                identityUserService.Dispose();
                userManager.Dispose();
            }
        }

        #region Hilfsprogramme

        // Wird für XSRF-Schutz beim Hinzufügen externer Anmeldungen verwendet
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }

        #endregion Hilfsprogramme
    }
}
