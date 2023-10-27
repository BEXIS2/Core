using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Services.DataStructure;
using BExIS.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using BExIS.Modules.Rpm.UI.Models.DataTypes;
using BExIS.Dlm.Entities.DataStructure;
using System.Linq;
using BExIS.Modules.Rpm.UI.Models;
using BExIS.Modules.Rpm.UI.Models.Dimensions;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class DataTypeController : Controller
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
        public JsonResult GetDataTypes()
        {
            using (DataTypeManager dataTypeManager = new DataTypeManager())
            {
                return Json(convertToDataTypeListItem(dataTypeManager.Repo.Get().OrderBy(ds => ds.Id).ToList()), JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetSystemTypes()
        {
            return Json(Enum.GetNames(typeof(TypeCode)), JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult EditDataType(DataTypeListItem dataTypeListItem)
        {
            ValidationResult validationResult = new ValidationResult
            {
                IsValid = false,
                ValidationItems = new List<ValidationItem>(),
            };
            object result = new
            {
                validationResult,
                dataTypeListItem,
            };

            if (dataTypeListItem != null)
            {
                using (DataTypeManager dataTypeManager = new DataTypeManager())
                {
                    validationResult = new ValidationResult();
                    validationResult = DataTypeValidation(dataTypeListItem, dataTypeManager);
                    if (validationResult.IsValid)
                    {
                        DataType dataType = new DataType();

                        if (dataTypeListItem.Id == 0)
                        {
                            dataType = dataTypeManager.Create(dataTypeListItem.Name, dataTypeListItem.Description, (TypeCode)Enum.Parse(typeof(TypeCode), dataTypeListItem.SystemType));
                        }
                        else
                        {
                            dataType = dataTypeManager.Repo.Get(dataTypeListItem.Id);
                            dataType.Name = dataTypeListItem.Name;
                            dataType.Description = dataTypeListItem.Description;
                            dataType.SystemType = dataTypeListItem.SystemType;

                        }
                        dataType = dataTypeManager.Update(dataType);
                        dataTypeListItem = convertToDataTypeListItem(dataType);

                    }
                    result = new
                    {
                        validationResult,
                        dataTypeListItem,
                    };
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult DeleteDataType(long id)
        {
            if (id > 0)
            {
                using (DataTypeManager dataTypeManager = new DataTypeManager())
                {
                    DataType dataType = dataTypeManager.Repo.Get(id);
                    if (dataType != null && !dataType.DataContainers.Any())
                    {
                        dataTypeManager.Delete(dataType);
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            return Json(false, JsonRequestBehavior.AllowGet);
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
            bool inuse = dataType.DataContainers.Any();

            DataTypeListItem dataTypeListItem = new DataTypeListItem
            {
                Id = dataType.Id,
                Name = dataType.Name,
                Description = dataType.Description,
                SystemType = dataType.SystemType.ToString(),
                InUse = inuse,
            };
            return dataTypeListItem;
        }

        private static ValidationResult DataTypeValidation(DataTypeListItem dataTypeListItem, DataTypeManager dataTypeManager)
        {
            if (dataTypeListItem != null && dataTypeManager != null)
            {
                ValidationResult validationResult = new ValidationResult { IsValid = true };
                List<DataType> dataTypes = dataTypeManager.Repo.Get().ToList();

                if (dataTypeListItem.Id != 0)
                {
                    DataType dataType = dataTypeManager.Repo.Get(dataTypeListItem.Id);
                    if (dataType.DataContainers.Any())
                    {
                        validationResult.IsValid = false;
                        validationResult.ValidationItems.Add(new ValidationItem { Name = "inUse", Message = "Data Type is in Use" });
                    }
                }


                if (string.IsNullOrEmpty(dataTypeListItem.Name))
                {
                    validationResult.IsValid = false;
                    validationResult.ValidationItems.Add(new ValidationItem { Name = "Name", Message = "Name is Null or Empty" });
                }
                else
                {
                    if (dataTypes.Where(p => p.Name.ToLower().Equals(dataTypeListItem.Name.ToLower())).Any())
                    {
                        if (dataTypeListItem.Id != dataTypes.Where(p => p.Name.ToLower().Equals(dataTypeListItem.Name.ToLower())).ToList().First().Id)
                        {
                            validationResult.IsValid = false;
                            validationResult.ValidationItems.Add(new ValidationItem { Name = "Name", Message = "Name already exist" });
                        }
                    }
                }

                if (!string.IsNullOrEmpty(dataTypeListItem.SystemType))
                {
                    TypeCode typeCode;
                    try
                    {
                        typeCode = (TypeCode)Enum.Parse(typeof(TypeCode), dataTypeListItem.SystemType);
                    }
                    catch
                    {
                        validationResult.IsValid = false;
                        validationResult.ValidationItems.Add(new ValidationItem { Name = "SystemType", Message = "System Type is invalide" });
                    }
                }
                else
                {
                    validationResult.IsValid = false;
                    validationResult.ValidationItems.Add(new ValidationItem { Name = "SystemType", Message = "No System Type selected" });
                }
                return validationResult;
            }
            return null;
        }
    }
}