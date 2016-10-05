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

        [HttpPost]
        public ActionResult Create(PartyModel partyModel, Dictionary<string, string> partyCustomAttributeValues)
        {
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
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            PartyManager partyManager = new PartyManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            ViewBag.Title = PresentationModel.GetViewTitle("Edit Party");
            var model = new PartyModel();
            model.PartyTypeList = partyTypeManager.Repo.Get().ToList();
            model.Party = partyManager.Repo.Get(id);

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

    }
}