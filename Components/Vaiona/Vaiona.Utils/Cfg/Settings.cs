using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Vaiona.Utils.IO;

namespace Vaiona.Utils.Cfg
{
    public class Settings
    {
        protected string id;
        protected JsonSettings jsonSettings;
        protected string settingsFullPath;

        private FileSystemWatcher watcher;

        public Settings(string id, string settingsFullPath)
        {
            this.id = id;
            this.settingsFullPath = settingsFullPath;
            if (string.IsNullOrWhiteSpace(settingsFullPath))
                throw new ArgumentNullException("Provided setting path is null or empty");
            if (!File.Exists(settingsFullPath))
                throw new FileNotFoundException($"Provided path {settingsFullPath} does not exist.");

            watcher = new FileSystemWatcher()
            {
                Path = Path.GetDirectoryName(settingsFullPath),
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
                Filter = Path.GetFileName(settingsFullPath),
                EnableRaisingEvents = true,
            };

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(onCatalogChanged);
            watcher.Created += new FileSystemEventHandler(onCatalogChanged);
            watcher.Deleted += new FileSystemEventHandler(onCatalogChanged);
            watcher.Renamed += new RenamedEventHandler(onCatalogChanged);

            loadSettings();
        }

        public JsonSettings GetAsJsonModel()
        {
            return jsonSettings;
        }

        /*
         * @sventhiel:
         * There is no situation in which that function will return false.
         */

        public T GetValueByKey<T>(string entryKey) where T : class
        {
            Entry entry = jsonSettings.Entries.Where(p => p.Key.Equals(entryKey, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (entry == null)
                return null;

            return JsonConvert.DeserializeObject<T>(entry.Value.ToString());
        }

        public object GetValueByKey(string entryKey)
        {
            Entry entry = jsonSettings.Entries.Where(p => p.Key.Equals(entryKey, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (entry == null)
                return null;

            EntryType type = entry.Type;

            switch (type)
            {
                case (EntryType.EntryList):
                    var value = entry.Value as JArray;
                    var list = new List<Entry>();
                    if (value != null)
                    {
                        foreach (var item in value)
                        {
                            list.Add(JsonConvert.DeserializeObject<Entry>(item.ToString()));
                        }
                    }
                    return list;

                case (EntryType.JSON):
                    return JsonConvert.DeserializeObject(entry.Value.ToString());

                default:
                    string entryValue = entry.Value != null ? entry.Value.ToString() : "";
                    return Convert.ChangeType(entryValue, (TypeCode)Enum.Parse(typeof(TypeCode), type.ToString()));
            }
        }

        public bool Update(JsonSettings _jsonSettings)
        {
            jsonSettings = _jsonSettings;

            try
            {
                /*
                 * @sventhiel:
                 * Re-evaluate if this part is necessary, because "File.CreateText" will overwrite the content anyway.
                 *
                 * if (File.Exists(settingsFullPath))
                 * File.Delete(settingsFullPath);
                 */

                using (StreamWriter file = File.CreateText(settingsFullPath))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, _jsonSettings);

                    file.Flush();
                    file.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"The settings update for module ({_jsonSettings.Id}) failed.", ex);
            }
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

        private void onCatalogChanged(object source, FileSystemEventArgs e)
        {
            loadSettings();
        }
    }
}