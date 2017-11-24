using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Bam.UI.Models;
using BExIS.Security.Services.Subjects;
using System;
using System.Collections.Generic;
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
            return View(new PartyRelationshipTypeManager().GetRootPartyTypesAndChildren());
        }

        public ActionResult LoadParties(string party_types = "")
        {
            PartyManager partyManager = null;
            try
            {
                var partyTypes = party_types.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                partyManager = new PartyManager();
                //When telerik grid is client based, it's not able to load lazy list and circular dependencies errors
                var partiesForGrid = new List<partyGridModel>();
                if (partyTypes.Any())
                {
                    // var childPartyTypes = new PartyRelationshipTypeManager().GetChildPartyTypes(id);
                    foreach (Party party in partyManager.PartyRepository.Get(cc => partyTypes.Contains(cc.PartyType.Title)))//childPartyTypes.Contains(c.PartyType)))
                        partiesForGrid.Add(new partyGridModel() { Id = party.Id, Name = party.Name, PartyTypeTitle = party.PartyType.DisplayName, StartDate = (party.StartDate != null && party.StartDate < new DateTime(1000, 1, 1) ? "" : party.StartDate.ToShortDateString()), EndDate = (party.EndDate != null && party.EndDate > new DateTime(3000, 1, 1) ? "" : party.EndDate.ToShortDateString()), IsTemp = party.IsTemp });
                }
                else
                    foreach (Party party in partyManager.PartyRepository.Get())
                        partiesForGrid.Add(new partyGridModel() { Id = party.Id, Name = party.Name, PartyTypeTitle = party.PartyType.DisplayName, StartDate = (party.StartDate != null && party.StartDate < new DateTime(1000, 1, 1) ? "" : party.StartDate.ToShortDateString()), EndDate = (party.EndDate != null && party.EndDate > new DateTime(3000, 1, 1) ? "" : party.EndDate.ToShortDateString()), IsTemp = party.IsTemp });
                return PartialView("_partiesPartial", partiesForGrid.OrderByDescending(cc => cc.IsTemp).ThenByDescending(cc => cc.StartDate).ThenBy(cc => cc.Name).ToList());
            }
            finally
            {
                partyManager?.Dispose();
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
                model.PartyTypeList = partyTypeManager.PartyTypeRepository.Get().ToList();
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
                model.PartyTypeList = partyTypeManager.PartyTypeRepository.Get().ToList();
                Party party = partyManager.PartyRepository.Get(id);
                model.Description = party.Description;
                model.Id = party.Id;
                model.PartyType = party.PartyType;
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
        public ActionResult CreateEdit(PartyModel partyModel, Dictionary<string, string> partyCustomAttributeValues)
        {
            var party = new Party();
            if (partyModel.Id==0)
                party = Helpers.Helper.CreateParty(partyModel, partyCustomAttributeValues);
            else
                party = Helpers.Helper.EditParty(partyModel, partyCustomAttributeValues);
            if(party.IsTemp)
                return RedirectToAction("CreateEdit", new { id = party.Id, relationTabAsDefault = true });
            else
                return RedirectToAction("Index");            
        }


        public JsonResult ValidateRelationships(int partyId)
        {
            return Json(Helpers.Helper.ValidateRelationships(partyId));
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
                model.EndDate = party.EndDate;
                model.Id = party.Id;
                model.PartyType = party.PartyType;
                model.StartDate = party.StartDate; ;
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
        /// <param name="partyRelationshipsDic">should be filled like PartyRelationship[FiledName_@secondPartyId]=Value in view </param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreatePartyRelationships(int partyId, Dictionary<string, string> partyRelationshipsDic)
        {
            PartyManager partyManager = null;
            try
            {
                partyManager = new PartyManager();
                Party party = partyManager.PartyRepository.Get(partyId);
                List<PartyRelationship> partyRelationships = ConvertDictionaryToPartyRelationships(partyRelationshipsDic);
                var partyRelationshipManager = new PartyRelationshipTypeManager();
                foreach (var partyRelationship in partyRelationships)
                {
                    Party secondParty = partyManager.PartyRepository.Get(partyRelationship.SecondParty.Id);
                    PartyRelationshipType partyRelationshipType = partyRelationshipManager.PartyRelationshipTypeRepository.Get(partyRelationship.PartyRelationshipType.Id);
                    //Min date value is sent from telerik date time element, if it was empty
                    if (partyRelationship.EndDate == DateTime.MinValue)
                        partyRelationship.EndDate = DateTime.MaxValue;
                    partyManager.AddPartyRelationship(party, secondParty, partyRelationshipType, partyRelationship.Title, partyRelationship.Description, partyRelationship.StartDate, partyRelationship.EndDate, partyRelationship.Scope);
                }
                partyManager?.Dispose();
                //partyManager = new PartyManager();
                ////if relationship rules are satisfied, it is not temp
                //  if (string.IsNullOrWhiteSpace(Helpers.Helper.ValidateRelationships(party.Id)))
                //    party.IsTemp = false;
                //else
                //    party.IsTemp = true;
                //partyManager.Update(party);
                return RedirectToAction("CreateEdit", "party", new { id = partyId, relationTabAsDefault = true });
            }
            finally
            {
                partyManager?.Dispose();
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
                return PartialView("~/Areas/BAM/Views/PartyService/_partyRelationshipsPartial.cshtml", model);
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
                Party party = Request.Params["partyId"] != null ? new PartyManager().PartyRepository.Get(long.Parse(Request.Params["partyId"])) : null;
                ViewBag.sourceParty = party;
                var partyRelationshipTypes = partyRelManager.GetAllPartyRelationshipTypes(party.PartyType.Id);

                return PartialView("_addPartyRelationshipPartial", partyRelationshipTypes.ToList());
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

        [HttpPost]
        public bool EditPartyRelationship(PartyRelationship partyRelationship)
        {
            PartyManager partyManager = null;
            try
            {
                partyManager = new PartyManager();
                return partyManager.UpdatePartyRelationship(partyRelationship.Id, partyRelationship.Title, partyRelationship.Description, partyRelationship.StartDate, partyRelationship.EndDate, partyRelationship.Scope);
            }
            finally { partyManager?.Dispose(); }
        }

        private List<PartyRelationship> ConvertDictionaryToPartyRelationships(Dictionary<string, string> partyRelationshipsDic)
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
                var partyRelationship = partyRelationships.FirstOrDefault(item => item.SecondParty.Id == id);
                if (partyRelationship == null)
                {
                    partyRelationship = new PartyRelationship();
                    partyRelationship.SecondParty.Id = id;
                    partyRelationships.Add(partyRelationship);
                }
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
                    }
            }
            return partyRelationships;
        }



    }
}