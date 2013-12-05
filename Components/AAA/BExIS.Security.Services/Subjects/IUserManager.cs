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

        int PasswordFailureAttemptsWindow { get; }
        string PasswordStrengthRegularExpression { get; }

        bool RequiresUniqueEmail { get; }

        int SecurityAnswerFailureAttemptsWindow { get; }

        #endregion


        #region Methods

        // A
        bool ApproveUser(User user);

        // C
        bool ChangePassword(User user, string password, string newPassword);
        bool ChangeSecurityQuestionAndSecurityAnswer(User user, string password, string newPasswordQuestion, string newPasswordAnswer);

        User Create(string userName, string email, string password, string passwordQuestion, string passwordAnswer, out UserCreateStatus status);

        // D
        bool Delete(User user);

        // E
        bool ExistsUserName(string userName);

        // F
        [Obsolete("Please use GetUsers() instead!")]
        IQueryable<User> FindUsersByEmail(string emailToMatch);
        [Obsolete("Please use GetUsers() instead!")]
        IQueryable<User> FindUserByName(string userNameToMatch);

        // G
        IQueryable<User> GetAllUsers();

        User GetUserByEmail(string email);
        User GetUserById(Int64 id, bool isOnline = false);
        User GetUserByName(string userName, bool isOnline = false);

        string GetUserNameByEmail(string email);
        string GetUserNameById(long id);

        int GetUsersOnline();

        // R
        string ResetPassword(User user, string passwordAnswer);


        // U
        bool UnlockUser(User user);

        User Update(User user);

        // V
        bool ValidateUser(string userName, string password);

        #endregion
    }

    public enum FailureType
    {
        Password = 1,
        SecurityAnswer = 2
    }
}