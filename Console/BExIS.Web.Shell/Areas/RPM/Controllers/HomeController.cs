using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.TypeSystem;
using BExIS.RPM.Output;
using BExIS.Web.Shell.Areas.RPM.Models;
using Vaiona.Persistence.Api;
using Vaiona.Utils.Cfg;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Web.Shell.Areas.RPM.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Planing/Home/

        string templateName = "BExISppTemplate_Clean.xlsm";

        public ActionResult Index()
        {
            return RedirectToAction("DataStructureDesigner");
        }

        #region Data Structure Designer

        private void setSessions()
        {
            Session["Window"] = false;
            Session["VariableWindow"] = false;
            Session["DatasetWindow"] = false;
            Session["variableId"] = 0;
            Session["saveAsWindow"] = false;
        }

        public ActionResult DataStructureDesigner()
        {
            ViewBag.Title = PresentationModel.GetViewTitle("Manage Data Structures");
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
            DSDM.show = false;
            
            setSessions();

            return View("DataStructureDesigner", DSDM);
        }

        public ActionResult createStructuredDataStructure()
        {
            ViewBag.Title = PresentationModel.GetViewTitle("Create structured Data Structure");
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();

            Session["Structured"] = true;
            setSessions();

            return View("DataStructureDesigner", DSDM); 
        }
        
        public ActionResult createUnStructuredDataStructure()
        {
            ViewBag.Title = PresentationModel.GetViewTitle("Create unstructured Data Structure");
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
            DSDM.structured = false;

            Session["Structured"] = false;
            setSessions();

            return View("DataStructureDesigner", DSDM);
        }

        #region Data Structure Info

        public ActionResult showDataStructure(string SelectedItem)
        {
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();

            string[] temp = SelectedItem.Split(',');

            DSDM.GetDataStructureByID(Convert.ToInt64(temp[0]), Convert.ToBoolean(temp[1]));
            ViewBag.Title = PresentationModel.GetViewTitle("Edit Data Structure: " + DSDM.dataStructure.Name + " (Id: " + DSDM.dataStructure.Id + ")");
            Session["Structured"] = DSDM.structured;
            Session["variableId"] = 0;
            return View("DataStructureDesigner", DSDM);
        }

        #endregion

        #region save Datastructure

        public ActionResult saveDataStructure(DataStructureDesignerModel DSDM, string category, string[] varName, long[] optional, long[] varId, string[] varDesc, long[] varUnit)
        {
            DataStructureManager DSM = new DataStructureManager();
            DSDM.dataStructure.Name = cutSpaces(DSDM.dataStructure.Name);
            DSDM.dataStructure.Description = cutSpaces(DSDM.dataStructure.Description);
            DSDM.structured = (bool)Session["Structured"];
            List<string> errorMsg = new List<string>();

            if(DSDM.dataStructure.Id == 0)
            {
                if (dataStructureValidation(DSDM.dataStructure) != null)
                    errorMsg.Add(dataStructureValidation(DSDM.dataStructure));

                if (errorMsg.Count() > 0)
                {
                    ViewBag.Title = PresentationModel.GetViewTitle("Edit Data Structure: " + DSDM.dataStructure.Name + " (Id: " + DSDM.dataStructure.Id + ")");
                    ViewData["errorMsg"] = errorMsg;
                    setSessions();
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
                        if (DSDM.structured)
                        {
                            ExcelTemplateProvider provider = new ExcelTemplateProvider(templateName);
                            StructuredDataStructure DS = DSM.CreateStructuredDataStructure(DSDM.dataStructure.Name, DSDM.dataStructure.Description, "", "", DSC, null);
                            DSM.UpdateStructuredDataStructure(DS);
                            provider.CreateTemplate(DS.Id);
                            DSDM.GetDataStructureByID(DS.Id);
                            DSDM.dataStructureTree = DSDM.getDataStructureTree();
                        }
                        else
                        { 
                            DSDM.dataStructure = DSM.CreateUnStructuredDataStructure(DSDM.dataStructure.Name, DSDM.dataStructure.Description);
                            DSDM.GetDataStructureByID(DSDM.dataStructure.Id);
                            DSDM.dataStructureTree = DSDM.getDataStructureTree();
                        }
                    }
                    else
                    {
                        ViewBag.Title = PresentationModel.GetViewTitle("Edit Data Structure: " + DSDM.dataStructure.Name + " (Id: " + DSDM.dataStructure.Id + ")");
                        errorMsg.Add("Please type a Name");
                        ViewData["errorMsg"] = errorMsg;
                        setSessions();
                        return View("DataStructureDesigner", DSDM);
                    }
                }
            }
            else
            {   
                if (Request.Params["create"] == "save")
                {
                    if (varName != null)
                    {
                        bool opt = false;
                        string tempMsg = null;

                        for (int i = 0; i < varId.Count(); i++)
                        {
                            if (optional != null)
                            {
                                if (optional.Contains(varId[i]))
                                    opt = true;
                                else
                                    opt = false;
                            }
                            else
                            {
                                opt = false;
                            }

                            tempMsg = saveVariable(cutSpaces(varName[i]), varId[i],varDesc[i], DSDM.dataStructure.Id, opt,varUnit[i]);

                            if (tempMsg != null)
                                errorMsg.Add(tempMsg);
                        }
                    }
                    if (dataStructureValidation(DSDM.dataStructure) != null)
                        errorMsg.Add(dataStructureValidation(DSDM.dataStructure));
                }
                else if (Request.Params["create"] == "saveAs")
                {
                    if (openSaveAsWindow(DSDM.dataStructure))
                    {
                        DataStructure tempDS = new StructuredDataStructure();
                        tempDS.Name = DSDM.dataStructure.Name;
                        if (dataStructureValidation(tempDS) != null)
                            errorMsg.Add(dataStructureValidation(tempDS));

                        ViewData["errorMsg"] = errorMsg;
                        DSDM.GetDataStructureByID(DSDM.dataStructure.Id, DSDM.structured);
                        ViewBag.Title = PresentationModel.GetViewTitle("Edit Data Structure: " + DSDM.dataStructure.Name + " (Id: " + DSDM.dataStructure.Id + ")");
                        setSessions();
                        Session["saveAsWindow"] = true;
                        return View("DataStructureDesigner", DSDM);
                    }
                }

                if (errorMsg.Count() > 0)
                {
                    ViewData["errorMsg"] = errorMsg;
                    DSDM.GetDataStructureByID(DSDM.dataStructure.Id, DSDM.structured);
                    ViewBag.Title = PresentationModel.GetViewTitle("Edit Data Structure: " + DSDM.dataStructure.Name + " (Id: " + DSDM.dataStructure.Id + ")");
                    setSessions();
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
                    if (DSDM.structured)
                    {
                        StructuredDataStructure DS = new StructuredDataStructure();
                        ExcelTemplateProvider provider = new ExcelTemplateProvider(templateName);
                        if (Request.Params["create"] == "save")
                        {
                            DS = DSM.StructuredDataStructureRepo.Get(DSDM.dataStructure.Id);
                            provider.deleteTemplate(DS.Id);
                            DS.Name = DSDM.dataStructure.Name;
                            DS.Description = DSDM.dataStructure.Description;
                            DS = DSM.UpdateStructuredDataStructure(DS);
                        }
                        else if (Request.Params["create"] == "saveAs")
                        {
                            StructuredDataStructure DsOld = DSM.StructuredDataStructureRepo.Get(DSDM.dataStructure.Id);
                            DS = DSM.CreateStructuredDataStructure(DSDM.dataStructure.Name, DSDM.dataStructure.Description, "", "", DSC, null);
                            List<Variable> variables = DSDM.getOrderedVariables(DSM.StructuredDataStructureRepo.Get(DsOld.Id));
                            XmlDocument doc = (XmlDocument)DS.Extra;
                            if (doc == null)
                            {
                                doc = new XmlDocument();
                                XmlNode root = doc.CreateNode(XmlNodeType.Element, "extra", null);
                                doc.AppendChild(root);
                            }
                            if (doc.GetElementsByTagName("original").Count == 0)
                            {
                                XmlNode original = doc.CreateNode(XmlNodeType.Element, "original", null);
                                XmlNode id = doc.CreateNode(XmlNodeType.Element, "id", null);
                                XmlNode versionNo = doc.CreateNode(XmlNodeType.Element, "versionNo", null);
                                id.InnerText = Convert.ToString(DsOld.Id);
                                versionNo.InnerText = Convert.ToString(DsOld.VersionNo);
                                original.AppendChild(id);
                                original.AppendChild(versionNo);
                                doc.FirstChild.AppendChild(original);
                            }
                            if(doc.GetElementsByTagName("order").Count == 0)
                                if (variables.Count != 0)
                                {
                                    XmlNode order = doc.CreateNode(XmlNodeType.Element, "order", null);
                                    Variable temp;
                                    bool opt = false;

                                    foreach (Variable v in variables)
                                    {
                                        if (varName != null)
                                        {
                                            for (int i = 0; i < varId.Count(); i++)
                                            {
                                                if (v.Id == varId[i])
                                                {
                                                    XmlNode variable = doc.CreateNode(XmlNodeType.Element, "variable", null);

                                                    if (optional != null)
                                                    {
                                                        if (optional.Contains(varId[i]))
                                                            opt = true;
                                                        else
                                                            opt = false;
                                                    }
                                                    else
                                                    {
                                                        opt = false;
                                                    }
                                                    if (DS.Variables.Where(p => cutSpaces(p.Label).ToLower().Equals(cutSpaces(varName[i]).ToLower())).Count() > 0 || cutSpaces(varName[i]) == "")
                                                    {
                                                        if (cutSpaces(varName[i]) == "")
                                                            errorMsg.Add("Can't rename Variable " + v.Label + ", invalid name");
                                                        else
                                                            errorMsg.Add("Can't rename Variable " + v.Label + ", name already exist");
                                                        temp = DSM.AddVariableUsage(DS, v.DataAttribute, opt, v.Label, null, null, v.Description, v.Unit);
                                                    }
                                                    else
                                                    {   
                                                        UnitManager unitManager = new UnitManager();
                                                        temp = DSM.AddVariableUsage(DS, v.DataAttribute, opt, cutSpaces(varName[i]), null, null, varDesc[i], unitManager.Repo.Get(varUnit[i]));
                                                    }
                                                    variable.InnerText = temp.Id.ToString();
                                                    order.AppendChild(variable);
                                                    ViewData["errorMsg"] = errorMsg;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            XmlNode variable = doc.CreateNode(XmlNodeType.Element, "variable", null);
                                            temp = DSM.AddVariableUsage(DS, v.DataAttribute, v.IsValueOptional, v.Label, null, null, v.Description);
                                            variable.InnerText = temp.Id.ToString();
                                            order.AppendChild(variable);
                                        }
                                    }
                                    doc.FirstChild.AppendChild(order);
                                    DS.Extra = doc;
                                }                                
                            DS = DSM.UpdateStructuredDataStructure(DS);
                        }
                        DSDM.GetDataStructureByID(DS.Id, DSDM.structured);
                        provider.CreateTemplate(DSM.StructuredDataStructureRepo.Get(DSDM.dataStructure.Id));
                        DSDM.dataStructureTree = DSDM.getDataStructureTree();
                    }
                    else
                    {
                        UnStructuredDataStructure DS = new UnStructuredDataStructure();
                        if (Request.Params["create"] == "save")
                        {
                            DS = DSM.UnStructuredDataStructureRepo.Get(DSDM.dataStructure.Id);
                            DS.Name = DSDM.dataStructure.Name;
                            DS.Description = DSDM.dataStructure.Description;
                            DS = DSM.UpdateUnStructuredDataStructure(DS);
                        }
                        else if (Request.Params["create"] == "saveAs")
                        {
                            DS = DSM.CreateUnStructuredDataStructure(DSDM.dataStructure.Name, DSDM.dataStructure.Description);                    
                        }
                        DSDM.GetDataStructureByID(DS.Id, DSDM.structured);
                        DSDM.dataStructureTree = DSDM.getDataStructureTree();
                    }
                }
            }
            setSessions();
            string returnString = DSDM.dataStructure.Id.ToString() + "," + DSDM.structured.ToString();
            return RedirectToAction("showDataStructure", new { SelectedItem = returnString });
        }

        private bool openSaveAsWindow(DataStructure dataStructure)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            List<DataStructure> dataStructureList = new List<DataStructure>();
            
            List<StructuredDataStructure> StrTemp = dataStructureManager.StructuredDataStructureRepo.Get().ToList();
            foreach (DataStructure ds in StrTemp)
            {
                dataStructureList.Add(ds);
            }

            List<UnStructuredDataStructure> UnSTemp = dataStructureManager.UnStructuredDataStructureRepo.Get().ToList();
            foreach (DataStructure ds in UnSTemp)
            {
                dataStructureList.Add(ds);
            }

            if (cutSpaces(dataStructure.Name) == "" || cutSpaces(dataStructure.Name) == null)
            {
                Session["saveAsWindow"] = true;
                return true;
            }
            else if (dataStructureList.Where(p => cutSpaces(p.Name).ToLower().Equals(cutSpaces(dataStructure.Name).ToLower())).Count() > 0)
            {
                Session["saveAsWindow"] = true;
                return true;
            }
            Session["saveAsWindow"] = false;
            return false;
        }

        private string dataStructureValidation(DataStructure dataStructure)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            List<DataStructure> dataStructureList = new List<DataStructure>();

                List<StructuredDataStructure> StrTemp = dataStructureManager.StructuredDataStructureRepo.Get().ToList();
                foreach (DataStructure ds in StrTemp)
                {
                    dataStructureList.Add(ds);
                }
            
                List<UnStructuredDataStructure> UnSTemp = dataStructureManager.UnStructuredDataStructureRepo.Get().ToList();
                foreach (DataStructure ds in UnSTemp)
                {
                    dataStructureList.Add(ds);
                }


                if (!(dataStructureList.Where(p => p.Id.Equals(dataStructure.Id)).Count() > 0) && dataStructure.Id != 0)
                {
                        return "Can\'t save Data Structure, doesn't exist anymore.";
                }
                if (dataStructure.Datasets.Count > 0)
                {
                    return "Can\'t save/rename Data Structure, is in use.";
                }
                if (cutSpaces(dataStructure.Name) == "" || cutSpaces(dataStructure.Name) == null)
                {
                    return "Can\'t save/rename Data Structure, invalid Name.";
                }
                else if (dataStructureList.Where(p => cutSpaces(p.Name).ToLower().Equals(cutSpaces(dataStructure.Name).ToLower())).Count() > 0)
                {
                    long newDataStructureId = dataStructureList.Where(p => cutSpaces(p.Name).ToLower().Equals(cutSpaces(dataStructure.Name).ToLower())).ToList().First().Id;
                    if (newDataStructureId != dataStructure.Id)
                            return "Can\'t save Data Structure, Name already exist.";
                    }
            return null;
                }

     
        public ActionResult deleteDataStructure(long id)
        {
            bool structured = (bool)Session["Structured"];


            //string message = "" ;

            //if (name != null && name != "")
            //    message = "Delete Data Structure " + name + "?";
            //else
            //    message = "Delete Data Structure?";

            //string caption = "Confirmation";
            //MessageBoxButtons buttons = MessageBoxButtons.YesNo;
            //DialogResult result;
            //// Displays the MessageBox.
            //result = MessageBox.Show(message, caption, buttons);
            if (id != 0)
            {
                if (structured)
                {
                    DataStructureManager dataStructureManager = new DataStructureManager();
                    StructuredDataStructure dataStructure = new StructuredDataStructure();
                    dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(id);

                    if (dataStructure != null)
                    {
                        if (dataStructure.Datasets.Count == 0)
                        {
                            DataStructureManager DSM = new DataStructureManager();
                            if (dataStructure.Variables.Count > 0)
                            {
                                foreach (Variable v in dataStructure.Variables)
                                {
                                    DSM.RemoveVariableUsage(v);
                                }
                            }
                            ExcelTemplateProvider provider = new ExcelTemplateProvider();
                            provider.deleteTemplate(id);
                            DSM.DeleteStructuredDataStructure(dataStructure);
                            return RedirectToAction("DataStructureDesigner");
                        }
                        else
                        {
                            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
                            DSDM.GetDataStructureByID(id);
                            return View("DataStructureDesigner", DSDM);
                        }
                    }
                    else
                    {
                        return RedirectToAction("DataStructureDesigner");
                    }

                }
                else
                {
                    DataStructureManager dataStructureManager = new DataStructureManager();
                    UnStructuredDataStructure dataStructure = new UnStructuredDataStructure();
                    dataStructure = dataStructureManager.UnStructuredDataStructureRepo.Get(id);
                    
                    if (dataStructure != null)
                    {
                        if (dataStructure.Datasets.Count == 0)
                        {
                            DataStructureManager DSM = new DataStructureManager();
                            DSM.DeleteUnStructuredDataStructure(dataStructure);
                            return RedirectToAction("DataStructureDesigner");
                        }
                        else
                        {
                            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
                            DSDM.GetDataStructureByID(id, false);
                            return View("DataStructureDesigner", DSDM);
                        }
                    }
                    else
                    {
                        return RedirectToAction("DataStructureDesigner");
                    }
                }
            }
            return RedirectToAction("DataStructureDesigner");
        }

        #endregion

        #region add Variable

        public ActionResult showVariables(long id)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
            
            if (id != 0)
            {
                DSDM.GetDataStructureByID(id);
                DataContainerManager dataAttributeManager = new DataContainerManager();
                DSDM.dataAttributeList = dataAttributeManager.DataAttributeRepo.Get().ToList();
            }
                    
            if ((bool)Session["Window"] == false)
            {
                Session["Window"] = true;
                Session["dataStructureId"] = DSDM.dataStructure.Id;
            }
            else
            {
                Session["Window"] = false;
                Session["selected"] = null;
            }
            return View("DataStructureDesigner", DSDM);
        }

        public ActionResult AddVariables()
        {
            long id = (long)Session["dataStructureId"];
            long[][] selected = (long[][])Session["selected"];

            DataStructureManager dataStructureManager = new DataStructureManager();
            StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(id);
            //StructuredDataStructure dataStructure = DSDM.GetDataStructureByID(id);

            if (dataStructure != null)
            {
                if (!(dataStructure.Datasets.Count() > 0))
                {
                    if (selected != null)
                    {
                        DataContainerManager dataAttributeManager = new DataContainerManager();
                        DataAttribute temp = new DataAttribute();
                        XmlDocument doc = (XmlDocument)dataStructure.Extra;
                        XmlNode order;
                        if (doc == null)
                        {
                            doc = new XmlDocument();
                            XmlNode root = doc.CreateNode(XmlNodeType.Element, "extra", null);
                            doc.AppendChild(root);
                        }
                        if (doc.GetElementsByTagName("order").Count != 0)
                        {
                            order = doc.GetElementsByTagName("order")[0];
                        }
                        else
                        {
                            order = order = doc.CreateNode(XmlNodeType.Element, "order", null);
                            doc.FirstChild.AppendChild(order);
                        }

                        Variable var = new Variable();
                        int count = 0;
                        string tempName = null;

                        for (int i = 0; i < selected.Length; i++)
                        {
                            count = 0;
                            temp = dataAttributeManager.DataAttributeRepo.Get(selected[i][0]);
                            tempName = temp.Name;
                            if (temp != null)
                            {
                                for (int j = 0; j < selected[i][1]; j++)
                                {
                                    while (dataStructure.Variables.Where(p => cutSpaces(p.Label).ToLower().Equals(cutSpaces(tempName).ToLower())).Count() > 0)
                                    {
                                        count++;
                                        tempName = temp.Name + " (" + count + ")";                                            
                                    }
                                    var = dataStructureManager.AddVariableUsage(dataStructure, temp, true,tempName, null, null, temp.Description, temp.Unit);

                                XmlNode variable = doc.CreateNode(XmlNodeType.Element, "variable", null);
                                variable.InnerText = var.Id.ToString();
                                    order.AppendChild(variable);                            
                            }
                        }
                        }
                        dataStructureManager.UpdateStructuredDataStructure(dataStructure);
                        ExcelTemplateProvider provider = new ExcelTemplateProvider(templateName);
                        provider.CreateTemplate(dataStructure);
                    }
                }
            }
            else
            {
                Session["selected"] = null;
                return RedirectToAction("DataStructureDesigner");
            }
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
            DSDM.GetDataStructureByID(dataStructure.Id);
            Session["selected"] = null;
            return View("DataStructureDesigner", DSDM);
        }

        public ActionResult deleteVariable(long id, long dataStructureId)
        {
            if (dataStructureId != 0)
            {
                DataStructureManager DSM = new DataStructureManager();
                StructuredDataStructure dataStructure = DSM.StructuredDataStructureRepo.Get(dataStructureId);
                DataStructureDesignerModel DSDM = new DataStructureDesignerModel();

                if (!(dataStructure.Datasets.Count > 0))
                {
                    Variable variable = DSM.VariableRepo.Get(id);

                    if (variable != null)
                    {
                        XmlDocument doc = (XmlDocument)dataStructure.Extra;

                        if (doc.GetElementsByTagName("order").Count != 0)
                        {
                            XmlNode order = doc.GetElementsByTagName("order")[0];
                            foreach (XmlNode v in order)
                            {
                                if (Convert.ToInt64(v.InnerText) == variable.Id)
                                {
                                    order.RemoveChild(v);
                                    break;
                                }
                            }
                        }

                        DSM.RemoveVariableUsage(variable);
                        ExcelTemplateProvider provider = new ExcelTemplateProvider(templateName);
                        provider.CreateTemplate(dataStructure);
                    }
                    DSDM.GetDataStructureByID(dataStructure.Id);
                    return View("DataStructureDesigner", DSDM);
                }
                else
                {
                    DSDM.GetDataStructureByID(dataStructure.Id);
                    return View("DataStructureDesigner", DSDM);
                }
            
            }
            return RedirectToAction("DataStructureDesigner");
        }
        public ActionResult editVariable(string name, long id, string description, long dataStructureId, bool optional, long unitId)
        {
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
            string errorMsg = saveVariable(name, id,description, dataStructureId, optional, unitId);
            DSDM.GetDataStructureByID(dataStructureId);
            if (errorMsg != null && errorMsg != "")
            {
                setSessions();
                ViewData["varErrorMsg"] = errorMsg;
                Session["VariableWindow"] = true;
                Session["variableId"] = id;
                return View("DataStructureDesigner", DSDM);
                
            }
            setSessions();
            return View("DataStructureDesigner", DSDM);
        }

        public string saveVariable(string name, long id,string description, long dataStructureId, bool optional, long unitId)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(dataStructureId);
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
            UnitManager unitmanger = new UnitManager();
            string errorMsg = null;

            name = cutSpaces(name);
            description = cutSpaces(description);
            if (!(dataStructure.Datasets.Count > 0))
            {
                if (id != 0)
                {
                    Variable var = dataStructureManager.VariableRepo.Get(id);

                    if (var != null)
                    {
                        if (name != null && name != "")
                        {
                            if (dataStructure.Variables.Where(p => cutSpaces(p.Label).ToLower().Equals(cutSpaces(name).ToLower())).Count() > 0)
                            {
                                long newVarId = dataStructure.Variables.Where(p => cutSpaces(p.Label).ToLower().Equals(cutSpaces(name).ToLower())).ToList().First().Id;
                                if (newVarId != var.Id)
                                {
                                    errorMsg = "Can't rename Variable "+var.Label+", name already exist";
                                }
                                else
                                {
                                    var.Label = name;
                                    var.IsValueOptional = optional;
                                    var.Description = description;
                                    var.Unit = unitmanger.Repo.Get(unitId);
                                    dataStructureManager.UpdateStructuredDataStructure(var.DataStructure);

                                    ExcelTemplateProvider provider = new ExcelTemplateProvider(templateName);
                                }
                            }
                            else
                            {
                                var.Label = name;
                                var.IsValueOptional = optional;
                                var.Description = description;
                                var.Unit = unitmanger.Repo.Get(unitId);
                                dataStructureManager.UpdateStructuredDataStructure(var.DataStructure);

                                ExcelTemplateProvider provider = new ExcelTemplateProvider(templateName);
                            }
                        }

                        else
                        {
                            errorMsg = "Can't rename Variable " + var.Label + ", invalid Name";
                        }
                    }
                }
            }
            return errorMsg;
        }

        public ActionResult shiftVariableLeft(long id, long dataStructureId)
        {
            DataStructureManager dsm = new DataStructureManager();
            StructuredDataStructure ds = dsm.StructuredDataStructureRepo.Get(dataStructureId);
            XmlDocument doc = (XmlDocument)ds.Extra;
            XmlNodeList order = doc.GetElementsByTagName("order");
            List<long> tempList = new List<long>();

            foreach (XmlNode x in order[0])
            {
                tempList.Add(Convert.ToInt64(x.InnerText));
            }

            for(int i = 0;i<tempList.Count();i++)
            {
                if (tempList.ElementAt(i) == id)
                {
                    long temp = tempList.ElementAt(i);
                    tempList.RemoveAt(i);
                    tempList.Insert(i - 1,temp);
                    break;
                }     
            }

            order[0].RemoveAll();

            foreach (long l in tempList)
            {

                XmlNode variable = doc.CreateNode(XmlNodeType.Element, "variable", null);
                variable.InnerText = l.ToString();
                order[0].AppendChild(variable);
            }
            ds.Extra = doc;
            ds = dsm.UpdateStructuredDataStructure(ds);
            DataStructureDesignerModel Model = new DataStructureDesignerModel();
            Model.GetDataStructureByID(ds.Id);
            return View("DataStructureDesigner", Model);
        }

        public ActionResult shiftVariableRight(long id, long dataStructureId)
        {
            DataStructureManager dsm = new DataStructureManager();
            StructuredDataStructure ds = dsm.StructuredDataStructureRepo.Get(dataStructureId);
            XmlDocument doc = (XmlDocument)ds.Extra;
            XmlNodeList order = doc.GetElementsByTagName("order");
            List<long> tempList = new List<long>();

            foreach (XmlNode x in order[0])
            {
                tempList.Add(Convert.ToInt64(x.InnerText));
            }

            for (int i = 0; i < tempList.Count(); i++)
            {
                if (tempList.ElementAt(i) == id)
                {
                    long temp = tempList.ElementAt(i);
                    tempList.RemoveAt(i);
                    tempList.Insert(i + 1, temp);
                    break;
                }
            }

            order[0].RemoveAll();

            foreach (long l in tempList)
            {

                XmlNode variable = doc.CreateNode(XmlNodeType.Element, "variable", null);
                variable.InnerText = l.ToString();
                order[0].AppendChild(variable);
            }
            ds.Extra = doc;
            ds = dsm.UpdateStructuredDataStructure(ds);
            DataStructureDesignerModel Model = new DataStructureDesignerModel();
            Model.GetDataStructureByID(ds.Id);
            return View("DataStructureDesigner", Model);
        }

        public ActionResult openVariableWindow(long id, long dataStructureId)
        {

            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
            DSDM.GetDataStructureByID(dataStructureId);

            if (!(DSDM.inUse))
            {
                Session["VariableWindow"] = true;
                Session["variableId"] = id;
            }
            else
            {
                Session["VariableWindow"] = false;
            }
            return View("DataStructureDesigner", DSDM);
        }

        #endregion

        public ActionResult downloadTemplate(long id)
        {
            if (id != 0)
            {
                DataStructureManager dataStructureManager = new DataStructureManager();
                StructuredDataStructure dataStructure = new StructuredDataStructure();
                dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(id);

                ExcelTemplateProvider provider = new ExcelTemplateProvider(templateName);
                provider.CreateTemplate(dataStructure);
                string path = "";

                XmlNode resources = dataStructure.TemplatePaths.FirstChild;

                XmlNodeList resource = resources.ChildNodes;

                foreach (XmlNode x in resource)
                {
                    if (x.Attributes.GetNamedItem("Type").Value == "Excel")
                        path = x.Attributes.GetNamedItem("Path").Value;
                    
                }
                string rgxPattern = "[<>?\":|\\\\/*]";
                string rgxReplace = "-";
                Regex rgx = new Regex(rgxPattern);

                string filename = rgx.Replace(dataStructure.Name, rgxReplace);

                if (filename.Length > 50)
                    filename = filename.Substring(0, 50);

                return File(Path.Combine(AppConfiguration.DataPath, path), "application/xlsm", "Template_" + dataStructure.Id + "_" + filename + ".xlsm");
            }
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
            DSDM.GetDataStructureByID(id);
            return View("DataStructureDesigner", DSDM);
        }

        #endregion

        #region Unit Manager

        private List<Unit> GetUnitRepo()
        {
            UnitManager UM = new UnitManager();
            List<Unit> repo = UM.Repo.Get().Where(u => u.DataContainers.Count != null && u.AssociatedDataTypes.Count != null).ToList();
            
            foreach(Unit u in repo)
            {
                UM.Repo.LoadIfNot(u.AssociatedDataTypes);
            }
            return(repo);
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
                    Session["errorMsg"] = "invalid Name";
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
                    ViewData["errorMsg"] = "invalid Name";
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
            ViewBag.Title = PresentationModel.GetViewTitle( "Manage Data Types");
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


            foreach (DataTypeCode tc in Enum.GetValues(typeof(DataTypeCode)))
            {
                if (tc.ToString() == systemType)
                {
                    typecode = (TypeCode)tc;
                    break;
                }
            }

            Model.Id = id;
            Model.Name = cutSpaces(Model.Name);
            Model.Description = cutSpaces(Model.Description);           

            if (Model.Name == "" | Model.Name == null)
            {
                Session["Window"] = true;
                Session["nameMsg"] = "invalid name";
                return RedirectToAction(parent);
            }
            else
            {
                bool nameExist = !(DataTypeList.Where(p => p.Name.ToLower().Equals(Model.Name.ToLower())).Count().Equals(0));

                if (Model.Id == 0)
                {
                    if (!nameExist)
                    {
                        DataType dt = dataTypeManager.Create(Model.Name, Model.Description, typecode);
                    }
                    else
                    {
                        Session["Window"] = true;
                        Session["nameMsg"] = "Name already exist";
                        return RedirectToAction(parent);
                    }
                }
                else
                {
                    DataType tempdataType = new DataType();
                    if(nameExist)
                        tempdataType = DataTypeList.Where(p => cutSpaces(p.Name).ToLower().Equals(cutSpaces(Model.Name.ToLower()))).ToList().First();
                    
                    if (!nameExist || Model.Id == tempdataType.Id)
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
                    else
                    {
                        Session["Window"] = true;
                        Session["nameMsg"] = "Name already exist";
                        return RedirectToAction(parent);
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

                Session["nameMsg"] = null;
                Session["DataType"] = dataType;
                Session["Window"] = true;              
            }
            else
            {
                Session["nameMsg"] = null;
                Session["DataType"] = new DataType();
                Session["Window"] = true;
            }
            return RedirectToAction("DataTypeManager");
        }

        public ActionResult deletDataType(long id, string name)
        {

            if (id != 0)
            {
                //string message = "Are you sure you want to delete the Data Type " + name +" ?";
                //string caption = "Confirmation";
                //MessageBoxButtons buttons = MessageBoxButtons.YesNo;
                //DialogResult result;
                //// Displays the MessageBox.
                //result = MessageBox.Show(message, caption, buttons);

                //if (result == DialogResult.Yes)
                //{
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

        public ActionResult showDatasets(long id, bool structured)
        {
            DataStructureManager dataStructureManager = new DataStructureManager();
            DataStructureDesignerModel DSDM = new DataStructureDesignerModel();
            if (id != 0)
            {
                DSDM.GetDataStructureByID(id, structured);
                ViewBag.Title = PresentationModel.GetViewTitle("Edit Data Structure: " + DSDM.dataStructure.Name + " (Id: " + DSDM.dataStructure.Id + ")");
                DSDM.fillDatasetList();
            }

            if ((bool)Session["Window"] == false)
            {
                Session["DatasetWindow"] = true;
            }
            else
            {
                Session["DatasetWindow"] = false;
            }
            return View("DataStructureDesigner", DSDM);
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

        public JsonResult setSelected(long[][] selected)
        {
            if (selected != null)
            {
                Session["selected"] = selected;
                Session["dataStructureId"] = Session["dataStructureId"];
            }
            else
            {
                Session["selected"] = null;
                Session["dataStructureId"] = Session["dataStructureId"];
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}


