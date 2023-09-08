using BExIS.Dim.Helpers.Models;
using BExIS.Dlm.Entities.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Vaelastrasz.Library.Models;
using Vaelastrasz.Library.Services;

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
    }
}
