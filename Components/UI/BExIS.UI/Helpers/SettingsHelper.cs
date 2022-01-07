using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;

namespace BExIS.UI.Helpers
{
    public class SettingsHelper
    {
        string _filePath = "";
        string _moduleId = "";

        public SettingsHelper(string moduleId)
        {
            if (moduleId.ToLower() == "shell")
                _filePath = Path.Combine(AppConfiguration.WorkspaceGeneralRoot, "General.Settings.xml");
            else
                _filePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath(moduleId), moduleId+".Settings.xml");
        }

        public bool KeyExist(string key)
        {
            XDocument settings = XDocument.Load(_filePath);
            XElement element = XmlUtility.GetXElementByAttribute("entry", "key", key, settings);

            return element != null?true:false;
        }

        public string GetValue(string key)
        {
            XDocument settings = XDocument.Load(_filePath);
            XElement element = XmlUtility.GetXElementByAttribute("entry", "key", key, settings);

            string value = "";
            value =  element.Attribute("value")?.Value;

            return value;
        }

        public List<KeyValuePair<string,string>> GetList(string value)
        {
            XDocument settings = XDocument.Load(_filePath);
            XElement element = XmlUtility.GetXElementByAttribute("list", "value", value, settings);

            List<KeyValuePair<string, string>> tmp = new List<KeyValuePair<string, string>>();

            if (element != null)
            {
                foreach (var item in element.Elements())
                {
                    string k = item.Attribute("key").Value;
                    string v = item.Attribute("value").Value;

                    KeyValuePair<string, string> kvp = new KeyValuePair<string, string>(k,v);
                    tmp.Add(kvp);
                }
            }

            return tmp;
        }

        #region looad and update

        public XDocument Load()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    return XDocument.Load(_filePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("settings file of Module (" + _moduleId + ") does not exist", ex);
            }

            return null;
        }

        public string AsJson()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    XDocument modulsettings = Load();
                    return JsonConvert.SerializeXNode(modulsettings);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("settings convertion of Module (" + _moduleId + ") failed", ex);
            }

            return null;
        }

        public bool Update(string json)
        {
            try
            {
                XDocument modulesettings = JsonConvert.DeserializeXNode(json);
                modulesettings.Save(_filePath);

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("settings update of Module (" + _moduleId + ") failed", ex);
            }

            return false;
        }

        #endregion
    }
}