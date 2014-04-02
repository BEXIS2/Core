using System;
using System.Linq;
using System.Web.Security;
using BExIS.Security.Entities.Subjects;

namespace BExIS.Security.Services.Subjects
{
    public interface IUserManager
    {
        #region Attributes

        bool AutoApproval { get; }

        string MachineKey { get; }

        int MaxPasswordFailureAttempts { get; }
        int MaxSecurityAnswerFailureAttempts { get; }

        int OnlineWindow { get; }

        int PasswordFailureAttemptsWindow { get; }
        string PasswordStrengthRegularExpression { get; }

        int SecurityAnswerFailureAttemptsWindow { get; }

        #endregion


        #region Methods

        // A
        bool ApproveUser(string userName);

        // C
        bool ChangePassword(string userName, string password, string newPassword);
        bool ChangeSecurityQuestionAndSecurityAnswer(string userName, string password, string newPasswordQuestion, string newPasswordAnswer);

        User CreateUser(string userName, string email, string password, string passwordQuestion, string passwordAnswer, out UserCreateStatus status);

        // D
        bool DeleteUserByName(string userName);
        bool DeleteUserById(long id);

        // E
        bool ExistsUserId(long id);
        bool ExistsUserName(string userName);

        // F
        [Obsolete("Please use GetUsers() instead!")]
        IQueryable<User> FindUsersByEmail(string emailToMatch);
        [Obsolete("Please use GetUsers() instead!")]
        IQueryable<User> FindUserByName(string userNameToMatch);

        // G
        IQueryable<User> GetAllUsers();

        User GetUserByEmail(string email);
        User GetUserById(long id, bool isOnline = false);
        User GetUserByName(string userName, bool isOnline = false);

        string GetUserNameByEmail(string email);
        string GetUserNameById(long id);

        int GetUsersOnline();

        // R
        string ResetPassword(string userName, string passwordAnswer);


        // U
        bool UnlockUser(string userName);

        User UpdateUser(User user);

        // V
        bool ValidateUser(string userName, string password);

        #endregion
    }

    public enum FailureType
    {
        Password = 1,
        SecurityAnswer = 2
    }

    public enum UserCreateStatus
    {
        Success,

        DuplicateEmail,
        DuplicateUserName,

        InvalidEmail,
        InvalidPassword,
        InvalidSecurityAnswer,
        InvalidSecurityQuestion,
        InvalidUserName
    }
}