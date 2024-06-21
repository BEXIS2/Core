using BExIS.Dim.Entities.Publications;
using BExIS.Dim.Helpers.Models;
using BExIS.Dlm.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Vaelastrasz.Library.Models;

namespace BExIS.Dim.Helpers.Vaelastrasz
{
    public static class VaelastraszExtensions
    {
        public static CreateDataCiteModel UpdateCreateDataCiteModel(this CreateDataCiteModel model, List<VaelastraszConfigurationItem> mappings, Dictionary<string, string> placeholders)
        {
            foreach (var mapping in mappings)
            {
                var replacement = placeholders.Aggregate(mapping.Value, (current, value) => current.Replace(value.Key, value.Value));

                switch (mapping.Name)
                {
                    #region URL
                    case "URL":

                        model.Data.Attributes.URL = $"{HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority)}/ddm/Data/ShowData/{replacement}";
                        break;
                    #endregion

                    #region Version
                    case "Version":

                        model.Data.Attributes.Version = replacement;
                        break;
                    #endregion

                    default:
                        break;

                }
            }

            return model;
        }

        public static string GetVersion(DatasetVersion datasetVersion, string type, string value)
        {
            string version = null;

            switch (type)
            {
                case "Property":

                    switch (value)
                    {
                        case "Id":

                            version = Convert.ToString(datasetVersion.Id);
                            break;

                        case "Name":

                            version = datasetVersion.VersionName;
                            break;

                        case "Number":

                            version = Convert.ToString(datasetVersion.VersionNo);
                            break;
                    }

                    break;

                default:
                    break;
            }

            return version;
        }
    }
}
