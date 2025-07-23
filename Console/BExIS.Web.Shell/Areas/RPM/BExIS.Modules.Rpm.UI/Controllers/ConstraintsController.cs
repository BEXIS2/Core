using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Meanings;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.Meanings;
using BExIS.Modules.Rpm.UI.Models;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Subjects;
using BExIS.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;

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
                    inUseChecker.reset();
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
        public JsonResult GetDatasetsByConstraint(long Id)
        {
            List<DatasetInfo> datasetInfos = new List<DatasetInfo>();
            if (Id > 0)
            {
                using (ConstraintManager constraintManager = new ConstraintManager())
                {
                    Dlm.Entities.DataStructure.Constraint constraint = constraintManager.Constraints.Where(c => c.Id == Id).FirstOrDefault();
                    constraint.Materialize();
                    List<long> variableIds = constraint.VariableConstraints.Select(v => v.Id).ToList();

                    if (variableIds != null && variableIds.Count > 0)
                    {
                        using (DataStructureManager dataStructureManager = new DataStructureManager())
                        {
                            List<StructuredDataStructure> structuredDataStructures = dataStructureManager.StructuredDataStructureRepo.Query(s => s.Variables.Any(v => variableIds.Contains(v.Id))).ToList();

                            if (structuredDataStructures != null && structuredDataStructures.Count > 0)
                            {
                                foreach (StructuredDataStructure structuredDataStructure in structuredDataStructures)
                                {
                                    structuredDataStructure.Materialize();
                                    if (structuredDataStructure.Datasets != null && structuredDataStructure.Datasets.Count > 0)
                                    {
                                        foreach (Dataset dataset in structuredDataStructure.Datasets)
                                        {
                                            string Name = String.IsNullOrEmpty(dataset.Versions.OrderBy(dv => dv.Id).Last().Title) ? "no Title" : dataset.Versions.OrderBy(dv => dv.Id).Last().Title;
                                            datasetInfos.Add(new DatasetInfo() { Id = dataset.Id, Name = Name, DatastructureId = structuredDataStructure.Id });
                                        }
                                        datasetInfos = datasetInfos.Distinct().ToList();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            Response.StatusCode = 200;
            return Json(datasetInfos, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult GetMeaningsByConstraint(long Id)
        {
            List<Info> MeaningInfos = new List<Info>();
            if (Id > 0)
            {
                using (ConstraintManager constraintManager = new ConstraintManager())
                {
                    Dlm.Entities.DataStructure.Constraint constraint = constraintManager.Constraints.Where(c => c.Id == Id).FirstOrDefault();
                    constraint.Materialize();
                    using (MeaningManager meaningManager = new MeaningManager())
                    {
                        List<Meaning> meanings = new List<Meaning>();
                        meanings = meaningManager.GetMeanings().Where(m => m.Constraints.Any(c => c.Id.Equals(Id))).ToList();
                        foreach (Meaning meaning in meanings)
                        {
                            MeaningInfos.Add(new Info { Id = meaning.Id, Name = meaning.Name, Description = meaning.Description });
                        }
                    }
                }
            }
            Response.StatusCode = 200;
            return Json(MeaningInfos, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult EditDomainConstraint(EditDomainConstraintModel constraintListItem)
        {
            string username = null;
            User user = null;

            using (UserManager userManager = new UserManager())
            {
                try
                {
                    username = HttpContext.User.Identity.Name;
                    user = userManager.FindByNameAsync(username).Result;
                }
                catch
                {
                    user = null;
                }
            }

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
                    List<Dlm.Entities.DataStructure.Constraint> constraints = constraintManager.Constraints.Where(c => c.DataContainer == null).ToList();
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
                                ConstraintSelectionPredicate = constraintListItem.SelectionPredicate != null ? constraintListItem.SelectionPredicate.GetJson() : null,
                            };

                            if (!String.IsNullOrEmpty(constraintListItem.Provider))
                                dc.Provider = (ConstraintProviderSource)Enum.Parse(typeof(ConstraintProviderSource), constraintListItem.Provider);

                            if (user != null)
                                dc.LastModifiedUserRef = user.Id;

                            dc = constraintManager.Create(dc);
                        }
                        else
                        {
                            dc = constraintManager.DomainConstraintRepository.Get(constraintListItem.Id);
                            dc.Name = constraintListItem.Name;
                            dc.Description = constraintListItem.Description;
                            dc.Negated = constraintListItem.Negated;
                            dc.Items = DomainConverter.convertDomainToDomainItems(constraintListItem.Domain);
                            dc.ConstraintSelectionPredicate = constraintListItem.SelectionPredicate != null ? constraintListItem.SelectionPredicate.GetJson() : null;

                            if (!String.IsNullOrEmpty(constraintListItem.Provider))
                                dc.Provider = (ConstraintProviderSource)Enum.Parse(typeof(ConstraintProviderSource), constraintListItem.Provider);

                            if (user != null)
                                dc.LastModifiedUserRef = user.Id;

                            dc = constraintManager.Update(dc);
                        }
                        ConstraintSelectionPredicate selectionPredicate = new ConstraintSelectionPredicate();
                        constraintListItem = new EditDomainConstraintModel()
                        {
                            Id = dc.Id,
                            Version = dc.VersionNo,
                            Name = dc.Name,
                            Description = dc.Description,
                            Negated = dc.Negated,
                            Domain = DomainConverter.convertDomainItemsToDomain(dc.Items),
                            Provider = dc.Provider != null ? dc.Provider.ToString() : null,
                            SelectionPredicate = dc.ConstraintSelectionPredicate != null ? selectionPredicate.Materialise(dc.ConstraintSelectionPredicate) : null,
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
            string username = null;
            User user = null;

            using (UserManager userManager = new UserManager())
            {
                try
                {
                    username = HttpContext.User.Identity.Name;
                    user = userManager.FindByNameAsync(username).Result;
                }
                catch
                {
                    user = null;
                }
            }

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
                    List<Dlm.Entities.DataStructure.Constraint> constraints = constraintManager.Constraints.Where(c => c.DataContainer == null).ToList();
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

                            if (user != null)
                                rc.LastModifiedUserRef = user.Id;

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

                            if (user != null)
                                rc.LastModifiedUserRef = user.Id;

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

        [JsonNetFilter]
        [HttpPost]
        public JsonResult EditPatternConstraint(EditPatternConstraintModel constraintListItem)
        {
            string username = null;
            User user = null;

            using (UserManager userManager = new UserManager())
            {
                try
                {
                    username = HttpContext.User.Identity.Name;
                    user = userManager.FindByNameAsync(username).Result;
                }
                catch
                {
                    user = null;
                }
            }

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
                    List<Dlm.Entities.DataStructure.Constraint> constraints = constraintManager.Constraints.Where(c => c.DataContainer == null).ToList();
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

                            if (user != null)
                                pc.LastModifiedUserRef = user.Id;

                            pc = constraintManager.Create(pc);
                        }
                        else
                        {
                            pc = constraintManager.PatternConstraints.Where(c => c.Id.Equals(constraintListItem.Id)).FirstOrDefault();
                            pc.Name = constraintListItem.Name;
                            pc.Description = constraintListItem.Description;
                            pc.Negated = constraintListItem.Negated;
                            pc.MatchingPhrase = constraintListItem.pattern;

                            if (user != null)
                                pc.LastModifiedUserRef = user.Id;

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

        private ValidationResult ConstraintValidation(List<Dlm.Entities.DataStructure.Constraint> constraints, EditConstraintModel constaint)
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

        private ValidationResult ConstraintValidation(List<Dlm.Entities.DataStructure.Constraint> constraints, EditDomainConstraintModel constaint)
        {
            ValidationResult result = ConstraintValidation(constraints.Cast<Dlm.Entities.DataStructure.Constraint>().ToList(), (EditConstraintModel)constaint);
            if (String.IsNullOrEmpty(constaint.Domain))
            {
                result.IsValid = false;
                result.ValidationItems.Add(new ValidationItem { Name = "Domain", Message = "Domain is not defined" });
            }
            return result;
        }

        private ValidationResult ConstraintValidation(List<Dlm.Entities.DataStructure.Constraint> constraints, EditRangeConstraintModel constaint)
        {
            ValidationResult result = ConstraintValidation(constraints.Cast<Dlm.Entities.DataStructure.Constraint>().ToList(), (EditConstraintModel)constaint);
            if (constaint.Lowerbound > constaint.Upperbound)
            {
                result.IsValid = false;
                result.ValidationItems.Add(new ValidationItem { Name = "Boundery", Message = "Lowerbound is bigger then Upperbound" });
            }
            return result;
        }

        private ValidationResult ConstraintValidation(List<Dlm.Entities.DataStructure.Constraint> constraints, EditPatternConstraintModel constaint)
        {
            ValidationResult result = ConstraintValidation(constraints.Cast<Dlm.Entities.DataStructure.Constraint>().ToList(), (EditConstraintModel)constaint);
            return result;
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetStruturedDatasetsByUserPermission()
        {
            string username = null;
            User user = null;
            int rights = 0;
            List<DatasetInfo> datasetInfos = new List<DatasetInfo>();
            List<Dataset> datasets = new List<Dataset>();

            using (UserManager userManager = new UserManager())
            {
                try
                {
                    username = HttpContext.User.Identity.Name;
                    user = userManager.FindByNameAsync(username).Result;
                }
                catch
                {
                    user = null;
                }
            }

            if (user != null)
            {
                using (DatasetManager datasetManager = new DatasetManager())
                {
                    datasets = datasetManager.DatasetRepo.Get().Where(ds => ds.DataStructure != null).ToList();

                    using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
                    {
                        foreach (Dataset ds in datasets)
                        {
                            rights = entityPermissionManager.GetEffectiveRightsAsync(user.Id, ds.EntityTemplate.EntityType.Id, ds.Id).Result;

                            if (rights > 0)
                            {
                                DatasetInfo dsi = new DatasetInfo()
                                {
                                    Id = ds.Id,
                                    Name = String.IsNullOrEmpty(ds.Versions.OrderBy(dv => dv.Id).Last().Title) ? "no Title" : ds.Versions.OrderBy(dv => dv.Id).Last().Title,
                                    Description = String.IsNullOrEmpty(ds.Versions.OrderBy(dv => dv.Id).Last().Description) ? "no Description" : ds.Versions.OrderBy(dv => dv.Id).Last().Description,
                                    DatasetVersionId = ds.Versions.OrderBy(dv => dv.Id).Last().Id,
                                    DatasetVersionNumber = ds.Versions.OrderBy(dv => dv.Id).Last().VersionNo,
                                    DatastructureId = ds.DataStructure.Id,
                                };
                                datasetInfos.Add(dsi);
                            }
                        }
                    }
                }
            }
            return Json(datasetInfos, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult GetDatastructure(long Id)
        {
            DatastructureInfo datastructureInfo = null;

            using (DataStructureManager dataStructureManager = new DataStructureManager())
            {
                StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(Id);
                if (dataStructure != null)
                {
                    datastructureInfo = new DatastructureInfo()
                    {
                        Id = dataStructure.Id,
                        Name = dataStructure.Name,
                        Description = dataStructure.Description,
                    };
                    if (dataStructure.Variables.Any())
                    {
                        ColumnInfo columnInfo = null;

                        foreach (var variable in dataStructure.Variables)
                        {
                            columnInfo = new ColumnInfo()
                            {
                                Id = variable.Id,
                                Name = variable.Label,
                                Description = variable.Description,
                                OrderNo = variable.OrderNo,
                                Unit = variable.Unit.Name,
                                DataType = variable.DataType.Name,
                            };
                            datastructureInfo.ColumnInfos.Add(columnInfo);
                            datastructureInfo.ColumnInfos = datastructureInfo.ColumnInfos.OrderBy(ci => ci.OrderNo).ToList();
                        }
                    }
                }
            }
            return Json(datastructureInfo, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult GetData(long Id, int pageSize = 0, long variableId = 0)
        {
            string username = null;
            User user = null;
            int rights = 0;

            DataTable dt = new DataTable();
            DataTable tempDt = new DataTable();

            using (UserManager userManager = new UserManager())
            {
                try
                {
                    username = HttpContext.User.Identity.Name;
                    user = userManager.FindByNameAsync(username).Result;
                }
                catch
                {
                    user = null;
                }
            }

            if (user != null)
            {
                using (EntityPermissionManager entityPermissionManager = new EntityPermissionManager())
                {
                    using (DatasetManager datasetManager = new DatasetManager())
                    {
                        Dataset dataset = datasetManager.DatasetRepo.Get(Id);

                        StructuredDataStructure dataStructure = new StructuredDataStructure();

                        using (DataStructureManager dataStructureManager = new DataStructureManager())
                        {
                            dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataset.DataStructure.Id);

                            rights = entityPermissionManager.GetEffectiveRightsAsync(user.Id, dataset.EntityTemplate.EntityType.Id, dataset.Id).Result;
                            if (rights > 0 && dataStructure != null && dataStructure.Id != 0)
                            {
                                if (pageSize > 0)
                                {
                                    try
                                    {
                                        dt = datasetManager.GetLatestDatasetVersionTuples(dataset.Id, 0, pageSize);
                                    }
                                    catch
                                    {
                                        return Json(dt, JsonRequestBehavior.AllowGet);
                                    }
                                    dt.Strip();

                                    tempDt = dt.Clone();
                                    foreach (DataColumn column in tempDt.Columns)
                                    {
                                        column.DataType = typeof(string);
                                    }

                                    foreach (DataRow row in dt.Rows)
                                    {
                                        tempDt.ImportRow(row);
                                    }

                                    VariableInstance variable = new VariableInstance();
                                    for (int i = 0; i < tempDt.Columns.Count; i++)
                                    {
                                        variable = dataStructure.Variables.Where(v => ("var" + v.Id).Equals(tempDt.Columns[i].ColumnName)).FirstOrDefault();
                                        for (int j = 0; j < tempDt.Rows.Count; j++)
                                        {
                                            foreach (MissingValue missingValue in variable.MissingValues)
                                            {
                                                if (tempDt.Rows[j].ItemArray[i].ToString() == missingValue.Placeholder)
                                                {
                                                    tempDt.Rows[j].SetField(i, missingValue.DisplayName);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    dt = tempDt.Copy();
                                }
                                else if (variableId > 0 && pageSize == 0)
                                {
                                    try
                                    {
                                        dt = datasetManager.GetLatestDatasetVersionTuples(dataset.Id, 0, pageSize);
                                    }
                                    catch
                                    {
                                        return Json(dt, JsonRequestBehavior.AllowGet);
                                    }
                                    dt.Strip();

                                    string columnName = "var" + variableId;

                                    while (dt.Columns[0] != dt.Columns[dt.Columns.Count - 1])
                                    {
                                        if (dt.Columns[0].ColumnName != columnName)
                                            dt.Columns.RemoveAt(0);

                                        if (dt.Columns[dt.Columns.Count - 1].ColumnName != columnName)
                                            dt.Columns.RemoveAt(dt.Columns.Count - 1);
                                    }

                                    VariableInstance variable = new VariableInstance();

                                    variable = dataStructure.Variables.Where(v => ("var" + v.Id).Equals(dt.Columns[0].ColumnName)).FirstOrDefault();

                                    int i = 0;

                                    do
                                    {
                                        foreach (MissingValue missingValue in variable.MissingValues)
                                        {
                                            if (dt.Rows[i].ItemArray[0].ToString() == missingValue.Placeholder)
                                            {
                                                dt.Rows.Remove(dt.Rows[i]);
                                                i--;
                                                break;
                                            }
                                        }
                                        i++;
                                    } while (dt.Rows[i] != dt.Rows[dt.Rows.Count - 1]);
                                }
                            }
                        }
                    }
                }
            }
            return Json(dt, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetProvider()
        {
            return Json(Enum.GetNames(typeof(ConstraintProviderSource)).ToList(), JsonRequestBehavior.AllowGet);
        }
    }
}