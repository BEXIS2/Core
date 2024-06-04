using BExIS.Dim.Helpers.Configurations;
using System;
using System.Collections.Generic;
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

        public DataCiteDOICredentials GetDataCiteDOICredentials()
        {
            try
            {
                return _settings.GetValueByKey<DataCiteDOICredentials>("dataCiteDOICredentials");
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<DataCiteDOIMapping> GetDataCiteDOIMappings()
        {
            try
            {
                return _settings.GetValueByKey<List<DataCiteDOIMapping>>("dataCiteDOIMappings");
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<DataCiteDOIPlaceholder> GetDataCiteDOIPlaceholders()
        {
            try
            {
                return _settings.GetValueByKey<List<DataCiteDOIPlaceholder>>("dataCiteDOIPlaceholders");
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}