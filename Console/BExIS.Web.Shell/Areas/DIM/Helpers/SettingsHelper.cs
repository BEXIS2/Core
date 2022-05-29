using BExIS.Modules.Dim.UI.Models;
using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Dim.UI.Helpers
{
    public class SettingsHelper
    {
        string filePath = "";

        public SettingsHelper()
        {
            filePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DIM"), "Dim.Settings.xml");
        }

        public bool KeyExist(string key)
        {
            XDocument settings = XDocument.Load(filePath);
            XElement element = XmlUtility.GetXElementByAttribute("entry", "key", key.ToLower(), settings);

            return element != null ? true : false;
        }

        public string GetValue(string key)
        {
            XDocument settings = XDocument.Load(filePath);
            XElement element = XmlUtility.GetXElementByAttribute("entry", "key", key.ToLower(), settings);

            string value = "";
            value = element.Attribute("value")?.Value;

            return value;
        }

        public List<DataCiteMapping> GetMappings()
        {
            XDocument settings = XDocument.Load(filePath);
            List<XElement> mappings = settings.Elements("mappings").ToList();

            return mappings.Select(m => new DataCiteMapping(m.Attribute("key").Value, m.Attribute("type").Value, m.Attribute("value").Value, m.Attribute("party")?.Value)).ToList();
        }
    }
}