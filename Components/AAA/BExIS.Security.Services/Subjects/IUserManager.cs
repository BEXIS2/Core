using System;
using System.Linq;
using System.Web.Security;
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
    public interface IUserManager
    {
        #region Attributes

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        bool AutoApproval { get; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        string MachineKey { get; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        int MaxPasswordFailureAttempts { get; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        int MaxSecurityAnswerFailureAttempts { get; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        int OnlineWindow { get; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        int PasswordFailureAttemptsWindow { get; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        string PasswordStrengthRegularExpression { get; }

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>        
        int SecurityAnswerFailureAttemptsWindow { get; }

        #endregion


        #region Methods

        // A

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="userName"></param>
        bool ApproveUser(string userName);

        // C

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="newPassword"></param>
        bool ChangePassword(string userName, string password, string newPassword);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="newPasswordQuestion"></param>
        /// <param name="newPasswordAnswer"></param>
        bool ChangeSecurityQuestionAndSecurityAnswer(string userName, string password, string newPasswordQuestion, string newPasswordAnswer);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="userName"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="passwordQuestion"></param>
        /// <param name="passwordAnswer"></param>
        /// <param name="status"></param>
        User CreateUser(string userName, string email, string password, string passwordQuestion, string passwordAnswer, out UserCreateStatus status);

        // D

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="userName"></param>
        bool DeleteUserByName(string userName);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        bool DeleteUserById(long id);

        // E

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        bool ExistsUserId(long id);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="userName"></param>
        bool ExistsUserName(string userName);

        // F

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="emailToMatch"></param>
        [Obsolete("Please use GetUsers() instead!")]
        IQueryable<User> FindUsersByEmail(string emailToMatch);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="userNameToMatch"></param>
        [Obsolete("Please use GetUsers() instead!")]
        IQueryable<User> FindUserByName(string userNameToMatch);

        // G

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        IQueryable<User> GetAllUsers();

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="email"></param>
        User GetUserByEmail(string email);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        /// <param name="isOnline"></param>
        User GetUserById(long id, bool isOnline = false);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="userName"></param>
        /// <param name="isOnline"></param>
        User GetUserByName(string userName, bool isOnline = false);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="email"></param>
        string GetUserNameByEmail(string email);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="id"></param>
        string GetUserNameById(long id);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param>NA</param>       
        int GetUsersOnline();

        // R

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="userName"></param>
        /// <param name="passwordAnswer"></param>
        string ResetPassword(string userName, string passwordAnswer);

        // U

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="userName"></param>
        bool UnlockUser(string userName);

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="user"></param>
        User UpdateUser(User user);

        // V

        /// <summary>
        ///
        /// </summary>
        /// <remarks></remarks>
        /// <seealso cref=""/>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        bool ValidateUser(string userName, string password);

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public enum FailureType
    {
        Password = 1,
        SecurityAnswer = 2
    }

    /// <summary>
    /// 
    /// </summary>
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