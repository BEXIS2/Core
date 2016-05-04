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

        bool ActivateUser(string username);

        bool ApproveUser(string username);

        bool ChangePassword(long id, string password);

        bool ChangeSecurityQuestionAndSecurityAnswer(long id, long securityQuestionId, string newPasswordAnswer);

        User CreateUser(string username, long authenticatorId);

        User CreateUser(string username, string password, string fullName, string email, long securityQuestionId, string securityAnswer, long authenticatorId);

        bool DeleteUserById(long id);

        bool DeleteUserByName(string username);

        bool ExistsUserId(long id);

        bool ExistsUsername(string username);

        bool ExistsUsernameWithAuthenticatorId(string username, long authenticatorId);
      
        IQueryable<User> GetAllUsers();

        User GetUserByEmail(string email);

        User GetUserById(long id, bool isOnline = false);

        User GetUserByName(string username, bool isOnline = false);

        string GetUsernameByEmail(string email);

        string GetUsernameById(long id);
      
        int GetUsersOnline();

        User RegisterUser(string username, string password, string fullName, string email, long securityQuestionId, string securityAnswer, long authenticatorId);

        bool UnlockUser(string username);

        User UpdateUser(User user);

        #endregion
    }

    public enum FailureType
    {
        Password = 1,
        SecurityAnswer = 2
    }
}