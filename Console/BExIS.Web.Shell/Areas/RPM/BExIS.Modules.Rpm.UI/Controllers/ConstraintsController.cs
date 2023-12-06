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
using BExIS.Modules.Rpm.UI.Models.DataTypes;
using BExIS.Utils.NH.Querying;
using System.Xml;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class ConstraintsController : Controller
    {
        public ActionResult Index()
        {
            string module = "RPM";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult DeleteConstraint(long Id)
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    var constraint = constraintManager.FindById(Id);

                    var isDeleted = constraintManager.DeleteById(Id);

                    if (isDeleted)
                    {
                        Response.StatusCode = 200;
                        return Json(true);
                    }

                    Response.StatusCode = 400;
                    return Json(false);
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
                    List<DomainConstraint> domainConstraints = new List<DomainConstraint>();
                    List<DomainConstraint> dcs = constraintManager.DomainConstraints.Where(c => c.DataContainer == null).ToList();
                    foreach (DomainConstraint dc in dcs) 
                    {
                        dc.Materialize();
                        domainConstraints.Add(dc);
                    }
                    List<ReadConstraintModel> domainConstraintsModel = domainConstraints.Select(u => ReadConstraintModel.Convert(u)).ToList();

                    List<PatternConstraint> patternConstraints = constraintManager.PatternConstraints.Where(c => c.DataContainer == null).ToList();
                    List<ReadConstraintModel> patternConstraintsModel = patternConstraints.Select(u => ReadConstraintModel.Convert(u)).ToList();

                    List<RangeConstraint> rangeConstraints = constraintManager.RangeConstraints.Where(c => c.DataContainer == null).ToList();
                    List<ReadConstraintModel> rangeConstraintsModel = rangeConstraints.Select(u => ReadConstraintModel.Convert(u)).ToList();

                    List<ReadConstraintModel> constraints = domainConstraintsModel.Concat(patternConstraintsModel).Concat(rangeConstraintsModel).OrderBy(c => c.Id).ToList();

                    Response.StatusCode = 200;
                    return Json(constraints, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult GetDomainConstraint(long Id)
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    DomainConstraint constraint = constraintManager.DomainConstraints.Where(c => c.Id == Id).FirstOrDefault();
                    constraint.Materialize();
                    ReadDomainConstraintModel domainConstraintModel = ReadDomainConstraintModel.Convert(constraint);

                    Response.StatusCode = 200;
                    return Json(domainConstraintModel, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult GetRangeConstraint(long Id)
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    RangeConstraint constraint = constraintManager.RangeConstraints.Where(c => c.Id == Id).FirstOrDefault();
                    ReadRangeConstraintModel rangeConstraintModel = ReadRangeConstraintModel.Convert(constraint);

                    Response.StatusCode = 200;
                    return Json(rangeConstraintModel, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult GetPatternConstraint(long Id)
        {
            try
            {
                using (var constraintManager = new ConstraintManager())
                {
                    PatternConstraint constraint = constraintManager.PatternConstraints.Where(c => c.Id == Id).FirstOrDefault();
                    ReadPatternConstraintModel patternConstraintModel = ReadPatternConstraintModel.Convert(constraint);

                    Response.StatusCode = 200;
                    return Json(patternConstraintModel, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult EditDomainConstraint(EditDomainConstraintModel constraintListItem)
        {
            ValidationResult validationResult = new ValidationResult
            {
                IsValid = false,
                ValidationItems = new List<ValidationItem>(),
            };
            object result = new
            {
                validationResult,
                constraintListItem,
            };

            if (constraintListItem != null)
            {
                using (ConstraintManager constraintManager = new ConstraintManager())
                {
                    List<Constraint> constraints = constraintManager.Constraints.Where(c => c.DataContainer == null).ToList();
                    validationResult = ConstraintValidation(constraints, constraintListItem);

                    if (validationResult.IsValid)
                    {
                        DomainConstraint dc = new DomainConstraint();

                        if (constraintListItem.Id == 0)
                        {
                            dc = new DomainConstraint()
                            {
                                Id = constraintListItem.Id,
                                Name = constraintListItem.Name,
                                Description = constraintListItem.Description,
                                Negated = constraintListItem.Negated,
                                Items = DomainConverter.convertDomainToDomainItems(constraintListItem.Domain),
                        };
                            dc = constraintManager.Create(dc);
                        }
                        else
                        {
                            dc = constraintManager.DomainConstraintRepository.Get(constraintListItem.Id);
                            dc.Name = constraintListItem.Name;
                            dc.Description = constraintListItem.Description;
                            dc.Negated = constraintListItem.Negated;
                            dc.Items = DomainConverter.convertDomainToDomainItems(constraintListItem.Domain);
    
                            dc = constraintManager.Update(dc);
                        }
                        constraintListItem = new EditDomainConstraintModel()
                        {
                            Id = dc.Id,
                            Version = dc.VersionNo,
                            Name = dc.Name,
                            Description = dc.Description,
                            Negated = dc.Negated,
                            Domain = DomainConverter.convertDomainItemsToDomain(dc.Items),
                            InUse = false
                        };

                    }
                    result = new
                    {
                        validationResult,
                        constraintListItem,
                    };
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult EditRangeConstraint(EditRangeConstraintModel constraintListItem)
        {
            ValidationResult validationResult = new ValidationResult
            {
                IsValid = false,
                ValidationItems = new List<ValidationItem>(),
            };
            object result = new
            {
                validationResult,
                constraintListItem,
            };

            if (constraintListItem != null)
            {
                using (ConstraintManager constraintManager = new ConstraintManager())
                {
                    List<Constraint> constraints = constraintManager.Constraints.Where(c => c.DataContainer == null).ToList();
                    validationResult = ConstraintValidation(constraints, constraintListItem);

                    if (validationResult.IsValid)
                    {
                        RangeConstraint rc = null;

                        if (constraintListItem.Id == 0)
                        {
                            rc = new RangeConstraint()
                            {
                                Id = constraintListItem.Id,
                                Name = constraintListItem.Name,
                                Description = constraintListItem.Description,
                                Negated = constraintListItem.Negated,
                                Lowerbound = constraintListItem.Lowerbound,
                                Upperbound = constraintListItem.Upperbound,
                                LowerboundIncluded = constraintListItem.LowerboundIncluded,
                                UpperboundIncluded = constraintListItem.UpperboundIncluded
                            };
                            rc = constraintManager.Create(rc);
                        }
                        else
                        {
                            rc = constraintManager.RangeConstraints.Where(c => c.Id == constraintListItem.Id).FirstOrDefault();
                            rc.Name = constraintListItem.Name;
                            rc.Description = constraintListItem.Description;
                            rc.Negated = constraintListItem.Negated;
                            rc.Lowerbound = constraintListItem.Lowerbound;
                            rc.Upperbound = constraintListItem.Upperbound;
                            rc.LowerboundIncluded = constraintListItem.LowerboundIncluded;
                            rc.UpperboundIncluded = constraintListItem.UpperboundIncluded;

                            rc = constraintManager.Update(rc);
                        }

                        constraintListItem = new EditRangeConstraintModel() 
                        { 
                            Name = rc.Name,
                            Description = rc.Description,
                            Negated = rc.Negated,
                            Lowerbound = rc.Lowerbound,
                            Upperbound = rc.Upperbound,
                            LowerboundIncluded = rc.LowerboundIncluded,
                            UpperboundIncluded = rc.UpperboundIncluded,
                            InUse = false
                        };

                    }
                    result = new
                    {
                        validationResult,
                        constraintListItem,
                    };
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public JsonResult EditPatternConstraint(EditPatternConstraintModel constraintListItem)
        {
            ValidationResult validationResult = new ValidationResult
            {
                IsValid = false,
                ValidationItems = new List<ValidationItem>(),
            };
            object result = new
            {
                validationResult,
                constraintListItem,
            };

            if (constraintListItem != null)
            {
                using (ConstraintManager constraintManager = new ConstraintManager())
                {
                    List<Constraint> constraints = constraintManager.Constraints.Where(c => c.DataContainer == null).ToList();
                    validationResult = ConstraintValidation(constraints, constraintListItem);

                    if (validationResult.IsValid)
                    {
                        PatternConstraint pc = null;

                        if (constraintListItem.Id == 0)
                        {
                            pc = new PatternConstraint()
                            {
                                Id = constraintListItem.Id,
                                Name = constraintListItem.Name,
                                Description = constraintListItem.Description,
                                Negated = constraintListItem.Negated,
                                MatchingPhrase = constraintListItem.pattern
                            };
                            pc = constraintManager.Create(pc);
                        }
                        else
                        {
                            pc = constraintManager.PatternConstraints.Where(c => c.Id.Equals(constraintListItem.Id)).FirstOrDefault();
                            pc.Name = constraintListItem.Name;
                            pc.Description = constraintListItem.Description;
                            pc.Negated = constraintListItem.Negated;
                            pc.MatchingPhrase = constraintListItem.pattern;

                            pc = constraintManager.Update(pc);
                        }
                        constraintListItem = new EditPatternConstraintModel()
                        {
                            Id = pc.Id,
                            Version = pc.VersionNo,
                            Name = pc.Name,
                            Description = pc.Description,
                            Negated = pc.Negated,
                            pattern = pc.MatchingPhrase,
                            InUse = false
                        };

                    }
                    result = new
                    {
                        validationResult,
                        constraintListItem,
                    };
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetConstraintTypes()
        {
            return Json(Enum.GetNames(typeof(ConstraintType)), JsonRequestBehavior.AllowGet);
        }

        private ValidationResult ConstraintValidation(List<Constraint> constraints, EditConstraintModel constaint)
        {
            ValidationResult result = new ValidationResult();

            if (constaint != null && constraints != null)
            {
                if (string.IsNullOrEmpty(constaint.Name))
                {
                    result.IsValid = false;
                    result.ValidationItems.Add(new ValidationItem { Name = "Name", Message = "Name is Null or Empty" });
                }
                else
                {
                    if (constraints.Where(p => p.Name.ToLower().Equals(constaint.Name.ToLower())).Any())
                    {
                        if (constaint.Id != constraints.Where(p => p.Name.ToLower().Equals(constaint.Name.ToLower())).ToList().First().Id)
                        {
                            result.IsValid = false;
                            result.ValidationItems.Add(new ValidationItem { Name = "Name", Message = "Name already exist" });
                        }
                    }
                }
            }                
            return result;
        }

        private ValidationResult ConstraintValidation( List<Constraint> constraints, EditDomainConstraintModel constaint)
        {

            ValidationResult result = ConstraintValidation(constraints.Cast<Constraint>().ToList(), (EditConstraintModel)constaint);
            if (String.IsNullOrEmpty(constaint.Domain))
            {
                result.IsValid = false;
                result.ValidationItems.Add(new ValidationItem { Name = "Domain", Message = "Domain is not defined" });
            }
            return result;
        }

        private ValidationResult ConstraintValidation(List<Constraint> constraints, EditRangeConstraintModel constaint)
        {

            ValidationResult result = ConstraintValidation(constraints.Cast<Constraint>().ToList(), (EditConstraintModel)constaint);
            if (constaint.Lowerbound > constaint.Upperbound)
            {
                result.IsValid = false;
                result.ValidationItems.Add(new ValidationItem { Name = "Boundery", Message = "Lowerbound is bigger then Upperbound" });
            }
            return result;
        }

        private ValidationResult ConstraintValidation(List<Constraint> constraints, EditPatternConstraintModel constaint)
        {

            ValidationResult result = ConstraintValidation(constraints.Cast<Constraint>().ToList(), (EditConstraintModel)constaint);
            return result;
        }
    }
}