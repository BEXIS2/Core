using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Subjects;

namespace BExIS.Security.Services.Subjects
{
    public interface ISubjectManager : IRoleManager, IUserManager
    {
        // G
        IQueryable<Subject> GetAllSubjects();
    }
}