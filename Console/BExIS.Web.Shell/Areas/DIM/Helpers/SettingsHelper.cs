using BExIS.Dim.Helpers.Vaelastrasz;
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

        public List<VaelastraszMapping> GetVaelastraszMappings()
        {
            try
            {
                return _settings.GetValueByKey<List<VaelastraszMapping>>("vaelastraszMappings");
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<VaelastraszPlaceholder> GetVaelastraszPlaceholders()
        {
            try
            {
                return _settings.GetValueByKey<List<VaelastraszPlaceholder>>("vaelastraszPlaceholders");
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}