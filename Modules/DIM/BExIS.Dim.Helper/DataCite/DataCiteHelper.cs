using BExIS.Dlm.Entities.Data;
using BExIS.Dlm.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Dim.Helpers.DataCite
{
    public class DataCiteHelper
    {
        public Dictionary<string, string> CreatePlaceholders(DatasetVersion datasetVersion, List<DataCitePlaceholder> placeholders)
        {
            var _placeholders = new Dictionary<string, string>();

            foreach (var placeholder in placeholders)
            {
                switch (placeholder.Key)
                {
                    case "DatasetId":

                        _placeholders.Add("{DatasetId}", Convert.ToString(datasetVersion.Dataset.Id));
                        break;

                    case "VersionId":

                        _placeholders.Add("{VersionId}", Convert.ToString(datasetVersion.Id));
                        break;

                    case "VersionName":

                        _placeholders.Add("{VersionName}", datasetVersion.VersionName);
                        break;

                    case "VersionNumber":

                        using (var datasetManager = new DatasetManager())
                        {
                            var versionNumber = datasetManager.GetDatasetVersionNr(datasetVersion);
                            _placeholders.Add("{VersionNumber}", Convert.ToString(versionNumber));
                        }
                        break;

                    default:
                        break;
                }
            }

            return _placeholders;
        }

    }
}
