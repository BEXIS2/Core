using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Text;

namespace BExIS.Security.Services.Authentication
{
    public sealed class ADAuthenticationManager : AuthenticationManager, IAuthenticationManager
    {
        private string AdminUsername { get; set; }
        private string AdminPassword { get; set; }
        private string Host { get; set; }
        private string BaseContainer { get; set; }

        public ADAuthenticationManager(string connectionString)
        {
            Dictionary<string, string> parameters = connectionString
                .Split(';')
                    .Select(x => x.Split(':'))
                        .ToDictionary(x => x[0], x => x[1]);

            foreach (KeyValuePair<string, string> entry in parameters)
            {
                switch (entry.Key)
                {
                    case "AdminUsername":
                        this.AdminUsername = entry.Value;
                        break;
                    case "AdminPassword":
                        this.AdminPassword = entry.Value;
                        break;
                    case "Host":
                        this.Host = entry.Value;
                        break;
                    case "BaseContainer":
                        this.BaseContainer = entry.Value;
                        break;
                    default:
                        break;
                }
            }
        }

        public bool ValidateUser(string username, string password)
        {
            PrincipalContext AD = new PrincipalContext(ContextType.Domain, this.Host, this.BaseContainer, this.AdminUsername, this.AdminPassword);

            if (AD.ValidateCredentials(username, password))
            {
                return true;
            }
            return false;
        }
    }
}
