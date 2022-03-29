using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Helpers.Mapping;
using BExIS.Dim.Services;
using BExIS.Dlm.Services.Data;
using BExIS.Dlm.Services.DataStructure;
using BExIS.Dlm.Services.MetadataStructure;
using BExIS.Modules.Dcm.UI.Models.Create;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BExIS.Modules.Dcm.UI.Controllers
{
    public class CreateController : Controller
    {
        // GET: Create
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Get data based on a EntityTemplate 
        /// System keys and datatypes
        /// list of datastructures if exist
        /// List of file types
        /// </summary>
        /// <param name="id">entity template id</param>
        /// <returns></returns>
        [JsonNetFilter]
        public JsonResult Get(long id)
        {
            if (id <= 0) throw new ArgumentNullException("The id of entitytemplate does not exist.");

            CreateModel model = new CreateModel();

            using (var entityTemplateManager = new EntityTemplateManager())
            using (var datastructureManager = new DataStructureManager())
            using (var metadataAttributeManager = new MetadataAttributeManager())
            using (var mappingManager = new MappingManager())
            {

                var entityTemplate = entityTemplateManager.Repo.Get(id);

                if (entityTemplate == null) throw new ArgumentNullException("The entitytemplate with id (" + id + ") does not exist.");

                model.Id = entityTemplate.Id;
                model.Name = entityTemplate.Name;
                model.Description = entityTemplate.Description;

                model.FileTypes = entityTemplate.AllowedFileTypes;

                // get structure names
                var structures = datastructureManager.AllTypesDataStructureRepo.Query(d => entityTemplate.DatastructureList.Contains(d.Id)).Select(d => d.Name);
                model.Datastructures = structures.ToList();

                // Get Metadata Input Fields
                if (entityTemplate.MetadataFields.Any())
                {
                    foreach (var key in entityTemplate.MetadataFields)
                    {
                        var systemKey = (Key)key;
                        long metadataStructureId = entityTemplate.MetadataStructure.Id;
                        LinkElement target = null;

                        // when there is a mapping get link element
                        if (MappingUtils.HasTarget(key, metadataStructureId, out target))
                        {
                            // get Datatype of link element
                            var datatype = MappingUtils.GetDataType(target);

                            model.InputFields.Add(new MetadataInputField()
                            {
                                Index = key,
                                Name = systemKey.ToString(),
                                Type = datatype
                            });
                        }
                    }
                }

                return Json(model, JsonRequestBehavior.AllowGet);
            }
        }

        [JsonNetFilter]
        [HttpPost]
        public JsonResult Create(CreateModel data)
        {
            if (data == null) return Json(false);

            return Json(true);
        }
    }
}
