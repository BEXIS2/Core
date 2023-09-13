using BExIS.Dlm.Services.DataStructure;
using BExIS.Modules.Rpm.UI.Models;
using BExIS.Modules.Rpm.UI.Models.Dimensions;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class ConstraintsController : ApiController
    {
        [HttpGet, GetRoute("api/constraints/{id}")]
        public async Task<HttpResponseMessage> GetById(long id)
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    var constraint = await constraintManager.FindByIdAsync(id);

                    if (constraint == null)
                        return Request.CreateResponse(HttpStatusCode.BadRequest, $"group with id: {id} does not exist.");

                    return Request.CreateResponse(HttpStatusCode.OK, ReadConstraintModel.Convert(constraint));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}