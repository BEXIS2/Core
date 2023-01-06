using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Helpers.Mapping;
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
        public JsonResult Test()
        {
            using (var conceptManager = new ConceptManager())
            {
                var mappingConcept = conceptManager.CreateMappingConcept("test", "description", "url");

                var mK = conceptManager.CreateMappingKey("key1", "description", "url", true, true, mappingConcept);
               
                var mK1 = conceptManager.CreateMappingKey("key2", "description", "url", true, true, mappingConcept, mK);

                //mK.Children.Add(mK1);

                conceptManager.UpdateMappingKey(mK);

                var mk3 = conceptManager.MappingKeyRepo.Get(3);

                return Json(mK, JsonRequestBehavior.AllowGet); 
            }
        }

        // GET: DIM/Mapping
        public ActionResult Index(long sourceId = 1, long targetId = 0, LinkElementType type = LinkElementType.System)
        {
            using (MappingManager mappingManager = new MappingManager())
            {

                MappingMainModel model = new MappingMainModel();
                // load from mds example
                model.Source = MappingHelper.LoadFromMetadataStructure(sourceId, LinkElementPostion.Source, mappingManager);


                switch (type)
                {
                    case LinkElementType.System:
                        {
                            model.Target = MappingHelper.LoadfromSystem(LinkElementPostion.Target, mappingManager);
                            model.SelectionList = MappingHelper.LoadSelectionList();
                            break;
                        }
                    case LinkElementType.MetadataStructure:
                        {
                            model.Target = MappingHelper.LoadFromMetadataStructure(targetId, LinkElementPostion.Target, mappingManager);
                            model.SelectionList = MappingHelper.LoadSelectionList();
                            break;
                        }
                    case LinkElementType.MappingConcept:
                        {
                            model.Target = MappingHelper.LoadMappingConcept(targetId, LinkElementPostion.Target, mappingManager);
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
 
        }

        public ActionResult Mapping(long sourceId = 1, long targetId = 0,
            LinkElementType sourceType = LinkElementType.System, LinkElementType targetType = LinkElementType.System,
            LinkElementPostion position = LinkElementPostion.Target)
        {
            var model = generateModel(sourceId, targetId, sourceType, targetType, position);

            return View("Index", model);
        }

        //public ActionResult Switch(long sourceId = 1, long targetId = 0,
        //    LinkElementType sourceType = LinkElementType.System, LinkElementType targetType = LinkElementType.System,
        //    LinkElementPostion position = LinkElementPostion.Target)
        //{
        //    var model = generateModel(targetId,sourceId, targetType, sourceType, position);

        //    return PartialView("Index", model);
        //}

        private MappingMainModel generateModel(long sourceId = 1, long targetId = 0,
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
                            model.Source = MappingHelper.LoadfromSystem(LinkElementPostion.Source, mappingManager);
                            if (!model.SelectionList.Any()) model.SelectionList = MappingHelper.LoadSelectionList();
                            break;
                        }
                    case LinkElementType.MetadataStructure:
                        {
                            model.Source = MappingHelper.LoadFromMetadataStructure(sourceId, LinkElementPostion.Source, mappingManager);
                            if (!model.SelectionList.Any()) model.SelectionList = MappingHelper.LoadSelectionList();
                            break;
                        }
                    case LinkElementType.MappingConcept:
                        {
                            model.Source = MappingHelper.LoadMappingConcept(sourceId, LinkElementPostion.Source, mappingManager);
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
                            model.Target = MappingHelper.LoadfromSystem(LinkElementPostion.Target, mappingManager);
                            if (!model.SelectionList.Any()) model.SelectionList = MappingHelper.LoadSelectionList();
                            break;
                        }
                    case LinkElementType.MetadataStructure:
                        {
                            model.Target = MappingHelper.LoadFromMetadataStructure(targetId, LinkElementPostion.Target, mappingManager);
                            if (!model.SelectionList.Any()) model.SelectionList = MappingHelper.LoadSelectionList();
                            break;
                        }
                    case LinkElementType.MappingConcept:
                        {
                            model.Target = MappingHelper.LoadMappingConcept(targetId, LinkElementPostion.Target, mappingManager);
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

                return model;
               
            }
            finally
            {
                mappingManager.Dispose();
            }
        }

        public ActionResult ReloadTarget(long sourceId = 1, long targetId = 0, LinkElementType sourceType = LinkElementType.System, LinkElementType targetType = LinkElementType.System, LinkElementPostion position = LinkElementPostion.Target)
        {
            MappingManager mappingManager = new MappingManager();

            try
            {
                LinkElementRootModel model = null;

                long id = position.Equals(LinkElementPostion.Source) ? sourceId : targetId;
                LinkElementType type = position.Equals(LinkElementPostion.Source) ? sourceType : targetType;


                switch (type)
                {
                    case LinkElementType.System:
                        {
                            model = MappingHelper.LoadfromSystem(position, mappingManager);

                            break;
                        }
                    case LinkElementType.MetadataStructure:
                        {
                            model = MappingHelper.LoadFromMetadataStructure(id, position, mappingManager);
                            break;
                        }
                    case LinkElementType.MappingConcept:
                        {
                            model = MappingHelper.LoadMappingConcept(id, position, mappingManager);
                            break;
                        }
                }

                return PartialView("LinkElemenRoot", model);
            }
            finally
            {
                mappingManager.Dispose();
            }
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
                            source = MappingHelper.LoadfromSystem(LinkElementPostion.Source, mappingManager);

                            break;
                        }
                    case LinkElementType.MetadataStructure:
                        {
                            source = MappingHelper.LoadFromMetadataStructure(sourceId, LinkElementPostion.Source, mappingManager);
                            break;
                        }
                    case LinkElementType.MappingConcept:
                        {
                            source = MappingHelper.LoadMappingConcept(sourceId, LinkElementPostion.Source, mappingManager);
                            break;
                        }
                }

                LinkElementRootModel target = null;
                switch (targetType)
                {
                    case LinkElementType.System:
                        {
                            target = MappingHelper.LoadfromSystem(LinkElementPostion.Target, mappingManager);

                            break;
                        }
                    case LinkElementType.MetadataStructure:
                        {
                            target = MappingHelper.LoadFromMetadataStructure(targetId, LinkElementPostion.Target, mappingManager);
                            break;
                        }
                    case LinkElementType.MappingConcept:
                        {
                            target = MappingHelper.LoadMappingConcept(targetId, position, mappingManager);
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
                LinkElement sourceParent = MappingHelper.CreateIfNotExistLinkElement(model.Source.Parent, mappingManager);

                //create source Parents if not exist
                LinkElement targetParent = MappingHelper.CreateIfNotExistLinkElement(model.Target.Parent, mappingManager);

                //create root mapping if not exist
                Mapping rootMapping = MappingHelper.CreateIfNotExistMapping(sourceParent, targetParent, 0, null, null, mappingManager);
                // also create the mapping in the other direction
                Mapping rootMappingReverse = MappingHelper.CreateIfNotExistMapping(targetParent, sourceParent, 0, null, null, mappingManager);

                #endregion

                #region save or update complex mapping
                LinkElement source;
                LinkElement target;

                //create source
                source = MappingHelper.CreateIfNotExistLinkElement(model.Source, sourceParent.Id, mappingManager);

                model.Source.Id = source.Id;
                model.Source = MappingHelper.LoadChildren(model.Source);

                //create target
                target = MappingHelper.CreateIfNotExistLinkElement(model.Target, targetParent.Id, mappingManager);

                model.Target.Id = target.Id;
                model.Target = MappingHelper.LoadChildren(model.Target);

                //save mapping
                Mapping mapping = MappingHelper.CreateIfNotExistMapping(source, target, 1, null, rootMapping, mappingManager);
                // also create the mapping in the other direction
                Mapping mappingReverse = MappingHelper.CreateIfNotExistMapping(target, source, 1, null, rootMappingReverse, mappingManager);

                model.Id = mapping.Id;
                model.ParentId = mapping.Parent.Id;
                #endregion

                #region create or update simple mapping

                MappingHelper.UpdateSimpleMappings(source.Id, target.Id, model.SimpleMappings, mapping, mappingManager);
                MappingHelper.UpdateSimpleMappings(target.Id, source.Id, model.SimpleMappings, mappingReverse, mappingManager);

                #endregion

                //load all mappings
                return PartialView("Mapping", model);
            }
            finally
            {
                mappingManager.Dispose();
                MappingUtils.Clear();
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
                using (MappingManager mappingManager = new MappingManager())
                {
                    MappingHelper.DeleteMapping(id, mappingManager);

                    //ToDo delete also all simple mappings that are belonging to the complex mapping
                    return Json(true);
                }
            }
            catch (Exception ex)
            {

                return Json(ex.Message);
            }
            finally
            {
                MappingUtils.Clear();
            }
        }

    }
}