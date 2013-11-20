using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities;

namespace BExIS.Security.Services
{
    public interface ISecurityManager
    {
        // UserName, Route [Area, Controller, Action]
        bool HasDataAccess(User user, DataContext dataContext);

        bool HasFeatureAccess(User user, FeatureContext featureContext);

    }
}
