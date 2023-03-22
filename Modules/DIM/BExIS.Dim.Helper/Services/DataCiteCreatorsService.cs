using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Helpers.Mapping;
using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Party;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Vaelastrasz.Library.Models.DataCite;

namespace BExIS.Dim.Helpers.Services
{
    public class DataCiteCreatorsService
    {
        public List<DataCiteCreator> GetCreators(DatasetVersion datasetVersion, string key, string fn = null, string ln = null)
        {
            var creators = new List<DataCiteCreator>();

            if (datasetVersion == null)
                return creators;

            if(!string.IsNullOrEmpty(fn) && !string.IsNullOrEmpty(ln))
            {
                var elements = MappingUtils.GetXElementFromMetadata((int)Enum.Parse(typeof(Key), key), LinkElementType.Key, datasetVersion.Dataset.MetadataStructure.Id, XmlUtility.ToXDocument(datasetVersion.Metadata));
                foreach (var element in elements)
                {
                    var creator = getCreator(element, fn, ln);
                    if (creator != null)
                        creators.Add(creator);
                }
            }
            else
            {
                var values = MappingUtils.GetValuesFromMetadata((int)Enum.Parse(typeof(Key), key), LinkElementType.Key, datasetVersion.Dataset.MetadataStructure.Id, XmlUtility.ToXDocument(datasetVersion.Metadata));

                foreach (var value in values)
                {
                    if(!string.IsNullOrEmpty(value))
                    {
                        var creator = new DataCiteCreator(value, DataCiteCreatorType.Personal);
                        if (creator != null)
                            creators.Add(creator);
                    }
                }

                    return values.Select(a => new DataCiteCreator(a, DataCiteCreatorType.Personal)).ToList();
            }

            return creators;
        }
        
        private DataCiteCreator getCreator(XElement element, string fn, string ln)
        {
            if (element == null)
                return null;

            var partyId = getPartyId(element);

            if (partyId != 0)
            {
                using (var partyManager = new PartyManager())
                {
                    var party = partyManager.GetParty(partyId);

                    if (!string.IsNullOrEmpty(fn) && !string.IsNullOrEmpty(ln))
                    {
                        var firstname = party.CustomAttributeValues.Where(a => a.CustomAttribute.Name == fn).FirstOrDefault()?.Value;
                        var lastname = party.CustomAttributeValues.Where(a => a.CustomAttribute.Name == ln).FirstOrDefault()?.Value;

                        if (firstname != null && lastname != null)
                        {
                            return new DataCiteCreator(firstname, lastname);
                        }
                    }
                }
            }

            return new DataCiteCreator(element.Value, DataCiteCreatorType.Personal);
        }

        private long getPartyId(XElement e)
        {
            if (e.Attributes().Any(x => x.Name.LocalName.Equals("partyid")))
            {
                return Convert.ToInt64(e.Attribute("partyid").Value);
            }
            if (e.Parent != null) return getPartyId(e.Parent);

            return 0;
        }
    }
}
