using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vaiona.Utils.IO;

namespace Vaiona.Utils.Cfg
{
    /// <summary>
    /// Base class for managing the settings of each module as well as shell (genral).
    /// This class monitors the specified path for file changes and re-syncs the settings if needed.
    /// </summary>
    public class Settings
    {
        /// <summary>
        /// Contains the settings entries.
        /// Each entry has a key, a value, and a type.
        /// The type field must match System.TypeCode enumeration, case sensitive.
        /// </summary>
        protected JsonSettings jsonSettings;

        /// <summary>
        /// The name of the module or shell or whatever this settings belongs to
        /// </summary>
        protected string id = "";

        /// <summary>
        /// The full path of the setting file. the file should be an XML containg a set of 'entry' items, each having key, value, and type.
        /// The file itself should follow <id>.settings.json naming format.
        /// The path can be anywhere, but in general, for the modules, its in the root folder of the modules in the workspace folder.
        /// </summary>
        protected string settingsFullPath = "";

        private FileSystemWatcher watcher = new FileSystemWatcher();

        public Settings(string id, string settingsFullPath)
        {
            this.id = id;
            this.settingsFullPath = settingsFullPath;
            if (string.IsNullOrWhiteSpace(settingsFullPath))
                throw new ArgumentNullException("Provided setting path is null or empty");
            if (!File.Exists(settingsFullPath))
                throw new FileNotFoundException($"Provided path {settingsFullPath} does not exist.");

            watcher.Path = Path.GetDirectoryName(settingsFullPath);
            /* Watch for changes in LastAccess and LastWrite times, and
               the renaming of files or directories. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
               | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch the manifest file.
            watcher.Filter = Path.GetFileName(settingsFullPath);

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(onCatalogChanged);
            watcher.Created += new FileSystemEventHandler(onCatalogChanged);
            watcher.Deleted += new FileSystemEventHandler(onCatalogChanged);
            watcher.Renamed += new RenamedEventHandler(onCatalogChanged);

            // Begin watching.
            watcher.EnableRaisingEvents = true;
            loadSettings();
        }

        /// <summary>
        /// return Settings as a class based on the json
        /// </summary>
        /// <returns></returns>
        public JsonSettings GetAsJsonModel()
        {
            return jsonSettings;
        }

        public bool Update(JsonSettings _jsonSettings)
        {
            jsonSettings = _jsonSettings;

            try
            {
                // if file exist, delete before
                if (File.Exists(settingsFullPath)) File.Delete(settingsFullPath);

                // create file and open it into a stream writer
                using (StreamWriter file = File.CreateText(settingsFullPath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    //serialize object directly into file stream
                    serializer.Serialize(file, _jsonSettings);

                    file.Flush();
                    file.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("settings update of Module (" + _jsonSettings.Id + ") failed", ex);
            }
        }

        public object GetValueByKey(string entryKey)
        {
            Entry entry = jsonSettings.Entries.Where(p => p.Key.Equals(entryKey, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (entry == null)
                return null;
            
            EntryType type = entry.Type;
            

            switch (type)
            {
                case(EntryType.EntryList):
                    var value = entry.Value as JArray;
                    var list = new List<Entry>();

                    foreach (var item in value)
                    {
                        list.Add(JsonConvert.DeserializeObject<Entry>(item.ToString()));
                    }
                    return list;

                case (EntryType.JSON):
                    return JsonConvert.DeserializeObject<Entry>(entry.Value.ToString());

                default:
                    return Convert.ChangeType(entry.Value.ToString(), (TypeCode)Enum.Parse(typeof(TypeCode), type.ToString()));
            }
        }

        //public Item[] GetList(string entryKey)
        //{
        //    Entry entry = jsonSettings.Entry.Where(p => 
        //        p.Key.Equals("name", StringComparison.InvariantCultureIgnoreCase) && 
        //        p.Value.ToString().Equals(entryKey, StringComparison.InvariantCultureIgnoreCase) &&
        //        p.Type=="list").FirstOrDefault();
        //    if (entry == null)
        //        return null;

        //    return entry.Item;
        //}

        private void onCatalogChanged(object source, FileSystemEventArgs e)
        {
            loadSettings();
        }

        private void loadSettings()
        {
            
            try
            {
                FileHelper.WaitForFile(settingsFullPath);
                using (StreamReader stream = File.OpenText(settingsFullPath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    jsonSettings = (JsonSettings)serializer.Deserialize(stream, typeof(JsonSettings));

                }
            }
            catch (Exception ex)
            {
                // do nothing
                // there seem to be situations where the settings file should be opened twice at almost the same time. 
                // it is also surprising that the wait function does not change anything.
                // The catch exits because there will be no problems if the error is ignored, since the settings are loaded anyway and the result is the same.
            }
        }
    }
}