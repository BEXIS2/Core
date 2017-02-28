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
        public ActionResult Index(long sourceId = 1, long targetId = 0, LinkElementType type = LinkElementType.System)
        {
            MappingMainModel model = new MappingMainModel();

            // load from mds example
            model.Source = MappingHelper.LoadFromMetadataStructure(sourceId, LinkElementPostion.Source);


            switch (type)
            {
                case LinkElementType.System:
                    {
                        model.Target = MappingHelper.LoadfromSystem(LinkElementPostion.Target);
                        model.SelectionList = MappingHelper.LoadSelectionList();
                        break;
                    }
                case LinkElementType.MetadataStructure:
                    {
                        model.Target = MappingHelper.LoadFromMetadataStructure(targetId, LinkElementPostion.Target);
                        model.SelectionList = MappingHelper.LoadSelectionList();
                        break;
                    }
            }
            if (model.Source != null && model.Target != null)
            {
                List<long> sourceListElementIds = new List<long>();
                if (model.Source != null && model.Source.LinkElements.Any())
                    sourceListElementIds = model.Source.LinkElements.Select(le => le.Id).ToList();

                List<long> targetListElementIds = new List<long>();
                if (model.Target != null && model.Target.LinkElements.Any())
                    targetListElementIds = model.Target.LinkElements.Select(le => le.Id).ToList();


                if (sourceListElementIds.Any() && targetListElementIds.Any())
                {
                    model.ParentMappings = MappingHelper.LoadMappings(
                        model.Source.ElementId, model.Source.Type, sourceListElementIds,
                        model.Target.ElementId, model.Target.Type, targetListElementIds);

                }


            }

            return View(model);
        }

        public ActionResult ReloadTarget(long sourceId = 1, long targetId = 0, LinkElementType sourceType = LinkElementType.System, LinkElementType targetType = LinkElementType.System, LinkElementPostion position = LinkElementPostion.Target)
        {
            LinkElementRootModel model = null;


            long id = position.Equals(LinkElementPostion.Source) ? sourceId : targetId;
            LinkElementType type = position.Equals(LinkElementPostion.Source) ? sourceType : targetType;


            switch (type)
            {
                case LinkElementType.System:
                    {
                        model = MappingHelper.LoadfromSystem(position);

                        break;
                    }
                case LinkElementType.MetadataStructure:
                    {
                        model = MappingHelper.LoadFromMetadataStructure(id, position);
                        break;
                    }
            }

            return PartialView("LinkElemenRoot", model);
        }

        public ActionResult ReloadMapping(long sourceId = 1, long targetId = 0, LinkElementType sourceType = LinkElementType.System, LinkElementType targetType = LinkElementType.System, LinkElementPostion position = LinkElementPostion.Target)
        {
            List<ComplexMappingModel> model = new List<ComplexMappingModel>();

            // load from mds example
            LinkElementRootModel source = null;

            switch (sourceType)
            {
                case LinkElementType.System:
                    {
                        source = MappingHelper.LoadfromSystem(LinkElementPostion.Source);

                        break;
                    }
                case LinkElementType.MetadataStructure:
                    {
                        source = MappingHelper.LoadFromMetadataStructure(targetId, LinkElementPostion.Source);
                        break;
                    }
            }

            LinkElementRootModel target = null;
            switch (targetType)
            {
                case LinkElementType.System:
                    {
                        target = MappingHelper.LoadfromSystem(LinkElementPostion.Target);

                        break;
                    }
                case LinkElementType.MetadataStructure:
                    {
                        target = MappingHelper.LoadFromMetadataStructure(targetId, LinkElementPostion.Target);
                        break;
                    }
            }

            if (target != null)
            {

                List<long> sourceListElementIds =
                    source.LinkElements.Where(m => m.Id > 0 && m.Complexity.Equals(LinkElementComplexity.Complex))
                        .Select(m => m.Id)
                        .ToList();
                List<long> targetListElementIds =
                    target.LinkElements.Where(m => m.Id > 0 && m.Complexity.Equals(LinkElementComplexity.Complex))
                        .Select(m => m.Id)
                        .ToList();

                if (sourceListElementIds.Any() && targetListElementIds.Any())
                {
                    model = MappingHelper.LoadMappings(
                        source.ElementId, source.Type, sourceListElementIds,
                        target.ElementId, target.Type, targetListElementIds);
                }
            }


            return PartialView("Mappings", model);
        }


        public ActionResult AddMappingElement(LinkElementModel linkElementModel)
        {
            linkElementModel = MappingHelper.LoadChildren(linkElementModel);

            return PartialView("MappingLinkElement", linkElementModel);
        }

        public ActionResult SaveMapping(ComplexMappingModel model)
        {
            MappingManager mappingManager = new MappingManager();
            //save link element if not exits
            //source 

            #region save or update RootMapping

            //create source Parents if not exist
            LinkElement sourceParent = MappingHelper.CreateIfNotExistLinkElement(model.Source.Parent);

            //create source Parents if not exist
            LinkElement targetParent = MappingHelper.CreateIfNotExistLinkElement(model.Target.Parent);

            //create root mapping if not exist
            Mapping rootMapping = MappingHelper.CreateIfNotExistMapping(sourceParent, targetParent, 0, null);

            #endregion

            #region save or update complex mapping
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
            Mapping mapping = MappingHelper.CreateIfNotExistMapping(source, target, 1, null);
            model.Id = mapping.Id;

            #endregion

            #region create or update simple mapping

            MappingHelper.UpdateSimpleMappings(source.Id, target.Id, model.SimpleMappings);


            #endregion

            //load all mappings
            return PartialView("Mapping", model);
        }

        public ActionResult LoadEmptyMapping()
        {
            return PartialView("Mapping", new ComplexMappingModel());
        }

        public JsonResult DeleteMapping(long id)
        {
            try
            {
                MappingHelper.DeleteMapping(id);

                //ToDo delete also all simple mappings that are belonging to the complex mapping

                return Json(true);
            }
            catch (Exception ex)
            {

                return Json(ex.Message);
            }
        }

    }
}