using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BExIS.Security.Entities;
using Vaiona.Persistence.Api;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Web.Security;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using BExIS.Security.Entities.Subjects;

namespace BExIS.Security.Services.Subjects
{
    public sealed class UserManager : IUserManager
    {
        public UserManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();

            this.UsersRepo = uow.GetReadOnlyRepository<User>();
            this.SecurityUsersRepo = uow.GetReadOnlyRepository<SecurityUser>();
        }


        #region Data Readers

        public IReadOnlyRepository<User> UsersRepo { get; private set; }
        public IReadOnlyRepository<SecurityUser> SecurityUsersRepo { get; private set; }

        #endregion


        #region Attributes

        public bool AutoApproval
        {
            get { return true; }
        }

        public string MachineKey
        {
            get { return "qwertzuioplkjhgfdsayxcvbqwertztu"; }
        }

        public int MaxPasswordFailureAttempts
        {
            get { return 10; }
        }

        public int MaxSecurityAnswerFailureAttempts
        {
            get { return 10; }
        }

        public int PasswordFailureAttemptsWindow
        {
            get { return 30; }
        }

        public string PasswordStrengthRegularExpression
        {
            get { return @"((?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[\W]).{6,20})"; }
        }

        public bool RequiresUniqueEmail
        {
            get { return true; }
        }

        public int SecurityAnswerFailureAttemptsWindow
        {
            get { return 30; }
        }

        #endregion


        #region Methods

        public bool ApproveUser(User user)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public bool ChangePassword(User user, string password, string newPassword)
        {
            SecurityUser securityUser = GetSecurityUserByName(user.Name);

            if (securityUser != null && user != null)
            {
                if (ValidateSecurityProperty(password, securityUser.Password, DecodeSalt(securityUser.PasswordSalt)))
                {
                    string passwordSalt = GenerateSalt();

                    user.LastActivityDate = DateTime.Now;
                    user.LastPasswordChangeDate = DateTime.Now;
                    securityUser.Password = EncodeSecurityProperty(newPassword, passwordSalt);
                    securityUser.PasswordSalt = EncodeSalt(passwordSalt);

                    using (IUnitOfWork uow = this.GetUnitOfWork())
                    {
                        IRepository<User> userRepo = uow.GetRepository<User>();
                        IRepository<SecurityUser> securityUserRepo = uow.GetRepository<SecurityUser>();

                        userRepo.Put(user);
                        securityUserRepo.Put(securityUser);

                        uow.Commit();
                    }

                    return (true);
                }
                else
                {
                    return (false);
                }
            }
            else
            {
                return (false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <param name="newPasswordQuestion"></param>
        /// <param name="newPasswordAnswer"></param>
        /// <returns></returns>
        public bool ChangeSecurityQuestionAndSecurityAnswer(User user, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            SecurityUser securityUser = GetSecurityUserByName(user.Name);

            if (securityUser != null && user != null)
            {
                if (ValidateSecurityProperty(password, securityUser.Password, DecodeSalt(securityUser.SecurityAnswerSalt)))
                {
                    string securityAnswerSalt = GenerateSalt();

                    user.LastActivityDate = DateTime.Now;
                    user.LastPasswordChangeDate = DateTime.Now;
                    securityUser.SecurityAnswer = EncodeSecurityProperty(newPasswordAnswer, securityAnswerSalt);
                    securityUser.SecurityAnswerSalt = EncodeSalt(securityAnswerSalt);
                    securityUser.SecurityQuestion = newPasswordQuestion;

                    using (IUnitOfWork uow = this.GetUnitOfWork())
                    {
                        IRepository<User> repo = uow.GetRepository<User>();
                        repo.Put(user);
                        uow.Commit();
                    }

                    return (true);
                }
                else
                {
                    return (false);
                }
            }
            else
            {
                return (false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="passwordQuestion"></param>
        /// <param name="passwordAnswer"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        public User Create(string userName, string email, string password, string passwordQuestion, string passwordAnswer, out UserCreateStatus status)
        {
            if (RequiresUniqueEmail && !String.IsNullOrWhiteSpace(GetUserNameByEmail(email)))
            {
                status = UserCreateStatus.DuplicateEmail;
                return null;
            }

            if (!Regex.Match(password, PasswordStrengthRegularExpression).Success)
            {
                status = UserCreateStatus.InvalidPassword;
                return null;
            }

            if (GetUserByName(userName) != null)
            {
                status = UserCreateStatus.DuplicateUserName;
                return null;
            }

            string passwordSalt = GenerateSalt();
            string securityAnswerSalt = GenerateSalt();

            User user = new User()
            {
                // Subject Properties
                Name = userName,

                // User Properties
                Email = email,

                LastActivityDate = DateTime.Now,
                LastLockOutDate = DateTime.Now,
                LastLoginDate = DateTime.Now,
                LastPasswordChangeDate = DateTime.Now,
                RegistrationDate = DateTime.Now,
                IsApproved = AutoApproval,
                IsLockedOut = false
            };

            SecurityUser securityUser = new SecurityUser()
            {
                Name = userName,

                Password = EncodeSecurityProperty(password, passwordSalt),
                PasswordSalt = EncodeSalt(passwordSalt),
                SecurityQuestion = passwordQuestion,
                SecurityAnswer = EncodeSecurityProperty(passwordAnswer, securityAnswerSalt),
                SecurityAnswerSalt = EncodeSalt(securityAnswerSalt),
                PasswordFailureCount = 0,
                SecurityAnswerFailureCount = 0,
                LastSecurityAnswerFailureDate = DateTime.Now,
                LastPasswordFailureDate = DateTime.Now
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<User> userRepo = uow.GetRepository<User>();
                IRepository<SecurityUser> securityUserRepo = uow.GetRepository<SecurityUser>();

                userRepo.Put(user);
                securityUserRepo.Put(securityUser);

                uow.Commit();
            }

            status = UserCreateStatus.Success;
            return (user);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="salt"></param>
        /// <returns></returns>
        private string DecodeSalt(string salt)
        {
            // Variables
            TripleDESCryptoServiceProvider encryptionEncoder = new TripleDESCryptoServiceProvider();

            encryptionEncoder.Mode = CipherMode.ECB;
            encryptionEncoder.Key = Convert.FromBase64String(MachineKey);
            encryptionEncoder.Padding = PaddingMode.None;

            ICryptoTransform DESDecrypt = encryptionEncoder.CreateDecryptor();

            // Computations
            return Encoding.Unicode.GetString(DESDecrypt.TransformFinalBlock(Convert.FromBase64String(salt), 0, Convert.FromBase64String(salt).Length));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public bool Delete(User user)
        {
            // Computations
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<User> userRepo = uow.GetRepository<User>();
                IRepository<Role> roleRepo = uow.GetRepository<Role>();

                IRepository<SecurityUser> securityUserRepo = uow.GetRepository<SecurityUser>();

                user = userRepo.Reload(user);
                userRepo.LoadIfNot(user.Roles);

                SecurityUser securityUser = GetSecurityUserByName(user.Name);

                foreach (Role role in user.Roles)
                {
                    roleRepo.LoadIfNot(role.Users);
                    role.Users.Remove(user);
                }

                userRepo.Delete(user);
                securityUserRepo.Delete(securityUser);

                uow.Commit();
            }

            return (true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="salt"></param>
        /// <returns></returns>
        private string EncodeSalt(string salt)
        {
            // Variables
            TripleDESCryptoServiceProvider encryptionEncoder = new TripleDESCryptoServiceProvider();

            encryptionEncoder.Mode = CipherMode.ECB;
            byte[] a = Convert.FromBase64String(MachineKey);
            encryptionEncoder.Key = Convert.FromBase64String(MachineKey);
            encryptionEncoder.Padding = PaddingMode.None;

            ICryptoTransform DESEncrypt = encryptionEncoder.CreateEncryptor();

            // Computations
            return Convert.ToBase64String(DESEncrypt.TransformFinalBlock(Encoding.Unicode.GetBytes(salt), 0, Encoding.Unicode.GetBytes(salt).Length));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        private string EncodeSecurityProperty(string value, string salt)
        {
            // Variables
            HMACSHA256 encryptionEncoder = new HMACSHA256(Convert.FromBase64String(salt));

            byte[] valueArray = Encoding.Unicode.GetBytes(value);

            // Computations
            return Convert.ToBase64String(encryptionEncoder.ComputeHash(valueArray, 0, valueArray.Length));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool ExistsUserName(string userName)
        {
            Contract.Requires(!String.IsNullOrWhiteSpace(userName));

            if (GetUserByName(userName) != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="emailToMatch"></param>
        /// <returns></returns>
        public IQueryable<User> FindUsersByEmail(string emailToMatch)
        {
            return UsersRepo.Query(u => u.Email.ToLower().Contains(emailToMatch.ToLower()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userNameToMatch"></param>
        /// <returns></returns>
        public IQueryable<User> FindUserByName(string userNameToMatch)
        {
            return UsersRepo.Query(u => u.Name.Contains(userNameToMatch));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GeneratePassword()
        {
            // Variables
            StringBuilder stringBuilder = new StringBuilder();
            string Content = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!§$%&/()=?*#-";
            Random rnd = new Random();

            // Computations
            for (int i = 0; i < 8; i++)
            {
                stringBuilder.Append(Content[rnd.Next(Content.Length)]);
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GenerateSalt()
        {
            // Variables
            byte[] salt = new byte[32];
            new RNGCryptoServiceProvider().GetBytes(salt);

            // Computations
            return Convert.ToBase64String(salt);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IQueryable<User> GetAllUsers()
        {
            return (UsersRepo.Query());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        private SecurityUser GetSecurityUserByName(string userName)
        {
            if (SecurityUsersRepo.Get(u => u.Name == userName).Count() == 1)
            {
                return SecurityUsersRepo.Get(u => u.Name == userName).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public User GetUserByEmail(string email)
        {
            if (UsersRepo.Get(u => u.Email.ToLower() == email.ToLower()).Count() == 1)
            {
                return UsersRepo.Get(u => u.Email.ToLower() == email.ToLower()).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isOnline"></param>
        /// <returns></returns>
        public User GetUserById(long id, bool isOnline = false)
        {
            if (UsersRepo.Get(u => u.Id == id).Count() == 1)
            {
                User user = UsersRepo.Get(u => u.Id == id).FirstOrDefault();

                if (user != null && isOnline)
                {
                    user.LastActivityDate = DateTime.Now;

                    using (IUnitOfWork uow = this.GetUnitOfWork())
                    {
                        IRepository<User> repo = uow.GetRepository<User>();
                        repo.Put(user);
                        uow.Commit();
                    }
                }

                return user;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="isOnline"></param>
        /// <returns></returns>
        public User GetUserByName(string userName, bool isOnline = false)
        {
            if (UsersRepo.Get(u => u.Name == userName).Count() == 1)
            {
                User user = UsersRepo.Get(u => u.Name == userName).FirstOrDefault();

                if (user != null && isOnline)
                {
                    user.LastActivityDate = DateTime.Now;

                    using (IUnitOfWork uow = this.GetUnitOfWork())
                    {
                        IRepository<User> repo = uow.GetRepository<User>();
                        repo.Put(user);
                        uow.Commit();
                    }
                }

                return user;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public string GetUserNameByEmail(string email)
        {
            if (UsersRepo.Get(u => u.Email.ToLower() == email.ToLower()).Count() == 1)
            {
                return UsersRepo.Get(u => u.Email.ToLower() == email.ToLower()).FirstOrDefault().Name;
            }
            else
            {
                return String.Empty;
            }
        }

        public string GetUserNameById(long id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetUsersOnline()
        {
            TimeSpan timeFrame = new TimeSpan(0, System.Web.Security.Membership.UserIsOnlineTimeWindow, 0);
            DateTime referenceTime = DateTime.Now.Subtract(timeFrame);

            return UsersRepo.Get(u => (DateTime.Compare(u.LastActivityDate, referenceTime) > 0)).Count();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="passwordAnswer"></param>
        /// <returns></returns>
        public string ResetPassword(User user, string passwordAnswer)
        {
            SecurityUser securityUser = GetSecurityUserByName(user.Name);

            string password = String.Empty;

            if (user != null && securityUser != null)
            {
                if (ValidateSecurityProperty(passwordAnswer, securityUser.SecurityAnswer, DecodeSalt(securityUser.SecurityAnswerSalt)))
                {
                    password = GeneratePassword();

                    string passwordSalt = GenerateSalt(); 

                    securityUser.Password = EncodeSecurityProperty(password, passwordSalt);
                    securityUser.PasswordSalt = EncodeSalt(passwordSalt);
                    user.LastPasswordChangeDate = DateTime.Now;

                    using (IUnitOfWork uow = this.GetUnitOfWork())
                    {
                        IRepository<User> userRepo = uow.GetRepository<User>();
                        IRepository<SecurityUser> securityUserRepo = uow.GetRepository<SecurityUser>();

                        userRepo.Put(user);
                        securityUserRepo.Put(securityUser);

                        uow.Commit();
                    }
                }
                else
                {
                    UpdateFailureCount(user, FailureType.SecurityAnswer);
                }
            }

            return password;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool UnlockUser(User user)
        {
            if (user != null)
            {
                user.IsLockedOut = false;

                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<User> repo = uow.GetRepository<User>();

                    repo.Put(user);
                    uow.Commit();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public User Update(User user)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<User> repo = uow.GetRepository<User>();
                repo.Put(user);
                uow.Commit();
            }

            return (user);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="failureType"></param>
        private void UpdateFailureCount(User user, FailureType failureType)
        {
            user = UsersRepo.Reload(user);
            SecurityUser securityUser = GetSecurityUserByName(user.Name);

            if (user != null && securityUser != null)
            {
                TimeSpan timeFrame;
                DateTime referenceTime;

                switch (failureType)
                {
                    case FailureType.Password:
                        timeFrame = new TimeSpan(0, PasswordFailureAttemptsWindow, 0);
                        referenceTime = DateTime.Now.Subtract(timeFrame);

                        if (DateTime.Compare(securityUser.LastPasswordFailureDate, referenceTime) > 0)
                        {
                            securityUser.LastPasswordFailureDate = DateTime.Now;
                            securityUser.PasswordFailureCount = securityUser.PasswordFailureCount + 1;

                            if (securityUser.PasswordFailureCount == MaxPasswordFailureAttempts)
                            {
                                user.LastLockOutDate = DateTime.Now;
                                user.IsLockedOut = true;
                            }

                            using (IUnitOfWork uow = this.GetUnitOfWork())
                            {
                                IRepository<User> userRepo = uow.GetRepository<User>();
                                IRepository<SecurityUser> securityUserRepo = uow.GetRepository<SecurityUser>();

                                userRepo.Put(user);
                                securityUserRepo.Put(securityUser);

                                uow.Commit();
                            }
                        }
                        else
                        {
                            securityUser.LastPasswordFailureDate = DateTime.Now;
                            securityUser.PasswordFailureCount = 1;

                            using (IUnitOfWork uow = this.GetUnitOfWork())
                            {
                                IRepository<User> userRepo = uow.GetRepository<User>();
                                IRepository<SecurityUser> securityUserRepo = uow.GetRepository<SecurityUser>();

                                userRepo.Put(user);
                                securityUserRepo.Put(securityUser);

                                uow.Commit();
                            }
                        }

                        break;
                    case FailureType.SecurityAnswer:
                        timeFrame = new TimeSpan(0, SecurityAnswerFailureAttemptsWindow, 0);
                        referenceTime = DateTime.Now.Subtract(timeFrame);

                        if (DateTime.Compare(securityUser.LastSecurityAnswerFailureDate, referenceTime) > 0)
                        {
                            user.LastActivityDate = DateTime.Now;
                            securityUser.LastSecurityAnswerFailureDate = DateTime.Now;
                            securityUser.SecurityAnswerFailureCount = securityUser.SecurityAnswerFailureCount + 1;

                            if (securityUser.SecurityAnswerFailureCount == MaxSecurityAnswerFailureAttempts)
                            {
                                user.LastLockOutDate = DateTime.Now;
                                user.IsLockedOut = true;
                            }

                            using (IUnitOfWork uow = this.GetUnitOfWork())
                            {
                                IRepository<User> userRepo = uow.GetRepository<User>();
                                IRepository<SecurityUser> securityUserRepo = uow.GetRepository<SecurityUser>();

                                userRepo.Put(user);
                                securityUserRepo.Put(securityUser);

                                uow.Commit();
                            }
                        }
                        else
                        {
                            user.LastActivityDate = DateTime.Now;
                            securityUser.LastSecurityAnswerFailureDate = DateTime.Now;
                            securityUser.SecurityAnswerFailureCount = 1;

                            using (IUnitOfWork uow = this.GetUnitOfWork())
                            {
                                IRepository<User> userRepo = uow.GetRepository<User>();
                                IRepository<SecurityUser> securityUserRepo = uow.GetRepository<SecurityUser>();

                                userRepo.Put(user);
                                securityUserRepo.Put(securityUser);

                                uow.Commit();
                            }
                        }

                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="referenceValue"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        private bool ValidateSecurityProperty(string value, string referenceValue, string salt)
        {
            if (referenceValue == EncodeSecurityProperty(value, salt))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool ValidateUser(string userName, string password)
        {
            bool validation = false;

            User user = GetUserByName(userName, true);
            SecurityUser securityUser = GetSecurityUserByName(user.Name);

            if (user != null && securityUser != null)
            {
                if (securityUser.PasswordFailureCount <= MaxPasswordFailureAttempts)
                {
                    if (user.IsApproved && !user.IsLockedOut)
                    {
                        if (ValidateSecurityProperty(password, securityUser.Password, DecodeSalt(securityUser.PasswordSalt)))
                        {
                            validation = true;

                            user.LastLoginDate = DateTime.Now;

                            using (IUnitOfWork uow = this.GetUnitOfWork())
                            {
                                IRepository<User> userRepo = uow.GetRepository<User>();
                                IRepository<SecurityUser> securityUserRepo = uow.GetRepository<SecurityUser>();

                                userRepo.Put(user);
                                securityUserRepo.Put(securityUser);

                                uow.Commit();
                            }
                        }
                        else
                        {
                            UpdateFailureCount(user, FailureType.Password);
                        }
                    }
                }
            }

            return validation;
        }

        #endregion
    }
}