using BExIS.Dim.Helpers.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Vaelastrasz.Library.Models;

namespace BExIS.Dim.Helpers.Extensions
{
    public static class DataCiteDoiExtensions
    {
        public static CreateDataCiteModel UpdateCreateDataCiteModel(this CreateDataCiteModel model, List<DataCiteDOISettingsItem> mappings, Dictionary<string, string> placeholders)
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

                    #endregion URL

                    #region Version

                    case "Version":

                        model.Data.Attributes.Version = replacement;
                        break;

                    #endregion Version

                    default:
                        break;
                }
            }

            return model;
        }
    }
}