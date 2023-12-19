using BExIS.Dim.Helpers.Models;
using BExIS.Modules.Dim.UI.Configurations;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public DataCiteDOICredentials GetDataCiteDOICredentials()
        {
            var entry = _settings.GetValueByKey("dataCiteDOICredentials") as Entry;

            return JsonConvert.DeserializeObject<DataCiteDOICredentials>(entry.Value);
        }

        public DataCiteDOIMappings GetDataCiteDOIMappings()
        {
            var entry = _settings.GetValueByKey("dataCiteDOIMappings") as Entry;

            return JsonConvert.DeserializeObject<DataCiteDOIMappings>(entry.Value);
        }

        public DataCiteDOIPlaceholders GetDataCiteDOIPlaceholders()
        {
            var entry = _settings.GetValueByKey("dataCiteDOIPlaceholders") as Entry;

            return JsonConvert.DeserializeObject<DataCiteDOIPlaceholders>(entry.Value);
        }

        public string GetDataCiteProperty(string propertyName)
        {
            XDocument settings = XDocument.Load(filePath);
            List<XElement> datacite = settings.Elements("datacite").ToList();

            return datacite.Select(m => m.Attribute(propertyName).Value).FirstOrDefault();
        }

        public List<DataCiteDOISettingsItem> GetDataCiteSettings(string name)
        {
            XDocument settings = XDocument.Load(filePath);
            List<XElement> mappings = XmlUtility.GetXElementByNodeName(name, settings).Descendants().ToList();

            //return mappings.Select(m => new DataCiteMapping(m.Attribute("name")?.Value, m.Attribute("type")?.Value, m.Attribute("value")?.Value, m.Attribute("partyAttributes")?.Value.Split(';').Select(part => part.Split('=')).Where(part => part.Length == 2).ToDictionary(sp => sp[0], sp => sp[1]))).ToList();
            return mappings.Select(m => new DataCiteDOISettingsItem(m.Attribute("name")?.Value, m.Attribute("type")?.Value, m.Attribute("value")?.Value, m.Attribute("extra")?.Value)).ToList();
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