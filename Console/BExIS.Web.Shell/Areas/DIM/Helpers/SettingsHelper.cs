﻿using BExIS.Dim.Helpers.Vaelastrasz;
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

        public Dictionary<string, string> GetDataCiteMappings()
        {
            try
            {
                return _settings.GetValueByKey<Dictionary<string, string>> ("dataciteMappings");
            }
            catch (Exception ex)
            {
                return new Dictionary<string, string>(); ;
            }
        }

        public Dictionary<string, string> GetDataCitePlaceholders()
        {
            try
            {
                return _settings.GetValueByKey<Dictionary<string, string>>("datacitePlaceholders");
            }
            catch (Exception ex)
            {
                return new Dictionary<string, string>();
            }
        }
    }
}