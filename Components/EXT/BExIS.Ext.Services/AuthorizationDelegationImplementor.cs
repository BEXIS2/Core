using System;

namespace BExIS.Ext.Services
{
    public static class AuthorizationDelegationImplementor
    {
        public static void CheckAuthorization(string areaName, string controllerName, string actionName, string username, bool isAuthenticated)
        {
            // BUG: the original method is not working anymore due to the changes inside entities and services.
            // TODO: reimplement the method for the authorization check.


            // validate the call using the extensibility information (modules, tasks, actions, etc)
            // Call security authorization api utilizing the IoC, Singleton lifetime
            //throw an exception based on the result

            // Ask for specific URLs (LogOn, Register, ...)

            //TaskManager taskManager = new TaskManager();

            //Task task = taskManager.GetTask(areaName, controllerName, "*");

            //if (task != null)
            //{
            //    if (task.Feature != null)
            //    {
            //        PermissionManager permissionManager = new PermissionManager();
            //        SubjectManager subjectManager = new SubjectManager();

            //        if (!permissionManager.ExistsFeaturePermission(subjectManager.GetGroupByName("everyone").Id, task.Feature.Id))
            //        {
            //            User user = subjectManager.GetUserByName(username);

            //            if (user != null)
            //            {
            //                if (!permissionManager.HasSubjectFeatureAccess(user.Id, task.Feature.Id))
            //                {
            //                    throw new UnauthorizedAccessException();
            //                }
            //            }
            //            else
            //            {
            //                throw new UnauthorizedAccessException();
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    throw new UnauthorizedAccessException();
            //}

            throw new UnauthorizedAccessException();
        }
    }
}
