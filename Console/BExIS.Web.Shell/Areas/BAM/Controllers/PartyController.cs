using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Bam.UI.Helpers;
using BExIS.Modules.Bam.UI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Bam.UI.Controllers
{
    public class PartyController : Controller
    {

        public ActionResult Index()
        {
            ViewBag.Title = "Manage Parties";
            var partyTypes = new PartyTypeManager().PartyTypeRepository.Get(cc=>!cc.SystemType);
            return View(partyTypes);
        }


        public ActionResult LoadCustomGridColumns(string partyTypeTitle)
        {
            PartyManager partyManager = null;
            PartyTypeManager partyTypeManager = null;
            try
            {
                partyManager = new PartyManager();
                partyTypeManager = new PartyTypeManager();
                var partyType = partyTypeManager.PartyTypeRepository.Get(cc => cc.Title.Equals(partyTypeTitle)).FirstOrDefault();
                var partyCustomGridColumns = partyManager.GetPartyCustomGridColumns(partyType.Id, all: true);
                //To avoid NHibernate.LazyInitializationException in view 
                //Todo: Finding a good soloution
                foreach (var partyCustomGridColumn in partyCustomGridColumns)
                    if (partyCustomGridColumn.CustomAttribute != null)
                        partyCustomGridColumn.CustomAttribute.DisplayName = partyCustomGridColumn.CustomAttribute.DisplayName;
                    else
                        partyCustomGridColumn.TypePair.Title = partyCustomGridColumn.TypePair.Title;
                return PartialView("_partyGridFields", partyCustomGridColumns.ToList());
            }
            finally
            {
                partyManager?.Dispose();
                partyTypeManager?.Dispose();
            }
        }

        public ActionResult customizeGridColumns(IEnumerable<PartyCustomGridColumns> partyCustomGridColumns)
        {
            PartyManager partyManager = null;
            String partytypeTitle = "";
            try
            {
                partyManager = new PartyManager();
                partyManager.UpdatePartyGridCustomColumns(partyCustomGridColumns);

                partytypeTitle = partyManager.PartyCustomGridColumnsRepository.Get(partyCustomGridColumns.First().Id).CustomAttribute.PartyType.Title;

            }
            finally
            {
                partyManager?.Dispose();
            }

            return RedirectToAction("Index", new { partyTypeTitle = partytypeTitle });
        }

        public ActionResult GetPartiesWithCustomColumn(string partyTypeTitle)
        {
            PartyManager partyManager = null;
            PartyTypeManager partyTypeManager = null;
            try
            {
                partyManager = new PartyManager();
                partyTypeManager = new PartyTypeManager();
                var partyType = partyTypeManager.PartyTypeRepository.Get(cc => cc.Title.Equals(partyTypeTitle));
                if (partyType.Any())
                {
                    DataTable dt = new DataTable();
                    var parties = partyManager.PartyRepository.Get(cc => cc.PartyType.Id == partyType.First().Id);
                    ViewBag.partyDataTable = Helper.getPartyDataTable(partyType.First(), parties.ToList());
                    ViewBag.partyTypeId = partyType.First().Id;
                    return PartialView("_partiesDynamicGridPartial");
                }
                else
                {
                    var partiesForGrid = new List<partyGridModel>();
                    foreach (Party party in partyManager.PartyRepository.Get(cc=>!cc.PartyType.SystemType))
                        partiesForGrid.Add(new partyGridModel() { Id = party.Id, Name = party.Name, PartyTypeTitle = party.PartyType.DisplayName, StartDate = (party.StartDate != null && party.StartDate < new DateTime(1000, 1, 1) ? "" : party.StartDate.ToString("yyyy-MM-dd")), EndDate = (party.EndDate != null && party.EndDate > new DateTime(3000, 1, 1) ? "" : party.EndDate.ToString("yyyy-MM-dd")), IsTemp = party.IsTemp });
                    return PartialView("_partiesPartial", partiesForGrid.OrderByDescending(cc => cc.IsTemp).ThenByDescending(cc => cc.StartDate).ThenBy(cc => cc.Name).ToList());
                }
            }
            finally
            {
                partyManager?.Dispose();
                partyTypeManager?.Dispose();
            }
        }

        public ActionResult Create()
        {
            PartyTypeManager partyTypeManager = null;
            try
            {
                partyTypeManager = new PartyTypeManager();
                ViewBag.Title = PresentationModel.GetGenericViewTitle("Create Party");
                var model = new PartyModel();
                model.PartyTypeList = partyTypeManager.PartyTypeRepository.Get(cc=>!cc.SystemType).ToList();
                ViewBag.RelationTabAsDefault = false;
                ViewBag.Title = "Create party";
                return View("CreateEdit", model);
            }
            finally
            {
                partyTypeManager?.Dispose();
            }
        }

        public ActionResult CreateEdit(int id, bool relationTabAsDefault = false)
        {
            PartyManager partyManager = null;
            PartyTypeManager partyTypeManager = null;
            try
            {
                partyManager = new PartyManager();
                partyTypeManager = new PartyTypeManager();
                ViewBag.Title = PresentationModel.GetGenericViewTitle("Edit Party");
                var model = new PartyModel();
                model.PartyTypeList = partyTypeManager.PartyTypeRepository.Get(cc => !cc.SystemType).ToList();
                Party party = partyManager.PartyRepository.Get(id);
                model.Description = party.Description;
                model.Id = party.Id;
                model.PartyType = party.PartyType;
                model.Name = party.Name;
                //Set dates to null to not showing the minimum and maximum dates in UI
                if (party.StartDate == DateTime.MinValue)
                    model.StartDate = null;
                else
                    model.StartDate = party.StartDate;
                if (party.EndDate.Date == DateTime.MaxValue.Date)
                    model.EndDate = null;
                else
                    model.EndDate = party.EndDate;
                ViewBag.RelationTabAsDefault = relationTabAsDefault;
                ViewBag.Title = "Edit party";
                return View("CreateEdit", model);
            }
            finally
            {
                partyManager?.Dispose();
                partyTypeManager?.Dispose();
            }
        }

        [HttpPost]
        public ActionResult CreateEdit(PartyModel partyModel, Dictionary<string, string> partyCustomAttributeValues,IList<PartyRelationship> systemPartyRelationships)
        {
            PartyManager partyManager = null;
            PartyRelationshipTypeManager partyRelationshipTypeManager = null;
            try
            {
                
                partyManager = new PartyManager();
                var party = new Party();
                if (partyModel.Id == 0)
                    party = Helper.CreateParty(partyModel, partyCustomAttributeValues);
                else
                    party = Helper.EditParty(partyModel, partyCustomAttributeValues,systemPartyRelationships);
                
                if (party.IsTemp)
                    return RedirectToAction("CreateEdit", new { id = party.Id, relationTabAsDefault = true });
                else
                    return RedirectToAction("Index");
            }
            finally
            {
                partyManager?.Dispose();
                partyRelationshipTypeManager?.Dispose();
            }
        }


        public JsonResult ValidateRelationships(int partyId)
        {
            return Json(Helpers.Helper.ValidateRelationships(partyId));
        }
        public ActionResult View1()
        {
            return View();
        }
        public ActionResult View(int id)
        {
            PartyManager partyManager = null;
            PartyTypeManager partyTypeManager = null;
            try
            {
                partyManager = new PartyManager();
                partyTypeManager = new PartyTypeManager();
                ViewBag.Title = PresentationModel.GetGenericViewTitle("View Party");
                var model = new PartyModel();
                model.PartyTypeList = partyTypeManager.PartyTypeRepository.Get().ToList();
                Party party = partyManager.PartyRepository.Get(id);
                model.Description = party.Description;
                model.PartyType = party.PartyType;
                model.Id = party.Id;
                model.Name = party.Name;
                if (party.StartDate == DateTime.MinValue)
                    model.StartDate = null;
                else
                    model.StartDate = party.StartDate;
                if (party.EndDate.Date == DateTime.MaxValue.Date)
                    model.EndDate = null;
                else
                    model.EndDate = party.EndDate;
                ViewBag.Title = "View party";
                return View(model);
            }
            finally
            {
                partyManager?.Dispose();
                partyTypeManager?.Dispose();
            }
        }

        public ActionResult ViewPartyDetail(int id)
        {
            PartyManager partyManager = null;
            PartyTypeManager partyTypeManager = null;
            try
            {
                partyManager = new PartyManager();
                partyTypeManager = new PartyTypeManager();
                ViewBag.Title = PresentationModel.GetGenericViewTitle("View Party");
                var model = new PartyModel();
                model.PartyTypeList = partyTypeManager.PartyTypeRepository.Get().ToList();
                Party party = partyManager.PartyRepository.Get(id);
                model.Description = party.Description;
                model.EndDate = party.EndDate;
                model.Id = party.Id;
                model.PartyType = party.PartyType;
                model.StartDate = party.StartDate;
                model.ViewMode = true;
                model.Name = party.Name;
                return PartialView("View", model);
            }
            finally
            {
                partyManager = null;
                partyTypeManager = null;
            }
        }

        public ActionResult DeleteConfirm(int id)
        {
            ViewBag.Title = PresentationModel.GetGenericViewTitle("Delete Party");
            ViewBag.partyId = id;
            return View();
        }

        [HttpPost]
        public ActionResult Delete(Party party)
        {
            PartyManager partyManager = null;
            try
            {
                partyManager = new PartyManager();
                partyManager.Delete(party);
                ViewBag.Title = "Delete party";
                return RedirectToAction("Index");
            }
            finally
            {
                partyManager?.Dispose();
            }
        }

        private void validateAttribute(PartyModel partyModel)
        {
            if (partyModel.StartDate > partyModel.EndDate)
                partyModel.Errors.Add(new IO.Transform.Validation.Exceptions.Error(IO.Transform.Validation.Exceptions.ErrorType.Other, "Start date is greater than end date!"));
            if (partyModel.PartyType.Id == 0)
                partyModel.Errors.Add(new IO.Transform.Validation.Exceptions.Error(IO.Transform.Validation.Exceptions.ErrorType.Other, "Please select party type!"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partyRelationshipsDic">should be filled like PartyRelationship[FiledName_@TargetPartyId]=Value in view </param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreatePartyRelationships(int partyId, Dictionary<string, string> partyRelationshipsDic)
        {
            PartyManager partyManager = null;
            PartyRelationshipTypeManager partyRelationshipManager = null;
            try
            {
                partyManager = new PartyManager();
                var party = partyManager.PartyRepository.Get(partyId);
                var partyRelationships = ConvertDictionaryToPartyRelationships(partyRelationshipsDic, party, partyManager);
                partyRelationshipManager = new PartyRelationshipTypeManager();
                foreach (var partyRelationship in partyRelationships)
                {
                    // Party TargetParty = partyManager.PartyRepository.Get(partyRelationship.TargetParty.Id);
                   // PartyRelationshipType partyRelationshipType = partyRelationshipManager.PartyRelationshipTypeRepository.Get(partyRelationship.PartyRelationshipType.Id);
                    PartyTypePair partyTypePair = partyRelationshipManager.PartyTypePairRepository.Get(partyRelationship.PartyTypePair.Id);
                    //Min date value is sent from telerik date time element, if it was empty
                    if (partyRelationship.EndDate == DateTime.MinValue)
                        partyRelationship.EndDate = DateTime.MaxValue;
                    partyManager.AddPartyRelationship(partyRelationship.SourceParty, partyRelationship.TargetParty,  partyRelationship.Title, partyRelationship.Description, partyTypePair, partyRelationship.StartDate, partyRelationship.EndDate, partyRelationship.Scope);
                }
                partyManager?.Dispose();
                return RedirectToAction("CreateEdit", "party", new { id = partyId, relationTabAsDefault = true });
            }
            finally
            {
                partyManager?.Dispose();
                partyRelationshipManager?.Dispose();
            }
        }

        [HttpPost]
        public string DeletePartyRelationship(int id)
        {
            PartyManager partyManager = null;
            try
            {
                partyManager = new PartyManager();
                partyManager.RemovePartyRelationship(partyManager.PartyRelationshipRepository.Get(id));
                return "successfull";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                partyManager?.Dispose();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">party id</param>
        /// <returns></returns>
        public ActionResult LoadPartyRelationships(int id)
        {
            PartyManager partyManager = null;
            try
            {
                partyManager = new PartyManager();
                var model = new PartyModel();
                Party party = partyManager.PartyRepository.Get(id);
                model.Description = party.Description;
                model.EndDate = party.EndDate;
                model.Id = party.Id;
                model.PartyType = party.PartyType;
                model.StartDate = party.StartDate;
                model.Name = party.Name;
                return PartialView("~/Areas/BAM/Views/PartyService/_partyRelationshipsPartial.cshtml", model);
            }
            finally { partyManager?.Dispose(); }
        }
        public ActionResult LoadSystemRelationships(int id)
        {
            PartyManager partyManager = null;
            try
            {
                partyManager = new PartyManager();
                Party party = partyManager.PartyRepository.Get(id);
                ViewBag.Party = party;
                var partyRelationships = partyManager.PartyRelationshipRepository.Get(cc => cc.SourceParty.Id == id && cc.TargetParty.PartyType.SystemType);
                return PartialView("~/Areas/BAM/Views/Party/_editSystemPartyTypes.cshtml", partyRelationships);
            }
            finally { partyManager?.Dispose(); }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">party type id</param>
        /// <returns></returns>
        public ActionResult LoadPartyRelationshipType(int id)
        {
            PartyRelationshipTypeManager partyRelManager = null;
            try
            {
                partyRelManager = new PartyRelationshipTypeManager();
                var partyId = Request.Params["partyId"] != null ? long.Parse(Request.Params["partyId"]) : 0;
                Party party = new PartyManager().PartyRepository.Get(partyId);
                ViewBag.sourceParty = party;
                var partyRelationshipTypes = partyRelManager.GetAllPartyRelationshipTypes(party.PartyType.Id, true);
                var addpartyRelationshipModel = new List<AddRelationshipModel>();
                // foreach (var partyRelationshipType in partyRelationshipTypes)
                var addRelationshipModel = new List<AddRelationshipModel>();
                addRelationshipModel.Add(new AddRelationshipModel()
                {
                    PartyRelationshipTypes = partyRelationshipTypes.Where(cc => cc.AssociatedPairs.Any(item => item.SourcePartyType.Id == party.PartyType.Id)),
                    SourceParty = party,
                    isAsSource = false
                });
                addRelationshipModel.Add(new AddRelationshipModel()
                {
                    PartyRelationshipTypes = partyRelationshipTypes.Where(cc => cc.AssociatedPairs.Any(item => item.TargetPartyType.Id == party.PartyType.Id)),
                    SourceParty = party,
                    isAsSource = true
                });
                return PartialView("_addPartyRelationshipPartial", addRelationshipModel);
            }
            finally
            {
                partyRelManager?.Dispose();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">party relationship id</param>
        /// <returns></returns>
        public ActionResult LoadPartyRelationship(int id)
        {
            PartyManager partyManager = null;
            try
            {
                partyManager = new PartyManager();
                PartyRelationship partyRelation = partyManager.PartyRelationshipRepository.Get(id);
                ViewBag.viewMode = Request.Params["viewMode"] != null ? Convert.ToBoolean(Request.Params["viewMode"]) : false;
                return PartialView("_relationshipEditViewPartial", partyRelation);
            }
            finally
            {
                partyManager?.Dispose();
            }
        }

        public ActionResult AddSystemSampleParties()
        {
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            //Example for adding system parties
            var customAttrs = new Dictionary<string, string>();
            customAttrs.Add("Name", "test dataset");
            Helper.CreateParty(DateTime.MinValue, DateTime.MaxValue, "", partyTypeManager.PartyTypeRepository.Get(cc=>cc.Title== "Dataset").First().Id, customAttrs);
            customAttrs = new Dictionary<string, string>();
            customAttrs.Add("Name", "test group");
            Helper.CreateParty(DateTime.MinValue, DateTime.MaxValue, "", partyTypeManager.PartyTypeRepository.Get(cc => cc.Title == "Group").First().Id, customAttrs);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public bool EditPartyRelationship(PartyRelationship partyRelationship)
        {
            PartyManager partyManager = null;
            try
            {
                partyManager = new PartyManager();
                return partyManager.UpdatePartyRelationship(partyRelationship.Id, partyRelationship.Title, partyRelationship.Description, partyRelationship.StartDate, partyRelationship.EndDate, partyRelationship.Scope,partyRelationship.Permission);
            }
            finally { partyManager?.Dispose(); }
        }

        private List<PartyRelationship> ConvertDictionaryToPartyRelationships(Dictionary<string, string> partyRelationshipsDic, Party sourceParty, PartyManager partyManager)
        {
            var partyRelationships = new List<PartyRelationship>();
            foreach (var partyRelationshipDic in partyRelationshipsDic)
            {

                var key = partyRelationshipDic.Key.Split('_');
                if (key.Length != 3)
                    continue;
                int id = int.Parse(key[1]);
                int partyTypePairId = int.Parse(key[2]);
                string fieldName = key[0];
                var partyRelationship = partyRelationships.FirstOrDefault(item => item.SourceParty.Id == id || item.TargetParty.Id == id);
                //In each iteratoin one field fills , so in the first iteration we need to do this
                if (partyRelationship == null)
                {
                    partyRelationship = new PartyRelationship();
                    partyRelationship.TargetParty = partyManager.PartyRepository.Get(id);
                    partyRelationship.SourceParty = sourceParty;
                    partyRelationships.Add(partyRelationship);
                }
                partyRelationship.PartyTypePair = new PartyTypePair();
                partyRelationship.PartyTypePair.Id = partyTypePairId;
                if (!string.IsNullOrEmpty(partyRelationshipDic.Value))
                    switch (fieldName.ToLower())
                    {
                        case "title":
                            partyRelationship.Title = partyRelationshipDic.Value;
                            break;
                        case "description":
                            partyRelationship.Description = partyRelationshipDic.Value;
                            break;
                        case "startdate":
                            partyRelationship.StartDate = Convert.ToDateTime(partyRelationshipDic.Value);
                            break;
                        case "enddate":
                            partyRelationship.EndDate = Convert.ToDateTime(partyRelationshipDic.Value);
                            break;
                        case "scope":
                            partyRelationship.Scope = partyRelationshipDic.Value;
                            break;
                        case "partyrelationshiptypeid":
                            partyRelationship.PartyRelationshipType.Id = int.Parse(partyRelationshipDic.Value);
                            break;
                        //when relationship come vice versa
                        case "issource":
                            if (partyRelationshipDic.Value.ToLower().Equals("true"))
                            {
                                partyRelationship.SourceParty = partyManager.PartyRepository.Get(id);
                                partyRelationship.TargetParty = sourceParty;
                            }
                            break;
                    }
            }
            return partyRelationships;
        }
    }
}