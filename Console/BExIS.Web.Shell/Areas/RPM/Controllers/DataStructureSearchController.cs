using System;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Modules.Rpm.UI.Models;
using BExIS.IO.Transform.Output;
using BExIS.Modules.Rpm.UI.Classes;
using System.Collections.Generic;
using Vaiona.Web.Mvc.Models;
using Vaiona.Web.Extensions;
using Vaiona.Logging;
using Vaiona.Web.Mvc;
using Vaiona.Persistence.Api;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class DataStructureSearchController : BaseController
    {
        public ActionResult Index()
        {
            ViewBag.Title = PresentationModel.GetViewTitleForTenant("Data Structure Search", this.Session.GetTenant());
            return View();
        }

        [GridAction]
        public ActionResult _dataStructureResultGridBinding(long[] previewIds, string searchTerms)
        {
            searchTerms = Server.UrlDecode(searchTerms);
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
            Name = Server.UrlDecode(Name);
            Description = Server.UrlDecode(Description);
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

        public ActionResult createDataStructure(long Id, string Name, bool isStructured, string Description = "", string cssId = "", bool inUse = false)
        {
            Name = Server.UrlDecode(Name);
            Description = Server.UrlDecode(Description);
            MessageModel DataStructureValidation = storeDataStructure(Id, Name.Trim(), isStructured, Description.Trim(), cssId, inUse);
            return PartialView("_message", DataStructureValidation);
        }

        public MessageModel storeDataStructure(long Id, string Name, bool isStructured, string Description ="", string cssId = "", bool inUse = false)
        {
            Name = Server.UrlDecode(Name);
            Description = Server.UrlDecode(Description);
            MessageModel DataStructureValidation = MessageModel.validateDataStructureInUse(Id);
            if (DataStructureValidation.hasMessage && inUse == false)
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
                    DataStructureManager dataStructureManager = null;
                    try
                    {
                        dataStructureManager = new DataStructureManager();

                        DataStructure dataStructure;
                        if (isStructured)
                        {
                            if (Id == 0)
                            {
                                dataStructure = dataStructureManager.CreateStructuredDataStructure(Name.Trim(), Description.Trim(), null, null, DataStructureCategory.Generic);
                                LoggerFactory.LogData(dataStructure.Id.ToString(), typeof(DataStructure).Name, Vaiona.Entities.Logging.CrudState.Created);
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
                                LoggerFactory.LogData(dataStructure.Id.ToString(), typeof(DataStructure).Name, Vaiona.Entities.Logging.CrudState.Created);
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
                                LoggerFactory.LogData(dataStructure.Id.ToString(), typeof(DataStructure).Name, Vaiona.Entities.Logging.CrudState.Created);
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
                                LoggerFactory.LogData(dataStructure.Id.ToString(), typeof(DataStructure).Name, Vaiona.Entities.Logging.CrudState.Created);
                                return new MessageModel()
                                {
                                    Message = "refresh DataStructureResultGrid",
                                    hasMessage = false,
                                    CssId = "refresh"
                                };
                            }
                        }
                    }
                    finally
                    {
                        dataStructureManager.Dispose();
                    }
                }
            }
        }

        public ActionResult _validateDataStructureName(long Id, string Name , string cssId)
        {
            Name = Server.UrlDecode(Name);
            return PartialView("_message", MessageModel.validateDataStructureName(Id, Name, cssId));
        }

        public ActionResult copyDataStructure(long Id, bool isStructured, string Name = "", string Description = "", string cssId = "")
        {
            Name = Server.UrlDecode(Name);
            Description = Server.UrlDecode(Description);
            DataStructureManager dataStructureManager = null;
            try
            {

                dataStructureManager = new DataStructureManager();


                if (!isStructured)
                {
                    UnStructuredDataStructure dataStructure = dataStructureManager.GetUnitOfWork().GetReadOnlyRepository<UnStructuredDataStructure>().Get(Id);
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
                        LoggerFactory.LogCustom("Copy Data Structure" + Id);
                        return createDataStructure(0, Name.Trim(), isStructured, Description.Trim(), cssId);
                    }
                }
                else
                {
                    StructuredDataStructure dataStructure = dataStructureManager.GetUnitOfWork().GetReadOnlyRepository<StructuredDataStructure>().Get(Id);
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
                            StructuredDataStructure dataStructureCopy = dataStructureManager.GetUnitOfWork().GetReadOnlyRepository<StructuredDataStructure>().Get(Convert.ToInt64(messageModel.Message));
                            dataStructureManager.GetUnitOfWork().GetReadOnlyRepository<StructuredDataStructure>().LoadIfNot(dataStructureCopy.Variables);
                            foreach (Variable v in DataStructureIO.getOrderedVariables(dataStructure))
                            {
                                variable = dataStructureManager.AddVariableUsage(dataStructureCopy, v.DataAttribute, v.IsValueOptional, v.Label.Trim(), v.DefaultValue, v.MissingValue, v.Description.Trim(), v.Unit);
                                order.Add(variable.Id);
                            }
                            DataStructureIO.setVariableOrder(dataStructureCopy, order);
                        }
                        LoggerFactory.LogCustom("Copy Data Structure" + Id);
                        return PartialView("_message", messageModel);
                    }
                }
                return PartialView("_message", new MessageModel());
            }
            finally
            {
                dataStructureManager.Dispose();
            }
        }
    }
}
