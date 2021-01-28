using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Globalization;

namespace BExIS.Security.Entities.Authentication
{
    public class LdapConfiguration
    {
        public LdapConfiguration()
        {
            HostName = ConfigurationManager.AppSettings["Ldap_Host_Name"];
            HostSsl = bool.Parse(ConfigurationManager.AppSettings["Ldap_Host_Ssl"]);
            HostPort = int.Parse(ConfigurationManager.AppSettings["Ldap_Host_Port"], NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
            HostVersion = int.Parse(ConfigurationManager.AppSettings["Ldap_Host_Version"], NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);
            HostAuthType = int.Parse(ConfigurationManager.AppSettings["Ldap_Host_AuthType"], NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);

            HostBaseDn = ConfigurationManager.AppSettings["Ldap_Host_BaseDn"];
            HostFilter = ConfigurationManager.AppSettings["Ldap_Host_Filter"];
            HostScope = int.Parse(ConfigurationManager.AppSettings["Ldap_Host_Scope"], NumberStyles.None, CultureInfo.InvariantCulture.NumberFormat);

            UserIdentifier = ConfigurationManager.AppSettings["Ldap_User_Identifier"];
        }

        public string HostName { get; set; }
        public int HostPort { get; set; }
        public int HostAuthType { get; set; }
        public bool HostSsl { get; set; }
        public int HostVersion { get; set; }
        public string HostBaseDn { get; set; }
        public string HostFilter { get; set; }
        public int HostScope { get; set; }
        public string UserIdentifier { get; set; }
    }
}
