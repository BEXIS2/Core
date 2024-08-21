using BExIS.Xml.Helpers;
using System.IO;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Ddm.UI.Helpers
{
    public class SettingsHelper
    {
        private string filePath = "";

        public SettingsHelper()
        {
            filePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("DDM"), "Ddm.Settings.xml");
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
            if (element != null)
            {
                value = element.Attribute("value")?.Value;
            }

            return value;
        }
    }
}