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

        public Dictionary<string, string> GetVaelastraszMappings()
        {
            try
            {
                return _settings.GetValueByKey< Dictionary<string, string>> ("vaelastraszMappings");
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public Dictionary<string, string> GetVaelastraszPlaceholders()
        {
            try
            {
                return _settings.GetValueByKey<Dictionary<string, string>>("vaelastraszPlaceholders");
            }
            catch (Exception ex)
            {
                return new Dictionary<string, string>();
            }
        }
    }
}