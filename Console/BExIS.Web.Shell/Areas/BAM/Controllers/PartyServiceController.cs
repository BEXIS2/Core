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

        public ActionResult UserRegistration()
        {
            PartyManager partyManager = null;
            PartyTypeManager partyTypeManager = null;
            PartyRelationshipTypeManager partyRelationshipTypeManager = null;
            UserManager userManager = null;
            try
            {
                if (!HttpContext.User.Identity.IsAuthenticated)
                    return RedirectToAction("Index", "Home");
                //Defined AccountPartyTypes vallue in web config format is like PartyType1:PartyTypePairTitle1-PartyTypePairTitle2,PartyType2
                var accountPartyTypes = new List<string>();
                var partyTypeAccountModel = new PartyTypeAccountModel();
                partyManager = new PartyManager();
                partyTypeManager = new PartyTypeManager();
                partyRelationshipTypeManager = new PartyRelationshipTypeManager();
                userManager = new UserManager();
                var allowedAccountPartyTypes = GetPartyTypesForAccount();
                if (allowedAccountPartyTypes == null)
                    throw new Exception("Allowed party types for registration in setting.xml are not exist!");
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
                            partyRelationshipType.AssociatedPairs = partyRelationshipType.AssociatedPairs.Where(item => partyType.Id == item.SourcePartyType.Id && item.TargetPartyType.Parties.Any()).ToList();
                            //try to find first type pair which has PartyRelationShipTypeDefault otherwise the first one
                            var defaultPartyTypePair = partyRelationshipType.AssociatedPairs.FirstOrDefault(item => item.PartyRelationShipTypeDefault);
                            
                            if (defaultPartyTypePair == null)
                                defaultPartyTypePair = partyRelationshipType.AssociatedPairs.FirstOrDefault();
                            if (defaultPartyTypePair != null)
                            {
                                if (defaultPartyTypePair.TargetPartyType.Parties != null)
                                {
                                    defaultPartyTypePair.TargetPartyType.Parties = defaultPartyTypePair.TargetPartyType.Parties.OrderBy(item => item.Name).ToList(); // order parties by name
                                }
                                allowedPartyTypePairs.Add(partyRelationshipType.DisplayName, defaultPartyTypePair);
                            }
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
                    return RedirectToAction("Edit");
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

                // check if 
                var userTask = userManager.FindByNameAsync(HttpContext.User.Identity.Name);
                userTask.Wait();
                var user = userTask.Result;

                //check if the party blongs to the user
                //Bind party if there is already a user associated to this party
                var partyuser = partyManager.GetPartyByUser(user.Id);
                if (partyuser == null)
                {
                    partyRelationshipManager = new PartyRelationshipTypeManager();
                    var partyType = partyTypeManager.PartyTypeRepository.Get(party.PartyType.Id);
                    var partyStatusType = partyTypeManager.GetStatusType(partyType, "Created");
                    //Create party
                    party = partyManager.Create(partyType, party.Description, null, null, partyCustomAttributeValues.ToDictionary(cc => long.Parse(cc.Key), cc => cc.Value));
                    if (partyRelationships != null)
                        foreach (var partyRelationship in partyRelationships)
                        {
                            //the duration is from current datetime up to the end of target party date
                            var TargetParty = partyManager.PartyRepository.Get(partyRelationship.TargetParty.Id);
                            // var partyRelationshipType = partyRelationshipManager.PartyRelationshipTypeRepository.Get(partyRelationship.PartyRelationshipType.Id);
                            var partyTypePair = partyRelationshipManager.PartyTypePairRepository.Get(partyRelationship.PartyTypePair.Id);
                            partyManager.AddPartyRelationship(party, TargetParty, partyRelationship.Title, partyRelationship.Description, partyTypePair, DateTime.Now, TargetParty.EndDate, partyRelationship.Scope);
                        }

                    partyManager.AddPartyUser(party, user.Id);

                    //set FullName in user
                    var p = partyManager.GetParty(party.Id);
                    string displayName = String.Join(" ",
                        p.CustomAttributeValues.
                        Where(ca => ca.CustomAttribute.IsMain.Equals(true)).
                        OrderBy(ca => ca.CustomAttribute.Id).
                        Select(ca => ca.Value).ToArray());

                    user.DisplayName = displayName;
                    userManager.UpdateAsync(user);
                }

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
                    return RedirectToAction("UserRegistration", "PartyService", new { area = "bam" });
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
                model.Name = party.Name;
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
            PartyManager partyManager = null;
            PartyTypeManager partyTypeManager = null;
            UserManager userManager = null;
            try
            {
                partyManager = new PartyManager();
                partyTypeManager = new PartyTypeManager();
                userManager = new UserManager();
                if (!HttpContext.User.Identity.IsAuthenticated)
                    return RedirectToAction("Index", "Home");

                var userTask = userManager.FindByNameAsync(HttpContext.User.Identity.Name);
                userTask.Wait();
                var user = userTask.Result;
                var userParty = partyManager.GetPartyByUser(user.Id);
                if (userParty.Id != partyModel.Id)
                    throw new Exception("Permission denied.");
                if (partyModel.Id == 0)
                    return RedirectToAction("Index", "Home");
                else
                {
                    party = Helpers.Helper.EditParty(partyModel, partyCustomAttributeValues, null);

                    var p = partyManager.GetParty(party.Id);
                    string displayName = String.Join(" ",
                        p.CustomAttributeValues.
                        Where(ca => ca.CustomAttribute.IsMain.Equals(true)).
                        OrderBy(ca => ca.CustomAttribute.Id).
                        Select(ca=>ca.Value).ToArray());

                    user.DisplayName = displayName;

                    if (ConfigurationManager.AppSettings["usePersonEmailAttributeName"] == "true")
                    {
                        var nameProp = partyTypeManager.PartyCustomAttributeRepository.Get(attr => (attr.PartyType == party.PartyType) && (attr.Name == ConfigurationManager.AppSettings["PersonEmailAttributeName"])).FirstOrDefault();
                        if (nameProp != null)
                        {               
                            var entity = party.CustomAttributeValues.FirstOrDefault(item => item.CustomAttribute.Id == nameProp.Id);
                            user.Email = entity.Value;
                        }
                    }
                    

                    userManager.UpdateAsync(user);

                }
                return RedirectToAction("Index", "Home", new { area = "" });
            }
            finally
            {
                partyManager?.Dispose();
                userManager?.Dispose();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="Id">PartyType Id</param>
        /// <returns></returns>
        public ActionResult LoadPartyCustomAttr(int id)
        {
            PartyManager partyManager = null;
            UserManager userManager = null;
            try
            {
                userManager = new UserManager();
                long partyId = 0;
                var partyIdStr = HttpContext.Request.Params["partyId"];

                ViewBag.userRegistration = HttpContext.Request.Params["userReg"];

                if (long.TryParse(partyIdStr, out partyId) && partyId != 0)
                {
                    partyManager = new PartyManager();
                    ViewBag.customAttrValues = partyManager.PartyRepository.Get(partyId).CustomAttributeValues.ToList();

                    var userId = partyManager.GetUserIdByParty(partyId);
                    var userTask = userManager.FindByIdAsync(userId);
                    userTask.Wait();
                    var user = userTask.Result;
                    if (user != null)
                    {
                        ViewBag.email = user.Email;
                    }

                }
                // if no user is linked assume it is the user registration
                else
                {

                    var userName = HttpContext.User.Identity.Name;
                    var userTask = userManager.FindByNameAsync(userName);
                    userTask.Wait();
                    var user = userTask.Result;

                    ViewBag.email = user.Email;
                }

                // Add attribute name for email
                if (ConfigurationManager.AppSettings["usePersonEmailAttributeName"] == "true")
                {
                    ViewBag.PersonEmailAttributeName = ConfigurationManager.AppSettings["PersonEmailAttributeName"];
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

        [HttpGet]
        public Boolean CheckUniqeness(int partyTypeId, int partyId, string hash)
        {
            PartyManager partyManager = null;
            try
            {
                partyManager = new PartyManager();
                PartyType partyType = new PartyTypeManager().PartyTypeRepository.Get(partyTypeId);
                Party party = partyManager.PartyRepository.Get(partyId);
                return partyManager.CheckUniqueness(partyManager.PartyRepository, partyType, hash, party);
            }
            finally { partyManager?.Dispose(); }
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

        public JsonResult ValidateRelationships(int partyId)
        {
            return Json(Helpers.Helper.ValidateRelationships(partyId));
        }
    }
}