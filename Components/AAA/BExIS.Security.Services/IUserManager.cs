using System;
using System.Linq;
using System.Web.Security;
using BExIS.Security.Entities;

namespace BExIS.Security.Services
{
    public interface IUserManager
    {
        #region Attributes



        #endregion


        #region Methods

        // A
        bool ApproveUser(string userName);

        // C
        bool ChangePassword(string userName, string password, string newPassword, string machineKey, int minPasswordLength);
        bool ChangePasswordQuestionAndPasswordAnswer(string userName, string password, string newPasswordQuestion, string newPasswordAnswer, string machineKey);

        User Create(string userName, string email, string password, string passwordQuestion, string passwordAnswer, string comment, bool uniqueEmail, int minPasswordLength, string machineKey, out UserCreateStatus status, bool isApproved = false);

        // D
        bool Delete(User user);

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
        string ResetPassword(string userName, string passwordAnswer, int maxFailureAttempts, int failureAttemptsWindow, int minPasswordLength, string machineKey);


        // V
        bool ValidateUser(string userName, string password, int maxFailureAttempts, int failureAttemptsWindow, string machineKey);

        // U
        bool UnlockUser(string userName);

        User Update(User user);

        #endregion
    }

    public enum FailureType
    {
        Password = 1,
        PasswordAnswer = 2
    }
}