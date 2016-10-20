using System;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.RPM.Output;

using BExIS.Web.Shell.Areas.RPM.Models;
using BExIS.Web.Shell.Areas.RPM.Classes;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;

namespace BExIS.Web.Shell.Areas.RPM.Controllers
{
    public class DataStructureEditController : Controller
    {
        public ActionResult Index(long DataStructureId = 0)
        {
            if (DataStructureId != 0)
                return View(DataStructureId);
            else
                return RedirectToAction("Index", "DataStructureSearch");
        }

        public ActionResult _attributeResultBinding()
        {
            return PartialView("_attributeSearchResult", new AttributePreviewModel().fill());
        }

        public ActionResult _attributeFilterBinding()
        {
            return PartialView("_attributeFilter", new AttributeFilterModel().fill());
        }

        public ActionResult _dataStructureBinding(long dataStructureId)
        {
            return PartialView("_dataStructure", new DataStructurePreviewModel().fill(dataStructureId));
        }

        public ActionResult _getVariableElement(long attributeId, string variableName)
        {
            AttributePreviewStruct variableElement = new VariablePreviewStruct().fill(attributeId);
            variableElement.Name = variableName;
            return PartialView("_variableElement", variableElement);
        }

        public ActionResult deleteDataStructure(long Id, string cssId = "")
        {
            MessageModel DataStructureDeleteValidation = MessageModel.validateDataStructureDelete(Id);
            if (DataStructureDeleteValidation.hasMessage)
            {
                return PartialView("_messageWindow", DataStructureDeleteValidation);
            }
            else
            {
                DataStructureManager dataStructureManager = new DataStructureManager();
                if (dataStructureManager.StructuredDataStructureRepo.Get(Id) != null)
                {
                    StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(Id);
                    DataStructureIO.deleteTemplate(dataStructure.Id);
                    foreach (Variable v in dataStructure.Variables)
                    {
                        dataStructureManager.RemoveVariableUsage(v);
                    }
                    dataStructureManager.DeleteStructuredDataStructure(dataStructure);
                }
                else
                {
                    UnStructuredDataStructure dataStructure = dataStructureManager.UnStructuredDataStructureRepo.Get(Id);
                    dataStructureManager.DeleteUnStructuredDataStructure(dataStructure);
                }
                return PartialView("_message", new MessageModel()
                {
                    Message = "DataStructure" + Id + "deleted",
                    hasMessage = false,
                    CssId = "deleted"
                });
            }
        }

        [HttpPost]
        public ActionResult storeVariables(long Id, storeVariableStruct[] variables)
        {
            if (variables != null && variables.Count() > 0)
            {
                DataStructureManager dataStructureManager = new DataStructureManager();
                StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(Id);
                MessageModel messageModel = MessageModel.validateDataStructureInUse(dataStructure.Id, dataStructure);
                if (messageModel.hasMessage)
                {
                    return PartialView("_messageWindow", messageModel);
                }

                Variable variable = new Variable();
                List<long> order = new List<long>();
            
                foreach (Variable v in dataStructure.Variables)
                {
                    if (!variables.Select(svs => svs.Id).ToList().Contains(v.Id))
                        dataStructureManager.RemoveVariableUsage(v);
                }

                foreach (storeVariableStruct svs in variables.Where(svs => svs.Id == 0).ToList())
                {
                    if (svs.Lable == null)
                        svs.Lable = "";
                    if (svs.Description == null)
                        svs.Description = "";

                    variable = dataStructureManager.AddVariableUsage(dataStructure, new DataContainerManager().DataAttributeRepo.Get(svs.AttributeId), svs.isOptional, svs.Lable.Trim(), null, null, svs.Description.Trim(), new UnitManager().Repo.Get(svs.UnitId));
                    svs.Id = variable.Id;
                }
                dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(Id);

                foreach (storeVariableStruct svs in variables.Where(svs => svs.Id != 0).ToList())
                {
                    if (svs.Lable == null)
                        svs.Lable = "";
                    if (svs.Description == null)
                        svs.Description = "";

                    variable = dataStructure.Variables.Where(v => v.Id == svs.Id).FirstOrDefault();
                    variable.Label = svs.Lable.Trim();
                    variable.Description = svs.Description.Trim();
                    variable.Unit = new UnitManager().Repo.Get(svs.UnitId);
                    variable.DataAttribute = new DataContainerManager().DataAttributeRepo.Get(svs.AttributeId);
                    variable.IsValueOptional = svs.isOptional;
                }

                dataStructure = dataStructureManager.UpdateStructuredDataStructure(dataStructure);
                DataStructureIO.setVariableOrder(dataStructure, variables.Select(svs => svs.Id).ToList());
            }
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}