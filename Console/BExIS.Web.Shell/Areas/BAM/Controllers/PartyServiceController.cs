using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Bam.UI.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Modules.Bam.UI.Controllers
{
    public class PartyServiceController : Controller
    {
        // GET: PartyService
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult UserRegisteration(int id=0)
        {

            //Select all the parties which are defined in web.config
            //Defined AccountPartyTypes vallue in web config format is like PartyType1:PartyTypePairTitle1-PartyTypePairTitle2,PartyType2
            var accountPartyTypes = new List<string>();
            var partyTypeAccountModel = new PartyTypeAccountModel();
            var pm = new PartyManager();
            var ptm = new PartyTypeManager();
            var prm = new PartyRelationshipTypeManager();
            var allowedAccountPartyTypes = GetPartyTypesForAccount();
            if (allowedAccountPartyTypes == null)
                throw new Exception("Allowed party types for registeration in setting.xml are not exist!");
            //Split them by "," and split each one by ":"
            foreach (var allowedAccountPartyType in allowedAccountPartyTypes)
            {
                var partyType = ptm.Repo.Get(item => item.Title == allowedAccountPartyType.Key).FirstOrDefault();
                if (partyType == null)
                    throw new Exception("AccountPartyType format in app setting is not correct or this 'partyType' doesn't exist.");
                var allowedPartyTypePairs = new Dictionary<string, PartyTypePair>();
                if (allowedAccountPartyType.Value != null)
                {

                    var partyRelationshipsType = prm.Repo.Get(item => allowedAccountPartyType.Value.Contains(item.Title));

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
                partyTypeAccountModel.PartyRelationshipsTypes.Add(partyType,allowedPartyTypePairs);
            }
            //Bind party if there is id
            if(id!=0)
                partyTypeAccountModel.Party=pm.Repo.Get(id);
            
          //  ViewBag.CallBackUrl = callbackUrl;
            return View("_userRegisterationPartial", partyTypeAccountModel);
            // return PartialView("_userRegisterationPartial", partyTypeAccountModel);
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
        public ActionResult CreateUserParty(Party party, Dictionary<string, string> partyCustomAttributeValues, List<PartyRelationship> partyRelationships)
        {
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            PartyManager partyManager = new PartyManager();
            var partyRelationshipManager = new PartyRelationshipTypeManager();
            var partyType = partyTypeManager.Repo.Get(party.PartyType.Id);
            var partyStatusType = partyTypeManager.GetStatusType(partyType, "Created");
            //Create party
            party = partyManager.Create(partyType, party.Description, party.StartDate, party.EndDate, partyCustomAttributeValues);

            if (partyRelationships != null)
                foreach (var partyRelationship in partyRelationships)
                {
                    var secondParty = partyManager.Repo.Get(partyRelationship.SecondParty.Id);
                    var partyRelationshipType = partyRelationshipManager.Repo.Get(partyRelationship.PartyRelationshipType.Id);
                    partyManager.AddPartyRelationship(party, secondParty, partyRelationshipType, partyRelationship.Title, partyRelationship.Description, partyRelationship.StartDate, partyRelationship.EndDate, partyRelationship.Scope);
                }
            //TODO: Call Sven method to link the party with account
            return RedirectToAction("Index");
        }

        public Dictionary<string, string[]> GetPartyTypesForAccount()
        {
            var result = new Dictionary<string, string[]>();
            var accountPartyTypesStr = Helpers.Settings.get("AccountPartyTypes");
            if (accountPartyTypesStr == null || string.IsNullOrEmpty(accountPartyTypesStr.ToString()))
            {
                return null;
            }
            else
            {
                foreach (string partyTypeAndRelationsStr in accountPartyTypesStr.ToString().Split(','))
                {
                    var partyTypeAndRelations = partyTypeAndRelationsStr.Split(':');
                    result.Add(partyTypeAndRelations[0], partyTypeAndRelations.Length > 1 ? partyTypeAndRelations[1].Split('-') : null);
                }
            }
            return result;
        }
    }
}