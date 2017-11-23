using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Bam.UI.Models;
using BExIS.Security.Services.Subjects;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Bam.UI.Controllers
{
    public class PartyServiceController : Controller
    {
        // GET: PartyService
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        public ActionResult UserRegisteration()
        {
            PartyManager partyManager = null;
            PartyTypeManager partyTypeManager = null;
            PartyRelationshipTypeManager partyRelationshipTypeManager = null;
            UserManager userManager = null;
            try
            {
                if (!HttpContext.User.Identity.IsAuthenticated)
                    return RedirectToAction("Index", "Home");
                //Select all the parties which are defined in web.config
                //Defined AccountPartyTypes vallue in web config format is like PartyType1:PartyTypePairTitle1-PartyTypePairTitle2,PartyType2
                var accountPartyTypes = new List<string>();
                var partyTypeAccountModel = new PartyTypeAccountModel();
                partyManager = new PartyManager();
                partyTypeManager = new PartyTypeManager();
                partyRelationshipTypeManager = new PartyRelationshipTypeManager();
                userManager = new UserManager();
                var allowedAccountPartyTypes = GetPartyTypesForAccount();
                if (allowedAccountPartyTypes == null)
                    throw new Exception("Allowed party types for registeration in setting.xml are not exist!");
                //Split them by "," and split each one by ":"
                foreach (var allowedAccountPartyType in allowedAccountPartyTypes)
                {
                    var partyType = partyTypeManager.PartyTypeRepository.Get(item => item.Title == allowedAccountPartyType.Key).FirstOrDefault();
                    if (partyType == null)
                        throw new Exception("AccountPartyType format in app setting is not correct or this 'partyType' doesn't exist.");
                    var allowedPartyTypePairs = new Dictionary<string, PartyTypePair>();
                    if (allowedAccountPartyType.Value != null)
                    {
                        var partyRelationshipsType = partyRelationshipTypeManager.PartyRelationshipTypeRepository.Get(item => allowedAccountPartyType.Value.Contains(item.Title));
                        foreach (var partyRelationshipType in partyRelationshipsType)
                        {
                            //filter AssociatedPairs to allowed pairs
                            partyRelationshipType.AssociatedPairs = partyRelationshipType.AssociatedPairs.Where(item => partyType.Id == item.AllowedSource.Id && item.AllowedTarget.Parties.Any()).ToList();
                            //try to find first type pair which has PartyRelationShipTypeDefault otherwise the first one 
                            var defaultPartyTypePair = partyRelationshipType.AssociatedPairs.FirstOrDefault(item => item.PartyRelationShipTypeDefault);
                            if (defaultPartyTypePair == null)
                                defaultPartyTypePair = partyRelationshipType.AssociatedPairs.FirstOrDefault();
                            if (defaultPartyTypePair != null)
                                allowedPartyTypePairs.Add(partyRelationshipType.DisplayName, defaultPartyTypePair);
                        }
                    }
                    partyTypeAccountModel.PartyRelationshipsTypes.Add(partyType, allowedPartyTypePairs);
                }
                //Bind party if there is already a user associated to this party
                var userTask = userManager.FindByNameAsync(HttpContext.User.Identity.Name);
                userTask.Wait();
                var user = userTask.Result;
                partyTypeAccountModel.Party = partyManager.GetPartyByUser(user.Id);
                //TODO: Discuss . Current soloution is to navigate the user to edit party
                if (partyTypeAccountModel.Party != null)
                    return RedirectToAction("CreateEdit", "Party", new { id = partyTypeAccountModel.Party.Id });
                return View("_userRegisterationPartial", partyTypeAccountModel);

            }
            finally
            {
                partyManager?.Dispose();
                partyTypeManager?.Dispose();
                partyRelationshipTypeManager?.Dispose();
                userManager?.Dispose();
            }
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
            PartyTypeManager partyTypeManager = null;
            PartyManager partyManager = null;
            PartyRelationshipTypeManager partyRelationshipManager = null;
            UserManager userManager = null;
            try
            {
                userManager = new UserManager();
                partyTypeManager = new PartyTypeManager();
                partyManager = new PartyManager();
                partyRelationshipManager = new PartyRelationshipTypeManager();
                var partyType = partyTypeManager.PartyTypeRepository.Get(party.PartyType.Id);
                var partyStatusType = partyTypeManager.GetStatusType(partyType, "Created");
                //Create party
                party = partyManager.Create(partyType, party.Description, null, null, partyCustomAttributeValues.ToDictionary(cc => long.Parse(cc.Key), cc => cc.Value));
                if (partyRelationships != null)
                    foreach (var partyRelationship in partyRelationships)
                    {
                        //the duration is from current datetime up to the end of target party date
                        var secondParty = partyManager.PartyRepository.Get(partyRelationship.SecondParty.Id);
                        var partyRelationshipType = partyRelationshipManager.PartyRelationshipTypeRepository.Get(partyRelationship.PartyRelationshipType.Id);
                        partyManager.AddPartyRelationship(party, secondParty, partyRelationshipType, partyRelationship.Title, partyRelationship.Description, DateTime.Now, secondParty.EndDate, partyRelationship.Scope);
                    }
                var userTask = userManager.FindByNameAsync(HttpContext.User.Identity.Name);
                userTask.Wait();
                var user = userTask.Result;
                partyManager.AddPartyUser(party, user.Id);
                return RedirectToAction("Index");
            }
            finally
            {
                partyTypeManager?.Dispose();
                partyManager?.Dispose();
                partyRelationshipManager?.Dispose();
            }
        }

        public ActionResult Edit(bool relationTabAsDefault = false)
        {
            PartyManager partyManager = null;
            PartyTypeManager partyTypeManager = null;
            UserManager userManager = null;
            try
            {
                partyManager = new PartyManager();
                partyTypeManager = new PartyTypeManager();
                userManager = new UserManager();

                var user = userManager.FindByNameAsync(HttpContext.User?.Identity?.Name).Result;

                if (user == null)
                    return RedirectToAction("Index", "Home", new { area = "" });

                ViewBag.Title = PresentationModel.GetGenericViewTitle("Edit Party");
                var model = new PartyModel();
                model.PartyTypeList = partyTypeManager.PartyTypeRepository.Get().ToList();
                Party party = partyManager.GetPartyByUser(user.Id);

                if (party == null)
                    return RedirectToAction("UserRegisteration", "PartyService", new { area = "bam" });


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

                ViewBag.RelationTabAsDefault = false;
                ViewBag.Title = "Edit party";

                return View(model);
            }
            finally
            {
                partyManager?.Dispose();
                partyTypeManager?.Dispose();
                userManager?.Dispose();
            }
        }

        [HttpPost]
        public ActionResult Edit(PartyModel partyModel, Dictionary<string, string> partyCustomAttributeValues)
        {
            var party = new Party();
            if (partyModel.Id == 0)
                return RedirectToAction("Index", "Home");
            else
                party = Helpers.Helper.EditParty(partyModel, partyCustomAttributeValues);
            return RedirectToAction("Index", "Home",new { area=""});
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Id">PartyType Id</param>
        /// <returns></returns>
        public ActionResult LoadPartyCustomAttr(int id)
        {
            PartyManager partyManager = null;
            try
            {
                long partyId = 0;
                var partyIdStr = HttpContext.Request.Params["partyId"];
                if (long.TryParse(partyIdStr, out partyId) && partyId != 0)
                {
                    partyManager = new PartyManager();
                    ViewBag.customAttrValues = partyManager.PartyRepository.Get(partyId).CustomAttributeValues.ToList();
                }
                var customAttrList = new List<PartyCustomAttribute>();
                PartyTypeManager partyTypeManager = new PartyTypeManager();
                IEnumerable<PartyType> partyType = partyTypeManager.PartyTypeRepository.Get(item => item.Id == id);
                if (partyType != null)
                    customAttrList = partyType.First().CustomAttributes.ToList();
                return PartialView("_customAttributesPartial", customAttrList);
            }
            finally
            {
                partyManager?.Dispose();
            }
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