using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Web.Shell.Areas.BAM.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Vaiona.Web.Mvc.Models;

namespace BExIS.Web.Shell.Areas.BAM.Controllers
{
    public class PartyController : Controller
    {
        
        // GET: DDM/Party
        public ActionResult Index()
        {
            Dlm.Services.Party.PartyManager partyManager = new Dlm.Services.Party.PartyManager();
            var parties = partyManager.Repo.Get();
            return View(parties);
        }
        public ActionResult ShowParty(long id)
        {
            Dlm.Services.Party.PartyManager partyManager = new Dlm.Services.Party.PartyManager();
            var party = partyManager.Repo.Get(id);
            return View(party);
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
        public ActionResult Create(PartyModel partyModel,Dictionary<string,string> partyCustomAttributes)
        {
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            PartyManager partyManager = new PartyManager();
            validateAttribute(partyModel);
            if (partyModel.Errors.Count > 0)
                return View(partyModel);
            var partyType = partyTypeManager.Repo.Get(partyModel.Party.PartyType.Id);
            var partyStatusType = partyTypeManager.GetStatusType(partyType, "Create");
            if(partyStatusType==null)
                partyStatusType = partyTypeManager.AddStatusType( partyType, "Create","", 0);
            var party=partyManager.Create(partyType, partyModel.Party.Name, "", "", partyModel.Party.StartDate, partyModel.Party.EndDate, partyStatusType);
            partyManager.AddPartyCustomAttriuteValue(party, CreatePartyCustomeAttrValuesDictionary(partyCustomAttributes));
            
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
        public ActionResult Edit(PartyModel partyModel, Dictionary<string, string> partyCustomAttributes)
        {
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            PartyManager partyManager = new PartyManager();
            validateAttribute(partyModel);
            if (partyModel.Errors.Count > 0)
                return View(partyModel);
            var partyType = partyTypeManager.Repo.Get(partyModel.Party.PartyType.Id);
            var partyStatusType = partyTypeManager.GetStatusType(partyType, "Edit");
            if (partyStatusType == null)
                partyStatusType = partyTypeManager.AddStatusType(partyType, "Edit", "", 0);
            var party = partyManager.Update(partyModel.Party);
            //Delete all "PartyCustomAttributeValues" for this party then add new values
            //partyManager.RemovePartyCustomAttriuteValue(party.CustomAttributeValues);
            partyManager.AddPartyCustomAttriuteValue(party, CreatePartyCustomeAttrValuesDictionary(partyCustomAttributes));

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
            if (partyModel.Party.PartyType.Id == 0 )
                partyModel.Errors.Add(new IO.Transform.Validation.Exceptions.Error(IO.Transform.Validation.Exceptions.ErrorType.Other, "Please select party type!"));
            if(string.IsNullOrEmpty( partyModel.Party.Name))
                partyModel.Errors.Add(new IO.Transform.Validation.Exceptions.Error(IO.Transform.Validation.Exceptions.ErrorType.Other, "Please enter a name for party!"));
            //Start and end date validation
        }

        private Dictionary<PartyCustomAttribute, string> CreatePartyCustomeAttrValuesDictionary(Dictionary<string, string> partyCustomAttributes)
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
                customAttrList= partyType.First().CustomAttributes.ToList();
            return PartialView("_customAttributesView", customAttrList);
        }

    }
}