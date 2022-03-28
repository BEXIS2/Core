using BExIS.App.Bootstrap.Attributes;
using BExIS.Dim.Entities.Mapping;
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
                    // get link elements for mapping source and target 
                    var systemLink = mappingManager.GetLinkElement(0, LinkElementType.System);
                    var metadataLink = mappingManager.GetLinkElement(entityTemplate.MetadataStructure.Id, LinkElementType.MetadataStructure);

                    // get mapping based on source and target link elements
                    var rootMapping = mappingManager.GetMapping(systemLink, metadataLink);

                    if (rootMapping != null) // no root mapping exist then skip
                    {

                        var childmappings = mappingManager.GetChildMappingFromRoot(rootMapping.Id, 2);

                        foreach (var key in entityTemplate.MetadataFields)
                        {
                            var systemKey = (Key)key;
                            var linkElement = mappingManager.GetLinkElement(key, LinkElementType.Key);

                            // if link element exist and mappings to this element exist
                            if (linkElement != null && childmappings.Any(m => m.Source.Id.Equals(linkElement.Id)))
                            {
                                //get target
                                var matchMapping = childmappings.Where(m => m.Source.Id.Equals(linkElement.Id)).FirstOrDefault();
                                if (matchMapping != null)
                                {
                                    var elementId = matchMapping.Target.ElementId;
                                    var elemenType = matchMapping.Target.Type;
                                    var datatype = "";

                                    if (elemenType == LinkElementType.MetadataAttributeUsage)
                                    {
                                        var mau = metadataAttributeManager.MetadataAttributeUsageRepo.Get(elementId);
                                        datatype = mau.MetadataAttribute.DataType.SystemType;
                                    }
                                    else if (elemenType == LinkElementType.MetadataNestedAttributeUsage)
                                    {
                                        var mnau = metadataAttributeManager.MetadataNestedAttributeUsageRepo.Get(elementId);
                                        datatype = mnau.Member.DataType.SystemType;
                                    }
                                    else if (elemenType == LinkElementType.SimpleMetadataAttribute)
                                    {
                                        var sma = metadataAttributeManager.MetadataSimpleAttributeRepo.Get(elementId);
                                        datatype = sma.DataType.SystemType;
                                    }
                                    else if (elemenType == LinkElementType.ComplexMetadataAttribute)
                                    {
                                        var lvl2mapping = mappingManager.GetChildMapping(matchMapping.Id, 2);

                                        //var sma = metadataAttributeManager.MetadataSimpleAttributeRepo.Get(elementId);
                                        //datatype = sma.DataType.SystemType;
                                    }

                                    model.InputFields.Add(new MetadataInputField()
                                    {
                                        Index = key,
                                        Name = systemKey.ToString(),
                                        Type = datatype
                                    });

                                }
                            }
                        }
                    }
                }
                // Get name of system keys

                // Get datatypes of the mapped metadata attributes



                return Json(model, JsonRequestBehavior.AllowGet);
            }


            return Json(false, JsonRequestBehavior.AllowGet);
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
