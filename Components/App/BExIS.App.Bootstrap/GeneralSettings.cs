using System.IO;
using System.Linq;
using Vaiona.Utils.Cfg;

namespace BExIS.App.Bootstrap
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
    public class GeneralSettings: Vaiona.Utils.Cfg.Settings
    {

        public GeneralSettings() : 
            base("Shell",
                Path.Combine(AppConfiguration.WorkspaceGeneralRoot, "General.Settings.json"))
        {
            // Nothing should be needed for now!
        }
    }
}