using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Subjects;
    
namespace BExIS.Security.Services.Subjects
{       
    public interface ISubjectManager : IGroupManager, IUserManager
    {  
        IQueryable<Subject> GetAllSubjects();
    }
}