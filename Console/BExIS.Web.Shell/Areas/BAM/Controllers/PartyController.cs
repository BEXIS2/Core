using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.IO.Transform.Validation.Exceptions;
using BExIS.Modules.Bam.UI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Persistence.Api;
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
            var partyTypes = party_types.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            PartyManager partyManager = new PartyManager();
            //When telerik grid is client based, it's not able to load lazy list and circular dependencies errors
            var partiesForGrid = new List<partyGridModel>();
            if (partyTypes.Any())
            {
                // var childPartyTypes = new PartyRelationshipTypeManager().GetChildPartyTypes(id);
                foreach (var party in partyManager.Repo.Get(cc => partyTypes.Contains(cc.PartyType.Title)))//childPartyTypes.Contains(c.PartyType)))
                    partiesForGrid.Add(new partyGridModel() { Id = party.Id, Name = party.Name, PartyTypeTitle = party.PartyType.Title, StartDate = party.StartDate, EndDate = party.EndDate, IsTemp = party.IsTemp });
            }
            else
                foreach (var party in partyManager.Repo.Get())
                    partiesForGrid.Add(new partyGridModel() { Id = party.Id, Name = party.Name, PartyTypeTitle = party.PartyType.Title, StartDate = party.StartDate, EndDate = party.EndDate, IsTemp = party.IsTemp });
            return PartialView("_partiesPartial", partiesForGrid.OrderByDescending(cc => cc.IsTemp).ThenByDescending(cc => cc.StartDate).ThenBy(cc => cc.Name).ToList());

        }

        public ActionResult Create()
        {
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            ViewBag.Title = PresentationModel.GetGenericViewTitle("Create Party");
            var model = new PartyModel();
            model.PartyTypeList = partyTypeManager.Repo.Get().ToList();
            ViewBag.RelationTabAsDefault = false;
            ViewBag.Title = "Create party";
            return View("CreateEdit", model);
        }

       

        public ActionResult CreateEdit(int id, bool relationTabAsDefault = false)
        {
            PartyManager partyManager = new PartyManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            ViewBag.Title = PresentationModel.GetGenericViewTitle("Edit Party");
            var model = new PartyModel();
            model.PartyTypeList = partyTypeManager.Repo.Get().ToList();
            var party = partyManager.Repo.Get(id);
            model.Description = party.Description;
            model.EndDate = party.EndDate;
            model.Id = party.Id;
            model.PartyType = party.PartyType;
            model.StartDate = party.StartDate;
            ViewBag.RelationTabAsDefault = relationTabAsDefault;
            ViewBag.Title = "Edit party";
            return View("CreateEdit", model);
        }

        [HttpPost]
        public ActionResult CreateEdit(PartyModel partyModel, Dictionary<string, string> partyCustomAttributeValues)
        {
            var redirectAction = RedirectToAction("Index");
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                PartyTypeManager partyTypeManager = new PartyTypeManager();
                PartyManager partyManager = new PartyManager();
                validateAttribute(partyModel);
                if (partyModel.Errors.Count > 0)
                    return View(partyModel);
                var newAddPartyCustomAttrValues = new Dictionary<PartyCustomAttribute, string>();
                var party = new Party();
                if (partyModel.Id != 0)
                {
                    party = partyManager.Repo.Get(partyModel.Id);
                    //Update some fields
                    party.Description = partyModel.Description;
                    party.StartDate = partyModel.StartDate;
                    party.EndDate = partyModel.EndDate;
                    //if relationship rules are satisfied, it is not temp
                    if (string.IsNullOrWhiteSpace(Helpers.Helper.ValidateRelationships(party.Id)))
                        party.IsTemp = false;
                    else
                        party.IsTemp = true;
                    party = partyManager.Update(party);
                    foreach (var partyCustomAttributeValueString in partyCustomAttributeValues)
                    {
                        var partyCustomAttribute = partyTypeManager.RepoPartyCustomAttribute.Get(int.Parse(partyCustomAttributeValueString.Key));
                        string value = string.IsNullOrEmpty(partyCustomAttributeValueString.Value) ? "" : partyCustomAttributeValueString.Value;
                        newAddPartyCustomAttrValues.Add(partyCustomAttribute, value);
                    }
                }
                else
                {
                    var partyType = partyTypeManager.Repo.Get(partyModel.PartyType.Id);
                    var partyStatusType = partyTypeManager.GetStatusType(partyType, "Created");
                    // save party as temp if the reationships are required
                    var requiredPartyRelationTypes = new PartyRelationshipTypeManager().GetAllPartyRelationshipTypes(partyType.Id).Where(cc => cc.MinCardinality > 0);

                    //Create party
                    party = partyManager.Create(partyType, "", partyModel.Description, partyModel.StartDate, partyModel.EndDate, partyStatusType, requiredPartyRelationTypes.Any());
                     //if relationship rules are satisfied, it is not temp
                    if (string.IsNullOrWhiteSpace(Helpers.Helper.ValidateRelationships(requiredPartyRelationTypes, party.Id)))
                        party.IsTemp = false;
                    else
                        party.IsTemp = true;
                    if (requiredPartyRelationTypes.Any())
                        redirectAction = RedirectToAction("CreateEdit", new { id = party.Id, relationTabAsDefault = true });
                }
                try
                {

                    partyManager.AddPartyCustomAttriuteValues(party, partyCustomAttributeValues.ToDictionary(cc=>long.Parse(cc.Key), cc=>cc.Value));
                }
                catch (Exception ex)
                {
                    //If there is error , it deletes the party and goes to create step again
                    partyManager.Delete(party);
                    ViewBag.Title = PresentationModel.GetGenericViewTitle("Create Party");
                    var model = new PartyModel();
                    model.PartyTypeList = partyTypeManager.Repo.Get().ToList();
                    ViewBag.RelationTabAsDefault = false;
                    ViewBag.Title = "Create party";
                    if (ex.Message.Contains("uniqueness"))
                        model.Errors.Add(new Error(ErrorType.Value, "Error: There was another party with the same main attributes.\r\n Please try again and use different values!"));
                    else
                        model.Errors.Add(new Error(ErrorType.Value, ex.Message));
                    return View("CreateEdit", model);
                }
            }
            return redirectAction;
        }


        public JsonResult ValidateRelationships(int partyId)
        {
            return Json(Helpers.Helper.ValidateRelationships(partyId));
        }
        public ActionResult View(int id)
        {
            PartyManager partyManager = new PartyManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            ViewBag.Title = PresentationModel.GetGenericViewTitle("View Party");
            var model = new PartyModel();
            model.PartyTypeList = partyTypeManager.Repo.Get().ToList();
            var party = partyManager.Repo.Get(id);
            model.Description = party.Description;
            model.EndDate = party.EndDate;
            model.Id = party.Id;
            model.PartyType = party.PartyType;
            model.StartDate = party.StartDate; ;
            ViewBag.Title = "View party";
            return View(model);
        }

        public ActionResult ViewPartyDetail(int id)
        {
            PartyManager partyManager = new PartyManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            ViewBag.Title = PresentationModel.GetGenericViewTitle("View Party");
            var model = new PartyModel();
            model.PartyTypeList = partyTypeManager.Repo.Get().ToList();
            var party = partyManager.Repo.Get(id);
            model.Description = party.Description;
            model.EndDate = party.EndDate;
            model.Id = party.Id;
            model.PartyType = party.PartyType;
            model.StartDate = party.StartDate;
            model.ViewMode = true;
            return PartialView("View", model);
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
            PartyManager partyManager = new PartyManager();
            partyManager.Delete(party);
            ViewBag.Title = "Delete party";
            return RedirectToAction("Index");
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
            PartyManager partyManager = new PartyManager();
            var party = partyManager.Repo.Get(partyId);
            List<PartyRelationship> partyRelationships = ConvertDictionaryToPartyRelationships(partyRelationshipsDic);
            var partyRelationshipManager = new PartyRelationshipTypeManager();
            foreach (var partyRelationship in partyRelationships)
            {
                var secondParty = partyManager.Repo.Get(partyRelationship.SecondParty.Id);
                var partyRelationshipType = partyRelationshipManager.Repo.Get(partyRelationship.PartyRelationshipType.Id);
                //Min date value is sent from telerik date time element, if it was empty
                if (partyRelationship.EndDate == DateTime.MinValue)
                    partyRelationship.EndDate = DateTime.MaxValue;
                partyManager.AddPartyRelationship(party, secondParty, partyRelationshipType, partyRelationship.Title, partyRelationship.Description, partyRelationship.StartDate, partyRelationship.EndDate, partyRelationship.Scope);
            }
            //if relationship rules are satisfied, it is not temp
            if (string.IsNullOrWhiteSpace(Helpers.Helper.ValidateRelationships(party.Id)))
                party.IsTemp = false;
            else
                party.IsTemp = true;
            partyManager.Update(party);
            return RedirectToAction("CreateEdit", "party", new { id = partyId, relationTabAsDefault = true });
        }
        [HttpGet]
        public Boolean CheckUniqeness(int partyTypeId, int partyId, string hash)
        {
            PartyManager partyManager = new PartyManager();
            var partyType = new PartyTypeManager().Repo.Get(partyTypeId);
            var party = partyManager.Repo.Get(partyId);
            return partyManager.CheckUniqueness(partyManager.Repo, partyType, hash, party);
        }
        [HttpPost]
        public string DeletePartyRelationship(int id)
        {
            PartyManager partyManager = new PartyManager();
            try
            {
                partyManager.RemovePartyRelationship(partyManager.RepoPartyRelationships.Get(id));

                return "successfull";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id">PartyType Id</param>
        /// <returns></returns>
        public ActionResult LoadPartyCustomAttr(int id)
        {

            long partyId = 0;
            var partyIdStr = HttpContext.Request.Params["partyId"];
            if (long.TryParse(partyIdStr, out partyId) && partyId != 0)
            {
                PartyManager pm = new PartyManager();
                ViewBag.customAttrValues = pm.Repo.Get(partyId).CustomAttributeValues.ToList();
            }
            var customAttrList = new List<PartyCustomAttribute>();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            var partyType = partyTypeManager.Repo.Get(item => item.Id == id);
            if (partyType != null)
                customAttrList = partyType.First().CustomAttributes.ToList();
            return PartialView("_customAttributesPartial", customAttrList);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">party id</param>
        /// <returns></returns>
        public ActionResult LoadPartyRelationships(int id)
        {
            var partyManager = new PartyManager();
            var model = new PartyModel();
            var party = partyManager.Repo.Get(id);
            model.Description = party.Description;
            model.EndDate = party.EndDate;
            model.Id = party.Id;
            model.PartyType = party.PartyType;
            model.StartDate = party.StartDate;
            return PartialView("_partyRelationshipsPartial", model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">party type id</param>
        /// <returns></returns>
        public ActionResult LoadPartyRelationshipType(int id)
        {
            var partyRelManager = new PartyRelationshipTypeManager();
            var party = Request.Params["partyId"] != null ? new PartyManager().Repo.Get(long.Parse(Request.Params["partyId"])) : null;
            ViewBag.sourceParty = party;
            var partyRelationshipTypes = partyRelManager.GetAllPartyRelationshipTypes(party.PartyType.Id);

            return PartialView("_addPartyRelationshipPartial", partyRelationshipTypes.ToList());
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">party relationship id</param>
        /// <returns></returns>
        public ActionResult LoadPartyRelationship(int id)
        {
            var partyManager = new PartyManager();
            var partyRelation = partyManager.RepoPartyRelationships.Get(id);
            ViewBag.viewMode = Request.Params["viewMode"] != null ? Convert.ToBoolean(Request.Params["viewMode"]) : false;
            return PartialView("_relationshipEditViewPartial", partyRelation);
        }

        [HttpPost]
        public bool EditPartyRelationship(PartyRelationship partyRelationship)
        {
            var partyManager = new PartyManager();
            return partyManager.UpdatePartyRelationship(partyRelationship.Id, partyRelationship.Title, partyRelationship.Description, partyRelationship.StartDate, partyRelationship.EndDate, partyRelationship.Scope);
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