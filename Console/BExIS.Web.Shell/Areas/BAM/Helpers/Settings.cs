using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Vaiona.Utils.Cfg;

namespace BExIS.Modules.Bam.UI.Helpers
{
    public class Settings
    {

        private static String filePath = Path.Combine(AppConfiguration.GetModuleWorkspacePath("BAM"), "settings.xml");
        private static Dictionary<string,object> settings = new Dictionary<string, object>();

        /// <summary>
        /// setup settings model
        /// * load settings from settings.xml
        /// </summary>
        static Settings()
        {
            // intial loading of settings
            reloadSettings();

            // set up file watcher to listen for changes
            FileSystemWatcher fw = new FileSystemWatcher();
            fw.Path = Path.GetDirectoryName( filePath );
            fw.Filter = Path.GetFileName( filePath );
            fw.Changed += new FileSystemEventHandler(fw_Changed);
            fw.EnableRaisingEvents = true;

        }

        /// <summary>
        /// retrieve a value from the settings file
        /// </summary>
        /// <param name="key">the key for the parameter</param>
        /// <returns>the respective value</returns>
        public static object get( String key )
        {
            if( settings.ContainsKey( key ) )
            {
                return settings[ key ];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// add or change an entry in the settings
        /// TODO persist changes in workflow file
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void set( String key, object value )
        {
            if (settings.ContainsKey(key))
            {
                settings[ key ] = value;
            }
            else
            {
                settings.Add( key, value );
            }
        }

        /// <summary>
        /// Handler to listen for changes in settings file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void fw_Changed(object sender, FileSystemEventArgs e)
        {
            reloadSettings();
        }

        /// <summary>
        /// load settings anew from settings.xml
        /// </summary>
        private static void reloadSettings()
        {
            // get XML data
            XDocument xDoc = XDocument.Load(filePath);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xDoc.CreateReader());

            // empty old settings list
            settings.Clear();

            // parse values
            foreach (XmlNode node in xmlDoc.SelectNodes("//settings/entry"))
            {
                // shortcuts
                var key = node.Attributes["key"] != null ? node.Attributes["key"].Value : null;
                var value = node.Attributes["value"] != null ? node.Attributes["value"].Value : null;
                var type = node.Attributes["type"] != null ? node.Attributes["type"].Value : null;

                // only parse valid entries
                if ((null == key) || (null == value))
                {
                    continue;
                }

                // convert types
                switch (type)
                {
                    case "int":
                        int intVal;
                        if (Int32.TryParse(value, out intVal))
                        {
                            settings.Add(key, intVal);
                        }
                        else
                        {
                            settings.Add(key, value);
                        }
                        break;

                    // default is string
                    default:
                        settings.Add(key, value);
                        break;
                }
            }
        }

    }
}