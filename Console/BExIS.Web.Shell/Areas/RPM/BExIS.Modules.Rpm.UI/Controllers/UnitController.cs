using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using BExIS.UI.Helpers;
using BExIS.UI.Hooks;
using BExIS.UI.Models;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Vaiona.Web.Extensions;
using BExIS.Modules.Rpm.UI.Models.Units;
using BExIS.Dlm.Entities.DataStructure;
using System.Linq;
using BExIS.Utils.Helpers;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class UnitController : Controller
    {
        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetUnits()
        {

            using (UnitManager unitManager = new UnitManager())
            {
                return Json(convertToUnitListItem(unitManager.Repo.Get().OrderBy(u => u.Id).ToList()), JsonRequestBehavior.AllowGet);
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

        public JsonResult editUnit(UnitListItem unitListItem)
        {
            ValidationResult validationResult = new ValidationResult {
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
                {
                    validationResult = new ValidationResult();
                    validationResult = unitValidation(unitListItem, unitManager);
                    if (validationResult.IsValid)
                    {
                        Unit unit = new Unit();
                        
                        if (unitListItem.Id == 0)
                        {
                            //add proper dimention later
                            unit = unitManager.Create(unitListItem.Name, unitListItem.Abbreviation, unitListItem.Description,unitManager.DimensionRepo.Get(1), (MeasurementSystem)Enum.Parse(typeof(MeasurementSystem), unitListItem.MeasurementSystem));
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
                            
                        }
                        if (unitListItem.Datatypes.Count() > 0)
                        {
                            unit.AssociatedDataTypes = dataTypeManager.Repo.Get().Where(p => unitListItem.Datatypes.Select(d => d.Id).Contains(p.Id)).ToList();
                        }
                        unit = unitManager.Update(unit);
                        unitListItem = convertToUnitListItem(unit);

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
        
        

        private List<DataTypeListItem> convertToDataTypeListItem(List<DataType> dataTypes)
        {
            List<DataTypeListItem> dataTypeListItems = new List<DataTypeListItem>();

            foreach (DataType dataType in dataTypes)
            {
                dataTypeListItems.Add(convertToDataTypeListItem(dataType));
            }
            return dataTypeListItems;
        }
        private DataTypeListItem convertToDataTypeListItem(DataType dataType)
        {
            DataTypeListItem dataTypeListItem = new DataTypeListItem
            {
                Id = dataType.Id,
                Name = dataType.Name,
                Description = dataType.Description,
                SystemType = dataType.SystemType,
            };
            return dataTypeListItem;
        }
        private List<UnitListItem> convertToUnitListItem(List<Unit> units)
        {
            List<UnitListItem> unitListItems = new List<UnitListItem>();

            foreach (Unit unit in units)
            {
                unitListItems.Add(convertToUnitListItem(unit));
            }
            return unitListItems;
        }
        private UnitListItem convertToUnitListItem(Unit unit)
        {
            UnitListItem unitListItem = new UnitListItem
            {
                Id = unit.Id,
                Name = unit.Name,
                Description = unit.Description,
                Abbreviation = unit.Abbreviation,
                Dimension = new DimensionItem
                {
                    Id = unit.Dimension.Id,
                    Name = unit.Dimension.Name,
                },
                Datatypes = convertToDataTypeListItem(unit.AssociatedDataTypes.ToList()),
                MeasurementSystem = unit.MeasurementSystem.ToString(),
            };
            return unitListItem;
        }

        private ValidationResult unitValidation(UnitListItem unitListItem, UnitManager unitManager)
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
                    if (!units.Where(p => p.Name.ToLower().Equals(unitListItem.Name.ToLower())).Count().Equals(0))
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
                    if (!units.Where(p => p.Abbreviation.ToLower().Equals(unitListItem.Abbreviation.ToLower())).Count().Equals(0))
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
                return validationResult;
            }
            return null;
        }
    }
}