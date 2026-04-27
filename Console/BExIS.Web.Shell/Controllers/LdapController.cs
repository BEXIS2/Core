using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authentication;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Config;
using BExIS.Utils.Config.Configurations;
using BExIS.Web.Shell.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Web.Shell.Controllers
{
    public class LdapController : Controller
    {
        private List<LdapConfiguration> _ldapConfigurations;
        private readonly UserManager _userManager;

        private SignInManager SignInManager => HttpContext.GetOwinContext().Get<SignInManager>();

        public LdapController(UserManager userManager)
        {
            _ldapConfigurations = GeneralSettings.LdapConfigurations;
            _userManager = userManager;
        }

        // GET: Ldap
        public ActionResult Login(string name, string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var model = new LdapLoginViewModel()
            {
                Name = name
            };

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LdapLoginViewModel model, string returnUrl)
        {
            try
            {
                var ldapAuthenticationManager = new LdapAuthenticationManager();
                // first, check always username and password at the corresponding ldap server
                var result = ldapAuthenticationManager.ValidateUser(model.Name, model.UserName, model.Password);
                switch (result)
                {
                    // credentials are valid
                    case SignInStatus.Success:
                        var user = await _userManager.FindByNameAsync(model.UserName);

                        if (user != null)
                        {
                            if (string.IsNullOrEmpty(user.Email))
                            {
                                ViewBag.ReturnUrl = returnUrl;
                                return View("Confirm", LdapLoginConfirmModel.Convert(user, model.Name));
                            }

                            if (!await _userManager.IsEmailConfirmedAsync(user.Id))
                            {
                                ViewBag.ErrorMessage = "You must have a confirmed email address to log in.";
                                return View("Error");
                            }

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
                        }
                        else
                        {
                            var ldapUser = new User() { Name = model.UserName, SecurityStamp = Guid.NewGuid().ToString() };
                            await _userManager.CreateAsync(ldapUser);
                            await _userManager.AddLoginAsync(ldapUser.Id, new UserLoginInfo(model.Name, ""));

                            ViewBag.ReturnUrl = returnUrl;
                            return View("Confirm", LdapLoginConfirmModel.Convert(ldapUser, model.Name));
                        }

                    default:
                        ModelState.AddModelError("", "Invalid login attempt.");
                        return View(model);
                }
            }
            catch (Exception ex)
            {
                return View(model);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Confirm(LdapLoginConfirmModel model, string returnUrl)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (_userManager.FindByEmailAsync(model.Email).Result != null)
                {
                    ModelState.AddModelError("email", "The email is already in use.");
                    return View(model);
                }

                var user = await _userManager.FindByIdAsync(model.Id);

                if (user != null)
                {
                    if (user.Logins.Any(l => l.LoginProvider.Equals(model.Name, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        //user.HasPrivacyPolicyAccepted = model.PrivacyPolicy;
                        user.HasTermsAndConditionsAccepted = model.TermsAndConditions;
                        user.Email = model.Email;

                        await _userManager.UpdateAsync(user);

                        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user.Id);
                        await SendEmailConfirmationTokenAsync(user.Id, "Account registration - Verify your email address");

                        ViewBag.Message = "Before you can log in to complete your registration please check your email and verify your email address.";

                        return View("Info");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid user - no ldap authentication.");
                        return View(model);
                    }
                }

                return RedirectToLocal(returnUrl);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while confirming your email address.");
                return View(model);
            }
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

                await _userManager.SendEmailAsync(userId, subject,
                    $"<p>please confirm your mail address and complete your registration by clicking <a href=\"{callbackUrl}\">here</a>." +
                    $" Once you finished the registration a system administrator will decide based on your provided information about your assigned permissions. " +
                    $"This process can take up to 3 days.</p>" +
                    $"<p>You agreed on our <a href=\"{policyUrl}\">data policy</a> and <a href=\"{termsUrl}\">terms and conditions</a>.</p>");

                return callbackUrl;
            }
            catch (Exception ex)
            {
                throw new Exception("Error sending email confirmation token.", ex);
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

        private IAuthenticationManager AuthenticationManager => HttpContext.GetOwinContext().Authentication;
    }
}