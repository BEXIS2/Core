using System.Configuration;
using System.IO;
using System.Linq;
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
        public GeneralSettings() :
            base("Shell",
                Path.Combine(AppConfiguration.WorkspaceGeneralRoot, "General.Settings.json"))
        {
        }

        public string ApplicationName
        {
            get
            {
                try
                {
                    return (this.GetEntryValue("applicationName").ToString());
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

        public string ApplicationInfo
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

        public string SystemEmail
        {
            get
            {
                try
                {
                    return (this.GetEntryValue("systemEmail").ToString());
                }
                catch { return (string.Empty); }
            }
        }
    }
}