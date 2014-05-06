using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.RPM.Model;
using BExIS.RPM.Output;
using Vaiona.Persistence.Api;
using Vaiona.Util.Cfg;

namespace BExIS.Web.Shell.Areas.RPM.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Planing/Home/

        string templateName = "BExISppTemplate_Clean.xlsm";

        public ActionResult Index()
        {
            DataStructureManager dm = new DataStructureManager();
            dm.StructuredDataStructureRepo.Get();
            return View();
        }

        #region Data Structure Designer

        public ActionResult DataStructureDesigner()
        {
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
            DataStructureManager dataStructureManager = new DataStructureManager();
            DSDM.dataStructureList = dataStructureManager.StructuredDataStructureRepo.Get();
            
            Session["inUse"] = false;
            Session["dataStructureId"] = (long)0;
            Session["Window"] = false;
            Session["VariableWindow"] = false;
            return View("DataStructureDesigner", DSDM);
        }

        #region Data Structure Info

        public ActionResult showDataStructure(long SelectedItem)
        {
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
            DataStructureManager dataStructureManager = new DataStructureManager();
            DSDM.dataStructureList = dataStructureManager.StructuredDataStructureRepo.Get();

            if (SelectedItem != 0)
            {
                DSDM.dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(SelectedItem);
                DSDM.numberOfVariables = DSDM.dataStructure.Variables.Count;
                DSDM.BuildDataTable();
                if (DSDM.dataStructure.Datasets.Count > 0)
                {
                    Session["inUse"] = true;
                }
                else
                {
                    Session["inUse"] = false;
                }
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
            DataStructureManager DSM = new DataStructureManager();
            DSDM.dataStructureList = DSM.StructuredDataStructureRepo.Get();

            DSDM.dataStructure.Name = cutSpaces(DSDM.dataStructure.Name);
            DSDM.dataStructure.Description = cutSpaces(DSDM.dataStructure.Description);
            if(DSDM.dataStructure.Id == 0)
            {
                if(datasStructureValidation(DSDM)!= "ok")
                {
                    ViewData["errorMsg"] = datasStructureValidation(DSDM);
                    return View("DataStructureDesigner", DSDM);
                }
                else
                {
                    if (DSDM.dataStructure.Name != "" && DSDM.dataStructure.Name != null)
                    {
                        DataStructureCategory DSC = new DataStructureCategory();
                        
                        foreach (DataStructureCategory dsc in Enum.GetValues(typeof(DataStructureCategory)))
                        {
                            if (dsc.ToString().Equals(category))
                            {
                                DSC = dsc;
                            }
                        }
                        DSDM.dataStructure = DSM.CreateStructuredDataStructure(DSDM.dataStructure.Name, DSDM.dataStructure.Description, "", "", DSC, null);
                        ExcelTemplateProvider provider = new ExcelTemplateProvider(templateName);
                        provider.CreateTemplate(DSDM.dataStructure.Id);
                    }
                    else
                    { 
                        ViewData["errorMsg"] = "Please type a Name";
                        return View("DataStructureDesigner", DSDM);
                    }
                }
            }
            else
            {
                StructuredDataStructure DS = DSM.StructuredDataStructureRepo.Get(DSDM.dataStructure.Id);

                if (datasStructureValidation(DSDM) != "ok")
                {
                    ViewData["errorMsg"] = datasStructureValidation(DSDM);
                    return View("DataStructureDesigner", DSDM);
                }
                else
                {
                    DataStructureCategory DSC = new DataStructureCategory();

                    foreach (DataStructureCategory dsc in Enum.GetValues(typeof(DataStructureCategory)))
                    {
                        if (dsc.ToString().Equals(category))
                        {
                            DSC = dsc;
                        }
                    }
                    ExcelTemplateProvider provider = new ExcelTemplateProvider(templateName);
                    DS.Name = DSDM.dataStructure.Name;
                    DS.Description = DSDM.dataStructure.Description;
                    DSDM.dataStructure = DSM.UpdateStructuredDataStructure(DS);
                    provider.CreateTemplate(DSDM.dataStructure.Id);
                }
            }

            DSDM.dataStructure = DSM.StructuredDataStructureRepo.Get(DSDM.dataStructure.Id);
            DSDM.dataStructureList = DSM.StructuredDataStructureRepo.Get();
            DSDM.BuildDataTable();
            
            if (DSDM.dataStructure.Datasets.Count > 0)
                Session["inUse"] = true;
            else
                Session["inUse"] = false;            

            Session["Window"] = false;
            Session["dataStructureId"] = DSDM.dataStructure.Id;
            return View("DataStructureDesigner", DSDM);
        }

        private string datasStructureValidation(DataStructureDesignerModel dataStructureDesignerModel)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();

            if (dataStructureManager.StructuredDataStructureRepo.Get(dataStructureDesignerModel.dataStructure.Id) == null && dataStructureDesignerModel.dataStructure.Id != 0)
                {
                        return "Can\'t save Data Structure doesn't exsist anymore";
                }
                if (dataStructureDesignerModel.dataStructureList.Where(p => cutSpaces(p.Name).Equals(dataStructureDesignerModel.dataStructure.Name)).Count() > 0)
                {
                    long newDataStructureId = dataStructureDesignerModel.dataStructureList.Where(p => p.Name.Equals(dataStructureDesignerModel.dataStructure.Name)).ToList().First().Id;
                    if (newDataStructureId != dataStructureDesignerModel.dataStructure.Id)
                        return "Can\'t save Data Structure Name already exsist";
                }
                if (dataStructureDesignerModel.dataStructure.Datasets.Count > 0)
                {
                    return "Can\'t save Data Structure is in use ";
                }
                if (cutSpaces(dataStructureDesignerModel.dataStructure.Name) == "" || cutSpaces(dataStructureDesignerModel.dataStructure.Name) == null)
                {
                    return "Can\'t save invalide Name";
                }
            return "ok";
        }

        public ActionResult deleteDataStructure(long id)
        {
            if (id != 0)
            {
                DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
                if (DSDM.GetDataStructureByID(id))
                {
                    if (DSDM.dataStructure.Datasets.Count == 0)
                    {
                        Session["inUse"] = false;
                        DataStructureManager DSM = new DataStructureManager();
                        if (DSDM.dataStructure.Variables.Count > 0)
                        {
                            foreach (Variable v in DSDM.dataStructure.Variables)
                            {
                                DSM.RemoveVariableUsage(v);
                            }
                        }
                        ExcelTemplateProvider provider = new ExcelTemplateProvider();
                        provider.deleteTemplate(id);
                        DSM.DeleteStructuredDataStructure(DSDM.dataStructure);
                        return RedirectToAction("DataStructureDesigner");
                    }
                    else
                    {
                        Session["inUse"] = true;
                        return View("DataStructureDesigner", DSDM);
                    }
                }
            }
            return RedirectToAction("DataStructureDesigner");
        }

        #endregion

        #region add Variable

        public ActionResult ShowVariables(long id)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
            DSDM.dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(id);
            DSDM.dataStructureList = dataStructureManager.StructuredDataStructureRepo.Get();
            DSDM.numberOfVariables = DSDM.dataStructure.Variables.Count;
            DSDM.BuildDataTable();

            if (DSDM.dataStructure.Datasets.Count > 0)
            {
                Session["inUse"] = true;
            }
            else
            {
                if (id != 0)
                {

                    Session["inUse"] = false;
                    DataContainerManager dataAttributeManager = new DataContainerManager();
                    DSDM.dataAttributeList = dataAttributeManager.DataAttributeRepo.Get();
                    
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

            Session["dataStructureId"] = id;
            return View("DataStructureDesigner", DSDM);
        }

        public ActionResult AddVariables(int[] checkedRecords)
        {
            long id = (long)Session["dataStructureId"];
            DataStructureManager dataStructureManager = new DataStructureManager();
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
            DSDM.dataStructureList = dataStructureManager.StructuredDataStructureRepo.Get();
            DSDM.dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(id);
            DSDM.numberOfVariables = DSDM.dataStructure.Variables.Count;
            
            if(id != 0)
            {
                if (!(DSDM.dataStructure.Datasets.Count > 0))
                {
                    if (checkedRecords != null)
                    {
                        DataContainerManager dataAttributeManager = new DataContainerManager();
                        DataAttribute temp = new DataAttribute();

                        for (int i = 0; i < checkedRecords.Length; i++)
                        {
                            temp = dataAttributeManager.DataAttributeRepo.Get().Where(p => p.Id.Equals(Convert.ToInt32(checkedRecords[i]))).FirstOrDefault();
                            if (temp != null)
                            {
                                DataStructureManager DSM = new DataStructureManager();
                                DSM.AddVariableUsage(DSDM.dataStructure, temp, true, temp.Name, null, null);
                            }
                        }

                        DSDM.dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(id);
                        ExcelTemplateProvider provider = new ExcelTemplateProvider(templateName);
                        provider.CreateTemplate(id);
                    }
                }
            }
            if (DSDM.dataStructure.Datasets.Count > 0)
                Session["inUse"] = true;
            else
                Session["inUse"] = false;

            DSDM.numberOfVariables = DSDM.dataStructure.Variables.Count;
            DSDM.BuildDataTable();
            return View("DataStructureDesigner", DSDM);
        }

        public ActionResult deleteVariable(long id, long dataStructureId)
        {
            if (dataStructureId != 0)
            {
                DataStructureManager DSM = new DataStructureManager();
                DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
                DSDM.dataStructureList = DSM.StructuredDataStructureRepo.Get();
                DSDM.dataStructure = DSM.StructuredDataStructureRepo.Get(dataStructureId);

                if (!(DSDM.dataStructure.Datasets.Count > 0))
                {
                    Session["inUse"] = false;
                    Variable variable = DSM.VariableRepo.Get(id);

                    if (variable != null)
                    {
                        DSM.RemoveVariableUsage(variable);
                        ExcelTemplateProvider provider = new ExcelTemplateProvider(templateName);
                        provider.CreateTemplate(DSDM.dataStructure.Id);
                    }
                    DSDM.dataStructure = DSM.StructuredDataStructureRepo.Get(dataStructureId);
                    DSDM.numberOfVariables = DSDM.dataStructure.Variables.Count;
                    DSDM.BuildDataTable();
                    return View("DataStructureDesigner", DSDM);
                }
                else
                {
                    Session["inUse"] = true;
                    DSDM.dataStructure = DSM.StructuredDataStructureRepo.Get(dataStructureId);
                    DSDM.numberOfVariables = DSDM.dataStructure.Variables.Count;
                    DSDM.BuildDataTable();
                    return View("DataStructureDesigner", DSDM);
                }
            }
            return RedirectToAction("DataStructureDesigner");
        }

        public ActionResult saveVariable(string name, long id, long dataStructureId)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
            DSDM.dataStructureList = dataStructureManager.StructuredDataStructureRepo.Get();
            DSDM.dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);

            name = cutSpaces(name);
            if (!(DSDM.dataStructure.Datasets.Count > 0))
            {
                if (id != 0 && name != null && name != "")
                {
                    Variable var = dataStructureManager.VariableRepo.Get(id);

                    if (var != null)
                    {
                        var.Label = name;
                        dataStructureManager.UpdateStructuredDataStructure(var.DataStructure);

                        Session["variableId"] = null;
                        Session["VariableWindow"] = false;
                        ExcelTemplateProvider provider = new ExcelTemplateProvider(templateName);
                        provider.CreateTemplate(dataStructureId);
                        DSDM.dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);
                    }
                }
            }
            DSDM.numberOfVariables = DSDM.dataStructure.Variables.Count;
            DSDM.BuildDataTable();
            return View("DataStructureDesigner", DSDM);
        }

        public ActionResult openVariableWindow(long id, long dataStructureId)
        {

            DataStructureManager dataStructureManager = new DataStructureManager();
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
            DSDM.dataStructureList = dataStructureManager.StructuredDataStructureRepo.Get();
            DSDM.dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);

            if (DSDM.dataStructure.Datasets.Count == 0)
            {
                Session["inUse"] = false;
                Session["VariableWindow"] = true;
                Session["variableId"] = id;
                Session["dataStructureId"] = DSDM.dataStructure.Id;

                DSDM.dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);
                DSDM.numberOfVariables = DSDM.dataStructure.Variables.Count;
                DSDM.BuildDataTable();
            }
            else
            {
                Session["inUse"] = true; 
                Session["VariableWindow"] = true;
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

                ExcelTemplateProvider provider = new ExcelTemplateProvider(templateName);
                provider.CreateTemplate(id);
                string path = "";

                XmlNode resources = DSDM.dataStructure.TemplatePaths.FirstChild;

                XmlNodeList resource = resources.ChildNodes;

                foreach (XmlNode x in resource)
                {
                    if (x.Attributes.GetNamedItem("Type").Value == "Excel")
                        path = x.Attributes.GetNamedItem("Path").Value;
                    
                }
                return File(Path.Combine(AppConfiguration.DataPath, path), "application/xlsm", "Template_" + DSDM.dataStructure.Name + ".xlsm");
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
            if (Session["Window"] == null)
                Session["Window"] = false;

            UnitManager unitManager = new UnitManager();
            List<Unit> unitList = unitManager.Repo.Get().Where(u => u.DataContainers.Count != null && u.AssociatedDataTypes.Count != null).ToList();

            DataTypeManager dataTypeManager = new DataTypeManager();
            Session["dataTypeList"] = dataTypeManager.Repo.Get().ToList();

            return View(unitList);
        }

        public ActionResult editUnit(Unit Model, string parent, long id, string measurementSystem, long[] checkedRecords)
        {
            Model.Name = cutSpaces(Model.Name);
            Model.Abbreviation = cutSpaces(Model.Abbreviation);
            Model.Description = cutSpaces(Model.Description);
            Model.Dimension = cutSpaces(Model.Dimension);

            List<Unit> unitList = GetUnitRepo();

            
                if (id == 0)
                {
                    if ((Model.Name != "" && Model.Name != null) && (Model.Abbreviation != "" && Model.Abbreviation != null))
                    {

                        bool nameNotExist = unitList.Where(p => p.Name.ToLower().Equals(Model.Name.ToLower())).Count().Equals(0);
                        bool abbreviationNotExist = unitList.Where(p => p.Abbreviation.ToLower().Equals(Model.Abbreviation.ToLower())).Count().Equals(0);
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
                            Unit unit = UM.Create(Model.Name, Model.Abbreviation, Model.Description, Model.Dimension, Model.MeasurementSystem);

                            updataAssociatedDataType(unit, checkedRecords);
                        }
                    }
                }
                else
                {
                    if ((Model.Name != "" && Model.Name != null) && (Model.Abbreviation != "" && Model.Abbreviation != null))
                    {
                        Unit tempUnitByName = new Unit();
                        Unit tempUnitByAbbreviation = new Unit();

                        bool nameNotExist = unitList.Where(p => p.Name.ToLower().Equals(Model.Name.ToLower())).Count().Equals(0);
                        bool abbreviationNotExist = unitList.Where(p => p.Abbreviation.ToLower().Equals(Model.Abbreviation.ToLower())).Count().Equals(0);
                        if (!nameNotExist)
                            tempUnitByName = unitList.Where(p => p.Name.Equals(Model.Name)).ToList().First();
                        if (!abbreviationNotExist)
                            tempUnitByAbbreviation = unitList.Where(p => p.Abbreviation.Equals(Model.Abbreviation)).ToList().First();



                        if ((nameNotExist && abbreviationNotExist) || (tempUnitByName.Id == id && tempUnitByAbbreviation.Id == id) || (tempUnitByName.Id == id && abbreviationNotExist) || (tempUnitByAbbreviation.Id == id && nameNotExist))
                        {
                            Unit unit = unitList.Where(p => p.Id.Equals(id)).ToList().First();
                            if (!(unit.DataContainers.Count() > 0))
                            {
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
                                unit = UM.Update(unit);
                                List<long> DataTypelIdList = new List<long>();

                                updataAssociatedDataType(unit, checkedRecords);
                            }
                        }
                    }

                }
          

            Session["Window"] = false;
            Session["Unit"] = new Unit();
            return RedirectToAction(parent);
        }

        private List<DataType> updataAssociatedDataType(Unit unit, long[] newDataTypelIds)
        {
            if (unit != null)
            {
                DataTypeManager dataTypeManger = new DataTypeManager();

                UnitManager unitManager = new UnitManager();

                unit = unitManager.Repo.Get(unit.Id);
                var existingDataTypes = unit.AssociatedDataTypes.ToList();                   
                var newDataTypes = newDataTypelIds == null? new List<DataType>() : dataTypeManger.Repo.Query().Where(p => newDataTypelIds.Contains(p.Id)).ToList();
                var tobeAddedDataTypes = newDataTypes.Except(existingDataTypes).ToList();

                if(tobeAddedDataTypes != null && tobeAddedDataTypes.Count() > 0)
                    unitManager.AddAssociatedDataType(unit, tobeAddedDataTypes);

                unit = unitManager.Repo.Get(unit.Id);
                existingDataTypes = unit.AssociatedDataTypes.ToList();
                var toBeRemoved = existingDataTypes.Except(newDataTypes).ToList();
                if (toBeRemoved != null && toBeRemoved.Count() > 0)
                    unitManager.RemoveAssociatedDataType(unit, toBeRemoved);

                unit = unitManager.Repo.Get(unit.Id);
                return unit.AssociatedDataTypes.ToList();
            }
            return null;
        }

        public ActionResult deletUnit(long id)
        {

            if (id != 0)
            {
                UnitManager UM = new UnitManager();
                Unit unit = UM.Repo.Get(id);
                if (unit != null)
                {
                    if (!unitInUse(unit))
                    {

                        UM.Delete(unit);
                    }
                }
            }
            return RedirectToAction("UnitManager");
        }

        public bool unitInUse(Unit unit)
        {
            List<DataAttribute> attributes = GetAttRepo();
            bool inUse = false;
            foreach (DataAttribute a in attributes)
            {
                if (a.Unit != null)
                {
                    if (a.Unit.Id == unit.Id)
                        inUse = true;
                }
            }
            return inUse;
        }

        public ActionResult openUnitWindow(long id)
        {
            if (id != 0)
            {
                UnitManager unitManager = new UnitManager();
                Unit unit = unitManager.Repo.Get().Where(u => u.Id == id && u.AssociatedDataTypes.Count != null).FirstOrDefault();
                
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

        public ActionResult openClassWindow(long id)
        {

            if (id != 0)
            {
                ClassifierManager CM = new ClassifierManager();
                Classifier classifier = CM.Repo.Get(id);
                if (classifier != null)
                {
                    Session["Class"] = classifier;
                    Session["Window"] = true;
                }
                else
                {
                    Session["Class"] = new Classifier();
                    Session["Window"] = false ;
                }
            }
            else
            {
                Session["Class"] = new Classifier();
                Session["Window"] = true;
            }
            return RedirectToAction("ClassificationManager");
        }

        public ActionResult deletClass(long id)
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

        public ActionResult DataTypeManager()
        {
            if (Session["Window"] == null)
                Session["Window"] = false;

            DataTypeManager dataTypeManager = new DataTypeManager();
            List<DataType> datatypeList = dataTypeManager.Repo.Get().Where(d=> d.DataContainers.Count != null).ToList();

            return View(datatypeList);
        }

        public ActionResult editDataType(DataType Model, long id,string systemType, string parent)
        {
            DataTypeManager dataTypeManager = new DataTypeManager();
            IList<DataType> DataTypeList = dataTypeManager.Repo.Get();
            TypeCode typecode = new TypeCode();

            foreach (TypeCode tc in Enum.GetValues(typeof(TypeCode)))
            {
                if (tc.ToString() == systemType)
                    typecode = tc;
            } 

            Model.Name = cutSpaces(Model.Name);
            Model.Description = cutSpaces(Model.Description);

            if (id == 0)
            {
                if (Model.Name != "" && Model.Name != null)
                {
                    bool nameNotExist = DataTypeList.Where(p => p.Name.ToLower().Equals(Model.Name.ToLower())).Count().Equals(0);
                    if (nameNotExist)
                    {
                        DataTypeManager DTM = new DataTypeManager();
                        DTM.Create(Model.Name, Model.Description, typecode);
                    }
                }
            }
            else
            {
                if (Model.Name != "" && Model.Name != null)
                {
                    DataType tempDataType = new DataType();
                    bool nameNotExist = DataTypeList.Where(p => p.Name.ToLower().Equals(Model.Name.ToLower())).Count().Equals(0);

                    if (!nameNotExist)
                        tempDataType = DataTypeList.Where(p => p.Name.Equals(Model.Name)).ToList().First();

                    if (nameNotExist || id == tempDataType.Id)
                    {
                        DataType dataType = DataTypeList.Where(p => p.Id.Equals(id)).ToList().First();
                        if (!(dataType.DataContainers.Count() > 0))
                        {
                            DataTypeManager DTM = new DataTypeManager();
                            dataType.Name = Model.Name;
                            dataType.Description = Model.Description;
                            dataType.SystemType = typecode.ToString();
                            DTM.Update(dataType);
                        }
                    }
                }
            }
            Session["Window"] = false;
            Session["DataType"] = new DataType();
            return RedirectToAction(parent);
        }

        public ActionResult openDataTypeWindow(long id)
        {

            if (id != 0)
            {

                DataTypeManager DTM = new DataTypeManager();
                DataType dataType = DTM.Repo.Get(id);
               
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

        public ActionResult deletDataType(long id)
        {

            if (id != 0)
            {
                DataTypeManager dataTypeManager = new DataTypeManager();
                DataType dataType = dataTypeManager.Repo.Get(id);
                if (dataType != null)
                {
                    if (dataType.DataContainers.Count == 0)
                    {
                        DataTypeManager DTM = new DataTypeManager();
                        DTM.Delete(dataType);
                    }
                }
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
            if (Session["Window"] == null)
                Session["Window"] = false;

            DataContainerManager dataAttributeManager = new DataContainerManager();
            List<DataAttribute> dataAttributeList = dataAttributeManager.DataAttributeRepo.Get().Where(a => a.UsagesAsVariable.Count != null && a.Unit.Name != null && a.DataType.Name != null).ToList();

            UnitManager unitManager = new UnitManager();
            DataTypeManager dataTypeManager = new DataTypeManager();
            Session["dataTypeList"] = dataTypeManager.Repo.Get().ToList();
            Session["unitlist"] = unitManager.Repo.Get().ToList();

            return View(dataAttributeList);
        }

        public ActionResult editAttribute(DataAttribute Model, long id, long unitId, long dataTypeId, string parent)
        {
            IList<DataAttribute> DataAttributeList = GetAttRepo();

            Model.ShortName = cutSpaces(Model.ShortName);
            Model.Name = cutSpaces(Model.Name);
            Model.Description = cutSpaces(Model.Description);

            if (id == 0)
            {
                if (Model.Name != "" && Model.Name != null)
                {
                    bool nameNotExist = DataAttributeList.Where(p => p.Name.ToLower().Equals(Model.Name.ToLower())).Count().Equals(0);
                    if (nameNotExist)
                    {
                        UnitManager UM = new UnitManager();
                        Unit unit = UM.Repo.Get(unitId);
                        DataTypeManager DTM = new DataTypeManager();
                        DataType dataType = DTM.Repo.Get(dataTypeId);
                        DataContainerManager DAM = new DataContainerManager();

                        DataAttribute temp = new DataAttribute();
                        DAM.CreateDataAttribute(Model.ShortName, Model.Name, Model.Description, false, false, "", MeasurementScale.Categorial, DataContainerType.ReferenceType, "", dataType, unit, null, null, null, null, null, null);
                    }
                }
            }
            else
            {
                if (Model.Name != "" && Model.Name != null)
                {
                    DataAttribute tempAttribute = new DataAttribute();

                    bool nameNotExist = DataAttributeList.Where(p => p.Name.ToLower().Equals(Model.Name.ToLower())).Count().Equals(0);
                    if (!nameNotExist)
                        tempAttribute = DataAttributeList.Where(p => p.Name.Equals(Model.Name)).ToList().First();

                    if (nameNotExist || tempAttribute.Id == id)
                    {
                        DataAttribute dataAttribute = DataAttributeList.Where(p => p.Id.Equals(id)).ToList().First();
                        if (!attributeInUse(dataAttribute))
                        {
                            DataContainerManager DAM = new DataContainerManager();
                            dataAttribute.Name = cutSpaces(Model.Name);
                            dataAttribute.ShortName = Model.ShortName;
                            dataAttribute.Description = Model.Description;
                            UnitManager UM = new UnitManager();
                            dataAttribute.Unit = UM.Repo.Get(unitId);
                            DataTypeManager DTM = new DataTypeManager();
                            dataAttribute.DataType = DTM.Repo.Get(dataTypeId);
                            DAM.UpdateDataAttribute(dataAttribute);
                        }
                    }
                }
            }
            Session["Window"] = false;
            Session["DataType"] = new DataType();
            return RedirectToAction(parent);
        }

        public ActionResult openAttributeWindow(long id)
        {

            if (id != 0)
            {
                DataContainerManager DAM = new DataContainerManager();
                DataAttribute dataAttribute = DAM.DataAttributeRepo.Get(id);
                    
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

        public bool attributeInUse(DataAttribute attribute)
        {         
           if (attribute.UsagesAsVariable.Count() == 0 && attribute.UsagesAsParameter.Count() == 0)
               return false;
           else
               return true;          
        }

        private string cutSpaces(string str)
        {
            if (str != "" && str != null)
            {
                str = str.Trim();
                if(str.Length > 255)
                    str = str.Substring(0, 255);
            }
            return (str);
        }

    }

}
