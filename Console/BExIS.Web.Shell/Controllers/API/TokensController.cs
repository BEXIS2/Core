using BExIS.App.Bootstrap.Attributes;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Config;
using BExIS.Utils.Route;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace BExIS.Web.Shell.Controllers.API
{
    public class TokensController : ApiController
    {
        // GET api/Token/
        /// <summary>
        /// Get the token based on basic authentication
        /// </summary>
        /// <returns>Token</returns>
        [HttpGet, GetRoute("api/tokens"), BExISApiAuthorize]
        public async Task<HttpResponseMessage> Get()
        {
            try
            {
                var jwtConfiguration = GeneralSettings.JwtConfiguration;

                using (var userManager = new UserManager())
                {
                    var user = ControllerContext.RouteData.Values["user"] as User;

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

                        return Request.CreateResponse(HttpStatusCode.OK, jwt_token);
                    }

                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }
    }
}