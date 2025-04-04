using BExIS.App.Bootstrap;
using BExIS.App.Bootstrap.Attributes;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Config;
using BExIS.Utils.Route;
using BExIS.Web.Shell.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace BExIS.Web.Shell.Controllers.API
{
    /// <summary>
    /// 
    /// </summary>
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
                        var jwt_token = await Task.FromResult(JwtHelper.GetTokenByUser(user));

                        return Request.CreateResponse(HttpStatusCode.OK, new ReadJwtModel() { Jwt = jwt_token });
                    }

                    return Request.CreateResponse(HttpStatusCode.BadRequest);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpPost, PostRoute("api/tokens/verify"), BExISApiAuthorize]
        public async Task<HttpResponseMessage> Verify(string token)
        {
            try
            {
                var jwtConfiguration = GeneralSettings.JwtConfiguration;

                var tokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration.IssuerSigningKey)),
                    RequireExpirationTime = jwtConfiguration.RequireExpirationTime,
                    ValidateLifetime = jwtConfiguration.ValidateLifetime,
                    ValidateAudience = jwtConfiguration.ValidateAudience,
                    ValidateIssuer = jwtConfiguration.ValidateAudience,
                    ValidIssuer = jwtConfiguration.ValidIssuer,
                    ValidAudience = jwtConfiguration.ValidAudience
                };

                if (validateToken(token, tokenValidationParameters))
                {
                    var TokenInfo = new Dictionary<string, string>();
                    var handler = new JwtSecurityTokenHandler();
                    var jwtSecurityToken = await Task.FromResult(handler.ReadJwtToken(token));

                    return Request.CreateResponse(HttpStatusCode.OK, jwtSecurityToken);
                }

                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        private static bool validateToken(string token, TokenValidationParameters tokenValidationParameters)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                SecurityToken securityToken;
                ClaimsPrincipal principal = handler.ValidateToken(token, tokenValidationParameters, out securityToken);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}