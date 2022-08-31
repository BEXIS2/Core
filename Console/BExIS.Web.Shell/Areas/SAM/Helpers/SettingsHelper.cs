using BExIS.Xml.Helpers;
using System.IO;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.SAM.UI.Helpers
{
    public class SettingsHelper
    {
        string filePath = "";

        public SettingsHelper()
        {
            filePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("SAM"), "Sam.Settings.xml");
        }

        public bool KeyExist(string key)
        {
            XDocument settings = XDocument.Load(filePath);
            XElement element = XmlUtility.GetXElementByAttribute("entry", "key", key, settings);

            return element != null ? true : false;
        }

        public string GetValue(string key)
        {
            XDocument settings = XDocument.Load(filePath);
            XElement element = XmlUtility.GetXElementByAttribute("entry", "key", key, settings);

            string value = "";
            if (element != null)
            {
                value = element.Attribute("value")?.Value;
            }

            return value;
        }
    }
}