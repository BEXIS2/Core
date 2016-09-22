using System;
using System.Web.Mvc;
using Telerik.Web.Mvc;
using BExIS.Dlm.Entities.DataStructure;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Web.Shell.Areas.RPM.Models;

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

        public ActionResult _createDataStructureBinding()
        {
            return PartialView("_createDataStructure");
        }

        public ActionResult _deleteDataStructureBinding(long Id, string cssId)
        {
            return PartialView("_messageDelete", MessageModel.validateDataStructureDelete(Id));
        }

        public ActionResult createDataStructure(long Id, string Name, string Description, bool isSructured, string cssId = "")
        {
            MessageModel DataStructureNameValidation = MessageModel.validateDataStructureName(Id, Name, cssId);
            if (DataStructureNameValidation.hasMessage)
            {
                return PartialView("_message", DataStructureNameValidation);
            }
            else
            {
                DataStructureManager dataStructureManager = new DataStructureManager();
                DataStructure dataStructure;
                if (isSructured)
                {
                    if (Id == 0)
                    {
                        dataStructure = dataStructureManager.CreateStructuredDataStructure(Name, Description, null, null, DataStructureCategory.Generic);
                        return PartialView("_message", new MessageModel()
                        {
                            Message = "/RPM/DataStructureEdit?dataStructureId=" + dataStructure.Id,
                            hasMessage = true,
                            CssId = "redirect"
                        });
                    }
                    else
                    {
                        return PartialView("_message", new MessageModel()
                        {
                            Message = "/RPM/DataStructureEdit?dataStructureId=" + Id,
                            hasMessage = true,
                            CssId = "redirect"
                        });
                    }
                }
                else
                {
                    if (Id == 0)
                    {
                        dataStructure = dataStructureManager.CreateUnStructuredDataStructure(Name, Description);
                        return PartialView("_message", new MessageModel()
                        {
                            Message = "refresh DataStructureResultGrid",
                            hasMessage = true,
                            CssId = "refresh"
                        });
                    }
                    else
                    {
                        UnStructuredDataStructure unStructuredDataStructure = dataStructureManager.UnStructuredDataStructureRepo.Get(Id); 
                        unStructuredDataStructure.Name = Name;
                        unStructuredDataStructure.Description = Description;
                        dataStructure = dataStructureManager.UpdateUnStructuredDataStructure(unStructuredDataStructure);
                        return PartialView("_message", new MessageModel()
                        {
                            Message = "refresh DataStructureResultGrid",
                            hasMessage = true,
                            CssId = "refresh"
                        });
                    }
                }
            }
        }

        public ActionResult deleteDataStructure(long Id, string cssId = "")
        {
            MessageModel DataStructureDeleteValidation = MessageModel.validateDataStructureDelete(Id);
            if (DataStructureDeleteValidation.hasMessage)
            {
                return PartialView("_messageDelete", DataStructureDeleteValidation);
            }
            else
            {
                DataStructureManager dataStructureManager = new DataStructureManager();
                if (dataStructureManager.StructuredDataStructureRepo.Get(Id) != null)
                {
                    StructuredDataStructure dataStructure = dataStructureManager.StructuredDataStructureRepo.Get(Id);
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
                return PartialView("_message", new MessageModel() {
                    Message = "DataStructure" + Id + "deleted",
                    hasMessage = false,
                    CssId = "deleted"
                });
            }
        }

        public ActionResult _validateDataStructureName(long Id, string Name , string cssId)
        {
            return PartialView("_message", MessageModel.validateDataStructureName(Id, Name, cssId));
        }
    }
}
