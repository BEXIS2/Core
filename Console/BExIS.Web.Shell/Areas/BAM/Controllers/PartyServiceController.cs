using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Modules.Bam.UI.Helpers;
using BExIS.Modules.Bam.UI.Models;
using BExIS.Security.Services.Subjects;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Modules.Bam.UI.Controllers
{
    public class PartyServiceController : Controller
    {
        [HttpGet]
        public Boolean CheckUniqeness(int partyTypeId, int partyId, string hash)
        {
            using (PartyManager partyManager = new PartyManager())
            using (PartyTypeManager partyTypeManager = new PartyTypeManager())
            {
                PartyType partyType = partyTypeManager.PartyTypeRepository.Get(partyTypeId);
                Party party = partyManager.PartyRepository.Get(partyId);
                return partyManager.CheckUniqueness(partyManager.PartyRepository, partyType, hash, party);
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
            using (PartyTypeManager partyTypeManager = new PartyTypeManager())
            using (PartyManager partyManager = new PartyManager())
            using (PartyRelationshipTypeManager partyRelationshipManager = new PartyRelationshipTypeManager())
            using (UserManager userManager = new UserManager())
            using (PartyRelationshipTypeManager partyRelationshipTypeManager = new PartyRelationshipTypeManager())
            {
                // check if
                var userTask = userManager.FindByNameAsync(HttpContext.User.Identity.Name);
                userTask.Wait();
                var user = userTask.Result;

                //check if the party blongs to the user
                //Bind party if there is already a user associated to this party
                var partyuser = partyManager.GetPartyByUser(user.Id);
                if (partyuser == null)
                {
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
                model.PartyRelationships = getPartyRelationships(party.Id);
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
            using (PartyManager partyManager = new PartyManager())
            using (PartyTypeManager partyTypeManager = new PartyTypeManager())
            using (UserManager userManager = new UserManager())
            {
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
                        Select(ca => ca.Value).ToArray());

                    user.DisplayName = displayName;

                    if (GeneralSettings.UsePersonEmailAttributeName)
                    {
                        var nameProp = partyTypeManager.PartyCustomAttributeRepository.Get(attr => (attr.PartyType == party.PartyType) && (attr.Name == GeneralSettings.PersonEmailAttributeName)).FirstOrDefault();
                        if (nameProp != null)
                        {
                            var entity = party.CustomAttributeValues.FirstOrDefault(item => item.CustomAttribute.Id == nameProp.Id);
                            if (user.Email != entity.Value)
                            {
                                using (var emailService = new EmailService())
                                {
                                    emailService.Send(MessageHelper.GetUpdateEmailHeader(),
                                        MessageHelper.GetUpdaterEmailMessage(user.DisplayName, user.Email, entity.Value),
                                        GeneralSettings.SystemEmail
                                        );
                                }
                                    
                            }
                            user.Email = entity.Value;
                        }
                    }

                    userManager.UpdateAsync(user);
                }
                return RedirectToAction("Index", "Home", new { area = "" });
            }
        }

        // GET: PartyService
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home", new { area = "" });
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="Id">PartyType Id</param>
        /// <returns></returns>
        public ActionResult LoadPartyCustomAttr(int id)
        {
            using (PartyManager partyManager = new PartyManager())
            using (UserManager userManager = new UserManager())
            using (PartyTypeManager partyTypeManager = new PartyTypeManager())
            {
                long partyId = 0;
                var partyIdStr = HttpContext.Request.Params["partyId"];

                ViewBag.userRegistration = HttpContext.Request.Params["userReg"];

                if (long.TryParse(partyIdStr, out partyId) && partyId != 0)
                {
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
                if (GeneralSettings.UsePersonEmailAttributeName)
                {
                    ViewBag.PersonEmailAttributeName = GeneralSettings.PersonEmailAttributeName;
                }

                var customAttrList = new List<PartyCustomAttribute>();

                IEnumerable<PartyType> partyType = partyTypeManager.PartyTypeRepository.Get(item => item.Id == id);
                if (partyType != null)
                    customAttrList = partyType.First().CustomAttributes.ToList();
                return PartialView("_customAttributesPartial", customAttrList);
            }
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
                var allowedAccountPartyTypes = getPartyTypesForAccount();
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

        public JsonResult ValidateRelationships(int partyId)
        {
            return Json(Helpers.Helper.ValidateRelationships(partyId));
        }

        // copied from PartyController.cs
        /// <summary>
        /// get all party releationships without system releationships like to entites like dataset
        /// </summary>
        /// <param name="partyId"></param>
        /// <returns></returns>
        private List<PartyRelationshipModel> getPartyRelationships(long partyId)
        {
            using (var partyManager = new PartyManager())
            {
                var temp = new List<PartyRelationshipModel>();

                var rList = partyManager.PartyRelationshipRepository.Get
                   (item => (item.SourceParty.Id == partyId || item.TargetParty.Id == partyId)
                   && (item.TargetParty.PartyType.SystemType == false && item.SourceParty.PartyType.SystemType == false)).ToList();

                partyManager.PartyRelationshipRepository.LoadIfNot(rList.Select(r => r.TargetParty));
                partyManager.PartyRelationshipRepository.LoadIfNot(rList.Select(r => r.TargetParty.PartyType));

                foreach (var r in rList)
                {
                    temp.Add(new PartyRelationshipModel()
                    {
                        Id = r.Id,
                        Title = r.Title,
                        Description = r.Description,
                        SourceName = r.SourceParty.Name,
                        TargetName = r.TargetParty.Name,
                        StartDate = r.StartDate,
                        EndDate = r.EndDate
                    });
                }

                return temp;
            }
        }

        private Dictionary<string, string[]> getPartyTypesForAccount()
        {
            try
            {
                var settingsHelper = new SettingsHelper();
                return settingsHelper.GetAccountPartyRelationshipTypes().Select(p => new KeyValuePair<string, string[]>(p.PartyType, p.PartyRelationshipTypes.ToArray())).ToDictionary(x => x.Key, x => x.Value);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}