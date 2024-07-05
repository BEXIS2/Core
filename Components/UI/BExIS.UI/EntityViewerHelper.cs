using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.UI.Helpers
{
    public class EntityViewerHelper
    {
        public static Tuple<string, string, string> GetEntityViewAction(string entityName, string moduleId, string modus)
        {
            if (string.IsNullOrEmpty(entityName)) return null;
            if (string.IsNullOrEmpty(moduleId)) return null;
            if (string.IsNullOrEmpty(modus)) return null;

            Tuple<string, string, string> tmp;

            if (ModuleManager.Exists(moduleId))
            {
                XElement manifestDoc = ModuleManager.GetModuleManifest(moduleId).ManifestDoc;

                /*
                 * check if there is a entry for the module and the entity
                 */

                Dictionary<string, string> attrs = new Dictionary<string, string>();
                attrs.Add("entity", entityName);
                attrs.Add("modus", modus);

                XElement x = XmlUtility.GetXElementByAttribute("EntityAction", attrs, manifestDoc);

                if (x == null) return null;

                XAttribute attrController = x.Attribute("controller");
                if (attrController == null || string.IsNullOrEmpty(attrController.Value)) return null;
                XAttribute attrAction = x.Attribute("action");
                if (attrAction == null || string.IsNullOrEmpty(attrAction.Value)) return null;
                XAttribute attrArea = x.Attribute("area");
                if (attrArea == null || string.IsNullOrEmpty(attrArea.Value)) return null;

                tmp = new Tuple<string, string, string>(attrArea.Value, attrController.Value, attrAction.Value);

                return tmp;
            }

            return null;
        }
    }
}