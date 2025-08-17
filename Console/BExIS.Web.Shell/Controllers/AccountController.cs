using BExIS.App.Bootstrap;
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
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Web.Shell.Controllers
{
    public class AccountController : Controller
    {
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

                using (var identityUserService = new IdentityUserService())
                using (var signInManager = new SignInManager(AuthenticationManager))
                {
                    var result = await identityUserService.ConfirmEmailAsync(userId, code);
                    if (!result.Succeeded) return View("Error");

                    var user = await identityUserService.FindByIdAsync(userId);
                    await signInManager.SignInAsync(user, false, false);

                    using (var emailService = new EmailService())
                    {
                        emailService.Send(MessageHelper.GetRegisterUserHeader(), MessageHelper.GetRegisterUserMessage(user.Id, user.Name, user.Email), GeneralSettings.SystemEmail);
                    }

                    return this.IsAccessible("bam", "PartyService", "UserRegistration")
                    ? RedirectToAction("UserRegistration", "PartyService", new { area = "bam" })
                    : RedirectToAction("Index", "Home");
                }
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
                using (var signInManager = new SignInManager(AuthenticationManager))
                {
                    var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync();
                    if (loginInfo == null)
                    {
                        return RedirectToAction("Login");
                    }

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
                            ViewBag.ReturnUrl = returnUrl;
                            ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                            return View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel() { Email = loginInfo.Email });
                    }
                }
            }
            catch(Exception ex)
            {
                return RedirectToAction("Login");
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
                using (var identityUserService = new IdentityUserService())
                using (var signInManager = new SignInManager(AuthenticationManager))
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
                                await signInManager.SignInAsync(user, false, false);
                                return RedirectToLocal(returnUrl);
                            }
                        }
                        AddErrors(result);
                    }

                    ViewBag.ReturnUrl = returnUrl;
                    return View(model);
                }
            }
            catch(Exception ex)
            {
                return null;
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
                
                // Create professional email template
                var displayName = !string.IsNullOrEmpty(user.DisplayName) ? user.DisplayName : user.UserName;
                var applicationName = BExIS.Utils.Config.GeneralSettings.ApplicationName;
                if (string.IsNullOrEmpty(applicationName)) applicationName = "BEXIS2";
                
                var emailBody = $@"
                <html>
                <body style='font-family: Arial, sans-serif; color: #333;'>
                    <p>Dear {displayName},</p>
                    
                    <p>We received a request to reset the password for your account.</p>
                    <p>If you made this request, please reset your password by following the secure link below:</p>
                    
                    <p style='margin: 20px 0;'>
                        <a href='{callbackUrl}' style='background-color: #007bff; color: white; padding: 10px 20px; text-decoration: none; border-radius: 4px; display: inline-block;'>Reset your password</a>
                    </p>
                    
                    <p>If you did not request a password reset, you can safely ignore this message. Your account will remain unchanged.</p>
                    
                    <p>Best regards,<br>
                    Your {applicationName} Support Team</p>
                </body>
                </html>";
                
                await identityUserService.SendEmailAsync(user.Id, "Reset Password", emailBody);
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
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Log In", this.Session.GetTenant());
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
            try
            {

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, change to shouldLockout: true
                using (var signInManager = new SignInManager(AuthenticationManager))
                using (var identityUserService = new IdentityUserService())
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

                    // set-cookie
                    if (user != null)
                    {
                        var jwt = JwtHelper.GetTokenByUser(user);

                        // Create a new cookie
                        HttpCookie cookie = new HttpCookie("jwt", jwt);

                        // Set additional properties if needed
                        cookie.Expires = DateTime.Now.AddDays(1); // Expires in 1 day
                        cookie.Domain = Request.Url.Host; // Set the domain
                        cookie.Path = "/"; // Set the path

                        // Add the cookie to the response
                        Response.Cookies.Add(cookie);
                    }

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
            var identityUserService = new IdentityUserService();

            try
            {
                if (!ModelState.IsValid) return View(model);

                if (!string.IsNullOrEmpty(model.Extra))
                    return View(model);

                var user = new User { UserName = model.UserName, FullName = model.UserName, Email = model.Email, HasTermsAndConditionsAccepted = model.TermsAndConditions };

                var result = await identityUserService.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    //var signInManager = new SignInManager(userManager, AuthenticationManager);
                    //await signInManager.SignInAsync(user, false, false);

                    // Weitere Informationen zum Aktivieren der Kontobestätigung und Kennwortzurücksetzung finden Sie unter "http://go.microsoft.com/fwlink/?LinkID=320771".
                    // E-Mail-Nachricht mit diesem Link senden
                    var code = await identityUserService.GenerateEmailConfirmationTokenAsync(user.Id);
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
            finally
            {
                identityUserService.Dispose();
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

                var applicationName = GeneralSettings.ApplicationName;

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