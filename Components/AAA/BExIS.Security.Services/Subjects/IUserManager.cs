using System;
using System.Linq;
using System.Web.Security;
using BExIS.Security.Entities.Subjects;
      
namespace BExIS.Security.Services.Subjects
{    
    public interface IUserManager
    {
        #region Attributes

        bool AutoActivation { get; }
        bool AutoApproval { get; }
        int MaxPasswordFailureAttempts { get; }
        int MaxSecurityAnswerFailureAttempts { get; }
        int OnlineWindow { get; }
        int PasswordFailureAttemptsWindow { get; }
        int SaltLength { get; }
        int SecurityAnswerFailureAttemptsWindow { get; }

        #endregion


        #region Methods

        bool ActivateUser(string userName);

        bool ApproveUser(string userName);

        bool ChangePassword(long id, string password);

        bool ChangeSecurityQuestionAndSecurityAnswer(long id, long securityQuestionId, string newPasswordAnswer);

        User CreateUser(string userName, long authenticatorId);

        User CreateUser(string userName, string password, string fullName, string email, long securityQuestionId, string securityAnswer, long authenticatorId);

        bool DeleteUserById(long id);

        bool DeleteUserByName(string userName);

        bool ExistsUserId(long id);

        bool ExistsUserName(string userName);

        bool ExistsUserNameWithAuthenticatorId(string userName, long authenticatorId);
      
        IQueryable<User> GetAllUsers();

        User GetUserByEmail(string email);

        User GetUserById(long id, bool isOnline = false);

        User GetUserByName(string userName, bool isOnline = false);

        string GetUserNameByEmail(string email);

        string GetUserNameById(long id);
      
        int GetUsersOnline();

        User RegisterUser(string userName, string password, string fullName, string email, long securityQuestionId, string securityAnswer, long authenticatorId);

        bool UnlockUser(string userName);

        User UpdateUser(User user);

        #endregion
    }

    public enum FailureType
    {
        Password = 1,
        SecurityAnswer = 2
    }
}