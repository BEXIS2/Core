using BExIS.Security.Entities.Subjects;
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
                        SignInStatus result = ldapAuthenticationManager.ValidateUser(model.UserName, model.Password);
                        switch (result)
                        {
                            case SignInStatus.Success:
                                var identity = await identityUserService.CreateIdentityAsync(user, DefaultAuthenticationTypes.ApplicationCookie);
                                AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = true }, identity);
                                if (!user.HasTermsAndConditionsAccepted || !user.HasPrivacyPolicyAccepted)
                                {
                                    return RedirectToAction("Confirm", returnUrl);
                                }
                                else
                                {
                                    return RedirectToLocal(returnUrl);
                                }
                                

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
                            var identity = await identityUserService.CreateIdentityAsync(ldapUser, DefaultAuthenticationTypes.ApplicationCookie);
                            AuthenticationManager.SignIn(new AuthenticationProperties() { IsPersistent = true }, identity);
                            return Confirm(returnUrl);

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
            }
        }

        public ActionResult Confirm(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Confirm(LoginConfirmModel model, string returnUrl)
        {
            var userManager = new UserManager();

            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var user = await userManager.FindByNameAsync(HttpContext.User.Identity.Name);

                if (user != null)
                {
                    if (user.Logins.Any(l => l.LoginProvider == "Ldap"))
                    {
                        user.HasPrivacyPolicyAccepted = true;
                        user.HasTermsAndConditionsAccepted = true;

                        await userManager.UpdateAsync(user);

                        return RedirectToLocal(returnUrl);
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