using System;
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

        public static object GetEntryValue(string entryKey)
        {
            Entry entry = Get().jsonSettings.Entry.Where(p => p.Key.Equals(entryKey, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (entry == null)
                return null;
            string value = entry.Value.ToString();
            string type = entry.Type;
            var typedValue = Convert.ChangeType(value, (TypeCode)Enum.Parse(typeof(TypeCode), type));
            return typedValue;
        }

        public static string ApplicationName
        {
            get
            {
                try
                {
                    return (GetEntryValue("applicationName").ToString());
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
                    return (GetEntryValue("systemEmail").ToString());
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
                    return (GetEntryValue("landingPage").ToString());
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
                    return (GetEntryValue("landingPageForUsers").ToString());
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
                    return (GetEntryValue("landingPageForUsersNoPermission").ToString());
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
                    return Convert.ToBoolean(GetEntryValue("sendExceptions"));
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
                    return Convert.ToBoolean(GetEntryValue("usePersonEmailAttributeName"));
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
                    return (GetEntryValue("personEmailAttributeName").ToString());
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
                    return Convert.ToBoolean(GetEntryValue("useMultiMediaModule"));
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
                    return (GetEntryValue("OwnerPartyRelationshipType").ToString());
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
                    return (GetEntryValue("faq").ToString());
                }
                catch { return (string.Empty); }
            }
        }

    }
}