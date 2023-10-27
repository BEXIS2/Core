using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Services.DataStructure;
using BExIS.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using BExIS.Modules.Rpm.UI.Models.Dimensions;
using BExIS.Dlm.Entities.DataStructure;
using System.Linq;
using BExIS.Modules.Rpm.UI.Models.Units;
using BExIS.Modules.Rpm.UI.Models;
using System.Runtime.CompilerServices;
using BExIS.Utils.Route;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class ConstraintsController : Controller
    {
        [JsonNetFilter]
        [HttpDelete]
        public JsonResult DeleteConstraintById(long constraintId)
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    var constraint = constraintManager.FindById(constraintId);

                    var isDeleted = constraintManager.DeleteById(constraintId);

                    if (isDeleted)
                    {
                        Response.StatusCode = 200;
                        return Json("The constraint was deleted successfully.");
                    }

                    Response.StatusCode = 400;
                    return Json("An error has occurred.");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetConstraints()
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    List<DomainConstraint> domainConstraints = constraintManager.DomainConstraints.Where(c => c.DataContainer != null).ToList();
                    List<ReadConstraintModel> domainConstraintsModel  = domainConstraints.Select(u => ReadConstraintModel.Convert(u)).ToList();

                    List<PatternConstraint> patternConstraints = constraintManager.PatternConstraints.Where(c => c.DataContainer != null).ToList();
                    List<ReadConstraintModel> patternConstraintsModel = patternConstraints.Select(u => ReadConstraintModel.Convert(u)).ToList();

                    List<RangeConstraint> rangeConstraints = constraintManager.RangeConstraints.Where(c => c.DataContainer != null).ToList();
                    List<ReadConstraintModel> rangeConstraintsModel = rangeConstraints.Select(u => ReadConstraintModel.Convert(u)).ToList();

                    List<ReadConstraintModel>constraints = domainConstraintsModel.Concat(patternConstraintsModel).Concat(rangeConstraintsModel).OrderBy(c => c.Id).ToList();

                    Response.StatusCode = 200;
                    return Json(constraints, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public ActionResult Index()
        {
            string module = "RPM";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }

        #region domain

        [JsonNetFilter]
        [HttpPost]
        public JsonResult CreateDomainConstraint(CreateDomainConstraintModel model)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetDomainConstraintById(long id)
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    var constraint = constraintManager.FindById<DomainConstraint>(id);

                    if (constraint == null)
                    {
                        Response.StatusCode = 400;
                        return Json("An error has occurred.");
                    }

                    var model = ReadDomainConstraintModel.Convert(constraint);

                    Response.StatusCode = 200;
                    return Json(model, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetDomainConstraints()
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    var constraints = constraintManager.DomainConstraints.ToList();
                    var model = constraints.Select(u => ReadDomainConstraintModel.Convert(u));

                    Response.StatusCode = 200;
                    return Json(model, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [JsonNetFilter]
        [HttpPut]
        public JsonResult UpdateDomainConstraint(long id, UpdateDomainConstraintModel model)
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    var constraint = constraintManager.FindById<DomainConstraint>(id);

                    if (constraint == null || model == null)
                    {
                        Response.StatusCode = 400;
                        return Json("An error has occurred.");
                    }

                    //if (!string.IsNullOrEmpty(model.Name))
                    //    constraint.Name = model.Name;

                    //if (!string.IsNullOrEmpty(model.Description))
                    //    constraint.Description = model.Description;

                    //return Request.CreateResponse(HttpStatusCode.OK, model);

                    Response.StatusCode = 200;
                    return Json("An error has occurred.");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion domain

        #region pattern

        [JsonNetFilter]
        [HttpPost]
        public async Task<HttpResponseMessage> CreatePatternConstraint(CreatePatternConstraintModel model)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetPatternConstraintById(long id)
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    var constraint = constraintManager.FindById<DomainConstraint>(id);

                    if (constraint == null)
                    {
                        Response.StatusCode = 400;
                        return Json("An error has occurred.");
                    }

                    var model = ReadDomainConstraintModel.Convert(constraint);

                    Response.StatusCode = 200;
                    return Json(model, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetPatternConstraints()
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    var constraints = constraintManager.PatternConstraints.ToList();
                    var model = constraints.Select(u => ReadPatternConstraintModel.Convert(u));

                    Response.StatusCode = 200;
                    return Json(model, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [JsonNetFilter]
        [HttpPut]
        public JsonResult UpdatePatternConstraint(long id, UpdatePatternConstraintModel model)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion pattern

        #region range

        [JsonNetFilter]
        [HttpPost]
        public async Task<HttpResponseMessage> CreateRangeConstraint(CreateRangeConstraintModel model)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetRangeConstraintById(long id)
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    var constraint = constraintManager.FindById<RangeConstraint>(id);

                    if (constraint == null)
                    {
                        Response.StatusCode = 400;
                        return Json("An error has occurred.");
                    }

                    var model = ReadRangeConstraintModel.Convert(constraint);

                    Response.StatusCode = 200;
                    return Json(model, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [JsonNetFilter]
        [HttpGet]
        public async Task<HttpResponseMessage> GetRangeConstraints()
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    var constraints = constraintManager.RangeConstraints.ToList();
                    var model = constraints.Select(u => ReadRangeConstraintModel.Convert(u));

                    Response.StatusCode = 200;
                    return Json(model, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [JsonNetFilter]
        [HttpPut]
        public async Task<HttpResponseMessage> UpdateRangeConstraint(long id, UpdatePatternConstraintModel model)
        {
            try
            {
                return null;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion range

        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetConstraintTypes()
        {
            return Json(Enum.GetNames(typeof(ConstraintType)), JsonRequestBehavior.AllowGet);
        }
    }
}