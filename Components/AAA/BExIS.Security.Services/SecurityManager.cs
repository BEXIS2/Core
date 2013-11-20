using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities;

namespace BExIS.Security.Services
{
    public sealed class SecurityManager : ISecurityManager
    {
        public bool HasDataAccess(User user, DataContext dataContext)
        {
            throw new NotImplementedException();
        }

        public bool HasFeatureAccess(User user, FeatureContext featureContext)
        {
            throw new NotImplementedException();
        }
    }
}
