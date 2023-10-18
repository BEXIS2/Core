using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Modules.Rpm.UI.Models;
using BExIS.Modules.Rpm.UI.Models.Dimensions;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using BExIS.Utils.Route;
using System;
using System.Collections.Generic;
using System.Data;
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
        #region domain

        [HttpPost, PostRoute("api/domainConstraints")]
        public async Task<HttpResponseMessage> CreateDomainConstraint(CreateDomainConstraintModel model)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, GetRoute("api/domainConstraints/{id}")]
        public async Task<HttpResponseMessage> GetDomainConstraintById(long id)
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    var constraint = constraintManager.FindById<DomainConstraint>(id);

                    if (constraint == null)
                        return Request.CreateResponse(HttpStatusCode.BadRequest);

                    var model = ReadDomainConstraintModel.Convert(constraint);

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

        [HttpPut, PutRoute("api/domainConstraints/{id}")]
        public async Task<HttpResponseMessage> UpdateDomainConstraint(long id, UpdateDomainConstraintModel model)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion domain

        #region pattern

        [HttpPost, PostRoute("api/patternConstraints")]
        public async Task<HttpResponseMessage> CreatePatternConstraint(CreatePatternConstraintModel model)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, GetRoute("api/patternConstraints/{id}")]
        public async Task<HttpResponseMessage> GetPatternConstraintById(long id)
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    var constraint = constraintManager.FindById<DomainConstraint>(id);

                    if (constraint == null)
                        return Request.CreateResponse(HttpStatusCode.BadRequest);

                    var model = ReadDomainConstraintModel.Convert(constraint);

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

        [HttpPut, PutRoute("api/domainConstraints/{id}")]
        public async Task<HttpResponseMessage> UpdatePatternConstraint(long id, UpdatePatternConstraintModel model)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion pattern

        #region range

        [HttpPost, PostRoute("api/rangeConstraints")]
        public async Task<HttpResponseMessage> CreateRangeConstraint(CreateRangeConstraintModel model)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        [HttpGet, GetRoute("api/rangeConstraints/{id}")]
        public async Task<HttpResponseMessage> GetRangeConstraintById(long id)
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    var constraint = constraintManager.FindById<RangeConstraint>(id);

                    if (constraint == null)
                        return Request.CreateResponse(HttpStatusCode.BadRequest);

                    var model = ReadRangeConstraintModel.Convert(constraint);

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

        [HttpPut, PutRoute("api/rangeConstraints/{id}")]
        public async Task<HttpResponseMessage> UpdateRangeConstraint(long id, UpdatePatternConstraintModel model)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

        #endregion range

        [HttpDelete, DeleteRoute("api/constraints/{constraintId}")]
        public async Task<HttpResponseMessage> DeleteConstraintById(long constraintId)
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    var constraint = constraintManager.FindById(constraintId);

                    return Request.CreateResponse(HttpStatusCode.OK, constraintManager.DeleteById(constraintId));
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex);
            }
        }

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
    }
}