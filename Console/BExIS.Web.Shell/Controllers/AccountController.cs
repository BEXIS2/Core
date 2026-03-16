using BExIS.App.Bootstrap;
using BExIS.App.Bootstrap.Helpers;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authentication;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Config;
using BExIS.Web.Shell.Models;
using Exceptionless;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NHibernate.Criterion;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Web.Shell.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager _userManager;

        private SignInManager SignInManager => HttpContext.GetOwinContext().Get<SignInManager>();

        public AccountController(UserManager userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult ClearSession()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);

            return RedirectToAction("SessionTimeout", "Home", new { area = "" });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task<ActionResult> ConfirmEmail(long userId, string code)
        {
            try
            {
                if (String.IsNullOrEmpty(code))
                {
                    return View("Error");
                }

                var result = await _userManager.ConfirmEmailAsync(userId, code);
                if (!result.Succeeded) return View("Error");

                var user = await _userManager.FindByIdAsync(userId);
                await SignInManager.SignInAsync(user, false, false);

                using (var emailService = new EmailService())
                {
                    emailService.Send(MessageHelper.GetRegisterUserHeader(), MessageHelper.GetRegisterUserMessage(user.Id, user.Name, user.Email), GeneralSettings.SystemEmail);
                }

                return this.IsAccessible("bam", "PartyService", "UserRegistration")
                ? RedirectToAction("UserRegistration", "PartyService", new { area = "bam" })
                : RedirectToAction("Index", "Home");
            }
            catch(Exception ex)
            {
                return View("Error");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLogin(string provider, string returnUrl)
        {
            return await Task.FromResult(new ChallengeResult(provider, Url.Action("ExternalLogin", "Account", new { ReturnUrl = returnUrl })));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            try
            {
                var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
                if (loginInfo == null)
                {
                    return RedirectToAction("Login");
                }

                var result = await SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
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
                        ViewBag.ReturnUrl = returnUrl;
                        ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                        return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel() { Email = loginInfo.Email });
                }
            }
            catch (Exception ex)
            {
                ExceptionlessClient.Default.SubmitLog("Account Registration", $"Error while registering user.", Exceptionless.Logging.LogLevel.Error);
                return View("Error");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
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

                    var result = await _userManager.CreateAsync(user);
                    if (result.Succeeded)
                    {
                        result = await _userManager.AddLoginAsync(user.Id, info.Login);
                        if (result.Succeeded)
                        {
                            await SignInManager.SignInAsync(user, false, false);
                            return RedirectToLocal(returnUrl);
                        }
                    }
                    AddErrors(result);
                }

                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }
            catch (Exception ex)
            {
                ExceptionlessClient.Default.SubmitLog("Account Registration", $"Error while registering user {model.Email}.", Exceptionless.Logging.LogLevel.Error);
                return View("Error");
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
            try
            {
                if (!ModelState.IsValid) return View(model);
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null || !(await _userManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                var code = await _userManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code }, Request.Url.Scheme);
                await _userManager.SendEmailAsync(user.Id, "Password Reset",
                    $"<p>Dear {user.DisplayName ?? user.Name},</p>" +
                    $"<p>We received a request to reset the password for your account. " +
                    $"If you made this request, please reset your password by following the secure link below:</p>" +
                    $"<p><a href=\"{callbackUrl}\">Reset Password</a></p>" +
                    $"<p>If you did not request a password reset, you can safely ignore this message. Your account will remain unchanged.</p>" +
                    $"<p>Best regards,</br>" +
                    $"Your Support Team</p>");
                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
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
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Login", this.Session.GetTenant());
            ViewBag.ReturnUrl = returnUrl;

            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Search for user by email, if not found search by username
                var user = await _userManager.FindByEmailAsync(model.UserName);

                if (user != null)
                {
                    model.UserName = user.UserName;
                }
                else
                {
                    user = await _userManager.FindByNameAsync(model.UserName);
                }

                // Require the user to have a confirmed email before they can log on.
                if (user != null)
                {
                    if (!await _userManager.IsEmailConfirmedAsync(user.Id))
                    {
                        ViewBag.errorMessage = "You must have a confirmed email address to log in. Please check your email and verify your email address. If you did not receive an email, please also check your spam folder.";
                        return View("Error");
                    }
                }

                // set-cookie
                if (user != null)
                {
                    var jwt = JwtHelper.GetTokenByUser(user);

                    // Create a new cookie
                    HttpCookie cookieJwt = new HttpCookie("jwt", jwt);

                    // Set additional properties if needed
                    cookieJwt.Expires = DateTime.Now.AddDays(1); // Expires in 1 day
                    cookieJwt.Domain = Request.Url.Host; // Set the domain
                    cookieJwt.Path = "/"; // Set the path

                    // Add the cookie to the response
                    Response.Cookies.Add(cookieJwt);


                }

                var result =
                    await SignInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);
                switch (result)
                {
                    case SignInStatus.Success:

                        // Standard-Identity erzeugen (enthält alle Standard-Claims + Rollen)
                        var identity = await _userManager.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);

                        // Deinen Claim hinzufügen
                        identity.AddClaim(new Claim("DisplayName", user.DisplayName ?? user.UserName ?? ""));

                        // Manuelles Sign-In mit der erweiterten Identity
                        var authManager = HttpContext.GetOwinContext().Authentication;
                        authManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
                        authManager.SignIn(new AuthenticationProperties { IsPersistent = model.RememberMe }, new ClaimsIdentity(identity)
                        );

                        return RedirectToLocal(returnUrl);

                    case SignInStatus.LockedOut:
                        return View("Lockout");

                    default:
                        ModelState.AddModelError("", "Invalid login attempt.");
                        return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
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

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public new async Task<ActionResult> Profile()
        {
            return await Task.FromResult(View());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
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
            try
            {
                if (!ModelState.IsValid) return View(model);

                if (!string.IsNullOrEmpty(model.Extra))
                    return View(model);

                var user = new User { UserName = model.UserName, FullName = model.UserName, Email = model.Email, HasTermsAndConditionsAccepted = model.TermsAndConditions };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //var signInManager = new SignInManager(userManager, AuthenticationManager);
                    //await signInManager.SignInAsync(user, false, false);

                    // Weitere Informationen zum Aktivieren der Kontobestätigung und Kennwortzurücksetzung finden Sie unter "http://go.microsoft.com/fwlink/?LinkID=320771".
                    // E-Mail-Nachricht mit diesem Link senden
                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    await SendEmailConfirmationTokenAsync(user.Id, "Account registration - Verify your email address");

                    using (var emailService = new EmailService())
                    {
                        emailService.Send(MessageHelper.GetTryToRegisterUserHeader(),
                            MessageHelper.GetTryToRegisterUserMessage(user.Id, user.Name, user.Email),
                            GeneralSettings.SystemEmail
                            );
                    }

                    ViewBag.Message = "Before you can log in to complete your registration, please check your email and verify your email address. If you did not receive an email, please also check your spam folder.";

                    ExceptionlessClient.Default.SubmitLog("Account Registration", $"{user.Name} has registered sucessfully.", Exceptionless.Logging.LogLevel.Info);

                    return View("Info");
                }

                AddErrors(result);

                return View(model);
            }
            catch (Exception ex)
            {
                ExceptionlessClient.Default.SubmitLog("Account Registration", $"Error while registering user {model.UserName}.", Exceptionless.Logging.LogLevel.Error);
                return View("Error");
            }
        }

        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // GET: /Account/ResetPassword
        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Nicht anzeigen, dass der Benutzer nicht vorhanden ist.
                    return RedirectToAction("ResetPasswordConfirmation", "Account");
                }

                var result = await _userManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
                if (result.Succeeded)
                {
                    user = await _userManager.FindByEmailAsync(model.Email);
                    user.IsEmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                    return RedirectToAction("ResetPasswordConfirmation", "Account");
                }

                AddErrors(result);
                return View();
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        private async Task<string> SendEmailConfirmationTokenAsync(long userId, string subject)
        {
            try
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(userId);
                var callbackUrl = Url.Action("ConfirmEmail", "Account",
                   new { userId, code }, Request.Url.Scheme);

                var policyUrl = Url.Action("Index", "PrivacyPolicy", null, Request.Url.Scheme);
                var termsUrl = Url.Action("Index", "TermsAndConditions", null, Request.Url.Scheme);

                var applicationName = GeneralSettings.ApplicationName;

                await _userManager.SendEmailAsync(userId, subject,
                    $"<p>Dear user,</p>" +
                    $"<p>please confirm your email address and complete your registration by clicking <a href=\"{callbackUrl}\">here</a>.</p>" +
                    $"<p>Once you finished the registration, a system administrator will decide based on your provided information about your assigned permissions. " +
                    $"This process can take up to 3 days.</p>" +
                    $"<p>You agreed on our <a href=\"{policyUrl}\">data policy</a> and <a href=\"{termsUrl}\">terms and conditions</a>.</p>" +
                    $"<br><p>Sincerely your {applicationName} administration team");

                return callbackUrl;
            }
            catch (Exception ex)
            {
                ExceptionlessClient.Default.SubmitLog("Account Registration", $"Error while sending email confirmation token to user {userId}.", Exceptionless.Logging.LogLevel.Error);
                return null;
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