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

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class UnitController : Controller
    {
        [JsonNetFilter]
        [HttpGet]
        public JsonResult Get()
        {

            using (UnitManager unitManager = new UnitManager())
            {
                List<UnitListItem> UnitListItems = new List<UnitListItem>();
                List<Unit> Units = unitManager.Repo.Get().ToList();
                foreach (Unit unit in Units)
                {
                    UnitListItems.Add(new UnitListItem {
                        Id = unit.Id,
                        Name = unit.Name,
                        Description = unit.Description,
                        Abbreviation = unit.Abbreviation,
                        Dimension = unit.Dimension.Name,
                        Datatypes = string.Empty,
                        MeasurementSystem = string.Empty,
                    });
                }
                return Json(UnitListItems, JsonRequestBehavior.AllowGet);
            }
        }
    }
}