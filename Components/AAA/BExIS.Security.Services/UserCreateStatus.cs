using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BExIS.Security.Services
{
    public enum UserCreateStatus
    {
        Success,

        DuplicateEmail,
        DuplicateUserName,
        
        InvalidEmail,
        InvalidPassword,
        InvalidSecurityAnswer,
        InvalidSecurityQuestion,
        InvalidUserName

        
   
        

        

    }
}
