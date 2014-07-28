using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Subjects;

/// <summary>
///
/// </summary>        
namespace BExIS.Security.Services.Subjects
{
    /// <summary>
    ///
    /// </summary>
    /// <remarks></remarks>        
    public interface ISubjectManager : IRoleManager, IUserManager
    {
        // G

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        IQueryable<Subject> GetAllSubjects();
    }
}