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
                using (IUnitOfWork uow = (new object()).GetUnitOfWork())
                {
                    var mappings = uow.GetReadOnlyRepository<BExIS.Dim.Entities.Mapping.Mapping>().Get() // this get is here because the expression is not supported by NH!
                        .Where(m =>
                            m.Target.ElementId.Equals(targetId) &&
                            m.Target.Type.Equals(targetType) &&
                            m.Source.ElementId.Equals(sourceId) &&
                            m.Source.Type.Equals(sourceType)
                        ).ToList();


                    return mappings.Any();
                }
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
                using (IUnitOfWork uow = (new object()).GetUnitOfWork())
                {
                    var mappings = uow.GetReadOnlyRepository<BExIS.Dim.Entities.Mapping.Mapping>().Get() // this get is here because the expression is not supported by NH!
                        .Where(m =>
                            m.Target.ElementId.Equals(targetId) &&
                            m.Target.Type.Equals(targetType) &&
                            m.Source.ElementId.Equals(sourceId) &&
                            m.Source.Type.Equals(sourceType)
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

        public static List<Entities.Mapping.Mapping> GetMappings(long parentMappingId)
        {
            try
            {
                using (IUnitOfWork uow = (new object()).GetUnitOfWork())
                {
                    var mappings = uow.GetReadOnlyRepository<BExIS.Dim.Entities.Mapping.Mapping>().Get() // this get is here because the expression is not supported by NH!
                        .Where(m =>
                            m.Parent.Id.Equals(parentMappingId)
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

        public static List<Entities.Mapping.Mapping> GetMappingsWhereSource(long sourceId, LinkElementType sourceType)
        {
            try
            {
                using (IUnitOfWork uow = (new object()).GetUnitOfWork())
                {
                    var mappings = uow.GetReadOnlyRepository<BExIS.Dim.Entities.Mapping.Mapping>().Get() // this get is here because the expression is not supported by NH!
                        .Where(m =>
                            m.Source.ElementId.Equals(sourceId) &&
                            m.Source.Type.Equals(sourceType)
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

        public static List<Entities.Mapping.Mapping> GetMappingsWhereSource(long sourceId, LinkElementType sourceType, int level)
        {
            try
            {
                using (IUnitOfWork uow = (new object()).GetUnitOfWork())
                {
                    var mappings = uow.GetReadOnlyRepository<BExIS.Dim.Entities.Mapping.Mapping>().Get() // this get is here because the expression is not supported by NH!
                        .Where(m =>
                            m.Source.ElementId.Equals(sourceId) &&
                            m.Source.Type.Equals(sourceType) &&
                            m.Level == level
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

        #endregion

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
                using (IUnitOfWork uow = (new object()).GetUnitOfWork())
                {
                    List<MappingPartyResultElemenet> tmp = new List<MappingPartyResultElemenet>();
                    var mappings = uow.GetReadOnlyRepository<BExIS.Dim.Entities.Mapping.Mapping>().Get() // this get is here because the expression is not supported by NH!
                        .Where(m =>
                            m.Target.ElementId.Equals(targetElementId) &&
                            m.Target.Type.Equals(targetType) &&
                            m.Source.Type.Equals(LinkElementType.PartyCustomType)
                        ).ToList();
                    tmp = getAllValuesFromSystem(mappings, value);
                    return tmp;
                }
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
                using (IUnitOfWork uow = (new object()).GetUnitOfWork())
                {
                    var mappings = uow.GetReadOnlyRepository<BExIS.Dim.Entities.Mapping.Mapping>().Get() // this get is here because the expression is not supported by NH!
                        .Where(m =>
                            m.Target.ElementId.Equals(targetId) &&
                            m.Target.Type.Equals(targetType) &&
                            m.Parent != null &&
                            m.Parent.Source.Type.Equals(LinkElementType.PartyType)
                        ).ToList();


                    return mappings.Any();
                }
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
                using (IUnitOfWork uow = (new object()).GetUnitOfWork())
                {
                    var mappings = uow.GetReadOnlyRepository<BExIS.Dim.Entities.Mapping.Mapping>().Get() // this get is here because the expression is not supported by NH!
                        .Where(m =>
                            m.Target.ElementId.Equals(targetId) &&
                            m.Target.Type.Equals(targetType) &&
                            m.Parent != null &&
                            m.Parent.Source.ElementId.Equals(partyId)
                        ).ToList();


                    return mappings.Any();
                }
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
            MappingManager _mappingManager = new MappingManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            PartyManager partyManager = new PartyManager();

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
                    parties = partyManager.PartyRepository.Get().Where(p => p.PartyType.Equals(partyType)).ToList();
                }
                else
                {
                    partyType = partyTypeManager.PartyTypeRepository.Get(parentTypeId);
                    parties = partyManager.PartyRepository.Get().Where(p => p.PartyType.Id.Equals(parentTypeId)).ToList();
                }

                if (parties != null)
                    foreach (var p in parties)
                    {
                        MappingPartyResultElemenet resultObject = new MappingPartyResultElemenet();
                        resultObject.PartyId = p.Id;

                        mask = mappings.FirstOrDefault().TransformationRule.Mask;

                        foreach (var mapping in mappings)
                        {
                            long attributeId = mapping.Source.ElementId;

                            PartyCustomAttributeValue attrValue =
                                partyManager.PartyCustomAttributeValueRepository.Get()
                                    .Where(v => v.CustomAttribute.Id.Equals(attributeId) && v.Party.Id.Equals(p.Id))
                                    .FirstOrDefault();

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
                    var mappings = uow.GetReadOnlyRepository<BExIS.Dim.Entities.Mapping.Mapping>().Get() // this get is here because the expression is not supported by NH!
                            .Where(m =>
                                m.Target.ElementId.Equals(targetElementId) &&
                                m.Target.Type.Equals(targetElementType) &&
                                m.Source.Type.Equals(LinkElementType.PartyCustomType)
                            ).ToList();


                    if (mappings.Any())
                    {
                        string mask = "";
                        mask = mappings.FirstOrDefault().TransformationRule.Mask;

                        foreach (var mapping in mappings)
                        {
                            long attributeId = mapping.Source.ElementId;

                            PartyCustomAttributeValue attrValue =
                                partyManager.PartyCustomAttributeValueRepository.Get()
                                    .Where(v => v.CustomAttribute.Id.Equals(attributeId) && v.Party.Id.Equals(partyid))
                                    .FirstOrDefault();

                            //if (attrValue != null)
                            //{

                            List<string> regExResultList = transform(attrValue.Value, mapping.TransformationRule);
                            string placeHolderName = attrValue.CustomAttribute.Name;

                            mask = setOrReplace(mask, regExResultList, placeHolderName);
                            //}
                            //else
                            //{
                            //    mask = "";
                            //}

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
                    var mappings = uow.GetReadOnlyRepository<BExIS.Dim.Entities.Mapping.Mapping>().Get() // this get is here because the expression is not supported by NH!
                        .Where(m =>
                            m.Target.ElementId.Equals(targetElementId) &&
                            m.Target.Type.Equals(targetElementType) &&
                            m.Source.Type.Equals(LinkElementType.PartyCustomType) &&
                            m.Parent != null
                        );

                    foreach (var mapping in mappings)
                    {
                        if (mapping != null)
                        {
                            PartyCustomAttribute pca = uow.GetReadOnlyRepository<PartyCustomAttribute>().Get(mapping.Source.ElementId);
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


        /// <summary>
        /// Check if there is a mapping to system key nodes
        /// return true if yes
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
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
                // start with element at target
                using (IUnitOfWork uow = (new object()).GetUnitOfWork())
                {
                    var mappings = uow.GetReadOnlyRepository<BExIS.Dim.Entities.Mapping.Mapping>().Get() // this get is here because the expression is not supported by NH!
                        .Where(m =>
                            m.Target.ElementId.Equals(id) &&
                            m.Target.Type.Equals(type) &&
                            100 <= m.Source.ElementId && m.Source.ElementId <= 106 &&
                            m.Source.Type.Equals(LinkElementType.Key)
                        ).ToList();


                    return mappings.Any();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }



        #endregion

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


                foreach (var m in mappings)
                {

                    Dictionary<string, string> AttrDic = new Dictionary<string, string>();

                    if (m.Source.Type.Equals(LinkElementType.MetadataAttributeUsage) ||
                        m.Source.Type.Equals(LinkElementType.MetadataNestedAttributeUsage))
                    {
                        AttrDic.Add("id", m.Source.ElementId.ToString());
                        AttrDic.Add("name", m.Source.Name);
                        AttrDic.Add("type", "MetadataAttributeUsage");

                        //find sourceelement in xmldocument
                        IEnumerable<XElement> elements = XmlUtility.GetXElementsByAttribute(AttrDic, metadata);

                        foreach (var element in elements)
                        {
                            tmp.Add(element.Value);
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


        #endregion


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
                Match m = r.Match(value);

                if (m.Success)
                {
                    foreach (var groupElement in m.Groups)
                    {
                        tmp.Add(groupElement.ToString());
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

        #endregion

    }
}
