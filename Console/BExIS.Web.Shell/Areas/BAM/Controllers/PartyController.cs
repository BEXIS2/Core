using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Web.Shell.Areas.BAM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Persistence.Api;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Web.Shell.Areas.BAM.Controllers
{
    public class PartyController : Controller
    {


        public ActionResult Index()
        {
            PartyManager partyManager = new PartyManager();
            var parties = partyManager.Repo.Get();
            return View(parties.ToList());
        }

        public ActionResult Create()
        {
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            ViewBag.Title = PresentationModel.GetViewTitle("Create Party");
            var model = new PartyModel();
            model.PartyTypeList = partyTypeManager.Repo.Get().ToList();
            return View(model);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="partyModel"></param>
        /// <param name="partyCustomAttributeValues">should be filled like partyCustomAttributeValues[Id]=value in view </param>
        /// <param name="partyRelationshipsDic">should be filled like PartyRelationship[FiledName_@secondPartyId]=Value in view </param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(string submit,PartyModel partyModel, Dictionary<string, string> partyCustomAttributeValues, Dictionary<string, string> partyRelationshipsDic)
        {
            //
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            PartyManager partyManager = new PartyManager();
            validateAttribute(partyModel);
            if (partyModel.Errors.Count > 0)
                return View(partyModel);
            //
            var partyType = partyTypeManager.Repo.Get(partyModel.Party.PartyType.Id);
            var partyStatusType = partyTypeManager.GetStatusType(partyType, "Create");

            //Create party
            var party = partyManager.Create(partyType, partyModel.Party.Name, "", "", partyModel.Party.StartDate, partyModel.Party.EndDate, partyStatusType);
            //Add customAttriuteValue to party
            partyManager.AddPartyCustomAttriuteValue(party, ConvertDictionaryToPartyCustomeAttrValuesDictionary(partyCustomAttributeValues));

            List<PartyRelationship> partyRelationships = ConvertDictionaryToPartyRelationships(partyRelationshipsDic);
            var partyRelationshipManager = new PartyRelationshipTypeManager();
            foreach (var partyRelationship in partyRelationships)
            {
                var secondParty = partyManager.Repo.Get(partyRelationship.SecondParty.Id);
                var partyRelationshipType = partyRelationshipManager.Repo.Get(partyRelationship.PartyRelationshipType.Id);
                partyManager.AddPartyRelationship(party, secondParty, partyRelationshipType, partyRelationship.Title, partyRelationship.Description, partyRelationship.StartDate, partyRelationship.EndDate, partyRelationship.Scope);
            }
            if (submit == "AddPartyRelationships")
                 return RedirectToAction("Edit", new {id=party.Id,addRelationship=true });
            else
               return RedirectToAction("Index");
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


        public ActionResult Edit(int id,bool relationTabAsDefault=false)
        {
            PartyManager partyManager = new PartyManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            ViewBag.Title = PresentationModel.GetViewTitle("Edit Party");
            var model = new PartyModel();
            model.PartyTypeList = partyTypeManager.Repo.Get().ToList();
            model.Party = partyManager.Repo.Get(id);
            ViewBag.RelationTabAsDefault = relationTabAsDefault;
            return View(model);
        }

        [HttpPost]
        public ActionResult Edit(PartyModel partyModel, Dictionary<string, string> partyCustomAttributeValues)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                PartyTypeManager partyTypeManager = new PartyTypeManager();
                PartyManager partyManager = new PartyManager();
                validateAttribute(partyModel);
                if (partyModel.Errors.Count > 0)
                    return View(partyModel);
                var party = partyManager.Repo.Reload(partyModel.Party);
                //Update some fields
                party.Description = partyModel.Party.Description;
                party.StartDate = partyModel.Party.StartDate;
                party.EndDate = partyModel.Party.EndDate;
                party.Name = partyModel.Party.Name;
                party = partyManager.Update(party);
                foreach (var partyCustomAttributeValueString in partyCustomAttributeValues)
                {
                    var partyCustomAttribute = partyTypeManager.RepoPartyCustomAttribute.Get(int.Parse(partyCustomAttributeValueString.Key));
                    partyManager.UpdatePartyCustomAttriuteValue(partyCustomAttribute, partyModel.Party, partyCustomAttributeValueString.Value);
                }
            }
            return RedirectToAction("Index");
        }

        public ActionResult View(int id)
        {
            PartyManager partyManager = new PartyManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            ViewBag.Title = PresentationModel.GetViewTitle("View Party");
            var model = new PartyModel();
            model.PartyTypeList = partyTypeManager.Repo.Get().ToList();
            model.Party = partyManager.Repo.Get(id);

            return View(model);
        }

        public ActionResult ViewPartyDetail(int id)
        {
            PartyManager partyManager = new PartyManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            ViewBag.Title = PresentationModel.GetViewTitle("View Party");
            var model = new PartyModel();
            model.PartyTypeList = partyTypeManager.Repo.Get().ToList();
            model.Party = partyManager.Repo.Get(id);
            model.ViewMode = true;
            return PartialView("View", model);
        }

        public ActionResult DeleteConfirm(int id)
        {
            ViewBag.Title = PresentationModel.GetViewTitle("Delete Party");
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

        private void validateAttribute(PartyModel partyModel)
        {
            if (partyModel.Party.PartyType.Id == 0)
                partyModel.Errors.Add(new IO.Transform.Validation.Exceptions.Error(IO.Transform.Validation.Exceptions.ErrorType.Other, "Please select party type!"));
            if (string.IsNullOrEmpty(partyModel.Party.Name))
                partyModel.Errors.Add(new IO.Transform.Validation.Exceptions.Error(IO.Transform.Validation.Exceptions.ErrorType.Other, "Please enter a name for party!"));
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
            foreach (var partyCustomAttribute in partyCustomAttributes)
            {
                result.Add(new PartyCustomAttribute() { Id = int.Parse(partyCustomAttribute.Key) }, partyCustomAttribute.Value);
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
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            PartyManager partyManager = new PartyManager();


            var party = partyManager.Repo.Get(partyId);

            List<PartyRelationship> partyRelationships = ConvertDictionaryToPartyRelationships(partyRelationshipsDic);
            var partyRelationshipManager = new PartyRelationshipTypeManager();
            foreach (var partyRelationship in partyRelationships)
            {
                var secondParty = partyManager.Repo.Get(partyRelationship.SecondParty.Id);
                var partyRelationshipType = partyRelationshipManager.Repo.Get(partyRelationship.PartyRelationshipType.Id);
                partyManager.AddPartyRelationship(party, secondParty, partyRelationshipType, partyRelationship.Title, partyRelationship.Description, partyRelationship.StartDate, partyRelationship.EndDate, partyRelationship.Scope);
            }
            return RedirectToAction("Edit", "party", new { id = partyId , relationTabAsDefault = true});
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
            if (long.TryParse(partyIdStr, out partyId))
            {
                PartyManager pm = new PartyManager();
                ViewBag.customAttrValues = pm.Repo.Get(partyId).CustomAttributeValues.ToList();
            }
            var customAttrList = new List<PartyCustomAttribute>();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            var partyType = partyTypeManager.Repo.Get(item => item.Id == id);
            if (partyType != null)
                customAttrList = partyType.First().CustomAttributes.ToList();
            return PartialView("_customAttributesView", customAttrList);
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

            return PartialView("_partyRelationshipTypesView", partyModel);
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
            var partyRelationTypes = partyRelManager.Repo.Get();
            foreach (var partyRelationType in partyRelationTypes)
                partyRelationType.AssociatedPairs = partyRelationType.AssociatedPairs.Where(item => item.AlowedSource.Id == id).ToList();
            return PartialView("_partyRelationshipTypeView", partyRelationTypes);
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
            return PartialView("_partyRelationshipEditView", partyRelation);
        }

        [HttpPost]
        public bool EditPartyRelationship(PartyRelationship partyRelationship)
        {
            var partyManager = new PartyManager();
            return partyManager.UpdatePartyRelationship(partyRelationship.Id, partyRelationship.Title, partyRelationship.Description, partyRelationship.StartDate, partyRelationship.EndDate, partyRelationship.Scope);
        }
     
    }
}