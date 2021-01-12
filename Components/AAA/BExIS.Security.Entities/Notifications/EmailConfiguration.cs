using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BExIS.Security.Entities.Notifications
{
    public class EmailConfiguration
    {
        public EmailConfiguration(string connectionString)
        {

        }

        public string Host { get; set; }
        public int Port { get; set; }
        public EmailAccount EmailAccount { get; set; }
        public EmailAddress Sender { get; set; }
    }

    public class EmailAccount
    {
        public string Name { get; set; }
        public string Password { get; set; }
    }

    public class EmailAddress
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }
}
