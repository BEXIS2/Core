using BExIS.Xml.Helpers;
using System.Collections.Generic;
using System;
using System.IO;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;
using BExIS.Modules.Dcm.UI.Models.View;

namespace BExIS.Modules.Dcm.UI.Helpers
{
    public class DDMSettingsHelper
    {
        private ModuleSettings _settings;
        private string filePath = "";

        public DDMSettingsHelper()
        {
            _settings = ModuleManager.GetModuleSettings("ddm");
        }

        public bool KeyExist(string key)
        {
            XDocument settings = XDocument.Load(filePath);
            XElement element = XmlUtility.GetXElementByAttribute("entry", "key", key.ToLower(), settings);

            return element != null ? true : false;
        }

        //public string GetValue(string key)
        //{
        //    XDocument settings = XDocument.Load(filePath);
        //    XElement element = XmlUtility.GetXElementByAttribute("entry", "key", key.ToLower(), settings);

        //    string value = "";
        //    if (element != null)
        //    {
        //        value = element.Attribute("value")?.Value;
        //    }

        //    return value;
        //}

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

        public CitationSettings GetCitationSettings()
        {
            try
            {
                return _settings.GetValueByKey<CitationSettings>("citationSettings");
            }
            catch (Exception ex)
            {
                return new CitationSettings();
            }
        }
    }
}