using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;

namespace BExIS.Security.Services.Objects
{
    public interface ISecurityQuestionManager
    {
        SecurityQuestion CreateSecurityQuestion(string question, bool isValid = true);

        bool DeleteSecurityQuestionById(long id);

        bool ExistsSecurityQuestionId(long id);

        IQueryable<SecurityQuestion> GetAllSecurityQuestions();

        SecurityQuestion GetSecurityQuestionById(long id);

        SecurityQuestion UpdateSecurityQuestion(SecurityQuestion securityQuestion);
    }
}
