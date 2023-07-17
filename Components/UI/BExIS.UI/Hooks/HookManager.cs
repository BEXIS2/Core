using BExIS.IO;
using BExIS.Xml.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;
using Vaiona.Web.Mvc.Modularity;

namespace BExIS.UI.Hooks
{
    public class HookManager
    {
        public List<Hook> GetHooksFor(string _entity, string _place, HookMode _mode)
        {
            return get<Hook>(_entity, _place, _mode);
        }

        public List<View> GetViewsFor(string _entity, string _place, HookMode _mode)
        {
            return get<View>(_entity, _place, _mode);
        }

        private List<T> get<T>(string _entity, string _place, HookMode _mode) where T : Hook
        {
            List<T> hooks = new List<T>();

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
                List<XElement> xHooks = XmlUtility.GetXElementsByAttribute(typeof(T).Name, attrs, manifestDoc).ToList();

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
                            var hook = (T)Activator.CreateInstance(type); //instance creation based on the type

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
        public T LoadCache<T>(string _entity, string _place, HookMode _mode, long id) where T : new()
        {
        
            //check incoming values
            if (string.IsNullOrEmpty(_entity)) throw new ArgumentNullException(nameof(_entity));
            if (string.IsNullOrEmpty(_place)) throw new ArgumentNullException(nameof(_place));
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));


            string sessionId = getSessionId(_entity, _place, _mode, id);

            T cache = new T();

            // load json if exist
            // generate filename based on mode,entity and place
            string filename = _mode.ToString().ToLower() + _entity.ToLower() + _place.ToLower() + "cache.json";

            // combine datapath + path + filename
            string filepath = Path.Combine(AppConfiguration.DataPath, _entity + "s", id.ToString(), filename);

            if (File.Exists(filepath)) // check if file exist
            {
                FileHelper.WaitForFile(filepath, FileAccess.Read); // wait if the file is still open

                // convert json to object
                cache = JsonConvert.DeserializeObject<T>(File.ReadAllText(filepath));
                
            }

            return cache;
        }

        public bool SaveCache<T>(T _cache, string _entity, string _place, HookMode _mode, long id)
        {
            string sessionId = getSessionId(_entity, _place, _mode, id);

            //check incoming values
            if (_cache == null) throw new ArgumentNullException(nameof(_cache));
            if (string.IsNullOrEmpty(_entity)) throw new ArgumentNullException(nameof(_entity));
            if (string.IsNullOrEmpty(_place)) throw new ArgumentNullException(nameof(_place));
            if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));

            // load json if exist
            // generate filename based on mode,entity and place
            string filename = _mode.ToString().ToLower() + _entity.ToLower() + _place.ToLower() + "cache.json";

            string directory = Path.Combine(AppConfiguration.DataPath, _entity + "s", id.ToString());
            // combine datapath + path + filename
            string filepath = Path.Combine(directory, filename);

          

            if (File.Exists(filepath))
            {
                FileHelper.WaitForFile(filepath); // wait if the file is still open
                File.Delete(filepath); // check if file exist, delete maybe? }
            }

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory); // create directory if not exist


            File.WriteAllText(filepath, JsonConvert.SerializeObject(_cache));

            return true;
        }

        /// <summary>
        /// create a session id based on the hook part
        /// </summary>
        /// <returns>session id as string</returns>
        private string getSessionId(string _entity, string _place, HookMode _mode, long id)
        {
            return string.Format("{0}-{1}-{2}-{3}", _entity, _place, _mode, id);
        }
    }
}