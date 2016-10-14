using System;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Web.Shell.Areas.RPM.Models;
using BExIS.RPM.Output;
using BExIS.Web.Shell.Areas.RPM.Classes;
using System.Collections.Generic;

namespace BExIS.Web.Shell.Areas.RPM.Controllers
{
    public class DataStructureSearchController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [GridAction]
        public ActionResult _dataStructureResultGridBinding(long[] previewIds, string searchTerms)
        {
            return View(new GridModel(new DataStructureResultsModel(previewIds, searchTerms).dataStructureResults));
        }

        [GridAction]
        public ActionResult _dataStructurePreviewGridBinding(long dataStructureId)
        {
            return View(new GridModel(new StructuredDataStructurePreviewModel(dataStructureId).VariablePreviews));
        }

        public ActionResult _dataStructurePreviewBinding(long dataStructureId, bool structured)
        {
            return PartialView("_dataStructuredStructurePreview", dataStructureId);
        }

        public ActionResult _dataStructureResultBinding(string searchTerms)
        {
            return PartialView("_datatructureSearchResult", searchTerms);
        }

        public ActionResult _createDataStructureBinding(long Id, string Name, string Description, bool isSructured, bool inUse, bool copy)
        {
            MessageModel DataStructureValidation = MessageModel.validateDataStructureInUse(Id);
            if (DataStructureValidation.hasMessage && DataStructureValidation.CssId == "0")
                return PartialView("_messageWindow", DataStructureValidation);        
            else
            {
                if (Id == 0)
                    return PartialView("_createDataStructure", new DataStructureCreateModel()
                    {
                        inUse = inUse
                    });
                else
                    return PartialView("_createDataStructure", new DataStructureCreateModel()
                    {
                        Id = Id,
                        Name = Name,
                        Description = Description,
                        isSructured = isSructured,
                        inUse = inUse,
                        copy = copy
                    });
            }

        }

        public ActionResult _deleteDataStructureBinding(long Id, string cssId)
        {
            return PartialView("_messageWindow", MessageModel.validateDataStructureDelete(Id));
        }

        public ActionResult createDataStructure(long Id, string Name, bool isStructured, string Description = "", string cssId = "")
        {
            MessageModel DataStructureValidation = storeDataStructure(Id, Name.Trim(), isStructured, Description.Trim(), cssId);
            return PartialView("_message", DataStructureValidation);
        }

        public MessageModel storeDataStructure(long Id, string Name, bool isStructured, string Description ="", string cssId = "")
        {
            MessageModel DataStructureValidation = MessageModel.validateDataStructureInUse(Id);
            if (DataStructureValidation.hasMessage)
            {
                return DataStructureValidation;
            }
            else
            {
                DataStructureValidation = MessageModel.validateDataStructureName(Id, Name, cssId);
                if (DataStructureValidation.hasMessage)
                {
                    return DataStructureValidation;
                }
                else
                {
                    DataStructureManager dataStructureManager = new DataStructureManager();
                    DataStructure dataStructure;
                    if (isStructured)
                    {
                        if (Id == 0)
                        {
                            dataStructure = dataStructureManager.CreateStructuredDataStructure(Name.Trim(), Description.Trim(), null, null, DataStructureCategory.Generic);
                            return new MessageModel()
                            {
                                Message = dataStructure.Id.ToString(),
                                hasMessage = false,
                                CssId = "redirect"
                            };
                        }
                        else
                        {
                            StructuredDataStructure StructuredDataStructure = dataStructureManager.StructuredDataStructureRepo.Get(Id);
                            StructuredDataStructure.Name = Name;
                            StructuredDataStructure.Description = Description;
                            dataStructure = dataStructureManager.UpdateStructuredDataStructure(StructuredDataStructure);
                            return new MessageModel()
                            {
                                Message = Id.ToString(),
                                hasMessage = false,
                                CssId = "redirect"
                            };
                        }
                    }
                    else
                    {
                        if (Id == 0)
                        {
                            dataStructure = dataStructureManager.CreateUnStructuredDataStructure(Name.Trim(), Description.Trim());
                            return new MessageModel()
                            {
                                Message = "refresh DataStructureResultGrid",
                                hasMessage = false,
                                CssId = "refresh"
                            };
                        }
                        else
                        {
                            UnStructuredDataStructure unStructuredDataStructure = dataStructureManager.UnStructuredDataStructureRepo.Get(Id);
                            unStructuredDataStructure.Name = Name;
                            unStructuredDataStructure.Description = Description;
                            dataStructure = dataStructureManager.UpdateUnStructuredDataStructure(unStructuredDataStructure);
                            return new MessageModel()
                            {
                                Message = "refresh DataStructureResultGrid",
                                hasMessage = false,
                                CssId = "refresh"
                            };
                        }
                    }
                }
            }
        }

        public ActionResult _validateDataStructureName(long Id, string Name , string cssId)
        {
            return PartialView("_message", MessageModel.validateDataStructureName(Id, Name, cssId));
        }

        public ActionResult copyDataStructure(long Id, bool isStructured, string Name = "" , string Description = "", string cssId = "")
        {
            DataStructureManager dataStructureManager = new DataStructureManager();

            if (!isStructured)
            {
                UnStructuredDataStructure dataStructure = dataStructureManager.UnStructuredDataStructureRepo.Get(Id);
                if (dataStructure != null)
                {
                    if (Name == "")
                    {
                        Name = dataStructure.Name + " - Copy";
                    }

                    if (Description == "" && dataStructure.Description != null)
                    {
                        Description = dataStructure.Description;
                    }  
                         
                    return createDataStructure(0, Name.Trim(), isStructured, Description.Trim(), cssId);
                }
            }
            else
            {
                StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(Id);
                if (dataStructure != null)
                {
                    if (Name == "")
                    {
                        Name = dataStructure.Name + " - Copy";
                    }

                    if (Description == "" && dataStructure.Description != null)
                    {
                        Description = dataStructure.Description;
                    }

                    MessageModel messageModel = storeDataStructure(0, Name.Trim(), isStructured, Description.Trim(), cssId);
                    List<long> order = new List<long>();
                    Variable variable = new Variable();

                    if (!messageModel.hasMessage)
                    {
                        StructuredDataStructure dataStructureCopy = dataStructureManager.StructuredDataStructureRepo.Get(Convert.ToInt64(messageModel.Message));
                        foreach (Variable v in DataStructureIO.getOrderedVariables(dataStructure))
                        {
                            variable = dataStructureManager.AddVariableUsage(dataStructureCopy, v.DataAttribute, v.IsValueOptional, v.Label.Trim(), v.DefaultValue, v.MissingValue, v.Description.Trim(), v.Unit);
                            order.Add(variable.Id);
                        }
                        DataStructureIO.setVariableOrder(dataStructureCopy, order);
                    }
                    return PartialView("_message", messageModel);
                }
            }
            return PartialView("_message", new MessageModel());
        }
    }
}
