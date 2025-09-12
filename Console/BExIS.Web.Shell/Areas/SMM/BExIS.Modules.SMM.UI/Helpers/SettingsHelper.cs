using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Smm.UI.Helpers
{
    public class SettingsHelper
    {
        private ModuleSettings _settings;
        private string filePath = "";

        public SettingsHelper()
        {
            _settings = ModuleManager.GetModuleSettings("Smm");
        }

        public T GetValue<T>(string key) where T : class
        {
            try
            {
                return _settings.GetValueByKey<T>(key);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool KeyExist(string key)
        {
            XDocument settings = XDocument.Load(filePath);
            XElement element = XmlUtility.GetXElementByAttribute("entry", "name", key.ToLower(), settings);

            return element != null ? true : false;
        }
    }
}