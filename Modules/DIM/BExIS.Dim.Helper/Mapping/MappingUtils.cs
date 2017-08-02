using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Services;
using BExIS.Dlm.Entities.Party;
using BExIS.Dlm.Services.Party;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace BExIS.Dim.Helpers.Mapping
{
    public class MappingUtils
    {

        #region GET FROM SYSTEM

        public static List<string> GetAllMatchesInSystem(long targetElementId, LinkElementType targetType, string value = "")
        {
            long destinationElementRootId = 0;
            LinkElementType destinationType = LinkElementType.System;

            MappingManager _mappingManager = new MappingManager();

            List<string> tmp = new List<string>();

            //getAll mappings
            var mappings = _mappingManager.GetMappings().Where(m =>
                m.Target.ElementId.Equals(targetElementId) &&
                m.Target.Type.Equals(targetType));

            List<Entities.Mapping.Mapping> mappingsForDestiantion = new List<Entities.Mapping.Mapping>();

            //get All mappings for the destination
            foreach (var mapping in mappings)
            {
                LinkElement root = getIdOfRoot(mapping.Source);
                if (root != null && root.ElementId.Equals(destinationElementRootId) &&
                     root.Type.Equals(destinationType))
                    mappingsForDestiantion.Add(mapping);
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


            tmp = getAllValuesFromSystem(mappingsForDestiantion, value);



            return tmp;
        }

        /// <summary>
        /// get all values from systen parties values
        /// 
        /// </summary>
        /// <param name="mappings"></param>
        /// <returns></returns>
        private static List<string> getAllValuesFromSystem(List<Entities.Mapping.Mapping> mappings, string value)
        {
            PartyTypeManager partyTypeManager = new PartyTypeManager();
            PartyManager partyManager = new PartyManager();

            List<string> tmp = new List<string>();

            if (mappings.Any())
            {
                // all mappings belong to the same parent
                long parentTypeId = mappings.FirstOrDefault().Source.Parent.ElementId;

                // all Masks are the same
                string mask = mappings.FirstOrDefault().Target.Mask;

                PartyType partyType = partyTypeManager.Repo.Get(parentTypeId);
                List<Party> parties = partyManager.Repo.Get().Where(p => p.PartyType.Equals(partyType)).ToList();

                foreach (var p in parties)
                {
                    mask = mappings.FirstOrDefault().Target.Mask;

                    foreach (var mapping in mappings)
                    {
                        long attributeId = mapping.Source.ElementId;

                        PartyCustomAttributeValue attrValue =
                            partyManager.RepoCustomAttrValues.Get()
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

        #region GET FROM Specific MetadataStrutcure // Source 


        /// <summary>
        /// Get Value from a target over a source
        /// e.G. you want to have a project name from a metadatStrutcure and start from the system
        /// 
        /// </summary>
        /// <param name="sourceElementId"></param>
        /// <param name="sourceType"></param>
        /// <param name="targetRootId"></param>
        /// <returns></returns>
        public static List<string> GetValuesFromMetadata(long targetElementId, LinkElementType targetType,
            long sourceRootId, XmlDocument metadata)
        {

            List<string> tmp = new List<string>();

            long destinationElementRootId = sourceRootId;
            LinkElementType destinationType = LinkElementType.MetadataStructure;

            MappingManager _mappingManager = new MappingManager();
            //getAll mappings
            var mappings = _mappingManager.GetMappings().Where(m =>
                m.Target.ElementId.Equals(targetElementId) &&
                m.Target.Type.Equals(targetType));

            List<Entities.Mapping.Mapping> mappingsForDestiantion = new List<Entities.Mapping.Mapping>();

            //get All mappings for the destination
            foreach (var mapping in mappings)
            {
                LinkElement root = getIdOfRoot(mapping.Source);
                if (root != null && root.ElementId.Equals(destinationElementRootId) &&
                     root.Type.Equals(destinationType))
                    mappingsForDestiantion.Add(mapping);
            }


            //find sourceelement in xmldocument




            return tmp;
        }


        #endregion


        #region Helpers

        private static LinkElement getIdOfRoot(LinkElement element)
        {
            if (element.Parent == null) return element;

            return getIdOfRoot(element.Parent);
        }

        #endregion

    }
}
