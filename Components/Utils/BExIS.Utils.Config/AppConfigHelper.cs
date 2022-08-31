using System.Configuration;
using System.IO;
using System.Linq;
using Vaiona.Utils.Cfg;

namespace BExIS.Utils.Config
{
    public class AppConfigHelper
    {
        public bool SetAppRootKey()
        {
            DirectoryInfo di = new DirectoryInfo(AppConfiguration.AppRoot);
            string path = "";

            var parent = di.Parent;
            while (parent != null)
            {
                if (parent.GetDirectories().Any(d => d.Name.ToLower() == "console"))
                {
                    break;
                }
                else
                {
                    parent = parent.Parent;
                }
            }

            path = Path.Combine(parent.FullName, "Console", "BExIS.Web.Shell");

            ConfigurationManager.AppSettings["ApplicationRoot"] = path;

            return true;
        }
    }
}