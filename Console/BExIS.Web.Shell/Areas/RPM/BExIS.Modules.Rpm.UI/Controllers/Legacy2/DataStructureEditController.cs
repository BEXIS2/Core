using System;
using System.Web;
using System.Web.Mvc;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Rpm.UI.Models;
using BExIS.Modules.Rpm.UI.Classes;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Extensions;
using Vaiona.Web.Mvc.Models;
using Vaiona.Persistence.Api;
using Vaiona.Logging;
using Vaiona.Web.Mvc;
using BExIS.Dlm.Services.TypeSystem;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Subjects;
using System.Xml;
using System.IO;
using BExIS.Dlm.Services.Data;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class DataStructureEditController : BaseController
    {
        public ActionResult Start(long id, int version = 0)
        {
            using (var datasetManager = new DatasetManager())
            { 
                var dataset = datasetManager.GetDataset(id);
                if (dataset != null)
                {
                    return RedirectToAction("Index", new { DataStructureId = dataset.DataStructure.Id });
                }
                else
                {
                    throw new NullReferenceException("Dataset with id " + id + " does not exist.");
                }
            }
        }

        public ActionResult Index(long DataStructureId = 0)
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Data Structure Edit", this.Session.GetTenant());
            DataStructureManager dsm = null;
            try
            {
                dsm = new DataStructureManager();
                if (DataStructureId != 0 && dsm.StructuredDataStructureRepo.Get(DataStructureId) != null)
                    return View(DataStructureId);
                else if (DataStructureId == 0)
                    return View(DataStructureId);
                else
                    return RedirectToAction("Index", "DataStructureSearch");
            }
            finally
            {
                dsm.Dispose();
            }
        }

        public ActionResult _attributeResultBinding()
        {
            ViewData["showCreateVariableTemplate"] = false;

            var moduleSettings = ModuleManager.GetModuleSettings("Ddm");
            var show = moduleSettings.GetEntryValue("showCreateVariableTemplate");
            var showButton = true;

            // check for settings
            if (show != null)
            {
                showButton = Convert.ToBoolean(show);
            }

            // show "Create Variable Template" button if settings true or not set and permission are true
            if (showButton && checkPermission(new Tuple<string, string, string>("RPM", "DataAttribute", "AttributeManager")))
            {
                ViewData["showCreateVariableTemplate"] = true;
            }

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
            variableName = Server.UrlDecode(variableName);
            MessageModel validateVariable = MessageModel.validateAttributeDelete(attributeId);
            if (validateVariable.hasMessage && validateVariable.CssId == "0")
            {
                return PartialView("_messageWindow", validateVariable);
            }
            else
            {
                AttributePreviewStruct variableElement = new VariablePreviewStruct().fill(attributeId);
                variableElement.Name = variableName;
                return PartialView("_variableElement", variableElement);
            }
        }

        public ActionResult _getMissingValueElement(long missinValueId = 0)
        {
            MissingValueStruct missingValueStruct = new MissingValueStruct();
            if (missinValueId > 0)
            {
                MissingValueManager missingValueManager = null;

                try
                {
                    missingValueManager = new MissingValueManager();
                    MissingValue missingValue = missingValueManager.Repo.Get(missinValueId);
                    if (missingValue == null)
                    {
                        return null;
                    }
                    else
                    {
                        missingValueStruct = new MissingValueStruct() {
                            Id = missingValue.Id,
                            DisplayName = missingValue.DisplayName,
                            Description = missingValue.Description,
                            Placeholder = missingValue.Placeholder
                        };
                    }
                }
                catch
                {
                    return null;
                }
                finally
                {
                    missingValueManager.Dispose();
                }
            }
            return PartialView("_missingValueElement", missingValueStruct);
        }

        public ActionResult _getAttributeElement(long attributeId, string variableName)
        {
            variableName = Server.UrlDecode(variableName);
            MessageModel validateAttribute = MessageModel.validateAttributeDelete(attributeId);
            if (validateAttribute.hasMessage && validateAttribute.CssId == "0")
            {
                return PartialView("_messageWindow", validateAttribute);
            }
            else
            {
                AttributePreviewStruct attributeElement = new AttributePreviewStruct().fill(attributeId);
                return PartialView("_attributeElement", attributeElement);
            }
        }

        [HttpPost]
        public bool _checkMissingValueName(long variableId, MissingValueStruct missingValue)
        {
            MissingValueManager missingValueManager = null;

            try
            {
                missingValueManager = new MissingValueManager();
                List<MissingValue> missingValues = missingValueManager.Repo.Query(mv => mv.Variable.Id.Equals(variableId)).ToList();
                foreach (MissingValue mv in missingValues)
                {
                    if (mv.DisplayName == missingValue.DisplayName && mv.Id != missingValue.Id)
                        return false;
                }
                return true;
            }
            finally
            {
                missingValueManager.Dispose();
            }
        }

        [HttpPost]
        public bool _checkMissingValuePlaceholder(long variableId, MissingValueStruct missingValue)
        {
            using (MissingValueManager missingValueManager = new MissingValueManager())
            using (DataStructureManager dataStructureManager = new DataStructureManager())
            {

                Variable variable = dataStructureManager.VariableRepo.Get(variableId);
                TypeCode typecode = new TypeCode();

                foreach (DataTypeCode tc in Enum.GetValues(typeof(DataTypeCode)))
                {
                    if (tc.ToString() == variable.DataAttribute.DataType.SystemType)
                    {
                        typecode = (TypeCode)tc;
                        break;
                    }
                }
                return missingValueManager.ValidatePlaceholder(typecode, missingValue.Placeholder, missingValue.Id);
            }
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
                DataStructureManager dataStructureManager = null;
                MissingValueManager missingValueManager = null;

                try
                {
                    dataStructureManager = new DataStructureManager();
                    missingValueManager = new MissingValueManager();
                    var structureRepo = dataStructureManager.GetUnitOfWork().GetReadOnlyRepository<StructuredDataStructure>();
                    StructuredDataStructure structuredDataStructure = structureRepo.Get(Id);

                    if (structuredDataStructure != null) // Javad: This one retrieves the entity withough using it, and then the next line agian fetches the same! 
                    {
                        DataStructureIO.deleteTemplate(structuredDataStructure.Id);
                        foreach (Variable v in structuredDataStructure.Variables)
                        {
                            List<MissingValue> missingValues = missingValueManager.Repo.Query(mv => mv.Variable.Id.Equals(v.Id)).ToList();
                            foreach (MissingValue mv in missingValues)
                            {
                                missingValueManager.Delete(mv);
                            }
                            dataStructureManager.RemoveVariableUsage(v);
                        }
                        dataStructureManager.DeleteStructuredDataStructure(structuredDataStructure);
                        LoggerFactory.LogData(structuredDataStructure.Id.ToString(), typeof(DataStructure).Name, Vaiona.Entities.Logging.CrudState.Deleted);
                    }
                    else
                    {
                        var unStructureRepo = dataStructureManager.GetUnitOfWork().GetReadOnlyRepository<UnStructuredDataStructure>();
                        UnStructuredDataStructure unStructuredDataStructure = unStructureRepo.Get(Id);
                        dataStructureManager.DeleteUnStructuredDataStructure(unStructuredDataStructure);
                        LoggerFactory.LogData(unStructuredDataStructure.Id.ToString(), typeof(DataStructure).Name, Vaiona.Entities.Logging.CrudState.Deleted);
                    }
                    return PartialView("_message", new MessageModel()
                    {
                        Message = "DataStructure" + Id + "deleted",
                        hasMessage = false,
                        CssId = "deleted"
                    });
                }
                finally
                {
                    dataStructureManager.Dispose();
                    missingValueManager.Dispose();
                }
            }
        }

        [HttpPost]
        public ActionResult storeVariables(long Id, storeVariableStruct[] variables)
        {
            DataStructureManager dataStructureManager = null;
            DataContainerManager dataContainerManager = null;
            MissingValueManager missingValueManager = null;
            UnitManager um = null;
            StructuredDataStructure dataStructure = null;

            try
            {
                dataStructureManager = new DataStructureManager();
                missingValueManager = new MissingValueManager();
                dataContainerManager = new DataContainerManager();
                um = new UnitManager();

                dataStructure = dataStructureManager.GetUnitOfWork().GetReadOnlyRepository<StructuredDataStructure>().Get(Id);
                MessageModel returnObject = new MessageModel();
                MessageModel messageModel = MessageModel.validateDataStructureInUse(dataStructure.Id, dataStructure);
                if (messageModel.hasMessage)
                {
                    foreach (Variable v in dataStructure.Variables)
                    {
                        if (variables.Select(svs => svs.Id).ToList().Contains(v.Id))
                        {
                            v.Description = variables.Where(svs => svs.Id == v.Id).FirstOrDefault().Description;
                        }
                    }

                    dataStructure = dataStructureManager.UpdateStructuredDataStructure(dataStructure);


                    return PartialView("_messageWindow", messageModel);
                }

                if (variables != null && variables.Any())
                {
                    Variable variable = new Variable();
                    List<long> order = new List<long>();

                    foreach (Variable v in dataStructure.Variables)
                    {
                        if (!variables.Select(svs => svs.Id).ToList().Contains(v.Id))
                        {
                            List<MissingValue> missingValues = missingValueManager.Repo.Query(mv => mv.Variable.Id.Equals(v.Id)).ToList();
                            foreach (MissingValue mv in missingValues)
                            {
                                missingValueManager.Delete(mv);
                            }
                            dataStructureManager.RemoveVariableUsage(v);
                            dataStructure.Variables.Remove(v);

                        }
                    }

                    foreach (storeVariableStruct svs in variables.Where(svs => svs.Id == 0).ToList())
                    {
                        if (svs.Lable == null)
                            svs.Lable = "";
                        if (svs.Description == null)
                            svs.Description = "";

                        DataAttribute dataAttribute = dataContainerManager.DataAttributeRepo.Get(svs.AttributeId);
                        if (dataAttribute != null)
                        {
                            variable = dataStructureManager.AddVariableUsage(dataStructure, dataAttribute, svs.isOptional, svs.Lable.Trim(), null, null, svs.Description.Trim(), um.Repo.Get(svs.UnitId));
                            svs.Id = variable.Id;
                            foreach (MissingValueStruct mvs in svs.MissingValues)
                            {
                                if (mvs.Id == 0)
                                {
                                    if (String.IsNullOrEmpty(mvs.Placeholder))
                                        missingValueManager.Create(mvs.DisplayName, mvs.Description, variable);
                                    else
                                        missingValueManager.Create(mvs.DisplayName, mvs.Description, variable, mvs.Placeholder);
                                }
                            }
                        }
                        else
                        {
                            returnObject = new MessageModel()
                            {
                                hasMessage = true,
                                Message = "Not all Variables are stored.",
                                CssId = "0"
                            };
                        }
                    }

                    //dataStructure = dataStructureManager.GetUnitOfWork().GetReadOnlyRepository<StructuredDataStructure>().Get(Id); // Javad: why it is needed?

                    variables = variables.Where(v => v.Id != 0).ToArray();
                    MissingValue missingValue = new MissingValue();
                    foreach (storeVariableStruct svs in variables.Where(svs => svs.Id != 0).ToList())
                    {
                        if (svs.Lable == null)
                            svs.Lable = "";
                        if (svs.Description == null)
                            svs.Description = "";

                        variable = dataStructure.Variables.Where(v => v.Id == svs.Id).FirstOrDefault();
                        if (variable != null)
                        {
                            variable.Label = svs.Lable.Trim();
                            variable.Description = svs.Description.Trim();
                            variable.Unit = um.Repo.Get(svs.UnitId);
                            variable.DataAttribute = dataContainerManager.DataAttributeRepo.Get(svs.AttributeId);
                            variable.IsValueOptional = svs.isOptional;


                            List<MissingValue> missingValues = missingValueManager.Repo.Query(mv => mv.Variable.Id.Equals(svs.Id)).ToList();
                            foreach (MissingValue mv in missingValues)
                            {
                                if (!svs.MissingValues.Select(mvs => mvs.Id).Contains(mv.Id))
                                {
                                    missingValueManager.Delete(mv);
                                }
                            }

                            foreach (MissingValueStruct mvs in svs.MissingValues)
                            {
                                if (mvs.Id == 0)
                                {
                                    if (String.IsNullOrEmpty(mvs.Placeholder))
                                        missingValueManager.Create(mvs.DisplayName, mvs.Description, variable);
                                    else
                                        missingValueManager.Create(mvs.DisplayName, mvs.Description, variable, mvs.Placeholder);
                                }
                                else if (mvs.Id > 0)
                                {
                                    missingValue = missingValues.Where(mv => mv.Id.Equals(mvs.Id)).FirstOrDefault();
                                    if (missingValue != null)
                                    {
                                        missingValue.DisplayName = mvs.DisplayName;
                                        missingValue.Description = mvs.Description;
                                        missingValue.Placeholder = mvs.Placeholder;
                                        missingValueManager.Update(missingValue);
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (Variable v in dataStructure.Variables)
                    {
                        List<MissingValue> missingValues = missingValueManager.Repo.Query(mv => mv.Variable.Id.Equals(v.Id)).ToList();
                        foreach (MissingValue mv in missingValues)
                        {
                            missingValueManager.Delete(mv);
                        }
                        dataStructureManager.RemoveVariableUsage(v);
                    }
                }
                dataStructure = DataStructureIO.setVariableOrder(dataStructure, variables.Select(svs => svs.Id).ToList());
                dataStructure = dataStructureManager.UpdateStructuredDataStructure(dataStructure);

                LoggerFactory.LogCustom("Variables for Data Structure " + Id + " stored.");
                return Json(returnObject, JsonRequestBehavior.AllowGet);
            }
            finally
            {
                dataStructureManager.Dispose();
                dataContainerManager.Dispose();
                um.Dispose();
                missingValueManager.Dispose();
            }
        }

        public ActionResult _deleteDataAttributeBinding(long Id, string cssId)
        {
            return PartialView("_messageWindow", MessageModel.validateAttributeDelete(Id));
        }

        public ActionResult deleteDataAttribute(long Id, string cssId = "")
        {
            MessageModel DataAttributeDeleteValidation = MessageModel.validateAttributeDelete(Id);
            if (DataAttributeDeleteValidation.hasMessage)
            {
                return PartialView("_messageWindow", DataAttributeDeleteValidation);
            }
            else
            {
                DataContainerManager dataAttributeManager = null;
                try
                {
                    dataAttributeManager = new DataContainerManager();

                    DataAttribute dataAttribute = dataAttributeManager.DataAttributeRepo.Get(Id);

                    dataAttributeManager.DeleteDataAttribute(dataAttribute);

                    return PartialView("_message", new MessageModel()
                    {
                        Message = "DataStructure" + Id + "deleted",
                        hasMessage = false,
                        CssId = "deleted"
                    });
                }
                finally
                {
                    dataAttributeManager.Dispose();
                }
            }
        }

        public ActionResult getMessageWindow(string message, bool hasMessage, string cssId)
        {
            message = Server.UrlDecode(message);
            return PartialView("_messageWindow", new MessageModel()
            {
                Message = message,
                hasMessage = hasMessage,
                CssId = cssId
            });
        }

        public ActionResult _createAttributeBinding(long attributeId)
        {
            return PartialView("_editAttribute", new AttributeEditStruct());
        }

        public ActionResult _validateAttributeName(long Id, string Name, string cssId)
        {
            Name = Server.UrlDecode(Name);
            return PartialView("_message", MessageModel.validateAttributeName(Id, Name, cssId));
        }

        public ActionResult createAtttribute(long Id, string Name, long unitId, long dataTypeId, string Description = "", string cssId = "", bool inUse = false)
        {
            Name = Server.UrlDecode(Name);
            Description = Server.UrlDecode(Description);
            MessageModel AttributeValidation = storeAtttribute(Id, Name.Trim(), unitId, dataTypeId, Description.Trim(), cssId, inUse);
            return PartialView("_message", AttributeValidation);
        }

        public MessageModel storeAtttribute(long Id, string Name, long unitId, long dataTypeId, string Description = "", string cssId = "", bool inUse = false)
        {
            Name = Server.UrlDecode(Name);
            Description = Server.UrlDecode(Description);
            MessageModel DataStructureValidation = MessageModel.validateAttributeInUse(Id);
            if (DataStructureValidation.hasMessage && inUse == false)
            {
                return DataStructureValidation;
            }
            else
            {
                DataStructureValidation = MessageModel.validateAttributeName(Id, Name, cssId);
                if (DataStructureValidation.hasMessage)
                {
                    return DataStructureValidation;
                }
                else
                {
                    DataContainerManager dataAttributeManager = null;
                    UnitManager um = null;
                    DataTypeManager dataTypeManager = null;

                    try
                    {

                        dataAttributeManager = new DataContainerManager();
                        DataAttribute dataAttribute;

                        if (Id == 0)
                        {
                            um = new UnitManager();

                            Unit unit = um.Repo.Get(unitId);

                            dataTypeManager = new DataTypeManager();

                            DataType dataType = dataTypeManager.Repo.Get(dataTypeId);
                            dataAttribute = dataAttributeManager.CreateDataAttribute(Name, Name, Description, false, false, "", MeasurementScale.Categorial, DataContainerType.ReferenceType, "", dataType, unit, null, null, null, null, null, null);
                            return new MessageModel()
                            {
                                Message = dataAttribute.Id.ToString(),
                                hasMessage = false,
                                CssId = "refresh"
                            };
                        }
                        else
                        {
                            dataAttribute = dataAttributeManager.DataAttributeRepo.Get(Id);
                            dataAttribute.Name = Name;
                            dataAttribute.Description = Description;
                            dataAttribute = dataAttributeManager.UpdateDataAttribute(dataAttribute);
                            return new MessageModel()
                            {
                                Message = Id.ToString(),
                                hasMessage = false,
                                CssId = "refresh"
                            };
                        }
                    }
                    finally
                    {
                        dataAttributeManager.Dispose();
                        um.Dispose();
                        dataTypeManager.Dispose();
                    }
                }
            }
        }

        public ActionResult _getDataTypes(long unitId)
        {
            List<ItemStruct> DataTypes = new List<ItemStruct>();
            UnitManager um = null;

            try
            {
                um = new UnitManager();

                Unit unit = um.Repo.Get(unitId);
                if (unit.Name.ToLower() != "none")
                {
                    foreach (DataType dt in unit.AssociatedDataTypes)
                    {
                        DataTypes.Add(new ItemStruct()
                        {
                            Name = dt.Name,
                            Id = dt.Id,
                            Description = dt.Description
                        });
                    }
                    return PartialView("_dropdown", DataTypes.OrderBy(dt => dt.Name).ToList());
                }
                else
                {
                    DataTypeManager dtm = null;
                    try
                    {
                        dtm = new DataTypeManager();
                        foreach (DataType dt in dtm.Repo.Get())
                        {
                            DataTypes.Add(new ItemStruct()
                            {
                                Name = dt.Name,
                                Id = dt.Id,
                                Description = dt.Description
                            });
                        }
                        return PartialView("_dropdown", DataTypes.OrderBy(dt => dt.Name).ToList());
                    }
                    finally
                    {
                        dtm.Dispose();
                    }
                }
            }
            finally
            {
                um.Dispose();
            }
        }

        public ActionResult _getRangeConstraint()
        {
            return PartialView("_rangeConstraintView", new RangeConstraintModel());
        }

        public ActionResult _getPatternConstraint()
        {
            return PartialView("_patternConstraintView", new PatternConstraintModel());
        }

        public ActionResult _getDomainConstraint()
        {
            return PartialView("_domainConstraintView", new DomainConstraintModel());
        }

        public JsonResult getRangeConstraintFormalDescription(bool invert, bool mininclude, bool maxinclude, double min = 0, double max = 0)
        {
            RangeConstraint temp = new RangeConstraint(ConstraintProviderSource.Internal, "", AppConfiguration.Culture.Name, "", invert, null, null, null, min, mininclude, max, maxinclude);
            return Json((temp.FormalDescription), JsonRequestBehavior.AllowGet);
        }

        public JsonResult getPatternConstraintFormalDescription(bool invert, string phrase)
        {
            PatternConstraint temp = new PatternConstraint(ConstraintProviderSource.Internal, "", AppConfiguration.Culture.Name, "", invert, null, null, null, phrase, false);
            return Json((temp.FormalDescription), JsonRequestBehavior.AllowGet);
        }

        public JsonResult getDomainConstraintFormalDescription(bool invert, string Terms)
        {
            List<DomainItem> items = createDomainItems(Terms.Trim());

            DomainConstraint Temp = new DomainConstraint(ConstraintProviderSource.Internal, "", AppConfiguration.Culture.Name, "", invert, null, null, null, items);
            return Json((Temp.FormalDescription), JsonRequestBehavior.AllowGet);
        }

        private List<DomainItem> createDomainItems(string Terms)
        {
            List<DomainItem> items = new List<DomainItem>();

            if (!String.IsNullOrEmpty(Terms))
            {
                string[] pairs = Terms.Split(';');
                if (pairs.Length > 1)
                {
                    foreach (string s in pairs)
                    {
                        string temp = s.Trim();

                        if (!String.IsNullOrEmpty(temp))
                        {
                            string[] terms = temp.Split(',');

                            string[] tempArray = new string[terms.Length];
                            for (int i = 0; i < terms.Length; i++)
                            {
                                tempArray[i] = terms[i].Trim();
                            }
                            if (tempArray.Length > 1)
                            {
                                if (!String.IsNullOrEmpty(tempArray[0]) && !String.IsNullOrEmpty(tempArray[1]))
                                {
                                    items.Add(new DomainItem() { Key = tempArray[0].Trim(), Value = tempArray[1].Trim() });
                                }
                            }
                            else
                            {
                                if (tempArray.Length > 0)
                                {
                                    if (!String.IsNullOrEmpty(tempArray[0]))
                                    {
                                        items.Add(new DomainItem() { Key = tempArray[0].Trim(), Value = "na" });
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    pairs[0] = pairs[0].Trim();

                    if (!String.IsNullOrEmpty(pairs[0]))
                    {
                        string[] terms = pairs[0].Split(',');

                        foreach (string s in terms)
                        {
                            string temp = s.Trim();

                            if (!String.IsNullOrEmpty(temp))
                            {
                                items.Add(new DomainItem() { Key = temp.Trim() });
                            }
                        }
                    }
                }
            }
            return items;
        }

        [HttpPost]
        public DataAttribute storeRangeConstraint(RangeConstraintModel constraintModel)
        {
            DataContainerManager dcManager = null;
            try
            {
                dcManager = new DataContainerManager();

                DataAttribute dataAttribute = dcManager.DataAttributeRepo.Get(constraintModel.AttributeId);

                if (constraintModel.Max != 0 || constraintModel.Min != 0)
                {
                    if (constraintModel.Id == 0)
                    {
                        RangeConstraint constraint = new RangeConstraint(ConstraintProviderSource.Internal, "", AppConfiguration.Culture.Name, constraintModel.Description, constraintModel.Negated, null, null, null, constraintModel.Min, constraintModel.MinInclude, constraintModel.Max, constraintModel.MaxInclude);
                        dcManager.AddConstraint(constraint, dataAttribute);
                    }
                    else
                    {
                        for (int i = 0; i < dataAttribute.Constraints.Count; i++)
                        {
                            if (dataAttribute.Constraints.ElementAt(i).Id == constraintModel.Id)
                            {
                                ((RangeConstraint)dataAttribute.Constraints.ElementAt(i)).Description = constraintModel.Description;
                                ((RangeConstraint)dataAttribute.Constraints.ElementAt(i)).Negated = constraintModel.Negated;
                                ((RangeConstraint)dataAttribute.Constraints.ElementAt(i)).Lowerbound = constraintModel.Min;
                                ((RangeConstraint)dataAttribute.Constraints.ElementAt(i)).LowerboundIncluded = constraintModel.MinInclude;
                                ((RangeConstraint)dataAttribute.Constraints.ElementAt(i)).Upperbound = constraintModel.Max;
                                ((RangeConstraint)dataAttribute.Constraints.ElementAt(i)).UpperboundIncluded = constraintModel.MaxInclude;
                                break;
                            }
                        }
                    }
                }
                return dataAttribute;
            }
            finally
            {
                dcManager.Dispose();
            }
        }

        [HttpPost]
        public DataAttribute storePatternConstraint(PatternConstraintModel constraintModel)
        {
            DataContainerManager dcManager = null;
            try
            {
                dcManager = new DataContainerManager();
                DataAttribute dataAttribute = dcManager.DataAttributeRepo.Get(constraintModel.AttributeId);

                if (constraintModel.MatchingPhrase != null && constraintModel.MatchingPhrase != "")
                {
                    if (constraintModel.Id == 0)
                    {
                        PatternConstraint constraint = new PatternConstraint(ConstraintProviderSource.Internal, "", AppConfiguration.Culture.Name, constraintModel.Description, constraintModel.Negated, null, null, null, constraintModel.MatchingPhrase, false);
                        dcManager.AddConstraint(constraint, dataAttribute);
                    }
                    else
                    {
                        for (int i = 0; i < dataAttribute.Constraints.Count; i++)
                        {
                            if (dataAttribute.Constraints.ElementAt(i).Id == constraintModel.Id)
                            {
                                ((PatternConstraint)dataAttribute.Constraints.ElementAt(i)).Description = constraintModel.Description;
                                ((PatternConstraint)dataAttribute.Constraints.ElementAt(i)).Negated = constraintModel.Negated;
                                ((PatternConstraint)dataAttribute.Constraints.ElementAt(i)).MatchingPhrase = constraintModel.MatchingPhrase;
                                break;
                            }
                        }
                    }
                }
                return dataAttribute;
            }
            finally
            {
                dcManager.Dispose();
            }
        }

        public DataAttribute storeDomainConstraint(DomainConstraintModel constraintModel)
        {
            DataContainerManager dcManager = null;
            try
            {
                dcManager = new DataContainerManager();

                DataAttribute dataAttribute = dcManager.DataAttributeRepo.Get(constraintModel.AttributeId);
                List<DomainItem> items = new List<DomainItem>();
                if (constraintModel.Terms != null && constraintModel.Terms.Trim() != "")
                    items = createDomainItems(constraintModel.Terms.Trim());

                if (items.Any())
                {
                    if (constraintModel.Id == 0)
                    {
                        DomainConstraint constraint = new DomainConstraint(ConstraintProviderSource.Internal, "", AppConfiguration.Culture.Name, constraintModel.Description, constraintModel.Negated, null, null, null, items);
                        dcManager.AddConstraint(constraint, dataAttribute);
                    }
                    else
                    {
                        DomainConstraint temp = new DomainConstraint();
                        for (int i = 0; i < dataAttribute.Constraints.Count; i++)
                        {
                            if (dataAttribute.Constraints.ElementAt(i).Id == constraintModel.Id)
                            {
                                temp = (DomainConstraint)dataAttribute.Constraints.ElementAt(i);
                                temp.Materialize();
                                temp.Description = constraintModel.Description;
                                temp.Negated = constraintModel.Negated;
                                temp.Items = items;
                                dcManager.AddConstraint(temp, dataAttribute);
                                break;
                            }
                        }
                    }
                }
                return dataAttribute;
            }
            finally
            {
                dcManager.Dispose();
            }
        }

        protected bool checkPermission(Tuple<string, string, string> Feature)
        {
            var featurePermissionManager = new FeaturePermissionManager();
            var operationManager = new OperationManager();
            var userManager = new UserManager();

            try
            {

                var areaName = Feature.Item1;
                if (areaName == "")
                {
                    areaName = "shell";
                }
                var controllerName = Feature.Item2;
                var actionName = Feature.Item3;

                var userName = HttpContext.User?.Identity?.Name;
                var operation = operationManager.Find(areaName, controllerName, "*");

                var feature = operation.Feature;
                if (feature == null) return true;

                var result = userManager.FindByNameAsync(userName);


                if (featurePermissionManager.HasAccess(result.Result?.Id, feature.Id))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                featurePermissionManager.Dispose();
                operationManager.Dispose();
                userManager.Dispose();
            }
        }
    }
}
