using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Globalization;
using System.Linq.Expressions;
using System.Diagnostics.Contracts;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Entities.Data;
using Vaiona.Persistence.Api;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.UI;
using BExIS.RPM.Model;
using BExIS.RPM.Output;
using System.IO;
using Vaiona.Util.Cfg;

namespace BExIS.Web.Shell.Areas.RPM.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Planing/Home/

        HtmlString notOk = new HtmlString("<div class=\"t-icon t-cancel\"></div>");
        HtmlString ok = new HtmlString("<div class=\"t-icon t-update\"></div>");

        public ActionResult Index()
        {
            return View();
        }

        #region Data Structure Designer

        public ActionResult DataStructureDesigner()
        {
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();

            
            Session["inUse"] = false;
            Session["dataStructureId"] = null;
            return View("DataStructureDesigner", DSDM);
        }

        #region Data Structure Info

        public ActionResult showDataStructure(long SelectedItem)
        {
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();


            if (SelectedItem != null && SelectedItem != 0)
            {
                DSDM.GetDataStructureByID(SelectedItem);
            }
            else
            {
                DSDM = new DataStructureDesignerModel();
            }
            Session["Window"] = false;
            Session["dataStructureId"] = DSDM.dataStructure.Id;
            return View("DataStructureDesigner", DSDM);
        }



        #endregion

        #region save Datastructure

        public ActionResult saveDataStructure(DataStructureDesignerModel DSDM, string category)
        {   

            if (DSDM.dataStructure.Id == 0)
            {
                if (DSDM.GetDataStructureList().Where(p => p.Name.Equals(DSDM.dataStructure.Name)).Count() > 0)
                {
                    ViewData["errorMsg"] = "Can\'t save Data Structure Name exsist already ";
                }
                else
                {
                    DataStructureManager DSM = new DataStructureManager();
                    DataStructureCategory DSC = new DataStructureCategory();
                    foreach (DataStructureCategory dsc in Enum.GetValues(typeof(DataStructureCategory)))
                    {
                        if (dsc.ToString().Equals(category))
                        {
                            DSC = dsc;
                        }
                    }
                    DSM.CreateStructuredDataStructure(DSDM.dataStructure.Name, DSDM.dataStructure.Description, "", "", DSC, null);
                }
            }
            else
            {
                string description = DSDM.dataStructure.Description;
                DSDM.GetDataStructureByID(DSDM.dataStructure.Id);
                DSDM.dataStructure.Description = description;

                if (DSDM.dataStructure.Datasets.Count > 0)
                {
                    ViewData["errorMsg"] = "Can\'t save Data Structure is in use ";
                }
                else
                {
                    DataStructureManager DSM = new DataStructureManager();
                    DataStructureCategory DSC = new DataStructureCategory();
                                        
                    foreach (DataStructureCategory dsc in Enum.GetValues(typeof(DataStructureCategory)))
                    {
                        if (dsc.ToString().Equals(category))
                        {
                            DSC = dsc;
                        }
                    }
                    DSM.UpdateStructuredDataStructure(DSDM.dataStructure);
                }
            }
            return View("DataStructureDesigner", DSDM);
        }

        #endregion

        #region add Variable

        public ActionResult ShowVariables()
        {
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
            if (Session["dataStructureId"] != null)
            {
                DSDM.GetDataStructureByID((long)Session["dataStructureId"]);
                DSDM.GetDataAttributeList();
                if (DSDM.dataStructure.Datasets.Count > 0)
                {
                    Session["inUse"] = true;
                }
                else
                {
                    
                    //DataTable VariableTable = new DataTable();

                    //VariableTable.Columns.Add(new DataColumn("Select"));
                    //VariableTable.Columns.Add(new DataColumn("Name"));
                    //VariableTable.Columns.Add(new DataColumn("VariableID"));
                    //VariableTable.Columns.Add(new DataColumn("Shortname"));
                    //VariableTable.Columns.Add(new DataColumn("Description"));
                    //VariableTable.Columns.Add(new DataColumn("Classification"));
                    //VariableTable.Columns.Add(new DataColumn("Unit"));
                    //VariableTable.Columns.Add(new DataColumn("Data Type"));

                    //DataRow Row = VariableTable.NewRow();

                    //foreach (Variable v in DSDM.dataAttributeList)
                    //{
                    //    long ID = v.Id;
                    //    string ClassificationName = "";
                    //    string UnitName = "";
                    //    string DataTypeName = "";

                    //    if (v.Classification != null)
                    //        ClassificationName = v.Classification.Name;
                    //    if (v.Unit != null)
                    //        UnitName = v.Unit.Name;
                    //    if (v.DataType != null)
                    //        DataTypeName = v.DataType.Name;

                    //    string[] row = {"", v.Name, ID.ToString(), v.ShortName, v.Description, ClassificationName, UnitName, DataTypeName };
                    //    Row = VariableTable.NewRow();
                    //    Row.ItemArray = row;
                    //    VariableTable.Rows.Add(Row);
                    //}
                    //Session["VariableTable"] = VariableTable;
                    Session["VariableList"] = DSDM.dataAttributeList;

                    Session["inUse"] = false;

                }
            }
            if ((bool)Session["Window"] == false)
            {
                Session["Window"] = true;
            }
            else
            {
                Session["Window"] = false;
            }            
            return View("DataStructureDesigner", DSDM);
        }

        public ActionResult AddVariables(int[] checkedRecords)

        {
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();

            if (Session["dataStructureId"] != null)
            {  
                DSDM.GetDataStructureByID((long)Session["dataStructureId"]);
                DSDM.dataAttributeList = (IList<DataAttribute>)Session["VariableList"];
                for (int i = 0; i < checkedRecords.Length; i++)
                {
                    DataAttribute temp = DSDM.dataAttributeList.Where(p => p.Id.Equals(Convert.ToInt32(checkedRecords[i]))).FirstOrDefault();
                    if (temp != null)
                    {
                        DataStructureManager DSM = new DataStructureManager();
                        DSM.AddVariableUsage(DSDM.dataStructure, temp, false, temp.Name);
                    }
                }
                DSDM.GetDataStructureByID((long)Session["dataStructureId"]);
            }
            return View("DataStructureDesigner", DSDM);
        }
        #endregion

        public ActionResult downloadTemplate(long id)
        {
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
            
            if (id != 0)
            {
                DSDM.GetDataStructureByID(id);

                ExcelTemplateProvider provider = new ExcelTemplateProvider("BExISppTemplate_Clean.xlsx");
                provider.CreateTemplate(id);
                string filename = DSDM.dataStructure.Name + ".xlsx";
                return File(Path.Combine(AppConfiguration.GetModuleWorkspacePath("RPM"), "Template", filename), "application/xlsx", "Template_" + filename);
            }
            return View("DataStructureDesigner", DSDM);
        }

        #endregion

        #region Unit Manager

        private List<Unit> GetUnitRepo()
        {
            UnitManager UM = new UnitManager();
            List<Unit> repo = UM.Repo.Get().ToList();
            
            foreach(Unit u in repo)
            {
                UM.Repo.LoadIfNot(u.AssociatedDataTypes);
            }
            return(repo);
        }

        public ActionResult UnitManager()
        {

            if (Session["Unit"] == null)
                Session["Unit"] = new Unit();

            if (Session["Window"] == null)
                Session["Window"] = false;

            return View();
        }

        public ActionResult editUnit(Unit Model, string parent, long id, string measurementSystem, long[] checkedRecords)
        {
            if (id == 0)
            {
                if (Model.Name != "" && Model.Name != null)
                {
                    List<Unit> unitList = GetUnitRepo();
                    bool nameNotExist = unitList.Where(p => p.Name.Equals(Model.Name)).Count().Equals(0);
                    bool abbreviationNotExist = unitList.Where(p => p.Abbreviation.Equals(Model.Abbreviation)).Count().Equals(0);
                    if (nameNotExist && abbreviationNotExist)
                    {
                        foreach (MeasurementSystem msCheck in Enum.GetValues(typeof(MeasurementSystem)))
                        {
                            if (msCheck.ToString().Equals(measurementSystem))
                            {
                                Model.MeasurementSystem = msCheck;
                            }
                        }
                        UnitManager UM = new UnitManager();
                        UM.Create(Model.Name, Model.Abbreviation, Model.Description, Model.Dimension, Model.MeasurementSystem);

                        unitList = GetUnitRepo();
                        Unit unit = unitList.Where(p => p.Name.Equals(Model.Name)).ToList().First();
                        IList<DataType> dataTypeList = GetDataTypeRepo().Get().ToList();
                        if (checkedRecords != null)
                        {
                            foreach (long dt in checkedRecords)
                            {
                                DataType dataType = dataTypeList.Where(p => p.Id.Equals(dt)).ToList().First();
                                UM.AddAssociatedDataType(unit, dataType);
                            }
                        }
                    }
                }
                else
                { 
                }
            }
            else
            {
                IList<Unit> unitList = GetUnitRepo();
                Unit unit = unitList.Where(p => p.Id.Equals(id)).ToList().First();
                unit.Name = Model.Name;
                unit.Description = Model.Description;
                unit.Abbreviation = Model.Abbreviation;
                unit.Dimension = Model.Dimension;
                foreach (MeasurementSystem msCheck in Enum.GetValues(typeof(MeasurementSystem)))
                {
                    if (msCheck.ToString().Equals(measurementSystem))
                    {
                        unit.MeasurementSystem = msCheck;
                    }
                }
                UnitManager UM = new UnitManager();
                UM.Update(unit);

                unitList = GetUnitRepo();
                unit = unitList.Where(p => p.Id.Equals(id)).ToList().First();
                IList<DataType> dataTypeList = GetDataTypeRepo().Get().ToList();
                if (checkedRecords != null)
                {
                    foreach (long dt in checkedRecords)
                    {
                        DataType dataType = dataTypeList.Where(p => p.Id.Equals(dt)).ToList().First();
                        UM.AddAssociatedDataType(unit, dataType);
                    }
                }
            }
            Session["Window"] = false;
            Session["Unit"] = new Unit();
            return RedirectToAction(parent);
        }

        public ActionResult deletUnit(int id)
        {

            if (id != 0)
            {
                IList<Unit> unitList = GetUnitRepo();
                Unit unit = unitList.Where(p => p.Id.Equals(id)).ToList().First();
                UnitManager UM = new UnitManager();
                UM.Delete(unit);
            }
            return RedirectToAction("UnitManager");
        }

        public ActionResult openUnitWindow(int id)
        {

            if (id != 0)
            {
                IList<Unit> unitList = GetUnitRepo();
                Unit unit = unitList.Where(p=>p.Id.Equals(id)).ToList().First();
                Session["Unit"] = unit;
                Session["Window"] = true;
            }
            else
            {
                Session["Unit"] = new Unit();
                Session["Window"] = true;
            }
            return RedirectToAction("UnitManager");
        }

        #endregion

        #region Classification Manager

        private IReadOnlyRepository<Classifier> GetClassRepo()
        {
            ClassifierManager CM = new ClassifierManager();
            return (CM.Repo);
        }

        public ActionResult ClassificationManager()
        {

            if (Session["Class"] == null)
                Session["Class"] = new Classifier();

            if (Session["Window"] == null)
                Session["Window"] = false;

            List<Classifier> ClassList = GetClassRepo().Get().ToList();
            return View(ClassList);
        }

        public ActionResult editClassifier(Classifier Model, string parent, long id, string ParentClassifier)
        {
            IList<Classifier> classList = GetClassRepo().Get().ToList();
            Classifier Parent = new Classifier();
            if(classList.Where(p => p.Name.Equals(ParentClassifier)).Count().Equals(0))
                Parent = null;
            else
                Parent = classList.Where(p => p.Name.Equals(ParentClassifier)).ToList().First();               

            if (id == 0)
            {
                bool nameNotExist = classList.Where(p => p.Name.Equals(Model.Name)).Count().Equals(0);
                if (nameNotExist && (Model.Name != "" && Model.Name != null))
                {
                    ClassifierManager CM = new ClassifierManager();
                    CM.Create(Model.Name, Model.Description, Parent);
                    Session["Window"] = false;
                }
                else
                {
                    Session["errorMsg"] = "invalide Name";
                    Session["Window"] = true;
                }
            }
            else
            {       
                bool nameNotExist = classList.Where(p => p.Name.Equals(Model.Name)).Count().Equals(0);
                if (nameNotExist && (Model.Name != "" && Model.Name != null))
                {
                    Classifier classifier = classList.Where(p => p.Id.Equals(id)).ToList().First();
                    ClassifierManager CM = new ClassifierManager();
                    classifier.Name = Model.Name;
                    classifier.Description = Model.Description;
                    classifier.Parent = Parent;
                    CM.Update(classifier);
                    Session["Window"] = false;
                }
                else
                {
                    ViewData["errorMsg"] = "invalide Name";
                    Session["Window"] = true;
                }
            }
            
            Session["Class"] = new Classifier();
            return RedirectToAction(parent);
        }

        public ActionResult openClassWindow(int id)
        {

            if (id != 0)
            {
                IList<Classifier> classList = GetClassRepo().Get().ToList();
                Classifier classifier = classList.Where(p => p.Id.Equals(id)).ToList().First();
                Session["Class"] = classifier;
                Session["Window"] = true;
            }
            else
            {
                Session["Class"] = new Classifier();
                Session["Window"] = true;
            }
            return RedirectToAction("ClassificationManager");
        }

        public ActionResult deletClass(int id)
        {

            if (id != 0)
            {
                IList<Classifier> classList = GetClassRepo().Get().ToList();
                Classifier classifier = classList.Where(p => p.Id.Equals(id)).ToList().First();
                ClassifierManager CM = new ClassifierManager();
                CM.Delete(classifier);
            }
            return RedirectToAction("ClassificationManager");
        }

        #endregion

        #region DataType Manager

        private IReadOnlyRepository<DataType> GetDataTypeRepo()
        {
            DataTypeManager DTM = new DataTypeManager();
            return (DTM.Repo);
        }

        public ActionResult DataTypeManager()
        {

            if (Session["DataType"] == null)
                Session["DataType"] = new DataType();

            if (Session["Window"] == null)
                Session["Window"] = false;

            List<DataType> DataTypeList = GetDataTypeRepo().Get().ToList();
            return View(DataTypeList);
        }

        public ActionResult editDataType(DataType Model, long id,string systemType, string parent)
        {
            IList<DataType> DataTypeList = GetDataTypeRepo().Get().ToList();
            TypeCode typecode = new TypeCode();

            foreach (TypeCode tc in Enum.GetValues(typeof(TypeCode)))
            {
                if (tc.ToString() == systemType)
                    typecode = tc;
            } 


            if (id == 0)
            {
                bool nameNotExist = DataTypeList.Where(p => p.Name.Equals(Model.Name)).Count().Equals(0);
                if (nameNotExist)
                {
                    DataTypeManager DTM = new DataTypeManager();
                    DTM.Create(Model.Name, Model.Description, typecode);
                }
            }
            else
            {
                DataType dataType = DataTypeList.Where(p => p.Id.Equals(id)).ToList().First();
                DataTypeManager DTM = new DataTypeManager();
                dataType.Name = Model.Name;
                dataType.Description = Model.Description;
                DTM.Update(dataType);
            }
            Session["Window"] = false;
            Session["DataType"] = new DataType();
            return RedirectToAction(parent);
        }

        public ActionResult openDataTypeWindow(int id)
        {

            if (id != 0)
            {
                IList<DataType> DataTypeList = GetDataTypeRepo().Get().ToList();
                DataType dataType = DataTypeList.Where(p => p.Id.Equals(id)).ToList().First();
                Session["DataType"] = dataType;
                Session["Window"] = true;
            }
            else
            {
                Session["DataType"] = new DataType();
                Session["Window"] = true;
            }
            return RedirectToAction("DataTypeManager");
        }

        public ActionResult deletDataType(int id)
        {

            if (id != 0)
            {
                IList<DataType> DataTypeList = GetDataTypeRepo().Get().ToList();
                DataType dataType = DataTypeList.Where(p => p.Id.Equals(id)).ToList().First();
                DataTypeManager DTM = new DataTypeManager();
                DTM.Delete(dataType);
            }
            return RedirectToAction("DataTypeManager");
        }

        #endregion

        private List<DataAttribute> GetAttRepo()
        {
            DataContainerManager DAM = new DataContainerManager();
            List<DataAttribute> repo = DAM.DataAttributeRepo.Get().ToList();

            return (repo);
        }

        public ActionResult AttributeManager()
        {

            if (Session["DataAttribute"] == null)
                Session["DataAttribute"] = new DataAttribute();

            if (Session["Window"] == null)
                Session["Window"] = false;

            return View(GetAttRepo());
        }

        public ActionResult editAttribute(DataAttribute Model, long id, long unitId, long dataTypeId, string parent)
        {
            IList<DataAttribute> DataAttributeList = GetAttRepo();
          
            if (id == 0)
            {
                bool nameNotExist = DataAttributeList.Where(p => p.Name.Equals(Model.Name)).Count().Equals(0);
                if (nameNotExist)
                {
                    UnitManager UM = new UnitManager();
                    Unit unit =  UM.Repo.Get(unitId);
                    DataTypeManager DTM = new DataTypeManager();
                    DataType dataType = DTM.Repo.Get(dataTypeId);
                    DataContainerManager DAM = new DataContainerManager();

                    DataAttribute temp = new DataAttribute();
                    DAM.CreateDataAttribute(Model.ShortName, Model.Name,Model.Description,false,false, "",MeasurementScale.Categorial,DataContainerType.ReferenceType,"",dataType,unit,null, null,null,null, null, null);
                }
            }
            else
            {
                DataAttribute dataAttribute = DataAttributeList.Where(p => p.Id.Equals(id)).ToList().First();
                DataContainerManager DAM = new DataContainerManager();
                dataAttribute.Name = Model.Name;
                dataAttribute.ShortName = Model.ShortName;
                dataAttribute.Description = Model.Description;
                UnitManager UM = new UnitManager();
                dataAttribute.Unit = UM.Repo.Get(unitId);
                DataTypeManager DTM = new DataTypeManager();
                dataAttribute.DataType = DTM.Repo.Get(dataTypeId);
                DAM.UpdateDataAttribute(dataAttribute);
                
            }
            Session["Window"] = false;
            Session["DataType"] = new DataType();
            return RedirectToAction(parent);
        }

        public ActionResult openAttributeWindow(int id)
        {

            if (id != 0)
            {
                IList<DataAttribute> AttributeList = GetAttRepo();
                DataAttribute dataAttribute = AttributeList.Where(p => p.Id.Equals(id)).ToList().First();
                Session["DataAttribute"] = dataAttribute;
                Session["Window"] = true;
            }
            else
            {
                Session["DataAttribute"] = new DataAttribute();
                Session["Window"] = true;
            }
            return RedirectToAction("AttributeManager");
        }

        public ActionResult deletAttribute(long id)
        {

            if (id != 0)
            {
                IList<DataAttribute> AttributeList = GetAttRepo();
                DataAttribute dataAttribute = AttributeList.Where(p => p.Id.Equals(id)).ToList().First();
                DataContainerManager DAM = new DataContainerManager();
                DAM.DeleteDataAttribute(dataAttribute);
            }
            return RedirectToAction("AttributeManager");
        }

        

    }

}
