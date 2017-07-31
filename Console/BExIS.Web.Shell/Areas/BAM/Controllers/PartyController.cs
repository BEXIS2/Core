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
                    partiesForGrid.Add(new partyGridModel() { Id = party.Id, Name = party.Name, PartyTypeTitle = party.PartyType.Title, StartDate = party.StartDate, EndDate = party.EndDate });
            }
            else
                foreach (var party in partyManager.Repo.Get())
                    partiesForGrid.Add(new partyGridModel() { Id = party.Id, Name = party.Name, PartyTypeTitle = party.PartyType.Title, StartDate = party.StartDate, EndDate = party.EndDate });
            return PartialView("_partiesPartial", partiesForGrid);

        }

        public ActionResult Create()
        {
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            ViewBag.Title = PresentationModel.GetGenericViewTitle("Create Party");
            var model = new PartyModel();
            model.PartyTypeList = partyTypeManager.Repo.Get().ToList();
            ViewBag.RelationTabAsDefault = false;
            return View("CreateEdit", model);
        }

        /// <summary>
        /// Create party
        /// </summary>
        /// <param name="party"></param>
        /// <param name="partyCustomAttributeValues"></param>
        /// <param name="partyRelationships"></param>
        /// <param name="callBackUrl">if there is, after creating the party, it will redirect to this</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(Party party, Dictionary<string, string> partyCustomAttributeValues, List<PartyRelationship> partyRelationships, string callBackUrl)
        {
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            PartyManager partyManager = new PartyManager();
            var partyRelationshipManager = new PartyRelationshipTypeManager();
            var partyType = partyTypeManager.Repo.Get(party.PartyType.Id);
            var partyStatusType = partyTypeManager.GetStatusType(partyType, "Created");
            //Create party
            party = partyManager.Create(partyType, party.Alias, party.Description, party.StartDate, party.EndDate, partyStatusType);
            //Add customAttriuteValue to party
            var customAttributeValues = ConvertDictionaryToPartyCustomeAttrValuesDictionary(partyCustomAttributeValues);
            partyManager.AddPartyCustomAttriuteValues(party, customAttributeValues);
            if (partyRelationships != null)
                foreach (var partyRelationship in partyRelationships)
                {
                    var secondParty = partyManager.Repo.Get(partyRelationship.SecondParty.Id);
                    var partyRelationshipType = partyRelationshipManager.Repo.Get(partyRelationship.PartyRelationshipType.Id);
                    partyManager.AddPartyRelationship(party, secondParty, partyRelationshipType, partyRelationship.Title, partyRelationship.Description, partyRelationship.StartDate, partyRelationship.EndDate, partyRelationship.Scope);
                }
            
            if (!string.IsNullOrEmpty(callBackUrl))
                //TODO: Sven impementing the callback 
                return RedirectToAction(callBackUrl + "&PID=" + party.Id);
            else
                return RedirectToAction("Index");
        }

        public ActionResult CreateEdit(int id, bool relationTabAsDefault = false)
        {
            PartyManager partyManager = new PartyManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            ViewBag.Title = PresentationModel.GetGenericViewTitle("Edit Party");
            var model = new PartyModel();
            model.PartyTypeList = partyTypeManager.Repo.Get().ToList();
            model.Party = partyManager.Repo.Get(id);
            ViewBag.RelationTabAsDefault = relationTabAsDefault;
           // var partyRelations = partyManager.RepoPartyRelationships.Get(cc => cc.FirstParty.Id == model.Party.Id && cc.SecondParty.Id == model.Party.Id);
            // var requiredPartyRelationTypes = new PartyRelationshipTypeManager().GetPartyRelationshipTypeWithAllowedAssociated(id).Where(cc => cc.MinCardinality > 0);
           //foreach (var requiredPartyRelationType in requiredPartyRelationTypes)
            //{
            //    if (partyRelations.Where(cc => cc.PartyRelationshipType.Id == requiredPartyRelationType.Id).Count() < requiredPartyRelationType.MinCardinality)
            //        model.Errors.Add(new IO.Transform.Validation.Exceptions.Error(ErrorType.Other, "At lease on relationship type '" + requiredPartyRelationType.DisplayName + "' for this party is required."));

            //}
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
                if (partyModel.Party.Id != 0)
                {
                    party = partyManager.Repo.Reload(partyModel.Party);
                    //Update some fields
                    party.Description = partyModel.Party.Description;
                    party.StartDate = partyModel.Party.StartDate;
                    party.EndDate = partyModel.Party.EndDate;
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
                    var partyType = partyTypeManager.Repo.Get(partyModel.Party.PartyType.Id);
                    var partyStatusType = partyTypeManager.GetStatusType(partyType, "Created");
                    //Create party
                    party = partyManager.Create(partyType, partyModel.Party.Alias, partyModel.Party.Description, partyModel.Party.StartDate, partyModel.Party.EndDate, partyStatusType);
                    //Add customAttriuteValue to party
                    newAddPartyCustomAttrValues = ConvertDictionaryToPartyCustomeAttrValuesDictionary(partyCustomAttributeValues);
                    redirectAction = RedirectToAction("CreateEdit", new { id = party.Id, relationTabAsDefault = true });
                }
                partyManager.AddPartyCustomAttriuteValues(party, newAddPartyCustomAttrValues);
            }
            return redirectAction;
        }

        public ActionResult View(int id)
        {
            PartyManager partyManager = new PartyManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            ViewBag.Title = PresentationModel.GetGenericViewTitle("View Party");
            var model = new PartyModel();
            model.PartyTypeList = partyTypeManager.Repo.Get().ToList();
            model.Party = partyManager.Repo.Get(id);

            return View(model);
        }

        public ActionResult ViewPartyDetail(int id)
        {
            PartyManager partyManager = new PartyManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            ViewBag.Title = PresentationModel.GetGenericViewTitle("View Party");
            var model = new PartyModel();
            model.PartyTypeList = partyTypeManager.Repo.Get().ToList();
            model.Party = partyManager.Repo.Get(id);
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
            //  party = partyManager.Repo.Reload(party);
            partyManager.Delete(party);
            return RedirectToAction("Index");
        }

        public ActionResult UserRegisteration(string callbackUrl)
        {

            //Select all the parties which are defined in web.config
            //Defined AccountPartyTypes vallue in web config format is like PartyType1:PartyTypePairTitle1-PartyTypePairTitle2,PartyType2
            var accountPartyTypes = new List<string>();
            var partyTypeAccountModel = new List<PartyTypeAccountModel>();
            var pm = new PartyTypeManager();
            var pr = new PartyRelationshipTypeManager();
            //Split them by "," and split each one by ":"
            foreach (string partyTypeAndRelationsStr in ConfigurationManager.AppSettings["AccountPartyTypes"].Split(','))
            {
                var partyTypeAndRelations = partyTypeAndRelationsStr.Split(':');
                var partyType = pm.Repo.Get(item => item.Title == partyTypeAndRelations[0]).FirstOrDefault();
                if (partyType == null)
                    throw new Exception("accountPartyType format in app setting is not correct or this 'partyType' doesn't exist.");
                var allowedPartyTypePairs = new Dictionary<string, PartyTypePair>();
                if (partyTypeAndRelations.Length > 1)
                {
                    var partyRelationshipsTypeStr = partyTypeAndRelations[1].Split('-');
                    var partyRelationshipsType = pr.Repo.Get(item => partyRelationshipsTypeStr.Contains(item.Title));

                    foreach (var partyRelationshipType in partyRelationshipsType)
                    {
                        //filter AssociatedPairs to allowed pairs
                        partyRelationshipType.AssociatedPairs = partyRelationshipType.AssociatedPairs.Where(item => partyType.Id == item.AllowedSource.Id && item.AllowedTarget.Parties.Any()).ToList();

                        //try to find first type pair witch has PartyRelationShipTypeDefault otherwise the first one 
                        var defaultPartyTypePair = partyRelationshipType.AssociatedPairs.FirstOrDefault(item => item.PartyRelationShipTypeDefault);
                        if (defaultPartyTypePair == null)
                            defaultPartyTypePair = partyRelationshipType.AssociatedPairs.FirstOrDefault();
                        if (defaultPartyTypePair != null)
                            allowedPartyTypePairs.Add(partyRelationshipType.DisplayName, defaultPartyTypePair);
                    }
                }
                partyTypeAccountModel.Add(new PartyTypeAccountModel()
                {
                    PartyType = partyType,
                    PartyRelationshipTypes = allowedPartyTypePairs
                });

            }
            ViewBag.CallBackUrl = callbackUrl;
            return View("_userRegisterationPartial", partyTypeAccountModel);
            // return PartialView("_userRegisterationPartial", partyTypeAccountModel);
        }


        private void validateAttribute(PartyModel partyModel)
        {
            if (partyModel.Party.StartDate > partyModel.Party.EndDate)
                partyModel.Errors.Add(new IO.Transform.Validation.Exceptions.Error(IO.Transform.Validation.Exceptions.ErrorType.Other, "Start date is greater than end date!"));
            if (partyModel.Party.PartyType.Id == 0)
                partyModel.Errors.Add(new IO.Transform.Validation.Exceptions.Error(IO.Transform.Validation.Exceptions.ErrorType.Other, "Please select party type!"));
            //if (string.IsNullOrEmpty(partyModel.Party.Name))
            //    partyModel.Errors.Add(new IO.Transform.Validation.Exceptions.Error(IO.Transform.Validation.Exceptions.ErrorType.Other, "Please enter a name for party!"));
            //Start and end date validation
        }

        /// <summary>
        /// Conver a simple string,string dictionary to PartyCustomAttribute, string
        /// </summary>
        /// <param name="partyCustomAttributes"></param>
        /// <returns></returns>
        private Dictionary<PartyCustomAttribute, string> ConvertDictionaryToPartyCustomeAttrValuesDictionary(Dictionary<string, string> partyCustomAttributes)
        {
            var result = new Dictionary<PartyCustomAttribute, string>();
            var partyTypeManager = new PartyTypeManager();
            foreach (var partyCustomAttribute in partyCustomAttributes)
            {
                //result.Add(new PartyCustomAttribute() { Id = int.Parse(partyCustomAttribute.Key) }, partyCustomAttribute.Value);
                var customAttribiute = partyTypeManager.RepoPartyCustomAttribute.Get(int.Parse(partyCustomAttribute.Key));
                if (customAttribiute == null || customAttribiute.Id == 0)
                    throw new Exception("Error in custom attribute values.");
                result.Add(customAttribiute, partyCustomAttribute.Value);
            }
            return result;
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
            var partyModel = new PartyModel();
            partyModel.Party = partyManager.Repo.Get(id);
            return PartialView("_partyRelationshipsPartial", partyModel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id">party type id</param>
        /// <returns></returns>
        public ActionResult LoadPartyRelationshipType(int id)
        {
            var partyRelManager = new PartyRelationshipTypeManager();

            ViewBag.sourcePartyId = Request.Params["partyId"] != null ? long.Parse(Request.Params["partyId"]) : 0;
            return PartialView("_addPartyRelationshipPartial", partyRelManager.GetPartyRelationshipTypeWithAllowedAssociated(id));
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