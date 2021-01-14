using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Security.Entities.Authentication
{
    public class LdapConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Base { get; set; }
        public string UId { get; set; }
        public string BindDn { get; set; }
        public string Password { get; set; }
        public string Encryption { get; set; }
        public string UserFilter { get; set; }
    }
}
