using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Subjects;

namespace BExIS.Security.Services.Authentication
{
    public interface IAuthenticationManager
    {
        bool ValidateUser(string username, string password);
    }
}
