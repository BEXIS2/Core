using BExIS.Utils.Config.Configurations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Vaiona.IoC;
using Vaiona.Utils.Cfg;

namespace BExIS.Utils.Config
{
    /// <summary>
    /// Contains all the general settings of the shell.
    /// </summary>
    /// <remarks>
    ///     <list type="bullet">
    ///         <item>This is a singleton class that must be instaniated via the IoC only</item>
    ///         <item>This class relies on the AppConfiguration config items such as workspace root, etc., hence those settings must not be moved to the settings.</item>
    ///         <item>Some web.config items such as databse connection string and the IoC condif items are needed before this object is instantiated, thgose config items can not leave web.config.</item>
    ///     </list>
    /// </remarks>
    public class GeneralSettings : Vaiona.Utils.Cfg.Settings
    {
        // GeneralSettings.Get().GetValue("key");

        public GeneralSettings() :
            base("Shell",
                Path.Combine(AppConfiguration.WorkspaceGeneralRoot, "General.Settings.json"))
        {

        }

        public static GeneralSettings Get()
        {
            GeneralSettings generalSettings = null;
            try
            {
                generalSettings = IoCFactory.Container.Resolve<GeneralSettings>() as GeneralSettings;
            }
            catch (Exception ex)
            {
                throw new Exception("Could not load general settings", ex);
            }
            return (generalSettings);
        }

        public static object GetValueByKey(string entryKey)
        {
            Entry entry = Get().jsonSettings.Entries.Where(p => p.Key.Equals(entryKey, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (entry == null)
                return null;
            string value = entry.Value.ToString();
            EntryType type = entry.Type;

            switch (type)
            {
                case (EntryType.EntryList):
                    return JsonConvert.DeserializeObject<List<Entry>>(value);

                default:
                    return Convert.ChangeType(value, (TypeCode)Enum.Parse(typeof(TypeCode), type.ToString()));
            }
        }

        public static JwtConfiguration JwtConfiguration
        {
            get
            {
                return JsonConvert.DeserializeObject<JwtConfiguration>(GetValueByKey("jwt").ToString());
            }
        }

        public static string ApplicationName
        {
            get
            {
                try
                {
                    return GetValueByKey("applicationName").ToString();
                }
                catch { return (string.Empty); }
            }
        }

        public static string ApplicationVersion
        {
            get
            {
                try
                {
                    return (ConfigurationManager.AppSettings["ApplicationVersion"]);
                }
                catch { return (string.Empty); }
            }
        }

        public static string ApplicationInfo
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ApplicationName))
                {
                    if (!string.IsNullOrWhiteSpace(ApplicationVersion))
                        return (string.Format("{0} {1}", ApplicationName, ApplicationVersion));
                    return (string.Format(ApplicationName));
                }
                return string.Empty;
            }
        }

        public static string SystemEmail
        {
            get
            {
                try
                {
                    return GetValueByKey("systemEmail").ToString();
                }
                catch { return (string.Empty); }
            }
        }

        public static string LandingPage
        {
            get
            {
                try
                {
                    return GetValueByKey("landingPage").ToString();
                }
                catch { return (string.Empty); }
            }
        }

        public static string LandingPageForUsers
        {
            get
            {
                try
                {
                    return GetValueByKey("landingPageForUsers").ToString();
                }
                catch { return (string.Empty); }
            }
        }

        public static string LandingPageForUsersNoPermission
        {
            get
            {
                try
                {
                    return GetValueByKey("landingPageForUsersNoPermission").ToString();
                }
                catch { return (string.Empty); }
            }
        }

        public static bool SendExceptions
        {
            get
            {
                try
                {
                    return Convert.ToBoolean(GetValueByKey("sendExceptions"));
                }
                catch { return (false); }
            }
        }

        public static bool UsePersonEmailAttributeName
        {
            get
            {
                try
                {
                    return Convert.ToBoolean(GetValueByKey("usePersonEmailAttributeName"));
                }
                catch { return (false); }
            }
        }

        public static string PersonEmailAttributeName
        {
            get
            {
                try
                {
                    return GetValueByKey("personEmailAttributeName").ToString();
                }
                catch { return (string.Empty); }
            }
        }

        public static bool UseMultiMediaModule
        {
            get
            {
                try
                {
                    return Convert.ToBoolean(GetValueByKey("useMultiMediaModule"));
                }
                catch { return (false); }
            }
        }

        public static string OwnerPartyRelationshipType
        {
            get
            {
                try
                {
                    return GetValueByKey("OwnerPartyRelationshipType").ToString();
                }
                catch { return (string.Empty); }
            }
        }

        public static string FAQ
        {
            get
            {
                try
                {
                    return GetValueByKey("faq").ToString();
                }
                catch { return (string.Empty); }
            }
        }

    }
}