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
                    throw new Exception("AccountPartyType format in app setting is not correct or this 'partyType' doesn't exist.");
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
    }
}