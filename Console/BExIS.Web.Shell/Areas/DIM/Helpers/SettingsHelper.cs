using BExIS.Dim.Helpers.DataCite;
using BExIS.Dim.Helpers.Models;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Helpers;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Dim.UI.Helpers
{
    public class SettingsHelper
    {
        private ModuleSettings _settings;
        private string filePath = "";

        public SettingsHelper()
        {
            _settings = ModuleManager.GetModuleSettings("dim");
        }

        public DataCiteCredentials GetDataCiteCredentials()
        {
            try
            {
                return _settings.GetValueByKey<DataCiteCredentials>("dataCiteDOICredentials");
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<DataCiteMapping> GetDataCiteMappings()
        {
            try
            {
                return _settings.GetValueByKey<List<DataCiteMapping>>("dataCiteDOIMappings");
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<DataCitePlaceholder> GetDataCitePlaceholders()
        {
            try
            {
                return _settings.GetValueByKey<List<DataCitePlaceholder>>("dataCiteDOIPlaceholders");
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}