using BExIS.App.Bootstrap.Helpers;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Config;
using BExIS.Web.Shell.Models;
using Exceptionless;
using Microsoft.AspNet.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Helpers;
using System.Web.Mvc;

namespace BExIS.Web.Shell.Controllers
{
    public class TokensController : Controller
    {
        public async Task<ActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateJwtModel model)
        {
            try
            {
                using (var userManager = new UserManager())
                using (var identityUserService = new IdentityUserService(userManager))
                {
                    var jwtConfiguration = GeneralSettings.JwtConfiguration;

                    long userId = 0;
                    long.TryParse(this.User.Identity.GetUserId(), out userId);

                    var user = await userManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration.IssuerSigningKey));
                        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                        //Create a List of Claims, Keep claims name short
                        var permClaims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.UserName)
                        };

                        //Create Security Token object by giving required parameters
                        var token = new JwtSecurityToken(jwtConfiguration.ValidIssuer,
                        jwtConfiguration.ValidAudience,
                        permClaims,
                        notBefore: DateTime.Now,
                            expires: model.Validity > 0 ? DateTime.Now.AddHours(model.Validity) : DateTime.Now.AddHours(jwtConfiguration.ValidLifetime),
                            signingCredentials: credentials);

                        var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);

                        return View("Read", model: new ReadJwtModel() { Jwt = jwt_token });
                    }

                    return View("Create");
                }
            }
            catch
            {
                return View("Create");
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetToken()
        {
            try
            {
                var jwtConfiguration = GeneralSettings.JwtConfiguration;

                using (var userManager = new UserManager())
                {
                    var user = BExISAuthorizeHelper.GetUserFromAuthorizationAsync(HttpContext).Result;

                    if (user != null)
                    {
                        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration.IssuerSigningKey));
                        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                        //Create a List of Claims, Keep claims name short
                        var permClaims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.UserName)
                        };

                        //Create Security Token object by giving required parameters
                        var token = new JwtSecurityToken(jwtConfiguration.ValidIssuer,
                            jwtConfiguration.ValidAudience,
                            permClaims,
                            notBefore: DateTime.Now,
                            expires: jwtConfiguration.ValidLifetime > 0 ? DateTime.Now.AddHours(jwtConfiguration.ValidLifetime) : DateTime.MaxValue,
                            signingCredentials: credentials);

                        var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);

                        ExceptionlessClient.Default.SubmitLog("Get Token", $"{user.Name} requested a JWT.", Exceptionless.Logging.LogLevel.Info);

                        return View("GetToken", model: new ReadJwtModel() { Jwt = jwt_token });
                    }

                    return View("NotAuthorized");
                }
            }
            catch (Exception ex)
            {
                return View("Error");
            }
        }

        // GET: /AntiForgeryToken/GetToken
        [HttpGet]
        public ActionResult GetAntiForgeryToken()
        {
            var oldCookieToken = this.HttpContext.Request.Cookies[AntiForgeryConfig.CookieName]?.Value;

            string cookieToken, formToken;
            // The GetTokens method generates the tokens without rendering a view.
            AntiForgery.GetTokens(oldCookieToken, out cookieToken, out formToken);

            // Return the tokens in a JSON format.
            // It's common to send the form token in the response body
            // and let the client handle the cookie token which is set automatically.
            return Json(new { csrfToken = formToken }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var jwtConfiguration = GeneralSettings.JwtConfiguration;

                using (var userManager = new UserManager())
                {
                    var user = BExISAuthorizeHelper.GetUserFromAuthorizationAsync(HttpContext).Result;
                    if (user != null)
                    {
                        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration.IssuerSigningKey));
                        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                        //Create a List of Claims, Keep claims name short
                        var permClaims = new List<Claim>
                        {
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.UserName)
                        };

                        //Create Security Token object by giving required parameters
                        var token = new JwtSecurityToken(jwtConfiguration.ValidIssuer,
                            jwtConfiguration.ValidAudience,
                            permClaims,
                            notBefore: DateTime.Now,
                            expires: jwtConfiguration.ValidLifetime > 0 ? DateTime.Now.AddHours(jwtConfiguration.ValidLifetime) : DateTime.MaxValue,
                            signingCredentials: credentials);

                        var jwt_token = new JwtSecurityTokenHandler().WriteToken(token);

                        ExceptionlessClient.Default.SubmitLog("Get Token", $"{user.Name} requested a JWT.", Exceptionless.Logging.LogLevel.Info);

                        return Json(jwt_token, JsonRequestBehavior.AllowGet);
                    }

                    return Json("NotAuthorized", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json("NotAuthorized", JsonRequestBehavior.AllowGet);
            }
        }

        // GET: Tokens
        public ActionResult Index()
        {
            return View();
        }
    }
}