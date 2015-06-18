using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.TypeSystem;
using BExIS.Web.Shell.Areas.RPM.Models;

namespace BExIS.Web.Shell.Areas.RPM.Controllers
{
    public class DataAttributeController : Controller
    {
        //
        // GET: /RPM/DataType/

        public ActionResult Index()
        {
            return AttributeManager();
        }

        public bool attributeInUse(DataAttribute attribute)
        {
            if (attribute.UsagesAsVariable.Count() == 0 && attribute.UsagesAsParameter.Count() == 0)
                return false;
            else
                return true;
        }

        public ActionResult AttributeManager()
        {
            if (Session["Window"] == null)
                Session["Window"] = false;

            DataContainerManager dataAttributeManager = new DataContainerManager();
            List<DataAttribute> dataAttributeList = dataAttributeManager.DataAttributeRepo.Get().Where(a => a.UsagesAsVariable.Count != null && a.Unit.Name != null && a.DataType.Name != null).ToList();

            List<DataAttributeModel> dataAttributeModelList = new List<DataAttributeModel>();

            dataAttributeList.ForEach(d => dataAttributeModelList.Add(new DataAttributeModel(d)));

            #region load datatypes and units

                Session["dataTypeList"] = getAllDatatypeItemModels();
                Session["unitlist"] = getAllUnitItemModels();

            #endregion


            return View(dataAttributeModelList);
        }

        public ActionResult editAttribute(DataAttributeModel Model, long id, string parent)
        {
            

            DataContainerManager dataAttributeManager = new DataContainerManager();
            IList<DataAttribute> DataAttributeList = dataAttributeManager.DataAttributeRepo.Get();
            long tempUnitId = Convert.ToInt64(Model.Unit.Id);
            long tempDataTypeId = Convert.ToInt64(Model.DataType.Id);

            Model.Id = id;
            Model.ShortName = cutSpaces(Model.ShortName);
            Model.Name = cutSpaces(Model.Name);
            Model.Description = cutSpaces(Model.Description);

            if (Model.DomainConstraints.Count > 0)
            {
                if (Model.DomainItems != null && Model.DomainItems.Count > 0)
                {
                    Model.DomainConstraints.FirstOrDefault().DomainItems = clearEmptyItems(Model.DomainItems);
                }
            }


            if (Model.Name == "" | Model.Name == null)
            {
                

                Session["nameMsg"] = "invalid Name";
                Session["Window"] = true;
                Session["DataAttribute"] = Model;
                return RedirectToAction(parent);
            }
            else
            {
                bool nameNotExist = DataAttributeList.Where(p => p.Name.ToLower().Equals(Model.Name.ToLower())).Count().Equals(0);

                if (Model.Id == 0)
                {
                    if (nameNotExist)
                    {
                        UnitManager UM = new UnitManager();
                        Unit unit = UM.Repo.Get(tempUnitId);
                        DataTypeManager DTM = new DataTypeManager();
                        DataType dataType = DTM.Repo.Get(tempDataTypeId);
                        DataContainerManager DAM = new DataContainerManager();

                        DataAttribute temp = new DataAttribute();
                        temp = DAM.CreateDataAttribute(Model.ShortName, Model.Name, Model.Description, false, false, "", MeasurementScale.Categorial, DataContainerType.ReferenceType, "", dataType, unit, null, null, null, null, null, null);
                    
                    
                        #region store constraint

                        if (Model.RangeConstraints.Count > 0 && (Model.RangeConstraints.FirstOrDefault().Min !=null || Model.RangeConstraints.FirstOrDefault().Max !=null)
                            && (Model.RangeConstraints.FirstOrDefault().Min != 0.0 || Model.RangeConstraints.FirstOrDefault().Max != 0.0 ))
                            temp = storeConstraint(Model.RangeConstraints.First(), temp);

                        if (Model.PatternConstraints.Count > 0 && !String.IsNullOrEmpty(Model.PatternConstraints.FirstOrDefault().MatchingPhrase))
                            temp = storeConstraint(Model.PatternConstraints.First(), temp);

                        if (Model.DomainConstraints.Count > 0 && Model.DomainItems.Count > 0)
                        {
                            DomainConstraintModel cmodel = Model.DomainConstraints.First();
                            cmodel.DomainItems = Model.DomainItems;
                            temp = storeConstraint(Model.DomainConstraints.First(), temp);
                        }

                        #endregion


                        temp = DAM.UpdateDataAttribute(temp);
                    }
                    else
                    {
                        Session["nameMsg"] = "Name already exist";
                        Session["Window"] = true;
                        Session["DataAttribute"] = Model;
                        return RedirectToAction(parent);
                    }
                }
                else
                {
                    if (nameNotExist || DataAttributeList.Where(p => p.Name.Equals(Model.Name)).ToList().First().Id == Model.Id)
                    {
                        DataAttribute dataAttribute = DataAttributeList.Where(p => p.Id.Equals(Model.Id)).ToList().First();
                        if (!attributeInUse(dataAttribute))
                        {
                            DataContainerManager DAM = new DataContainerManager();
                            dataAttribute.Name = cutSpaces(Model.Name);
                            dataAttribute.ShortName = Model.ShortName;
                            dataAttribute.Description = Model.Description;
                            UnitManager UM = new UnitManager();
                            dataAttribute.Unit = UM.Repo.Get(tempUnitId);
                            DataTypeManager DTM = new DataTypeManager();
                            dataAttribute.DataType = DTM.Repo.Get(tempDataTypeId);
                            DAM.UpdateDataAttribute(dataAttribute);
                        }
                    }
                    else
                    {
                        Session["nameMsg"] = "Name already exist";
                        Session["Window"] = true;
                        return RedirectToAction(parent);
                    }
                }
            }

            Session["Window"] = false;
            Session["DataAttribute"] = new DataAttributeModel();
            return RedirectToAction(parent);
        }

        public ActionResult deletAttribute(long id, string name)
        {

            if (id != 0)
            {
                DataContainerManager DAM = new DataContainerManager();
                DataAttribute dataAttribute = DAM.DataAttributeRepo.Get(id);
                if (dataAttribute != null)
                {
                    if (!attributeInUse(dataAttribute))
                    {

                        DAM.DeleteDataAttribute(dataAttribute);
                    }

                }
            }
            return RedirectToAction("AttributeManager");
        }

        public ActionResult openAttributeWindow(long id)
        {

            if (id != 0)
            {
                DataContainerManager DAM = new DataContainerManager();
                DataAttribute dataAttribute = DAM.DataAttributeRepo.Get(id);

                Session["nameMsg"] = null;
                Session["DataAttribute"] = new DataAttributeModel(dataAttribute);
                Session["Window"] = true;

            }
            else
            {
                Session["nameMsg"] = null;
                Session["DataAttribute"] = new DataAttributeModel();
                Session["Window"] = true;
            }
            return RedirectToAction("AttributeManager");
        }

        public JsonResult getDatatypeList(string Id)
        {
            long tempId = Convert.ToInt64(Id);
            UnitManager unitManager = new UnitManager();
            Unit tempUnit = unitManager.Repo.Get(tempId);
            List<DataType> dataTypeList = new List<DataType>();
            if (tempUnit.Name.ToLower() == "none")
            {
                DataTypeManager dataTypeManager = new DataTypeManager();
                dataTypeList = dataTypeManager.Repo.Get().ToList();
                dataTypeList = dataTypeList.OrderBy(p => p.Name).ToList();
            }
            else
            {
                dataTypeList = unitManager.Repo.Get(tempId).AssociatedDataTypes.ToList();
                dataTypeList = dataTypeList.OrderBy(p => p.Name).ToList();
            }

            return Json(new SelectList(dataTypeList.ToArray(), "Id", "Name"), JsonRequestBehavior.AllowGet);
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


        #region units and dataTypes

        private List<DataTypeItemModel> getAllDatatypeItemModels()
        {
            DataTypeManager dataTypeManager = new DataTypeManager();

            List<DataType> dataTypes = dataTypeManager.Repo.Get().OrderBy(p => p.Name).ToList();

            List<DataTypeItemModel> dataTypeItemModels = new List<DataTypeItemModel>();
            dataTypes.ForEach(d => dataTypeItemModels.Add(new DataTypeItemModel(d)));

            return dataTypeItemModels;


           
        }

        private List<UnitItemModel> getAllUnitItemModels()
        {
            UnitManager unitManager = new UnitManager();
         

            List<Unit> units = unitManager.Repo.Get().OrderBy(p => p.Name).ToList();

            List<UnitItemModel> unitItemModels = new List<UnitItemModel>();
            units.ForEach(u => unitItemModels.Add(new UnitItemModel(u)));

            return unitItemModels;
        }


        #endregion

        #region constraints

        public ActionResult AddDomainItem()
        {
            return PartialView("_domainItemView", new DomainConstraintItemModel());
        }

        private DataAttribute storeConstraint(ConstraintModel constraintModel, DataAttribute dataAttribute)
        {
            DataContainerManager dcManager = new DataContainerManager();
            var attr = dataAttribute;

            
            if (constraintModel is RangeConstraintModel)
            {
                RangeConstraintModel rcm = (RangeConstraintModel)constraintModel;

                var constraint = new RangeConstraint(ConstraintProviderSource.Internal, "", "en-US", rcm.Description, rcm.Negated, null, null, null, rcm.Min, true, rcm.Max, true);
                dcManager.AddConstraint(constraint, attr);
            }

            if (constraintModel is PatternConstraintModel)
            {
                PatternConstraintModel rcm = (PatternConstraintModel)constraintModel;

                var constraint = new PatternConstraint(ConstraintProviderSource.Internal, "", "en-US", rcm.Description, rcm.Negated, null, null, null, rcm.MatchingPhrase, false);
                dcManager.AddConstraint(constraint, attr);
            }

            if (constraintModel is DomainConstraintModel)
            {
                DomainConstraintModel dcm = (DomainConstraintModel)constraintModel;

                List<DomainItem> items = new List<DomainItem>();

                dcm.DomainItems.ToList().ForEach(d => items.Add(new DomainItem() {
                    Key = d.Key,
                    Value = d.Value
                }));

                var constraint = new DomainConstraint(ConstraintProviderSource.Internal, "", "en-US", dcm.Description, dcm.Negated, null, null, null, items);
                dcManager.AddConstraint(constraint, attr);
            }

            return attr;
        }

        private List<DomainConstraintItemModel> clearEmptyItems(List<DomainConstraintItemModel> list)
        {
            for (int i = 0; i < list.Count;i++ )
            {
                if (list.ElementAt(i).Key == null)
                {
                    list.RemoveAt(i);
                }
            }

            return list;
        }

        #endregion

    }
}
