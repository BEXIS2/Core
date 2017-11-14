using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using BExIS.Modules.Rpm.UI.Models;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class UnitController : BaseController
    {
        //
        // GET: /RPM/Unit/

        public ActionResult Index()
        {
            return RedirectToAction("UnitManager");
        }

        public ActionResult UnitManager()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant( "Manage Units", this.Session.GetTenant());
            if (Session["Window"] == null)
                Session["Window"] = false;

            return View(new UnitManagerModel());
        }

        public ActionResult editUnit(EditUnitModel Model, string measurementSystem, long[] checkedRecords)
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant( "Manage Units", this.Session.GetTenant());

            UnitManager unitManager = null;
            try
            {
                unitManager = new UnitManager();

                Model.Unit.Name = cutSpaces(Model.Unit.Name);
                Model.Unit.Abbreviation = cutSpaces(Model.Unit.Abbreviation);
                Model.Unit.Description = cutSpaces(Model.Unit.Description);
                Model.Unit.Dimension.Name = cutSpaces(Model.Unit.Dimension.Name);
                Model.Unit.Dimension.Specification = cutSpaces(Model.Unit.Dimension.Specification);

                if (Model.Unit.Id == 0)
                {
                    if (unitValidation(Model.Unit, checkedRecords))
                    {
                        foreach (MeasurementSystem msCheck in Enum.GetValues(typeof(MeasurementSystem)))
                        {
                            if (msCheck.ToString().Equals(measurementSystem))
                            {
                                Model.Unit.MeasurementSystem = msCheck;
                            }
                        }
                        if (Model.Unit.Dimension.Id != 0)
                            Model.Unit.Dimension = unitManager.DimensionRepo.Get(Model.Unit.Dimension.Id);
                        else
                        {
                            if (!String.IsNullOrEmpty(Model.Unit.Dimension.Name))
                                Model.Unit.Dimension.Name = "no Name";
                            Model.Unit.Dimension = unitManager.Create(Model.Unit.Dimension.Name, Model.Unit.Dimension.Name, Model.Unit.Dimension.Specification);
                        }
                        Model.Unit = unitManager.Create(Model.Unit.Name, Model.Unit.Abbreviation, Model.Unit.Description, Model.Unit.Dimension, Model.Unit.MeasurementSystem);

                        updataAssociatedDataType(Model.Unit, checkedRecords);
                    }
                    else
                    {
                        Session["Window"] = true;

                        return View("UnitManager", new UnitManagerModel());
                    }
                }
                else
                {
                    if (unitValidation(Model.Unit, checkedRecords))
                    {
                        Unit unit = unitManager.Repo.Get(Model.Unit.Id);

                        if (!(unit.DataContainers.Count() > 0))
                        {
                            unit.Name = Model.Unit.Name;
                            unit.Description = Model.Unit.Description;
                            unit.Abbreviation = Model.Unit.Abbreviation;
                            foreach (MeasurementSystem msCheck in Enum.GetValues(typeof(MeasurementSystem)))
                            {
                                if (msCheck.ToString().Equals(measurementSystem))
                                {
                                    unit.MeasurementSystem = msCheck;
                                }
                            }

                            if (Model.Unit.Dimension.Id != 0)
                                unit.Dimension = unitManager.DimensionRepo.Get(Model.Unit.Dimension.Id);
                            else
                            {
                                if (!String.IsNullOrEmpty(unit.Dimension.Name))
                                    unit.Dimension.Name = "no Name";
                                unit.Dimension = unitManager.Create(Model.Unit.Dimension.Name, Model.Unit.Dimension.Name, Model.Unit.Dimension.Specification);
                            }
                            unit = unitManager.Update(unit);
                            List<long> DataTypelIdList = new List<long>();

                            updataAssociatedDataType(unit, checkedRecords);
                        }
                    }
                    else
                    {
                        Session["Window"] = true;
                        return View("UnitManager", new UnitManagerModel(Model.Unit.Id));
                    }
                }
            }
            finally
            {
                unitManager.Dispose();
            }
            Session["Window"] = false;
            Session["checked"] = null;
            return RedirectToAction("UnitManager");

        }

        private bool unitValidation(Unit unit, long[] checkedRecords)
        {
            bool check = true;
            UnitManager unitManager = null;
            try
            {
                unitManager = new UnitManager();

                List<Unit> unitList = unitManager.Repo.Get().ToList(); ;

                if (unit.Name == null || unit.Name == "")
                {
                    Session["nameMsg"] = "invalid Name";
                    check = false;
                }
                else
                {
                    bool nameExist = !(unitList.Where(p => p.Name.ToLower().Equals(unit.Name.ToLower())).Count().Equals(0));
                    if (nameExist)
                    {
                        Unit tempUnit = unitList.Where(p => p.Name.ToLower().Equals(unit.Name.ToLower())).ToList().First();
                        if (unit.Id != tempUnit.Id)
                        {
                            Session["nameMsg"] = "Name already exist";
                            check = false;
                        }
                        else
                        {
                            Session["nameMsg"] = null;
                        }
                    }
                    else
                    {
                        Session["nameMsg"] = null;
                    }
                }

                if (unit.Abbreviation == null || unit.Abbreviation == "")
                {
                    Session["abbrMsg"] = "invalid Abbreviation";
                    check = false;
                }
                else
                {
                    bool abbreviationExist = !(unitList.Where(p => p.Abbreviation.ToLower().Equals(unit.Abbreviation.ToLower())).Count().Equals(0));
                    if (abbreviationExist)
                    {
                        Unit tempUnit = unitList.Where(p => p.Abbreviation.ToLower().Equals(unit.Abbreviation.ToLower())).ToList().First();
                        if (unit.Id != tempUnit.Id)
                        {
                            Session["abbrMsg"] = "Abbreviation already exist";
                            check = false;
                        }
                        else
                        {
                            Session["abbrMsg"] = null;
                        }
                    }
                    else
                    {
                        Session["abbrMsg"] = null;
                    }
                }

                if (checkedRecords != null)
                {
                    Session["dataTypeMsg"] = null;
                }
                else
                {
                    Session["dataTypeMsg"] = "Choose at least one Data Type.";
                    check = false;
                }

                if (!String.IsNullOrEmpty(unit.Dimension.Name) && unit.Dimension.Name != "Select or Enter")
                {
                    Session["dimensionMsg"] = null;
                }
                else
                {
                    Session["dimensionMsg"] = "Select or create an Dimension.";
                    check = false;
                }
            }
            finally
            {
                unitManager.Dispose();
            }
            return check;
        }

        private List<DataType> updataAssociatedDataType(Unit unit, long[] newDataTypeIds)
        {
            if (unit != null)
            {
                DataTypeManager dataTypeManger = null;
                try
                {
                    dataTypeManger = new DataTypeManager();

                    UnitManager unitManager = null;
                    try
                    {
                        unitManager = new UnitManager();


                        unit = unitManager.Repo.Get(unit.Id);
                        List<DataType> existingDataTypes = unit.AssociatedDataTypes.ToList();
                        List<DataType> newDataTypes = newDataTypeIds == null ? new List<DataType>() : dataTypeManger.Repo.Query().Where(p => newDataTypeIds.Contains(p.Id)).ToList();
                        List<DataType> tobeAddedDataTypes = newDataTypes.Except(existingDataTypes).ToList();

                        if (tobeAddedDataTypes != null && tobeAddedDataTypes.Count > 0)
                            unitManager.AddAssociatedDataType(unit, tobeAddedDataTypes);

                        unit = unitManager.Repo.Get(unit.Id);
                        existingDataTypes = unit.AssociatedDataTypes.ToList();
                        List<DataType> toBeRemoved = existingDataTypes.Except(newDataTypes).ToList();
                        if (toBeRemoved != null && toBeRemoved.Count() > 0)
                            unitManager.RemoveAssociatedDataType(unit, toBeRemoved);

                        unit = unitManager.Repo.Get(unit.Id);
                        return unit.AssociatedDataTypes.ToList();
                    }
                    finally
                    {
                        unitManager.Dispose();
                    }
                }
                finally
                {
                    dataTypeManger.Dispose();
                }
            }
            return null;
        }

        public ActionResult deletUnit(long id)
        {
            if (id != 0)
            {
                EditUnitModel editUnitModel = new EditUnitModel(id);
                if (editUnitModel.Unit != null)
                {
                    if (!editUnitModel.inUse)
                    {
                        UnitManager unitManager = null;
                        try
                        {
                            unitManager = new UnitManager();

                            unitManager.Delete(editUnitModel.Unit);
                        }
                        finally
                        {
                            unitManager.Dispose();
                        }
                    }

                }
            }
            return RedirectToAction("UnitManager");
        }

        public ActionResult openUnitWindow(long id)
        {


            UnitManager unitManager = null;
            DataTypeManager dataTypeManager = null;
            UnitManagerModel Model;

            try
            {
                unitManager = new UnitManager();
                dataTypeManager = new DataTypeManager();
                if (id != 0)
                {
                    Model = new UnitManagerModel(id);
                    ViewBag.Title = PresentationModel.GetViewTitleForTenant("Edit Unit: " + Model.editUnitModel.Unit.Name + "(Id: " + Model.editUnitModel.Unit.Id + ")", this.Session.GetTenant());
                    Session["nameMsg"] = null;
                    Session["abbrMsg"] = null;
                    Session["dataTypeMsg"] = null;
                    if (Model.editUnitModel.Unit != new Unit())
                    {
                        Unit temp = Model.editUnitModel.Unit;
                        if (temp.Id != Model.editUnitModel.Unit.Id)
                            Session["checked"] = null;
                    }
                    Session["Window"] = true;
                    Session["dimensionMsg"] = null;
                }
                else
                {
                    ViewBag.Title = PresentationModel.GetViewTitleForTenant("Create Unit", this.Session.GetTenant());
                    Model = new UnitManagerModel();
                    Session["nameMsg"] = null;
                    Session["abbrMsg"] = null;
                    Session["dataTypeMsg"] = null;
                    Session["Window"] = true;
                    Session["dimensionMsg"] = null;
                }
            }
            finally
            {
                unitManager.Dispose();
                dataTypeManager.Dispose();
            }
            return View("UnitManager", Model);
        }

        public JsonResult setChecked(long[] checkedIDs)
        {
            if (checkedIDs != null)
            {
                List<long> Ids = new List<long>();
                foreach (long l in checkedIDs)
                {
                    Ids.Add(Convert.ToInt64(l));
                }
                Session["checked"] = Ids;
            }
            else
                Session["checked"] = null;
            return Json(true, JsonRequestBehavior.AllowGet);
        }

        private string cutSpaces(string str)
        {
            if (str != "" && str != null)
            {
                str = str.Trim();
                if (str.Length > 255)
                    str = str.Substring(0, 255);
            }
            return (str);
        }

    }

}
