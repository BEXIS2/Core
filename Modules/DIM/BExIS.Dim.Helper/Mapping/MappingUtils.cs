using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using BExIS.Xml.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Vaiona.Persistence.Api;

namespace BExIS.Dim.Helpers.Mapping
{
    public class MappingUtils
    {

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
        public static List<string> GetAllMatchesInSystem(long targetElementId, LinkElementType targetType,
            string value = "")
        {

            //get all mapppings where target is mapped
            // LinkElementType.PartyCustomType is set because of the function name
            // all mapped attributes are LinkElementType.PartyCustomType in this case
            using (IUnitOfWork uow = (new object()).GetUnitOfWork())
            {
                List<string> tmp = new List<string>();
                var mappings = uow.GetReadOnlyRepository<BExIS.Dim.Entities.Mapping.Mapping>().Get() // this get is here because the expression is not supported by NH!
                    .Where(m =>
                        m.Target.ElementId.Equals(targetElementId) &&
                        m.Target.Type.Equals(targetType) &&
                        m.Source.Type.Equals(LinkElementType.PartyCustomType)
                    ).ToList();
                tmp = getAllValuesFromSystem(mappings, value);
                return tmp;
            }

            /*
         *e.g. 
         * Metadata Attr Usage -> MicroAgent/Name -> entering "David Blaa"
         * 
         * linkt to 
         * 
         * Person/FirstName     David
         * Person/SecondName    Blaa
         * 
         * 
         * => all mappings know must be only the Person/FirstName & Person/SecondName
         */


        }

        /// <summary>
        /// get all values from systen parties values
        /// 
        /// </summary>
        /// <param name="mappings"></param>
        /// <returns></returns>
        private static List<string> getAllValuesFromSystem(IEnumerable<Entities.Mapping.Mapping> mappings, string value)
        {
            MappingManager _mappingManager = new MappingManager();
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            PartyManager partyManager = new PartyManager();

            List<string> tmp = new List<string>();

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
                    parties = partyManager.PartyRepository.Get().Where(p => p.PartyType.Equals(partyType)).ToList();
                }

                if (parties != null)
                    foreach (var p in parties)
                    {
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


                        }

                        if (mask.ToLower().Contains(value.ToLower()))
                            tmp.Add(mask);
                    }
            }

            return tmp;
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
            if (transformationRule.RegEx != null)
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
            foreach (var r in replacers)
            {
                string completePlaceHolderName = attrName + "[" + replacers.IndexOf(r) + "]";

                if (mask.Contains(completePlaceHolderName))
                    mask = mask.Replace(completePlaceHolderName, r);
                else
                    mask += r;
            }

            return mask;
        }

        #endregion

    }
}
