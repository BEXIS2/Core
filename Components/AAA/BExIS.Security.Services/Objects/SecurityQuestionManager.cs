using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities.Objects;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Objects
{
    public sealed class SecurityQuestionManager : ISecurityQuestionManager
    {
        public SecurityQuestionManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();

            this.SecurityQuestionsRepo = uow.GetReadOnlyRepository<SecurityQuestion>();
        }

        #region Data Readers
      
        public IReadOnlyRepository<SecurityQuestion> SecurityQuestionsRepo { get; private set; }    

        #endregion

        public SecurityQuestion CreateSecurityQuestion(string question, bool isValid = true)
        {
            SecurityQuestion securityQuestion = new SecurityQuestion()
            {
                Question = question,
                IsValid = isValid
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<SecurityQuestion> usersRepo = uow.GetRepository<SecurityQuestion>();
                usersRepo.Put(securityQuestion);

                uow.Commit();
            }

            return (securityQuestion);
        }

        public bool DeleteSecurityQuestionById(long id)
        {
            throw new NotImplementedException();
        }

        public bool ExistsSecurityQuestionId(long id)
        {
            return SecurityQuestionsRepo.Get(id) != null ? true : false;
        }

        public IQueryable<SecurityQuestion> GetAllSecurityQuestions()
        {
            return SecurityQuestionsRepo.Query();
        }

        public SecurityQuestion GetSecurityQuestionById(long id)
        {
            return SecurityQuestionsRepo.Get(id);
        }

        public SecurityQuestion UpdateSecurityQuestion(SecurityQuestion securityQuestion)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<SecurityQuestion> securityQuestionsRepo = uow.GetRepository<SecurityQuestion>();
                securityQuestionsRepo.Put(securityQuestion);
                uow.Commit();
            }

            return (securityQuestion);
        }
    }
}
