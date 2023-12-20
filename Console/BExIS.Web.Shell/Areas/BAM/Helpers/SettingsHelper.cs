using BExIS.Modules.Bam.UI.Configurations;
using BExIS.Modules.Bam.UI.Models;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.Modules.Bam.UI.Helpers
{
    public class SettingsHelper
    {
        private ModuleSettings _settings;
        private string filePath = "";

        public SettingsHelper()
        {
            _settings = ModuleManager.GetModuleSettings("bam");
        }

        public List<AccountPartyRelationshipType> GetAccountPartyRelationshipTypes()
        {
            try
            {
                return _settings.GetValueByKey<List<AccountPartyRelationshipType>>("AccountPartyRelationshipTypes");
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public string GetValue(string key)
        {
            XDocument settings = XDocument.Load(filePath);
            XElement element = XmlUtility.GetXElementByAttribute("entry", "name", key.ToLower(), settings);

            string value = "";
            value = element.Attribute("value")?.Value;

            return value;
        }

        public bool KeyExist(string key)
        {
            XDocument settings = XDocument.Load(filePath);
            XElement element = XmlUtility.GetXElementByAttribute("entry", "name", key.ToLower(), settings);

            return element != null ? true : false;
        }
    }
}