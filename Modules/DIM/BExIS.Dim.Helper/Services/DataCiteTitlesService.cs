using BExIS.Dim.Entities.Mapping;
using BExIS.Dim.Helpers.Mapping;
using BExIS.Dlm.Entities.Data;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vaelastrasz.Library.Models.DataCite;

namespace BExIS.Dim.Helpers.Services
{
    public class DataCiteTitlesService
    {
        public List<DataCiteTitle> GetTitles(DatasetVersion datasetVersion, string key)
        {
            var titles = new List<DataCiteTitle>();

            if (datasetVersion == null)
                return titles;

            var values = MappingUtils.GetValuesFromMetadata((int)Enum.Parse(typeof(Key), key), LinkElementType.Key, datasetVersion.Dataset.MetadataStructure.Id, XmlUtility.ToXDocument(datasetVersion.Metadata));
            foreach (var value in values)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var title = new DataCiteTitle(value);
                    if (title != null)
                        titles.Add(title);
                }
            }

            return titles;
        }
    }
}
