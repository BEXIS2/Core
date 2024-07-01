using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Config;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BExIS.App.Bootstrap
{
    public class JwtHelper
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <returns>JwtToken</returns>
        public static string Get(User user)
        {
            try
            {
                var jwtConfiguration = GeneralSettings.JwtConfiguration;

                using (var userManager = new UserManager())
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

                    return jwt_token;
                }
            }
            catch (Exception ex)
            {
                return String.Empty;
            }
        }

        public static ClaimsPrincipal Get(string token)
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

            return validateToken(token, tokenValidationParameters);
        }

        private static ClaimsPrincipal validateToken(string token, TokenValidationParameters tokenValidationParameters)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                SecurityToken securityToken;
                return handler.ValidateToken(token, tokenValidationParameters, out securityToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}