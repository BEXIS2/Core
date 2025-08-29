using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Modules.Rpm.UI.Models;
using BExIS.Modules.Rpm.UI.Models.Dimensions;
using BExIS.UI.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class DimensionController : Controller
    {
        public ActionResult Index()
        {
            string module = "RPM";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetDimensions()
        {
            using (UnitManager unitManager = new UnitManager())
            {
                return Json(convertToDimensionListItem(unitManager.DimensionRepo.Get().OrderBy(ds => ds.Id).ToList()), JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult EditDimension(DimensionListItem dimensionListItem)
        {
            ValidationResult validationResult = new ValidationResult
            {
                IsValid = false,
                ValidationItems = new List<ValidationItem>(),
            };
            object result = new
            {
                validationResult,
                dimensionListItem,
            };

            if (dimensionListItem != null)
            {
                using (UnitManager unitManager = new UnitManager())
                {
                    validationResult = new ValidationResult();
                    validationResult = dimensionValidation(dimensionListItem, unitManager);
                    if (validationResult.IsValid)
                    {
                        Dimension dimension = new Dimension();

                        if (dimensionListItem.Id == 0)
                        {
                            dimension = unitManager.Create(dimensionListItem.Name, dimensionListItem.Description, dimensionListItem.Specification);
                        }
                        else
                        {
                            dimension = unitManager.DimensionRepo.Get(dimensionListItem.Id);
                            dimension.Name = dimensionListItem.Name;
                            dimension.Description = dimensionListItem.Description;
                            dimension.Specification = dimensionListItem.Specification;
                        }
                        dimension = unitManager.Update(dimension);
                        dimensionListItem = convertToDimensionListItem(dimension);
                    }
                    result = new
                    {
                        validationResult,
                        dimensionListItem,
                    };
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpPost, ValidateAntiForgeryToken]
        public JsonResult DeleteDimension(long id)
        {
            if (id > 0)
            {
                using (UnitManager unitManager = new UnitManager())
                {
                    Dimension dimension = unitManager.DimensionRepo.Get(id);
                    if (dimension != null && !dimension.Units.Any())
                    {
                        unitManager.Delete(dimension);
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        private List<DimensionListItem> convertToDimensionListItem(List<Dimension> dimensions)
        {
            List<DimensionListItem> dimensionListItems = new List<DimensionListItem>();

            foreach (Dimension dimension in dimensions)
            {
                dimensionListItems.Add(convertToDimensionListItem(dimension));
            }
            return dimensionListItems;
        }

        private DimensionListItem convertToDimensionListItem(Dimension dimension)
        {
            bool inuse = false;
            if (dimension.Units.Any())
                inuse = true;
            else
                inuse = false;

            DimensionListItem dimensionListItem = new DimensionListItem
            {
                Id = dimension.Id,
                Name = dimension.Name,
                Description = dimension.Description,
                Specification = dimension.Specification,
                InUse = inuse,
            };
            return dimensionListItem;
        }

        private static ValidationResult dimensionValidation(DimensionListItem dimensionListItem, UnitManager unitManager)
        {
            if (dimensionListItem != null && unitManager != null)
            {
                ValidationResult validationResult = new ValidationResult { IsValid = true };
                List<Dimension> dimensions = unitManager.DimensionRepo.Get().ToList();

                if (dimensionListItem.Id != 0)
                {
                    Dimension dimension = unitManager.DimensionRepo.Get(dimensionListItem.Id);
                    if (dimension.Units.Any())
                    {
                        validationResult.IsValid = false;
                        validationResult.ValidationItems.Add(new ValidationItem { Name = "inUse", Message = "Dimension is in Use" });
                    }
                }

                if (string.IsNullOrEmpty(dimensionListItem.Name))
                {
                    validationResult.IsValid = false;
                    validationResult.ValidationItems.Add(new ValidationItem { Name = "Name", Message = "Name is Null or Empty" });
                }
                else
                {
                    if (dimensions.Where(p => p.Name.ToLower().Equals(dimensionListItem.Name.ToLower())).Any())
                    {
                        if (dimensionListItem.Id != dimensions.Where(p => p.Name.ToLower().Equals(dimensionListItem.Name.ToLower())).ToList().First().Id)
                        {
                            validationResult.IsValid = false;
                            validationResult.ValidationItems.Add(new ValidationItem { Name = "Name", Message = "Name already exist" });
                        }
                    }
                }
                return validationResult;
            }
            return null;
        }
    }
}