using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Vaiona.Persistence.Api;

using BExIS.Dim.Entities.Mapping;

using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Authorization;

namespace BExIS.Dim.Helpers.Mapping
{
    public class MappingUtils
    {
        public static object LinkeElementType { get; private set; }

        #region generic

        public static bool ExistMappings(long sourceId, LinkElementType sourceType, long targetId, LinkElementType targetType)
        {
            try
            {
                IList<Entities.Mapping.Mapping> mapping = CachedMappings();
                return mapping.Where(m =>
                          m.Target.ElementId.Equals(targetId) &&
                          m.Target.Type.Equals(targetType) &&
                          m.Source.ElementId.Equals(sourceId) &&
                          m.Source.Type.Equals(sourceType)
                      ).ToList().Any();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<Entities.Mapping.Mapping> GetMappings(long sourceId, LinkElementType sourceType, long targetId, LinkElementType targetType)
        {
            try
            {
                IList<Entities.Mapping.Mapping> mapping = CachedMappings();
                var mapping_result = mapping.Where(m =>
                           m.Target.ElementId.Equals(targetId) &&
                           m.Target.Type.Equals(targetType) &&
                           m.Source.ElementId.Equals(sourceId) &&
                           m.Source.Type.Equals(sourceType)
                       ).ToList();

                if (mapping_result.Any())
                {
                    return mapping_result;
                }
                else
                {
                    return new List<Entities.Mapping.Mapping>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<Entities.Mapping.Mapping> GetMappings(long parentMappingId)
        {
            try
            {
                IList<Entities.Mapping.Mapping> mapping = CachedMappings();
                var mapping_result = mapping.Where(m =>
                            m.Parent.Id.Equals(parentMappingId)
                        ).ToList();

                if (mapping_result.Any()) return mapping_result;

                return new List<Entities.Mapping.Mapping>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<Entities.Mapping.Mapping> GetMappingsWhereSource(long sourceId, LinkElementType sourceType)
        {
            try
            {
                IList<Entities.Mapping.Mapping> mapping = CachedMappings();
                var mapping_result = mapping.Where(m =>
                            m.Source.ElementId.Equals(sourceId) &&
                            m.Source.Type.Equals(sourceType)
                        ).ToList();

                if (mapping_result.Any()) return mapping_result;

                return new List<Entities.Mapping.Mapping>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<Entities.Mapping.Mapping> GetMappingsWhereSource(long sourceId, LinkElementType sourceType, int level)
        {
            try
            {
                IList<Entities.Mapping.Mapping> mapping = CachedMappings();
                var mapping_result = mapping.Where(m =>
                            m.Source.ElementId.Equals(sourceId) &&
                            m.Source.Type.Equals(sourceType) &&
                            m.Level == level
                        ).ToList();

                if (mapping_result.Any()) return mapping_result;

                return new List<Entities.Mapping.Mapping>();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<Entities.Mapping.Mapping> GetMappingsWhereTarget(long targetId, LinkElementType targetType)
        {
            try
            {
                using (IUnitOfWork uow = (new object()).GetUnitOfWork())
                {
                    var mappings = uow.GetReadOnlyRepository<BExIS.Dim.Entities.Mapping.Mapping>().Get() // this get is here because the expression is not supported by NH!
                        .Where(m =>
                            m.Target.ElementId.Equals(targetId) &&
                            m.Target.Type.Equals(targetType)
                        ).ToList();

                    if (mappings.Any()) return mappings;

                    return new List<Entities.Mapping.Mapping>();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion generic

        #region get from entites

        public static bool ExistMappingWithEntity(long targetId, LinkElementType targetType)
        {
            try
            {
                IList<Entities.Mapping.Mapping> mapping = CachedMappings();
                return mapping.Where(m =>
                        m.Target.ElementId.Equals(targetId) &&
                        m.Target.Type.Equals(targetType) &&
                        m.Parent != null &&
                        m.Parent.Source.Type.Equals(LinkElementType.Entity)
                    ).ToList().Any();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// e.g. root is a metadata structre, the function will check if there is any mapping to any entity
        /// </summary>
        /// <param name="rootId"></param>
        /// <param name="rootType"></param>
        /// <returns></returns>
        public static bool ExistMappingWithEntityFromRoot(long rootId, LinkElementType rootType)
        {
            try
            {
                IList<Entities.Mapping.Mapping> mapping = CachedMappings();
                return mapping.Where(m =>
                            m.Parent != null &&
                            m.Parent.Source.Type.Equals(LinkElementType.Entity) &&
                            m.Parent.Target.Type.Equals(rootType) &&
                            m.Parent.Target.ElementId.Equals(rootId)
                        ).ToList().Any();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// e.g. root is a metadata structre, the function will check if there is any mapping to a specific entity
        /// </summary>
        /// <param name="rootId"></param>
        /// <param name="rootType"></param>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public static bool ExistMappingWithEntityFromRoot(long rootId, LinkElementType rootType, long entityId)
        {
            try
            {
                IList<Entities.Mapping.Mapping> mapping = CachedMappings();
                return mapping.Where(m =>
                            m.Parent != null &&
                            m.Source.ElementId.Equals(entityId) &&
                            m.Source.Type.Equals(LinkElementType.Entity) &&
                            m.Parent.Target.Type.Equals(rootType) &&
                            m.Parent.Target.ElementId.Equals(rootId)
                        ).ToList().Any();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static List<MappingEntityResultElement> GetAllMatchesInEntities(long targetElementId, LinkElementType targetType,
            string value = "")
        {
            try
            {
                //get all mapppings where target is mapped
                // LinkElementType.PartyCustomType is set because of the function name
                // all mapped attributes are LinkElementType.PartyCustomType in this case
                List<MappingEntityResultElement> tmp = new List<MappingEntityResultElement>();

                IList<Entities.Mapping.Mapping> mapping = CachedMappings();
                var mapping_result = mapping.Where(m =>
                        m.Target.ElementId.Equals(targetElementId) &&
                        m.Target.Type.Equals(targetType) &&
                        m.Source.Type.Equals(LinkElementType.Entity)
                    ).ToList();
                tmp = getAllValuesFromEntites(mapping_result, value);
                return tmp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static List<MappingEntityResultElement> getAllValuesFromEntites(IEnumerable<Entities.Mapping.Mapping> mappings, string value)
        {
            List<MappingEntityResultElement> tmp = new List<MappingEntityResultElement>();
            EntityManager entityManager = new EntityManager();
            EntityPermissionManager entityPermissionManager = new EntityPermissionManager();

            try
            {
                foreach (var mapping in mappings)
                {
                    //load the entiy for each source element id wich is the entity id
                    var entity = entityManager.EntityRepository.Get(mapping.Source.ElementId);
                    // load all existing entity objects iwith the defined manager

                    var instanceStore = (IEntityStore)Activator.CreateInstance(entityManager.FindById(entity.Id).EntityStoreType);
                    var instances = instanceStore.GetEntities().Where(e => e.Title.ToLower().Contains(value.ToLower())).Select(i => new MappingEntityResultElement()
                    {
                        EntityId = i.Id,
                        EntityTypeId = entity.Id,
                        Value = i.Title,
                        Url = "url to " + i.Id
                    }).ToList();

                    //check if allready exist in list
                    if (instances.Any())
                    {
                        foreach (var instance in instances)
                        {
                            if (!tmp.Any(i => i.EntityId.Equals(instance.EntityId) && i.EntityTypeId.Equals(instance.EntityTypeId)))
                                tmp.Add(instance);
                        }
                    }
                }
            }
            finally
            {
                entityManager.Dispose();
                entityPermissionManager.Dispose();
            }

            return tmp.OrderBy(i => i.Value).ToList();
        }

        #endregion get from entites

        #region GET FROM SYSTEM

        /// <summary>
        /// e.g.
        /// targetElementId : 3
        /// targetType : nested usage
        /// value search
        /// </summary>
        /// <param name="targetElementId"></param>
        /// <param name="targetType"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static List<MappingPartyResultElemenet> GetAllMatchesInSystem(long targetElementId, LinkElementType targetType,
            string value = "")
        {
            try
            {
                //get all mapppings where target is mapped
                // LinkElementType.PartyCustomType is set because of the function name
                // all mapped attributes are LinkElementType.PartyCustomType in this case

                List<MappingPartyResultElemenet> tmp = new List<MappingPartyResultElemenet>();
                IList<Entities.Mapping.Mapping> mapping = CachedMappings();
                var mapping_result = mapping.Where(m =>
                        m.Target.ElementId.Equals(targetElementId) &&
                        m.Target.Type.Equals(targetType) &&
                        m.Source.Type.Equals(LinkElementType.PartyCustomType)
                    ).ToList();
                tmp = getAllValuesFromSystem(mapping_result, value);
                return tmp;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool ExistMappingWithPartyCustomType(long targetId, LinkElementType targetType)
        {
            try
            {
                IList<Entities.Mapping.Mapping> mapping = CachedMappings();
                return mapping.Where(m =>
                            m.Target.ElementId.Equals(targetId) &&
                            m.Target.Type.Equals(targetType) &&
                            m.Parent != null &&
                            m.Parent.Source.Type.Equals(LinkElementType.PartyCustomType)
                        ).ToList().Any();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// if a simple attr is direct mapped to a PartyCustomType without context informations about th parent
        /// in the database mappings for level 1 & 2 existing for the simple attribute
        /// level one means then simple to a complex
        /// level two means then simple to a complex/simple
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static bool ExistSimpleMappingWithPartyCustomType(long targetId, LinkElementType targetType)
        {
            try
            {
                //if party is the parent
                IList<Entities.Mapping.Mapping> mapping = CachedMappings();
                bool mappingsWhenPartyIsParent = mapping.Where(m =>
                        m.Target.ElementId.Equals(targetId) &&
                        m.Target.Type.Equals(targetType) &&
                        m.Parent != null &&
                        m.Parent.Source.Type.Equals(LinkElementType.PartyCustomType) &&
                        m.Level.Equals(2)
                    ).ToList().Any();

                bool mapping_result = mapping.Where(m =>
                        m.Target.ElementId.Equals(targetId) &&
                        m.Target.Type.Equals(targetType) &&
                        m.Source.Type.Equals(LinkElementType.PartyCustomType) &&
                        m.Level.Equals(1)
                    ).ToList().Any();

                return (mapping_result && mappingsWhenPartyIsParent);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// if a simple attr is mapped to a party with context informations about the parent
        /// in the database mappings for level 1 & 2 existing for the simple attribute
        /// level one means then parent to a complex
        /// level two means then simple to a complex/simple
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static bool ExistComplexMappingWithPartyCustomType(long targetId, LinkElementType targetType)
        {
            try
            {
                IList<Entities.Mapping.Mapping> mapping = CachedMappings();
                bool mappings_result = mapping.Where(m =>
                             m.Target.ElementId.Equals(targetId) &&
                             m.Target.Type.Equals(targetType) &&
                             m.Parent != null &&
                             m.Parent.Source.Type.Equals(LinkElementType.PartyCustomType) &&
                             m.Level.Equals(2)
                        ).ToList().Any();

                //if party same level like target

                bool mappingsWhenPartyIsParent = mapping.Where(m =>
                            m.Target.ElementId.Equals(targetId) &&
                            m.Target.Type.Equals(targetType) &&
                            m.Source.Type.Equals(LinkElementType.PartyCustomType) &&
                            m.Level.Equals(1)
                        ).ToList().Any();

                return (!mappings_result && mappingsWhenPartyIsParent);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool ExistMappingWithParty(long targetId, LinkElementType targetType)
        {
            try
            {
                IList<Entities.Mapping.Mapping> mapping = CachedMappings();
                return mapping.Where(m =>
                            m.Target.ElementId.Equals(targetId) &&
                            m.Target.Type.Equals(targetType) &&
                            m.Parent != null &&
                            m.Parent.Source.Type.Equals(LinkElementType.PartyType)
                        ).ToList().Any();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// if a simple attr is direct mapped to a party without context informations about th parent
        /// in the database mappings for level 1 & 2 existing for the simple attribute
        /// level one means then simple to a complex
        /// level two means then simple to a complex/simple
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static bool ExistSimpleMappingWithParty(long targetId, LinkElementType targetType)
        {
            try
            {
                IList<Entities.Mapping.Mapping> mapping = CachedMappings();
                bool mappings_result = mapping.Where(m =>
                            m.Target.ElementId.Equals(targetId) &&
                            m.Target.Type.Equals(targetType) &&
                            m.Parent != null &&
                            m.Parent.Source.Type.Equals(LinkElementType.PartyType) &&
                            m.Level.Equals(2)
                        ).ToList().Any();

                //if party same lvel like target
                bool mappingsWhenPartyIsParent = mapping.Where(m =>
                            m.Target.ElementId.Equals(targetId) &&
                            m.Target.Type.Equals(targetType) &&
                            m.Source.Type.Equals(LinkElementType.PartyType) &&
                            m.Level.Equals(1)
                        ).ToList().Any();

                return (mappings_result && mappingsWhenPartyIsParent);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// if a simple attr is mapped to a party with context informations about the parent
        /// in the database mappings for level 1 & 2 existing for the simple attribute
        /// level one means then parent to a complex
        /// level two means then simple to a complex/simple
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static bool ExistComplexMappingWithParty(long targetId, LinkElementType targetType)
        {
            try
            {
                IList<Entities.Mapping.Mapping> mapping = CachedMappings();
                bool mappings_result = mapping.Where(m =>
                            m.Target.ElementId.Equals(targetId) &&
                            m.Target.Type.Equals(targetType) &&
                            m.Parent != null &&
                            m.Parent.Source.Type.Equals(LinkElementType.PartyType) &&
                            m.Level.Equals(2)
                        ).ToList().Any();

                //if party same level like target
                bool mappingsWhenPartyIsParent = mapping.Where(m =>
                            m.Target.ElementId.Equals(targetId) &&
                            m.Target.Type.Equals(targetType) &&
                            m.Source.Type.Equals(LinkElementType.PartyType) &&
                            m.Level.Equals(1)
                        ).ToList().Any();

                return (mappings_result && !mappingsWhenPartyIsParent);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool ExistMappingWithParty(long targetId, LinkElementType targetType, long partyId)
        {
            try
            {
                IList<Entities.Mapping.Mapping> mapping = CachedMappings();
                return mapping.Where(m =>
                            m.Target.ElementId.Equals(targetId) &&
                            m.Target.Type.Equals(targetType) &&
                            m.Parent != null &&
                            m.Parent.Source.ElementId.Equals(partyId)
                        ).ToList().Any();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// get all values from systen parties values
        ///
        /// </summary>
        /// <param name="mappings"></param>
        /// <returns></returns>
        private static List<MappingPartyResultElemenet> getAllValuesFromSystem(IEnumerable<Entities.Mapping.Mapping> mappings, string value)
        {
            using (MappingManager _mappingManager = new MappingManager())
            using (PartyTypeManager partyTypeManager = new PartyTypeManager())
            using (PartyManager partyManager = new PartyManager())
            {
                List<MappingPartyResultElemenet> tmp = new List<MappingPartyResultElemenet>();

                IEnumerable<long> parentIds = mappings.Where(m => m.Parent != null).Select(m => m.Parent.Id).Distinct();

                IEnumerable<Entities.Mapping.Mapping> selectedMappings;

                // all Masks are the same
                string mask = "";
                PartyType partyType = null;
                List<Party> parties = null;

                foreach (var pId in parentIds)
                {
                    Entities.Mapping.Mapping parentMapping = _mappingManager.GetMapping(pId);

                    selectedMappings =
                        mappings.Where(m => m.Parent != null && m.Parent.Id.Equals(pId));

                    long parentTypeId = parentMapping.Source.ElementId;
                    long sourceId = selectedMappings.FirstOrDefault().Source.ElementId;

                    //mappings.FirstOrDefault().TransformationRule.Mask;

                    if (parentTypeId == 0)
                    {
                        partyType =
                            partyTypeManager.PartyTypeRepository.Query()
                                .FirstOrDefault(p => p.CustomAttributes.Any(c => c.Id.Equals(sourceId)));
                        parties = partyManager.PartyRepository.Query().Where(p => p.PartyType.Equals(partyType)).ToList();
                    }
                    else
                    {
                        partyType = partyTypeManager.PartyTypeRepository.Get(parentTypeId);
                        parties = partyManager.PartyRepository.Query().Where(p => p.PartyType.Id.Equals(parentTypeId)).ToList();
                    }

                    //get all mapped element ids
                    var elementIds = mappings.Select(m => m.Source.ElementId);
                    // get all attributes based on the element id list
                    var attributeValues = partyManager.PartyCustomAttributeValueRepository.Query(y => elementIds.Contains(y.CustomAttribute.Id)).ToList();
                    attributeValues = attributeValues.Where(y => y.Value.ToLower().Contains(value.ToLower())).ToList();
                    //get all party ids
                    var partyIds = attributeValues.Select(a => a.Party.Id).Distinct();

                    foreach (var partyId in partyIds)
                    {
                        MappingPartyResultElemenet resultObject = new MappingPartyResultElemenet();
                        resultObject.PartyId = partyId;

                        //get mask from first mapping
                        mask = mappings.FirstOrDefault().TransformationRule.Mask;

                        var allMappedAttrValues = partyManager.PartyCustomAttributeValueRepository.Query(y =>
                            y.Party.Id.Equals(partyId) && elementIds.Contains(y.CustomAttribute.Id)).ToList();

                        foreach (var attrValue in allMappedAttrValues)
                        {
                            //get mapping for the attrvalue
                            var mapping = mappings.Where(m => m.Source.ElementId.Equals(attrValue.CustomAttribute.Id)).FirstOrDefault();

                            List<string> regExResultList = transform(attrValue.Value, mapping.TransformationRule);
                            string placeHolderName = attrValue.CustomAttribute.Name;

                            mask = setOrReplace(mask, regExResultList, placeHolderName);

                            resultObject.Value = mask;
                        }

                        if (mask.ToLower().Contains(value.ToLower()))
                            tmp.Add(resultObject);
                    }
                }

                return tmp;
            }
        }

        public static string GetValueFromSystem(long partyid, long targetElementId, LinkElementType targetElementType)
        {
            MappingManager _mappingManager = new MappingManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            PartyManager partyManager = new PartyManager();
            try
            {
                using (IUnitOfWork uow = (new object()).GetUnitOfWork())
                {
                    string value = "";

                    IList<Entities.Mapping.Mapping> mapping = CachedMappings();
                    var mapping_result = mapping.Where(m =>
                                m.Target.ElementId.Equals(targetElementId) &&
                                m.Target.Type.Equals(targetElementType) &&
                                m.Source.Type.Equals(LinkElementType.PartyCustomType)
                            ).ToList();

                    if (mapping_result.Any())
                    {
                        string mask = "";
                        mask = mapping_result.FirstOrDefault().TransformationRule.Mask;

                        foreach (var mapping_element in mapping_result)
                        {
                            long attributeId = mapping_element.Source.ElementId;

                            PartyCustomAttributeValue attrValue =
                                partyManager.PartyCustomAttributeValueRepository.Query()
                                    .Where(v => v.CustomAttribute.Id.Equals(attributeId) && v.Party.Id.Equals(partyid))
                                    .FirstOrDefault();

                            if (attrValue != null)
                            {
                                List<string> regExResultList = transform(attrValue.Value, mapping_element.TransformationRule);
                                string placeHolderName = attrValue.CustomAttribute.Name;

                                mask = setOrReplace(mask, regExResultList, placeHolderName);
                            }
                        }

                        if (mask.ToLower().Contains(value.ToLower()))

                            return mask;
                    }
                }

                return "";
            }
            finally
            {
                _mappingManager.Dispose();
                partyTypeManager.Dispose();
                partyManager.Dispose();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="targetElementId"></param>
        /// <param name="targetElementType"></param>
        /// <param name="parentPartyId"></param>
        /// <returns></returns>
        public static bool PartyAttrIsMain(long targetElementId, LinkElementType targetElementType)
        {
            try
            {
                //get all mapppings where target is mapped
                // LinkElementType.PartyCustomType is set because of the function name
                // all mapped attributes are LinkElementType.PartyCustomType in this case
                using (IUnitOfWork uow = (new object()).GetUnitOfWork())
                {
                    List<MappingPartyResultElemenet> tmp = new List<MappingPartyResultElemenet>();

                    //Select all mappings where the target is mapped to a party custom attr with the party id
                    IList<Entities.Mapping.Mapping> mapping = CachedMappings();
                    var mapping_result = mapping.Where(m =>
                                m.Target.ElementId.Equals(targetElementId) &&
                                m.Target.Type.Equals(targetElementType) &&
                                m.Source.Type.Equals(LinkElementType.PartyCustomType) &&
                                m.Parent != null
                            );

                    foreach (var mapping_element in mapping_result)
                    {
                        if (mapping != null)
                        {
                            PartyCustomAttribute pca = uow.GetReadOnlyRepository<PartyCustomAttribute>().Get(mapping_element.Source.ElementId);
                            if (pca != null && pca.IsMain == true) return true;
                        }
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool ExistSystemFieldMappings(long id, LinkElementType type)
        {
            //Automatic System Keys starts at 100
            //Id = 100,
            //Version = 101,
            //DateOfVersion = 102,
            //MetadataCreationDate = 103,
            //MetadataLastModfied = 104,
            //DataFirstEntry = 105,
            //DataLastModified = 106, // also for Dubline Core date

            try
            {
                IList<Entities.Mapping.Mapping> mapping = CachedMappings();
                return mapping.Where(m =>
                            m.Target.ElementId.Equals(id) &&
                            m.Target.Type.Equals(type) &&
                            100 <= m.Source.ElementId && m.Source.ElementId <= 106 &&
                            m.Source.Type.Equals(LinkElementType.Key)
                        ).ToList().Any();

                //  return false;
                // }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Check if there is a mapping to system key nodes
        /// return true if yes
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        ///
        public static IList<Entities.Mapping.Mapping> CachedMappings()
        {
            // System.Web.HttpContext may not existing during the async upload, so check wheter the context exist
            if (System.Web.HttpContext.Current != null)
            {
                if (System.Web.HttpContext.Current.Session["mappings"] != null)
                {
                    return (IList<Entities.Mapping.Mapping>)System.Web.HttpContext.Current.Session["mappings"];
                }
                else
                {
                    using (IUnitOfWork uow = (new object()).GetUnitOfWork())
                    {
                        var mappings = uow.GetReadOnlyRepository<BExIS.Dim.Entities.Mapping.Mapping>().Get();
                        System.Web.HttpContext.Current.Session["mappings"] = mappings;
                        return mappings;
                    }
                }
            }
            else // if the System.Web.HttpContext is not existing, mappings need to be loaded by every call
            {
                using (IUnitOfWork uow = (new object()).GetUnitOfWork())
                {
                    return uow.GetReadOnlyRepository<BExIS.Dim.Entities.Mapping.Mapping>().Get();
                }
            }
        }

        public static void Clear()
        {
            if (System.Web.HttpContext.Current.Session["mappings"] != null)
                System.Web.HttpContext.Current.Session.Remove("mappings");
        }

        #endregion GET FROM SYSTEM

        #region GET FROM Specific MetadataStructure // Source

        /// <summary>
        ///
        /// </summary>
        /// <param name="targetElementId"></param>
        /// <param name="targetType"></param>
        /// <param name="sourceRootId"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static List<string> GetValuesFromMetadata(long targetElementId, LinkElementType targetType,
            long sourceRootId, XDocument metadata)
        {
            //grab values from metadata where targetelementid and targetType is mapped
            // e.g. get title from metadata

            MappingManager mappingManager = new MappingManager();

            try
            {
                List<string> tmp = new List<string>();

                var mappings = mappingManager.GetMappings().Where(m =>
                    m.Target.ElementId.Equals(targetElementId) &&
                    m.Target.Type.Equals(targetType) &&
                    getRootMapping(m) != null &&
                    getRootMapping(m).Source.ElementId.Equals(sourceRootId) &&
                    getRootMapping(m).Source.Type == LinkElementType.MetadataStructure &&
                    m.Level.Equals(2));

                // possinle cases                   mapping count
                // 1 - 1                            1
                // x,y to z (combination merge)     2
                // x -> z1,z2,z3 (split)            1
                // x1,x2,x3 -> z (merge)            1

                if (mappings.Count() == 1)
                {
                    //possible cases =
                    // 1 - 1
                    // x -> z1,z2,z3 (split)
                    // x1,x2,x3 -> z (join)

                    Entities.Mapping.Mapping m = mappings.FirstOrDefault();

                    if (m != null)
                    {
                        if (m.Source.Type.Equals(LinkElementType.MetadataAttributeUsage) ||
                            m.Source.Type.Equals(LinkElementType.MetadataNestedAttributeUsage))
                        {
                            IEnumerable<XElement> elements = getXElementsFromAMapping(m, metadata);

                            if (elements.Count() == 1)
                            {
                                var element = elements.First();
                                string mask = m.TransformationRule.Mask;
                                // 1 - 1
                                // x -> z1,z2,z3 (split)
                                List<string> result = transform(element.Value, m.TransformationRule);

                                if (result.Count == 1) // 1 - 1
                                {
                                    mask = setOrReplace(mask, result, m.Source.Name);
                                    tmp.Add(mask);
                                }
                                else // x -> z1,z2,z3 (split)
                                {
                                    if (string.IsNullOrEmpty(mask)) tmp.AddRange(result);
                                    else
                                    {
                                        //ToDo Add mask
                                        foreach (string r in result)
                                        {
                                            mask = setOrReplace(mask, new List<string>() { r }, m.Source.Name);
                                            tmp.Add(mask);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // x1,x2,x3 -> z (join)

                                foreach (var element in elements)
                                {
                                    tmp.AddRange(transform(element.Value, m.TransformationRule));
                                }
                            }
                        }
                        else if (m.Source.Type.Equals(LinkElementType.Default))
                        {
                            string mask = m.TransformationRule.Mask;
                            tmp.Add(mask);
                        }
                    }
                }
                // x,y to z (combination merge)
                // x1,x2, y to z list of  enties
                // if multiply mappings to the same source, it is a merge
                else
                {
                    // get all parent ids to collection mappings that belongs togehter
                    // if they belong to one parent, together then they should merge
                    IEnumerable<long> parentIds = mappings.Select(m => m.Parent.Id).Distinct();

                    foreach (int parentId in parentIds)
                    {
                        string mask = "";

                        //load all mapping that belongs to the parent mapping with id -> parentId
                        IEnumerable<Entities.Mapping.Mapping> tmpMappingsSubset = mappings.Where(m => m.Parent.Id.Equals(parentId));

                        // if there is only one mapping belong to the parent, add each xelement as one entry
                        if (tmpMappingsSubset.Count() == 1)
                        {
                            var m = tmpMappingsSubset.First();

                            mask = "";

                            if (m.Source.Type.Equals(LinkElementType.MetadataAttributeUsage) ||
                                m.Source.Type.Equals(LinkElementType.MetadataNestedAttributeUsage))
                            {
                                IEnumerable<XElement> elements = getXElementsFromAMapping(m, metadata);

                                //the elements are the result of one mapping
                                foreach (var element in elements)
                                {
                                    mask = m.TransformationRule.Mask;
                                    List<string> regExResultList = transform(element.Value, m.TransformationRule);
                                    string placeHolderName = m.Source.Name;

                                    mask = setOrReplace(mask, regExResultList, placeHolderName);
                                    tmp.Add(mask);
                                }
                            }
                        }
                        else
                        {
                            mask = "";

                            foreach (var m in tmpMappingsSubset)
                            {
                                if (string.IsNullOrEmpty(mask)) mask = tmpMappingsSubset.FirstOrDefault().TransformationRule.Mask;

                                if (m.Source.Type.Equals(LinkElementType.MetadataAttributeUsage) ||
                                    m.Source.Type.Equals(LinkElementType.MetadataNestedAttributeUsage))
                                {
                                    IEnumerable<XElement> elements = getXElementsFromAMapping(m, metadata);

                                    //the elements are the result of one mapping
                                    foreach (var element in elements)
                                    {
                                        List<string> regExResultList = transform(element.Value, m.TransformationRule);
                                        string placeHolderName = m.Source.Name;

                                        mask = setOrReplace(mask, regExResultList, placeHolderName);
                                    }
                                }
                            }

                            tmp.Add(mask);
                        }
                    }
                }

                return tmp;
            }
            finally
            {
                mappingManager.Dispose();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="targetElementId"></param>
        /// <param name="targetType"></param>
        /// <param name="sourceRootId"></param>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static List<XElement> GetXElementFromMetadata(long targetElementId, LinkElementType targetType,
            long sourceRootId, XDocument metadata)
        {
            //grab values from metadata where targetelementid and targetType is mapped
            // e.g. get title from metadata

            MappingManager mappingManager = new MappingManager();

            try
            {
                List<XElement> tmp = new List<XElement>();

                var mappings = mappingManager.GetMappings().Where(m =>
                    m.Target.ElementId.Equals(targetElementId) &&
                    m.Target.Type.Equals(targetType) &&
                    getRootMapping(m) != null &&
                    getRootMapping(m).Source.ElementId.Equals(sourceRootId) &&
                    getRootMapping(m).Source.Type == LinkElementType.MetadataStructure &&
                    m.Level.Equals(2));

                // possinle cases                   mapping count
                // 1 - 1                            1
                // x,y to z (combination merge)     2
                // x -> z1,z2,z3 (split)            1
                // x1,x2,x3 -> z (merge)            1

                if (mappings.Count() == 1)
                {
                    //possible cases =
                    // 1 - 1
                    // x -> z1,z2,z3 (split)
                    // x1,x2,x3 -> z (join)

                    Entities.Mapping.Mapping m = mappings.FirstOrDefault();

                    if (m != null &&
                        (m.Source.Type.Equals(LinkElementType.MetadataAttributeUsage) ||
                         m.Source.Type.Equals(LinkElementType.MetadataNestedAttributeUsage)))
                    {
                        IEnumerable<XElement> elements = getXElementsFromAMapping(m, metadata);

                        if (elements.Count() == 1)
                        {
                            tmp.Add(elements.First());
                        }
                        else
                        {
                            // x1,x2,x3 -> z (join)
                            foreach (var element in elements)
                            {
                                tmp.Add(element);
                            }
                        }
                    }
                }
                // x,y to z (combination merge)
                // if multiply mappings to the same source, it is a merge
                else
                {
                    // all mappings that have the same parent mapping should be handelt together
                    IEnumerable<long> parentIds = mappings.Select(m => m.Parent.Id).Distinct();

                    foreach (int parentId in parentIds)
                    {
                        string mask = "";

                        //load all maaping that belongs to the parent mapping with id -> parentId
                        IEnumerable<Entities.Mapping.Mapping> tmpMappings = mappings.Where(m => m.Parent.Id.Equals(parentId));

                        foreach (var m in tmpMappings)
                        {
                            if (string.IsNullOrEmpty(mask)) mask = mappings.FirstOrDefault().TransformationRule.Mask;

                            if (m.Source.Type.Equals(LinkElementType.MetadataAttributeUsage) ||
                                m.Source.Type.Equals(LinkElementType.MetadataNestedAttributeUsage))
                            {
                                IEnumerable<XElement> elements = getXElementsFromAMapping(m, metadata);

                                //the elements are the result of one mapping
                                foreach (var element in elements)
                                {
                                    tmp.Add(element);
                                }
                            }
                        }
                    }
                }

                return tmp;
            }
            finally
            {
                mappingManager.Dispose();
            }
        }

        private static IEnumerable<XElement> getXElementsFromAMapping(Entities.Mapping.Mapping m, XDocument metadata)
        {
            Dictionary<string, string> AttrDic = new Dictionary<string, string>();
            List<XElement> elements = new List<XElement>();

            //get parent elemenet with parent mapping
            var parentMapping = m.Parent;
            // if a mapping is linked direct to a simple attribute, then lvl 1&2 are  connected to the same linkelement
            bool directSimpleMapping = false;
            // if the parent and the current source are equals, then the child can loaded directly
            if (parentMapping.Source.Id.Equals(m.Source.Id)) directSimpleMapping = true;

            if (directSimpleMapping)
            {
                AttrDic = new Dictionary<string, string>();
                AttrDic.Add("id", m.Source.ElementId.ToString());
                AttrDic.Add("name", m.Source.Name);
                AttrDic.Add("type", "MetadataAttributeUsage");

                //the usage is the head node of a attr, if there are more then one, there are listed inside of the usage
                // always :  usage/type/value
                // if cardianlity is more then 1 its listed like usage/type[0]/value, usage/type[n]/value
                // in this case the childs of the usage is needed
                var usages = XmlUtility.GetXElementsByAttribute(AttrDic, metadata);
                foreach (var u in usages)
                {
                    elements.AddRange(u.Elements());
                }
            }
            else
            {
                AttrDic.Add("id", parentMapping.Source.ElementId.ToString());
                AttrDic.Add("name", parentMapping.Source.Name);

                //get parents from metadata
                IEnumerable<XElement> parents = XmlUtility.GetXElementsByAttribute(AttrDic, metadata);

                foreach (var parent in parents)
                {
                    //sett attrs for the child
                    AttrDic = new Dictionary<string, string>();
                    AttrDic.Add("id", m.Source.ElementId.ToString());
                    AttrDic.Add("name", m.Source.Name);
                    AttrDic.Add("type", "MetadataAttributeUsage");

                    //the usage is the head node of a attr, if there are more then one, there are listed inside of the usage
                    // always :  usage/type/value
                    // if cardianlity is more then 1 its listed like usage/type[0]/value, usage/type[n]/value
                    // in this case the childs of the usage is needed
                    var usages = XmlUtility.GetXElementsByAttribute(AttrDic, parent);
                    foreach (var u in usages)
                    {
                        elements.AddRange(u.Elements());
                    }
                }
            }
            return elements;
        }

        #endregion GET FROM Specific MetadataStructure // Source

        #region Helpers

        private static Entities.Mapping.Mapping getRootMapping(Entities.Mapping.Mapping mapping)
        {
            if (mapping.Parent == null) return mapping;

            return getRootMapping(mapping.Parent);
        }

        private static List<string> transform(string value, TransformationRule transformationRule)
        {
            List<string> tmp = new List<string>();
            if (!string.IsNullOrEmpty(transformationRule.RegEx))
            {
                Regex r = new Regex(transformationRule.RegEx, RegexOptions.IgnoreCase);

                // Match the regular expression pattern against a text string.
                MatchCollection matchCollection = r.Matches(value);

                foreach (var match in matchCollection.Cast<Match>())
                {
                    if (match.Success)
                    {
                        foreach (var groupElement in match.Groups)
                        {
                            tmp.Add(groupElement.ToString().Trim());
                        }
                    }
                }
            }
            else
            {
                tmp.Add(value);
            }

            return tmp;
        }

        private static string setOrReplace(string mask, List<string> replacers, string attrName)
        {
            // in case of haven a boolean value in the metadata, true or false is not the value that should came out.
            // the name of that what is checked is important here
            for (int i = 0; i < replacers.Count(); i++)
            {
                string value = replacers[i];
                if (value.ToLower().Equals("true"))
                {
                    value = attrName;
                    replacers[i] = value;
                }
                else
                if (value.ToLower().Equals("false")) replacers.Remove(value);
            }

            if (replacers != null && replacers.Any())
            {
                if (!string.IsNullOrEmpty(mask))
                {
                    foreach (var r in replacers)
                    {
                        string completePlaceHolderName = attrName + "[" + replacers.IndexOf(r) + "]";

                        if (mask.Contains(completePlaceHolderName))
                            mask = mask.Replace(completePlaceHolderName, r);
                        else
                        {
                            if (!string.IsNullOrEmpty(r))
                                mask = string.IsNullOrEmpty(mask) ? mask = r : mask += " " + r;
                        }
                    }

                    return mask;
                }

                if (replacers.Count > 1) return string.Join(", ", replacers.ToArray());

                return replacers[0];
            }

            return "";
        }

        #endregion Helpers
    }
}