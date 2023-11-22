using BExIS.App.Bootstrap.Attributes;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Config;
using BExIS.Web.Shell.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Web.Shell.Controllers
{
    public class TokensController : Controller
    {
        [HttpGet, JsonNetFilter]
        public async Task<JsonResult> GetToken()
        {
            try
            {
                var jwtConfiguration = GeneralSettings.JwtConfiguration;

                using (var userManager = new UserManager())
                {
                    var user = await userManager.FindByNameAsync(HttpContext.User.Identity.Name);
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

                        return Json(jwt_token, JsonRequestBehavior.AllowGet);
                    }

                    return Json(false);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // GET: Tokens
        public ActionResult Index()
        {
            return View();
        }
    }
}