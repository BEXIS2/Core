using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Services;
using BExIS.Web.Shell.Areas.DIM.Helper;
using BExIS.Web.Shell.Areas.DIM.Models.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace BExIS.Web.Shell.Areas.DIM.Controllers
{
    public class MappingController : Controller
    {
        // GET: DIM/Mapping
        public ActionResult Index()
        {
            MappingMainModel model = new MappingMainModel();

            // load from mds example
            model.Source = MappingHelper.LoadFromMetadataStructure(2, LinkElementPostion.Source);
            //model.Target = MappingHelper.LoadFromMetadataStructure(3, LinkElementPostion.Target);
            model.Target = MappingHelper.LoadfromSystem(LinkElementPostion.Target);

            List<long> sourceListElementIds = model.Source.LinkElements.Where(m => m.Id > 0).Select(m => m.Id).ToList();
            List<long> targetListElementIds = model.Target.LinkElements.Where(m => m.Id > 0).Select(m => m.Id).ToList();

            if (sourceListElementIds.Any() && targetListElementIds.Any())
            {
                model.ParentMappings = MappingHelper.LoadMappings(
                model.Source.ElementId, model.Source.Type, sourceListElementIds,
                model.Target.ElementId, model.Target.Type, targetListElementIds);
            }




            return View(model);
        }


        public ActionResult AddMappingElement(LinkElementModel linkElementModel)
        {
            linkElementModel = MappingHelper.LoadChildren(linkElementModel);

            return PartialView("MappingLinkElement", linkElementModel);
        }

        public ActionResult SaveMapping(MappingModel model)
        {
            MappingManager mappingManager = new MappingManager();
            //save link element if not exits
            //source 

            //create source Parents if not exist
            LinkElement sourceParent = MappingHelper.CreateIfNotExistLinkElement(model.Source.Parent);

            //create source Parents if not exist
            LinkElement targetParent = MappingHelper.CreateIfNotExistLinkElement(model.Target.Parent);

            //create root mapping if not exist
            Mapping rootMapping = MappingHelper.CreateIfNotExistMapping(sourceParent, targetParent, null);


            LinkElement source;
            LinkElement target;

            //create source
            source = MappingHelper.CreateIfNotExistLinkElement(model.Source, sourceParent.Id);

            model.Source.Id = source.Id;
            model.Source = MappingHelper.LoadChildren(model.Source);

            //create target
            target = MappingHelper.CreateIfNotExistLinkElement(model.Target, targetParent.Id);

            model.Target.Id = target.Id;
            model.Target = MappingHelper.LoadChildren(model.Target);

            //save mapping
            Mapping mapping = MappingHelper.CreateIfNotExistMapping(source, target, null);
            model.Id = mapping.Id;


            //load all mappings
            return PartialView("Mapping", model);
        }

        public ActionResult LoadEmptyMapping()
        {
            return PartialView("Mapping", new MappingModel());
        }

        public JsonResult DeleteMapping(long id)
        {
            try
            {
                MappingManager mappingManager = new MappingManager();
                mappingManager.DeleteMapping(id);

                return Json(true);
            }
            catch (Exception ex)
            {

                return Json(ex.Message);
            }
        }

    }
}