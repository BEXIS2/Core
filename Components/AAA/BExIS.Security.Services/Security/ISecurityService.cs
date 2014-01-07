using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;

namespace BExIS.Security.Services.Security
{
    public interface ISecurityService
    {
        bool HasDataAccess(User user, DataContext dataContext);

        bool HasTaskAccess(User user, TaskContext taskContext);

        bool HasTaskAccess(string userName, TaskContext taskContext);
    }
}
