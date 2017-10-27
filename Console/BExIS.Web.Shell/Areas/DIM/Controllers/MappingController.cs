using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Services;
using BExIS.Modules.Dim.UI.Helper;
using BExIS.Modules.Dim.UI.Models.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace BExIS.Modules.Dim.UI.Controllers
{
    public class MappingController : Controller
    {
        // GET: DIM/Mapping
        public ActionResult Index(long sourceId = 1, long targetId = 0, LinkElementType type = LinkElementType.System)
        {
            MappingManager mappingManager = new MappingManager();


            try
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
                    //get linkelements
                    LinkElement source = mappingManager.GetLinkElement(sourceId, LinkElementType.MetadataStructure);
                    LinkElement target = mappingManager.GetLinkElement(targetId, type);

                    if (source != null && target != null)
                    {

                        //get root mapping
                        Mapping rootMapping = mappingManager.GetMapping(source, target);

                        if (rootMapping != null)
                        {
                            //get complex mappings
                            model.ParentMappings = MappingHelper.LoadMappings(rootMapping);
                        }

                    }
                }

                return View(model);
            }
            finally
            {
                mappingManager.Dispose();
            }
        }

        public ActionResult Mapping(long sourceId = 1, long targetId = 0,
            LinkElementType sourceType = LinkElementType.System, LinkElementType targetType = LinkElementType.System,
            LinkElementPostion position = LinkElementPostion.Target)
        {
            MappingManager mappingManager = new MappingManager();

            try
            {
                MappingMainModel model = new MappingMainModel();
                // load from mds example
                //model.Source = MappingHelper.LoadFromMetadataStructure(sourceId, LinkElementPostion.Source);
                /*
                 * Here the source and target will switch the sides
                 */
                #region load Source from Target

                switch (sourceType)
                {
                    case LinkElementType.System:
                        {
                            model.Source = MappingHelper.LoadfromSystem(LinkElementPostion.Source);
                            if (!model.SelectionList.Any()) model.SelectionList = MappingHelper.LoadSelectionList();
                            break;
                        }
                    case LinkElementType.MetadataStructure:
                        {
                            model.Source = MappingHelper.LoadFromMetadataStructure(sourceId, LinkElementPostion.Source);
                            if (!model.SelectionList.Any()) model.SelectionList = MappingHelper.LoadSelectionList();
                            break;
                        }
                }

                #endregion

                #region load Target
                switch (targetType)
                {
                    case LinkElementType.System:
                        {
                            model.Target = MappingHelper.LoadfromSystem(LinkElementPostion.Target);
                            if (!model.SelectionList.Any()) model.SelectionList = MappingHelper.LoadSelectionList();
                            break;
                        }
                    case LinkElementType.MetadataStructure:
                        {
                            model.Target = MappingHelper.LoadFromMetadataStructure(targetId, LinkElementPostion.Target);
                            if (!model.SelectionList.Any()) model.SelectionList = MappingHelper.LoadSelectionList();
                            break;
                        }
                }

                #endregion
                if (model.Source != null && model.Target != null)
                {
                    //get linkelements
                    LinkElement source = mappingManager.GetLinkElement(sourceId, sourceType);
                    LinkElement target = mappingManager.GetLinkElement(targetId, targetType);

                    if (source != null && target != null)
                    {

                        //get root mapping
                        Mapping rootMapping = mappingManager.GetMapping(source, target);

                        if (rootMapping != null)
                        {
                            //get complex mappings
                            model.ParentMappings = MappingHelper.LoadMappings(rootMapping);
                        }
                    }
                }

                return View("Index", model);
            }
            finally
            {
                mappingManager.Dispose();
            }
        }

        public ActionResult Switch(long sourceId = 1, long targetId = 0,
            LinkElementType sourceType = LinkElementType.System, LinkElementType targetType = LinkElementType.System,
            LinkElementPostion position = LinkElementPostion.Target)
        {

            return RedirectToAction("Mapping", new
            {
                sourceId = targetId,
                targetId = sourceId,
                sourceType = targetType,
                targetType = sourceType,
                position = position
            });
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

            MappingManager mappingManager = new MappingManager();
            try
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

                    //get linkelements
                    LinkElement sourceLE = mappingManager.GetLinkElement(sourceId, sourceType);
                    LinkElement targetLE = mappingManager.GetLinkElement(targetId, targetType);

                    if (sourceLE != null && targetLE != null)
                    {

                        //get root mapping
                        Mapping rootMapping = mappingManager.GetMapping(sourceLE, targetLE);

                        if (rootMapping != null)
                        {
                            //get complex mappings
                            model = MappingHelper.LoadMappings(rootMapping);
                        }
                    }
                }


                return PartialView("Mappings", model);
            }
            finally
            {
                mappingManager.Dispose();
            }
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
            try
            {

                #region save or update RootMapping

                //create source Parents if not exist
                LinkElement sourceParent = MappingHelper.CreateIfNotExistLinkElement(model.Source.Parent);

                //create source Parents if not exist
                LinkElement targetParent = MappingHelper.CreateIfNotExistLinkElement(model.Target.Parent);

                //create root mapping if not exist
                Mapping rootMapping = MappingHelper.CreateIfNotExistMapping(sourceParent, targetParent, 0, null, null);

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
                Mapping mapping = MappingHelper.CreateIfNotExistMapping(source, target, 1, null, rootMapping);
                model.Id = mapping.Id;
                model.ParentId = mapping.Parent.Id;
                #endregion

                #region create or update simple mapping

                MappingHelper.UpdateSimpleMappings(source.Id, target.Id, model.SimpleMappings, mapping);

                #endregion

                //load all mappings
                return PartialView("Mapping", model);
            }
            finally
            {
                mappingManager.Dispose();
            }
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