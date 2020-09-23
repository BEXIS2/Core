﻿using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authentication;
using BExIS.Security.Services.Subjects;
using BExIS.Web.Shell.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Web.Shell.Controllers
{
    public class LdapController : Controller
    {
        // GET: Ldap
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            var ldapAuthenticationManager = new LdapAuthenticationManager(ConfigurationManager.AppSettings["Ldap_ConnectionString"]);
            var userManager = new UserManager();
            var signInManager = new SignInManager(AuthenticationManager);
            var identityUserService = new IdentityUserService();

            try
            {
                var user = await userManager.FindByNameAsync(model.UserName);

                if (user != null)
                {
                    if (user.Logins.Any(l => l.LoginProvider == "Ldap"))
                    {
                        if(string.IsNullOrEmpty(user.Email))
                        {
                            ViewBag.ReturnUrl = returnUrl;
                            return View("Confirm", LoginConfirmModel.Convert(user));
                        }

                        if (!await identityUserService.IsEmailConfirmedAsync(user.Id))
                        {
                            ViewBag.ErrorMessage = "You must have a confirmed email address to log in.";
                            return View("Error");
                        }

                        SignInStatus result = ldapAuthenticationManager.ValidateUser(model.UserName, model.Password);
                        switch (result)
                        {
                            case SignInStatus.Success:
                                var identity = await identityUserService.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                                AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = true }, identity);
                                return RedirectToLocal(returnUrl);

                            default:
                                ModelState.AddModelError("", "Invalid login attempt.");
                                return View(model);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid login attempt.");
                        return View(model);
                    }
                }
                else
                {
                    var result = ldapAuthenticationManager.ValidateUser(model.UserName, model.Password);
                    switch (result)
                    {
                        case SignInStatus.Success:
                            var ldapUser = new User() { Name = model.UserName, SecurityStamp = Guid.NewGuid().ToString() };
                            await userManager.CreateAsync(ldapUser);
                            await userManager.AddLoginAsync(ldapUser, new UserLoginInfo("Ldap", ""));

                            ViewBag.ReturnUrl = returnUrl;
                            return View("Confirm", LoginConfirmModel.Convert(ldapUser));

                        default:
                            ModelState.AddModelError("", "Invalid login attempt.");
                            return View(model);
                    }
                }
            }
            finally
            {
                ldapAuthenticationManager.Dispose();
                userManager.Dispose();
                signInManager.Dispose();
                identityUserService.Dispose();
            }
        }

        [HttpPost]
        public async Task<ActionResult> Confirm(LoginConfirmModel model, string returnUrl)
        {
            var userManager = new UserManager();
            var identityUserService = new IdentityUserService();

            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await userManager.FindByIdAsync(model.Id);

                if (user != null)
                {
                    if (user.Logins.Any(l => l.LoginProvider == "Ldap"))
                    {
                        user.HasPrivacyPolicyAccepted = model.PrivacyPolicy;
                        user.HasTermsAndConditionsAccepted = model.TermsAndConditions;
                        user.Email = model.Email;

                        await userManager.UpdateAsync(user);

                        var code = await identityUserService.GenerateEmailConfirmationTokenAsync(user.Id);
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
            finally
            {
                userManager.Dispose();
                identityUserService.Dispose();
            }
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

                await identityUserService.SendEmailAsync(userId, subject,
                    $"<p>please confirm your mail address and complete your registration by clicking <a href=\"{callbackUrl}\">here</a>." +
                    $" Once you finished the registration a system administrator will decide based on your provided information about your assigned permissions. " +
                    $"This process can take up to 3 days.</p>" +
                    $"<p>You agreed on our <a href=\"{policyUrl}\">data policy</a> and <a href=\"{termsUrl}\">terms and conditions</a>.</p>");

                return callbackUrl;
            }
            finally
            {
                identityUserService.Dispose();
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