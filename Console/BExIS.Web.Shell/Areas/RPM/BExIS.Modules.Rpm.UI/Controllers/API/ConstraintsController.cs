using BExIS.Dlm.Services.DataStructure;
using BExIS.Modules.Rpm.UI.Models;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Rpm.UI.Controllers.API
{
    public class ConstraintsController : ApiController
    {
        [HttpGet, GetRoute("api/constraints")]
        public async Task<HttpResponseMessage> GetConstraints()
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    var constraints = constraintManager.Constraints.ToList();
                    var model = constraints.Select(u => ReadConstraintModel.Convert(u));

                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, GetRoute("api/domainConstraints")]
        public async Task<HttpResponseMessage> GetDomainConstraints()
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    var constraints = constraintManager.DomainConstraints.ToList();
                    var model = constraints.Select(u => ReadDomainConstraintModel.Convert(u));

                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, GetRoute("api/patternConstraints")]
        public async Task<HttpResponseMessage> GetPatternConstraints()
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    var constraints = constraintManager.PatternConstraints.ToList();
                    var model = constraints.Select(u => ReadPatternConstraintModel.Convert(u));

                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, GetRoute("api/rangeConstraints")]
        public async Task<HttpResponseMessage> GetRangeConstraints()
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    var constraints = constraintManager.RangeConstraints.ToList();
                    var model = constraints.Select(u => ReadRangeConstraintModel.Convert(u));

                    return Request.CreateResponse(HttpStatusCode.OK, model);
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }
    }
}