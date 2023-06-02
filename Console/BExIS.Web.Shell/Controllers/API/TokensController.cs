using BExIS.App.Bootstrap.Attributes;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;

namespace BExIS.Web.Shell.Controllers.API
{
    public class TokensController : ApiController
    {

        // GET api/Token/
        /// <summary>
        /// Get the token based on basic authentication
        /// </summary>
        /// <returns>Token</returns>
        [BExISApiAuthorize]
        [GetRoute("api/token")]
        [HttpGet]
        public async Task<HttpResponseMessage> Get()
        {
            try
            {
                using (var userManager = new UserManager())
                {
                    var user = ControllerContext.RouteData.Values["user"] as User;
                    if (user != null)
                    {
                        if (user.Token == null)
                        {
                            await userManager.SetTokenAsync(user);
                            user = userManager.FindByIdAsync(user.Id).Result;
                        }

                        return Request.CreateResponse(HttpStatusCode.OK, new { token = user.Token });
                    }

                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Authentication failed");
                }
            }
            catch
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Authentication failed");
            }
        }
    }
}