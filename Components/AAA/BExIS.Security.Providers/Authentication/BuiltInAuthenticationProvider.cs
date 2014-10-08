using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Subjects;

namespace BExIS.Security.Providers.Authentication
{
    public sealed class BuiltInAuthenticationProvider : IAuthenticationProvider
    {

        public BuiltInAuthenticationProvider(string connectionString)
        {

        }

        public bool IsUserAuthenticated(string userName, string password)
        {
            throw new NotImplementedException();
        }
    }
}
