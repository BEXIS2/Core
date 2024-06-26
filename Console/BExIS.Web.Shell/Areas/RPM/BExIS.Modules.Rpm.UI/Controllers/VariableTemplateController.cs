﻿using BExIS.App.Bootstrap.Attributes;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.Meanings;
using BExIS.Modules.Rpm.UI.Helpers;
using BExIS.Modules.Rpm.UI.Models;
using BExIS.UI.Helpers;
using BExIS.UI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace BExIS.Modules.Rpm.UI.Controllers
{
    public class VariableTemplateController : Controller
    {
        // GET: VariableTemplate
        public ActionResult Index()
        {
            string module = "Rpm";

            ViewData["app"] = SvelteHelper.GetApp(module);
            ViewData["start"] = SvelteHelper.GetStart(module);

            return View();
        }

        [JsonNetFilter]
        [HttpGet]
        public JsonResult GetVariableTemplates()
        {
            List<VariableTemplateModel> tmp = new List<VariableTemplateModel>();
            VariableHelper helper = new VariableHelper();
            using (var variableManager = new VariableManager())
            {
                var variableTemplates = variableManager.VariableTemplateRepo.Get();

                variableTemplates.ToList().ForEach(vt => tmp.Add(helper.ConvertTo(vt)));

                // set templates in use
                List<long> inUse = variableManager.VariableInstanceRepo.Query(v => v.VariableTemplate != null).Select(v => v.VariableTemplate.Id).ToList();
                tmp.ToList().ForEach(vt => vt.InUse = inUse.Contains(vt.Id));
            }

            return Json(tmp.ToArray(), JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult Update(VariableTemplateModel model)
        {
            if (model == null) throw new ArgumentNullException("model");

            VariableHelper helper = new VariableHelper();
            var variableTemplate = helper.ConvertTo(model);

            using (var variableManager = new VariableManager())
            using (var missingValueManager = new MissingValueManager())
            using (var meaningManager = new MeaningManager())
            {
                if (variableTemplate.Id == 0)
                {
                    variableTemplate = variableManager.CreateVariableTemplate(
                            variableTemplate.Label,
                            variableTemplate.DataType,
                            variableTemplate.Unit,
                            variableTemplate.Description,
                            variableTemplate.DefaultValue,
                            variableTemplate.FixedValue,
                            variableTemplate.Meanings,
                            variableTemplate.VariableConstraints,
                            variableTemplate.Approved);
                }
                else
                {
                    variableTemplate = variableManager.UpdateVariableTemplate(variableTemplate);
                }

                //// update missing values
                //if (model.MissingValues.Any())
                //{
                //    foreach (var missingValueItem in model.MissingValues)
                //    {
                //        if (missingValueItem.Id > 0)
                //            missingValueManager.Repo.Get(missingValueItem.Id);
                //        else
                //            missingValueManager.Create(missingValueItem.DisplayName, missingValueItem.Description, variableTemplate);
                //    }
                //}
            }

            return Json(true);
        }

        [JsonNetFilter]
        [HttpDelete]
        public JsonResult Delete(long id)
        {
            if (id <= 0) throw new NullReferenceException("id of the structure should be greater then 0");

            using (var variableManager = new VariableManager())
            using (var missingValueManager = new MissingValueManager())
            {
                if (id > 0)
                {
                    variableManager.DeleteVariableTemplate(id);
                }
            }

            // get default missing values

            return Json(true);
        }

        [JsonNetFilter]
        public JsonResult GetUnits()
        {
            using (var unitManager = new UnitManager())
            {
                var units = unitManager.Repo.Get().ToList();
                List<UnitItem> list = new List<UnitItem>();

                if (units.Any())
                {
                    foreach (var unit in units)
                    {
                        list.Add(new UnitItem(
                            unit.Id,
                            unit.Abbreviation,
                            unit.AssociatedDataTypes.Select(x => x.Name).ToList(),
                            "other"
                            ));
                    }
                }

                // get default missing values
                return Json(list.OrderBy(i => i.Group), JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        public JsonResult GetMeanings()
        {
            VariableHelper helper = new VariableHelper();
            List<MeaningItem> list = helper.GetMeanings();

            // get default missing values
            return Json(list.OrderBy(i => i.Group), JsonRequestBehavior.AllowGet);
        }

        [JsonNetFilter]
        public JsonResult GetConstraints()
        {
            VariableHelper helper = new VariableHelper();
            List<ListItem> list = helper.GetConstraints();

            return Json(list.OrderBy(i => i.Group), JsonRequestBehavior.AllowGet);
        }
    }
}