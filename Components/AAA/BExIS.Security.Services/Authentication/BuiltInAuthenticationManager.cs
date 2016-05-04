using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Subjects;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Authentication
{
    public sealed class BuiltInAuthenticationManager : AuthenticationManager, IAuthenticationManager
    {
        public BuiltInAuthenticationManager(string connectionString) { }

        public bool ValidateUser(string username, string password)
        {
            SubjectManager subjectManager = new SubjectManager();

            return subjectManager.ValidateUser(username, password);
        }
    }
}
