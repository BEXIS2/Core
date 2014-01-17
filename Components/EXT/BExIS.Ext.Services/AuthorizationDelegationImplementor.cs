using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Objects;
using BExIS.Security.Services.Security;
using BExIS.Security.Services.Subjects;

namespace BExIS.Ext.Services
{
    public static class AuthorizationDelegationImplementor
    {
        public static void CheckAuthorization(string areaName, string controllerName, string actionName, string userName, bool isAuthenticated)
        {
            // validate the call using the extensibility information (modules, tasks, actions, etc)
            // Call security authorization api utilizing the IoC, Singleton lifetime
            //throw an exception based on the result

            // Ask for specific URLs (LogOn, Register, ...)

            List<string> publics = new List<string>();

            publics.Add("Auth.Account");
            publics.Add("Site.Nav");
            publics.Add("Shell.Home");
            publics.Add("System.Utils"); 
            publics.Add("Auth.Users.ValidateUserName");
            publics.Add("Auth.Users.ValidateEmail");
            publics.Add("Auth.Users.ErrorCodeToErrorMessage");
            publics.Add("Auth.Users.ErrorCodeToErrorKey");

            

            if (!publics.Contains(areaName + "." + controllerName) && !publics.Contains(areaName + "." + controllerName + "." + actionName))
            {              
                if (string.IsNullOrWhiteSpace(userName) || !isAuthenticated)
                {
                    throw new UnauthorizedAccessException();
                }
                else
                {
                    TaskContext taskContext = new TaskContext()
                    {
                        AreaName = areaName,
                        ControllerName = controllerName,
                        ActionName = "*"
                    };

                    SecurityService securityService = new SecurityService();               
                    securityService.HasTaskAccess(userName, taskContext);
                }
            }
        }
    }
}
