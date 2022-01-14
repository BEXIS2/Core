using BExIS.UI.Models;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;

namespace BExIS.UI.Helpers
{
    public class SettingsHelper
    {
        private string _filePath = "";

        private string _filePathJson = "";
        private string _moduleId = "";

        public SettingsHelper(string moduleId)
        {
            _moduleId = moduleId;
            if (moduleId.ToLower() == "shell")
                _filePath = Path.Combine(AppConfiguration.WorkspaceGeneralRoot, "General.Settings.xml");
            else
                _filePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath(moduleId), moduleId + ".Settings.xml");

            if (moduleId.ToLower() == "shell")
                _filePathJson = Path.Combine(AppConfiguration.WorkspaceGeneralRoot, "General.Settings.json");
            else
                _filePathJson = Path.Combine(AppConfiguration.GetModuleWorkspacePath(moduleId), moduleId + ".Settings.json");
        }

        public ModuleSettings LoadSettings()
        {
            if (File.Exists(_filePathJson))
            {
                using (StreamReader file = File.OpenText(_filePathJson))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    ModuleSettings settings = (ModuleSettings)serializer.Deserialize(file, typeof(ModuleSettings));

                    return settings;
                }
            }

            return null;
        }

        public bool KeyExist(string key)
        {
            XDocument settings = XDocument.Load(_filePath);
            XElement element = XmlUtility.GetXElementByAttribute("entry", "key", key, settings);

            return element != null ? true : false;
        }

        public string GetValue(string key)
        {
            XDocument settings = XDocument.Load(_filePath);
            XElement element = XmlUtility.GetXElementByAttribute("entry", "key", key, settings);

            string value = "";
            value = element.Attribute("value")?.Value;

            return value;
        }

        public List<KeyValuePair<string, string>> GetList(string value)
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

                    KeyValuePair<string, string> kvp = new KeyValuePair<string, string>(k, v);
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

        /// <summary>
        /// Convert settings model to json and store it in the worskapce based on id
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool Update(ModuleSettings settings)
        {
            try
            {
                // if file exist, delete before
                if (File.Exists(_filePathJson)) File.Delete(_filePathJson);

                // create file and open it into a stream writer
                using (StreamWriter file = File.CreateText(_filePathJson))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    //serialize object directly into file stream
                    serializer.Serialize(file, settings);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("settings update of Module (" + _moduleId + ") failed", ex);
            }

            return false;
        }

        #endregion looad and update
    }
}