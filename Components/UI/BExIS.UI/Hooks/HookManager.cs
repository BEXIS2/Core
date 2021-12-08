using BExIS.Xml.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.UI.Hooks
{
    public class HookManager
    {
        public List<Hook> GetHooksFor(string _entity, string _place, HookMode _mode)
        {
            List<Hook> hooks = new List<Hook>();

            // check active modules
            var activeModules = ModuleManager.ModuleInfos.Where(m => ModuleManager.IsActive(m.Id));

            // go throw each module and load matching hooks
            foreach (var module in activeModules)
            {
                // get manifestfile where hooks defiened
                var manifestDoc = module.Manifest.ManifestDoc;

                // set query attr for xml search
                Dictionary<string, string> attrs = new Dictionary<string, string>();
                attrs.Add("entity", _entity);
                attrs.Add("mode", _mode.ToString());
                attrs.Add("place", _place);

                // get xHooks based on the attrs
                List<XElement> xHooks = XmlUtility.GetXElementsByAttribute("Hook", attrs, manifestDoc).ToList();

                foreach (var xHook in xHooks)
                {
                    // check if required attr exist
                    // name, description, icon, mode, entity, place, type
                    // if one of this attr not exits or empty the hook will not be loaded

                    bool isValid = true;

                    // name
                    XAttribute attrName = xHook.Attribute("name");
                    if (attrName == null || string.IsNullOrEmpty(attrName.Value)) isValid = false;

                    XAttribute attrDisplayName = xHook.Attribute("displayName");
                    if (attrDisplayName == null || string.IsNullOrEmpty(attrDisplayName.Value)) isValid = false;

                    //mode
                    XAttribute attrMode = xHook.Attribute("mode");
                    if (attrMode == null || string.IsNullOrEmpty(attrMode.Value)) isValid = false;

                    //entity
                    XAttribute attrEntity = xHook.Attribute("entity");
                    if (attrEntity == null || string.IsNullOrEmpty(attrEntity.Value)) isValid = false;

                    //place
                    XAttribute attrPlace = xHook.Attribute("place");
                    if (attrPlace == null || string.IsNullOrEmpty(attrPlace.Value)) isValid = false;

                    //type
                    XAttribute attrType = xHook.Attribute("type");
                    if (attrType == null || string.IsNullOrEmpty(attrType.Value)) isValid = false;

                    //moduleid
                    XAttribute attrModule = xHook.Attribute("module");
                    if (attrModule == null || string.IsNullOrEmpty(attrModule.Value)) isValid = false;

                    // if no attr missing or empty try create a instance of the type
                    if (isValid)
                    {
                        Type type = Type.GetType(attrType.Value);

                        if (type != null) //type transform works - go forward
                        {
                            Hook hook = (Hook)Activator.CreateInstance(type); //instance creation based on the type

                            if (hook != null) // stop if hook is null
                            {
                                hook.Name = attrName.Value;
                                hook.DisplayName = attrDisplayName.Value;
                                hook.Mode = (HookMode)Enum.Parse(typeof(HookMode), attrMode.Value);
                                hook.Entity = attrEntity.Value;
                                hook.Place = attrPlace.Value;
                                hook.Module = attrModule.Value;
                                hooks.Add(hook);
                            }
                        }
                    }
                }
            }

            return hooks;
        }

        // load cache
    }
}