using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authorization;
using BExIS.Security.Services.Objects;
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

            TaskManager taskManager = new TaskManager();

            Task task = taskManager.GetTask(areaName, controllerName, "*");

            if (task != null)
            {
                if (task.Feature != null)
                {
                    PermissionManager permissionManager = new PermissionManager();
                    SubjectManager subjectManager = new SubjectManager();

                    if (!permissionManager.ExistsFeaturePermission(subjectManager.GetGroupByName("everyone").Id, task.Feature.Id))
                    {
                        User user = subjectManager.GetUserByName(userName);

                        if (user != null)
                        {
                            if (!permissionManager.HasSubjectFeatureAccess(user.Id, task.Feature.Id))
                            {
                                throw new UnauthorizedAccessException();
                            }
                        }
                        else
                        {
                            throw new UnauthorizedAccessException();
                        }
                    }
                }
            }
            else
            {
                throw new UnauthorizedAccessException();
            }
        }
    }
}
