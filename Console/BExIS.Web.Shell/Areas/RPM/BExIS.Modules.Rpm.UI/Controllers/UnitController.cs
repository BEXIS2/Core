using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Meanings;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.Meanings;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Modules.Rpm.UI.Models;
using BExIS.Modules.Rpm.UI.Models;
using BExIS.Modules.Rpm.UI.Models.Dimensions;
using BExIS.Modules.Rpm.UI.Models.Units;
using BExIS.UI.Helpers;
using BExIS.Utils.NH.Querying;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class UnitController : Controller
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
        public JsonResult GetUnits()
        {
            using (UnitManager unitManager = new UnitManager())
            {
                return Json(convertToUnitListItem(unitManager.Repo.Query().OrderBy(u => u.Id).ToList()), JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetMeasurementSystems()
        {
            return Json(Enum.GetNames(typeof(MeasurementSystem)).ToList(), JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetDataTypes()
        {
            using (DataTypeManager dataTypeManager = new DataTypeManager())
            {
                return Json(convertToDataTypeListItem(dataTypeManager.Repo.Get().OrderBy(dt => dt.Id).ToList()), JsonRequestBehavior.AllowGet);
            }
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
        [HttpGet]
        public JsonResult GetExternalLinks()
        {
            using (MeaningManager meaningManager = new MeaningManager())
            {
                return Json(convertToLinkItem(meaningManager.GetExternalLinks().Where(el => el.Type.Equals(ExternalLinkType.link)).OrderBy(el => el.Id).ToList()), JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        [HttpPost, CustomValidateAntiForgeryToken]
        public JsonResult EditUnit(UnitListItem unitListItem)
        {
            ValidationResult validationResult = new ValidationResult
            {
                IsValid = false,
                ValidationItems = new List<ValidationItem>(),
            };
            object result = new
            {
                validationResult,
                unitListItem,
            };

            if (unitListItem != null)
            {
                using (UnitManager unitManager = new UnitManager())
                using (DataTypeManager dataTypeManager = new DataTypeManager())
                using (MeaningManager meaningManager = new MeaningManager())
                {
                    validationResult = new ValidationResult();
                    validationResult = unitValidation(unitListItem, unitManager);
                    if (validationResult.IsValid)
                    {
                        Unit unit = new Unit();
                        bool inUse = false;

                        if (unitListItem.Id == 0)
                        {
                            unit = unitManager.Create(unitListItem.Name, unitListItem.Abbreviation, unitListItem.Description, unitManager.DimensionRepo.Get(unitListItem.Dimension.Id), (MeasurementSystem)Enum.Parse(typeof(MeasurementSystem), unitListItem.MeasurementSystem));
                            // The element have to be reloaded after ceate
                            unit = unitManager.Repo.Get(unit.Id);
                        }
                        else
                        {
                            unit = unitManager.Repo.Get(unitListItem.Id);
                            unit.Name = unitListItem.Name;
                            unit.Abbreviation = unitListItem.Abbreviation;
                            unit.Description = unitListItem.Description;
                            unit.Dimension = unitManager.DimensionRepo.Get(unitListItem.Dimension.Id);
                            unit.MeasurementSystem = (MeasurementSystem)Enum.Parse(typeof(MeasurementSystem), unitListItem.MeasurementSystem);
                            inUse = unit.DataContainers.Any();
                        }
                        if (unitListItem.Datatypes.Count > 0)
                        {
                            unit.AssociatedDataTypes = dataTypeManager.Repo.Get().Where(p => unitListItem.Datatypes.Select(d => d.Id).Contains(p.Id)).ToList();
                        }
                        if (unitListItem.Link != null && unitListItem.Link.Id != 0)
                        {
                            unit.ExternalLink = meaningManager.GetExternalLink(unitListItem.Link.Id);
                        }
                        else
                        {
                            unit.ExternalLink = null;
                        }
                        unit = unitManager.Update(unit);
                        unitListItem = convertToUnitListItem(unit, true);
                    }
                    result = new
                    {
                        validationResult,
                        unitListItem,
                    };
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }       

        [JsonNetFilter]
        [HttpPost, CustomValidateAntiForgeryToken]
        public JsonResult DeleteUnit(long id)
        {
            if (id > 0)
            {
                using (UnitManager unitManager = new UnitManager())
                {
                    Unit unit = unitManager.Repo.Get(id);
                    if (unit != null && !unit.DataContainers.Any())
                    {
                        unitManager.Delete(unit);
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(false, JsonRequestBehavior.AllowGet);
        }

        private List<DimensionListItem> convertToDimensionListItem(List<Dimension> dimensions)
        {
            if (dimensions == null || dimensions.Count == 0)
                return new List<DimensionListItem>();

            List<DimensionListItem> dimensionListItems = new List<DimensionListItem>();

            foreach (Dimension dimension in dimensions)
            {
                dimensionListItems.Add(convertToDimensionListItem(dimension));
            }
            return dimensionListItems;
        }

        private DimensionListItem convertToDimensionListItem(Dimension dimension)
        {
            if (dimension == null)
                return new DimensionListItem();

            DimensionListItem dimensionListItem = new DimensionListItem
            {
                Id = dimension.Id,
                Name = dimension.Name,
                Description = dimension.Description,
                Specification = dimension.Specification,
            };
            return dimensionListItem;
        }

        private List<DataTypeListItem> convertToDataTypeListItem(List<Dlm.Entities.DataStructure.DataType> dataTypes)
        {
            if (dataTypes == null || dataTypes.Count == 0)
                return new List<DataTypeListItem>();

            List<DataTypeListItem> dataTypeListItems = new List<DataTypeListItem>();

            foreach (Dlm.Entities.DataStructure.DataType dataType in dataTypes)
            {
                dataTypeListItems.Add(convertToDataTypeListItem(dataType));
            }
            return dataTypeListItems;
        }
        private DataTypeListItem convertToDataTypeListItem(Dlm.Entities.DataStructure.DataType dataType)
        {
            if (dataType == null)
                return new DataTypeListItem();

            DataTypeListItem dataTypeListItem = new DataTypeListItem
            {
                Id = dataType.Id,
                Name = dataType.Name,
                Description = dataType.Description,
                SystemType = dataType.SystemType,
            };
            return dataTypeListItem;
        }

        private List<LinkItem> convertToLinkItem(List<ExternalLink> externalLinks)
        {
            if (externalLinks == null || externalLinks.Count == 0)
                return new List<LinkItem>();

            List<LinkItem> linkItems = new List<LinkItem>();

            foreach (ExternalLink externalLink in externalLinks)
            {
                linkItems.Add(convertToLinkItem(externalLink));
            }
            return linkItems;
        }

        private LinkItem convertToLinkItem(ExternalLink externalLink)
        {
            if (externalLink == null)
                return new LinkItem();

            LinkItem LinkItem = new LinkItem
            {
                Id = externalLink.Id,
                Name = externalLink.Name,
                URI = externalLink.URI,
            };
            return LinkItem;
        }

        private List<UnitListItem> convertToUnitListItem(List<Unit> units)
        {
            List<UnitListItem> unitListItems = new List<UnitListItem>();

            using (var metadataAttributeManager = new MetadataAttributeManager())
            using (var variableManager = new VariableManager())
            {
                Dictionary<long, long> maUsage = new Dictionary<long, long>();
                Dictionary<long, long> viUsage = new Dictionary<long, long>();
                Dictionary<long, long> vtUsage = new Dictionary<long, long>();

                // metadata attributes
                var mas = metadataAttributeManager.MetadataAttributeRepo.Query(ma => ma.Unit != null).Select(x => new {
                    Id = x.Id,
                    UnitId = x.Unit.Id
                });

                // variable instance
                var vis = variableManager.VariableInstanceRepo.Query(v => v.Unit != null).Select(x => new {
                    Id = x.Id,
                    UnitId = x.Unit.Id
                });
                // variable templates
                var vts = variableManager.VariableTemplateRepo.Query(v => v.Unit != null).Select(x => new {
                    Id = x.Id,
                    UnitId = x.Unit.Id
                });

                foreach (Unit unit in units)
                {   
                    bool inUse = false;

                    inUse = mas.Any(ma => ma.UnitId == unit.Id);
                    if (!inUse) inUse = vis.Any(v => v.UnitId == unit.Id);
                    if (!inUse) inUse = vts.Any(v => v.UnitId == unit.Id);

                    unitListItems.Add(convertToUnitListItem(unit, inUse));
                }
            }
            return unitListItems;
        }



        private UnitListItem convertToUnitListItem(Unit unit, bool inUse)
        {
            UnitListItem unitListItem = new UnitListItem
            {
                Id = unit.Id,
                Name = unit.Name,
                Description = unit.Description,
                Abbreviation = unit.Abbreviation,
                Dimension = convertToDimensionListItem(unit.Dimension),
                Datatypes = convertToDataTypeListItem(unit.AssociatedDataTypes.ToList()),
                MeasurementSystem = unit.MeasurementSystem.ToString(),
                InUse = inUse,
                Link = convertToLinkItem(unit.ExternalLink),
            };
            return unitListItem;
        }

        private static ValidationResult unitValidation(UnitListItem unitListItem, UnitManager unitManager)
        {
            if (unitListItem != null && unitManager != null)
            {
                ValidationResult validationResult = new ValidationResult { IsValid = true };
                List<Unit> units = unitManager.Repo.Get().ToList();

                if (string.IsNullOrEmpty(unitListItem.Name))
                {
                    validationResult.IsValid = false;
                    validationResult.ValidationItems.Add(new ValidationItem { Name = "Name", Message = "Name is Null or Empty" });
                }
                else
                {
                    if (units.Where(p => p.Name.ToLower().Equals(unitListItem.Name.ToLower())).Any())
                    {
                        if (unitListItem.Id != units.Where(p => p.Name.ToLower().Equals(unitListItem.Name.ToLower())).ToList().First().Id)
                        {
                            validationResult.IsValid = false;
                            validationResult.ValidationItems.Add(new ValidationItem { Name = "Name", Message = "Name already exist" });
                        }
                    }
                }

                if (string.IsNullOrEmpty(unitListItem.Abbreviation))
                {
                    validationResult.IsValid = false;
                    validationResult.ValidationItems.Add(new ValidationItem { Name = "Abbreviation", Message = "Abbreviation is Null or Empty" });
                }
                else
                {
                    if (units.Where(p => p.Abbreviation.ToLower().Equals(unitListItem.Abbreviation.ToLower())).Any())
                    {
                        if (unitListItem.Id != units.Where(p => p.Abbreviation.ToLower().Equals(unitListItem.Abbreviation.ToLower())).ToList().First().Id)
                        {
                            validationResult.IsValid = false;
                            validationResult.ValidationItems.Add(new ValidationItem { Name = "Abbreviation", Message = "Abbreviation already exist" });
                        }
                    }
                }
                if (unitListItem.Datatypes == null || unitListItem.Datatypes.Count() == 0)
                {
                    validationResult.IsValid = false;
                    validationResult.ValidationItems.Add(new ValidationItem { Name = "Datatypes", Message = "Choose at least one Data Type" });
                }
                if (unitListItem.Dimension.Id == 0)
                {
                    validationResult.IsValid = false;
                    validationResult.ValidationItems.Add(new ValidationItem { Name = "Dimension", Message = "Select an Dimension" });
                }
                if (String.IsNullOrEmpty(unitListItem.MeasurementSystem))
                {
                    validationResult.IsValid = false;
                    validationResult.ValidationItems.Add(new ValidationItem { Name = "Measurement System", Message = "Select an Measurement System" });
                }
                else if (!Enum.IsDefined(typeof(MeasurementSystem), unitListItem.MeasurementSystem))
                {
                    validationResult.IsValid = false;
                    validationResult.ValidationItems.Add(new ValidationItem { Name = "Measurement System", Message = "Wrong Measurement System" });
                }
                return validationResult;
            }
            return null;
        }
    }
}