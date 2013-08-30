using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using BExIS.Security.Entities;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services
{
    public sealed class UserManager : IUserManager
    {
        public UserManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();

            this.Repo = uow.GetReadOnlyRepository<User>();
        }


        #region Data Readers

        public IReadOnlyRepository<User> Repo { get; private set; }

        #endregion


        #region User

        //
        public bool ChangePassword(string userName, string password, string newPassword, string machineKey, int minPasswordLength)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(userName));
            Contract.Requires(!String.IsNullOrWhiteSpace(password));
            Contract.Requires(!String.IsNullOrWhiteSpace(newPassword));
            Contract.Requires(newPassword.Length >= minPasswordLength);

            // Variables
            User user = GetUserByName(userName);

            if (user != null)
            {
                if (ValidateSecurityProperty(password, user.Password, DecodeSalt(user.Salt, machineKey)))
                {
                    user.LastActivityDate = DateTime.Now;
                    user.LastPasswordChangeDate = DateTime.Now;
                    user.Password = EncodeSecurityProperty(newPassword, DecodeSalt(user.Salt, machineKey));

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

        //
        public bool ChangePasswordQuestionAndPasswordAnswer(string userName, string password, string newPasswordQuestion, string newPasswordAnswer, string machineKey)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(userName));
            Contract.Requires(!String.IsNullOrWhiteSpace(password));
            Contract.Requires(!String.IsNullOrWhiteSpace(newPasswordQuestion));
            Contract.Requires(!String.IsNullOrWhiteSpace(newPasswordAnswer));

            // Variables
            User user = GetUserByName(userName);

            if (user != null)
            {
                if (ValidateSecurityProperty(password, user.Password, DecodeSalt(user.Salt, machineKey)))
                {
                    user.LastActivityDate = DateTime.Now;
                    user.LastPasswordChangeDate = DateTime.Now;
                    user.PasswordAnswer = EncodeSecurityProperty(newPasswordAnswer, DecodeSalt(user.Salt, machineKey));
                    user.PasswordQuestion = newPasswordQuestion;

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

        //
        public User Create(string userName, string email, string password, string passwordQuestion, string passwordAnswer, bool uniqueEmail, int minPasswordLength, string machineKey, out MembershipCreateStatus status, string comment = "", bool isApproved = false)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(userName));
            Contract.Requires(!String.IsNullOrWhiteSpace(email));
            Contract.Requires(!String.IsNullOrWhiteSpace(password));
            Contract.Requires(!String.IsNullOrWhiteSpace(passwordQuestion));
            Contract.Requires(!String.IsNullOrWhiteSpace(passwordAnswer));
            Contract.Requires(!String.IsNullOrWhiteSpace(machineKey));
            Contract.Requires(uniqueEmail != null);
            Contract.Requires(comment != null);
            Contract.Requires(isApproved != null);

            // Variables
            
            // Computations
            if (uniqueEmail && !String.IsNullOrWhiteSpace(GetUserNameByEmail(email)))
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            if (password.Length < minPasswordLength)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }

            User referenceUser = GetUserByName(userName);

            if (referenceUser == null)
            {
                string salt = GenerateSalt();

                User user = new User()
                {
                    // Subject Properties
                    Name = userName,

                    Comment = comment,

                    // User Properties
                    Email = email,

                    Password = EncodeSecurityProperty(password, salt),
                    Salt = EncodeSalt(salt, machineKey),
                    PasswordQuestion = passwordQuestion,
                    PasswordAnswer = EncodeSecurityProperty(passwordAnswer, salt),

                    PasswordFailureCount = 0,
                    PasswordAnswerFailureCount = 0,

                    LastActivityDate = DateTime.Now,
                    LastLockOutDate = DateTime.Now,
                    LastLoginDate = DateTime.Now,
                    LastPasswordAnswerFailureDate = DateTime.Now,
                    LastPasswordChangeDate = DateTime.Now,
                    LastPasswordFailureDate = DateTime.Now,

                    RegistrationDate = DateTime.Now,

                    IsApproved = isApproved,
                    IsLockedOut = false
                };

                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<User> repo = uow.GetRepository<User>();

                    repo.Put(user);
                    uow.Commit();
                }

                status = MembershipCreateStatus.Success;
                return (user);
            }
            else
            {
                status = MembershipCreateStatus.DuplicateUserName;
                return null;
            }
        }

        //
        private string DecodeSalt(string salt, string machineKey)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(salt));
            Contract.Requires(!String.IsNullOrWhiteSpace(machineKey));

            // Variables
            TripleDESCryptoServiceProvider encryptionEncoder = new TripleDESCryptoServiceProvider();

            encryptionEncoder.Mode = CipherMode.ECB;
            encryptionEncoder.Key = Convert.FromBase64String(machineKey);
            encryptionEncoder.Padding = PaddingMode.None;

            ICryptoTransform DESDecrypt = encryptionEncoder.CreateDecryptor();

            // Computations
            return Encoding.Unicode.GetString(DESDecrypt.TransformFinalBlock(Convert.FromBase64String(salt), 0, Convert.FromBase64String(salt).Length));
        }

        //
        public bool Delete(User user)
        {
            // Requirements
            Contract.Requires(user != null);

            // Computations
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<User> repo = uow.GetRepository<User>();

                user = repo.Reload(user);
                repo.Delete(user);
                uow.Commit();
            }

            return (true);
        }

        //
        private string EncodeSalt(string salt, string machineKey)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(salt));
            Contract.Requires(!String.IsNullOrWhiteSpace(machineKey));

            // Variables
            TripleDESCryptoServiceProvider encryptionEncoder = new TripleDESCryptoServiceProvider();

            encryptionEncoder.Mode = CipherMode.ECB;
            byte[] a = Convert.FromBase64String(machineKey);
            encryptionEncoder.Key = Convert.FromBase64String(machineKey);
            encryptionEncoder.Padding = PaddingMode.None;

            ICryptoTransform DESEncrypt = encryptionEncoder.CreateEncryptor();

            // Computations
            return Convert.ToBase64String(DESEncrypt.TransformFinalBlock(Encoding.Unicode.GetBytes(salt), 0, Encoding.Unicode.GetBytes(salt).Length));
        }

        //
        private string EncodeSecurityProperty(string value, string salt)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(value));
            Contract.Requires(!String.IsNullOrWhiteSpace(salt));

            // Variables
            HMACSHA256 encryptionEncoder = new HMACSHA256(Convert.FromBase64String(salt));

            byte[] valueArray = Encoding.Unicode.GetBytes(value);

            // Computations
            return Convert.ToBase64String(encryptionEncoder.ComputeHash(valueArray, 0, valueArray.Length));
        }

        //
        public IQueryable<User> FindUsersByEmail(string emailToMatch)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(emailToMatch));

            // Computations
            return Repo.Query(u => u.Email.ToLower().Contains(emailToMatch.ToLower()));

        }

        //
        public IQueryable<User> FindUserByName(string userNameToMatch)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(userNameToMatch));

            // Computations
            return Repo.Query(u => u.Name.Contains(userNameToMatch));
        }

        //
        private string GeneratePassword(int minPasswordLength)
        {
            // Requirements
            Contract.Requires(minPasswordLength > 0);

            // Variables
            StringBuilder stringBuilder = new StringBuilder();
            string Content = "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!§$%&/()=?*#-";
            Random rnd = new Random();

            // Computations
            for (int i = 0; i < minPasswordLength; i++)
            {
                stringBuilder.Append(Content[rnd.Next(Content.Length)]);
            }

            return stringBuilder.ToString();
        }

        //
        private string GenerateSalt()
        {
            // Variables
            byte[] salt = new byte[32];
            new RNGCryptoServiceProvider().GetBytes(salt);
            
            // Computations
            return Convert.ToBase64String(salt);
        }

        //
        public IQueryable<User> GetAllUsers()
        {            
            return (Repo.Query());
        }

        //
        public string GetPassword(string userName, string passwordAnswer, int maxFailureAttempts, int failureAttemptsWindow, int minPasswordLength, string machineKey)
        {
            throw new NotImplementedException();
        }

        //
        public User GetUserByEmail(string email)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(email));

            // Variables
            if (Repo.Get(u => u.Email.ToLower() == email.ToLower()).Count() == 1)
            {
                return Repo.Get(u => u.Email.ToLower() == email.ToLower()).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        //
        public User GetUserById(long id, bool isOnline = false)
        {
            // Requirements
            Contract.Requires(id >= 0);

            // Variables
            if (Repo.Get(u => u.Id == id).Count() == 1)
            {
                User user = Repo.Get(u => u.Id == id).FirstOrDefault();

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

        //
        public User GetUserByName(string userName, bool isOnline = false)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(userName));

            // Variables
            if (Repo.Get(u => u.Name == userName).Count() == 1)
            {
                User user = Repo.Get(u => u.Name == userName).FirstOrDefault();

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

        //
        public string GetUserNameByEmail(string email)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(email));

            // Variables
            if (Repo.Get(u => u.Email.ToLower() == email.ToLower()).Count() == 1)
            {
                return Repo.Get(u => u.Email.ToLower() == email.ToLower()).FirstOrDefault().Name;
            }
            else
            {
                return String.Empty;
            }
        }

        //
        public int GetUsersOnline()
        {
            // Variables
            TimeSpan timeFrame = new TimeSpan(0, System.Web.Security.Membership.UserIsOnlineTimeWindow, 0);
            DateTime referenceTime = DateTime.Now.Subtract(timeFrame);

            // Computations
            return Repo.Get(u => (DateTime.Compare(u.LastActivityDate, referenceTime) > 0)).Count();
        }

        //
        public string ResetPassword(string userName, string passwordAnswer, int maxFailureAttempts, int failureAttemptsWindow, int minPasswordLength, string machineKey)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(userName));
            Contract.Requires(!String.IsNullOrWhiteSpace(passwordAnswer));
            Contract.Requires(!String.IsNullOrWhiteSpace(machineKey));
            Contract.Requires(maxFailureAttempts > 0);
            Contract.Requires(failureAttemptsWindow >= 0);
            Contract.Requires(minPasswordLength > 0);

            // Variables
            string password = String.Empty;

            // Variables
            User user = GetUserByName(userName);

            // Compuations
            if (user != null)
            {
                string salt = DecodeSalt(user.Salt, machineKey);

                if (ValidateSecurityProperty(passwordAnswer, user.PasswordAnswer, salt))
                {
                    password = GeneratePassword(minPasswordLength);

                    user.Password = EncodeSecurityProperty(password, salt);
                    user.LastPasswordChangeDate = DateTime.Now;

                    using (IUnitOfWork uow = this.GetUnitOfWork())
                    {
                        IRepository<User> repo = uow.GetRepository<User>();
                        repo.Put(user);
                        uow.Commit();
                    }
                }
                else
                {
                    UpdateFailureCount(user, FailureType.PasswordAnswer, maxFailureAttempts, failureAttemptsWindow);
                }
            }

            return password;
        }

        //
        public bool UnlockUser(string userName)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(userName));

            // Variables
            User user = GetUserByName(userName);

            // Computations
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

        //
        public User Update(User user)
        {
            Contract.Requires(user != null);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<User> repo = uow.GetRepository<User>();
                repo.Put(user);
                uow.Commit();
            }

            return (user);
        }

        //
        private void UpdateFailureCount(User user, FailureType failureType, int maxFailureAttempts, int failureAttemptsWindow)
        {
            // Requirements
            Contract.Requires(user != null);
            Contract.Requires(failureType != null);
            Contract.Requires(maxFailureAttempts > 0);
            Contract.Requires(failureAttemptsWindow >= 0);

            // Variables
            TimeSpan timeFrame = new TimeSpan(0, failureAttemptsWindow, 0);
            DateTime referenceTime = DateTime.Now.Subtract(timeFrame);

            user = Repo.Reload(user);

            // Computations
            switch(failureType)
            {
                case FailureType.Password:
                    if (DateTime.Compare(user.LastPasswordFailureDate, referenceTime) > 0)
                    {
                        user.LastPasswordFailureDate = DateTime.Now;
                        user.PasswordFailureCount = user.PasswordFailureCount + 1;

                        if (user.PasswordFailureCount == maxFailureAttempts)
                        {
                            user.LastLockOutDate = DateTime.Now;
                            user.IsLockedOut = true;
                        }

                        using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                            IRepository<User> repo = uow.GetRepository<User>();
                            repo.Put(user);
                            uow.Commit();
                        }
                }
                    else
                    {
                        user.LastPasswordFailureDate = DateTime.Now;
                        user.PasswordFailureCount = 1;

                        using (IUnitOfWork uow = this.GetUnitOfWork())
                        {
                            IRepository<User> repo = uow.GetRepository<User>();
                            repo.Put(user);
                uow.Commit();
            }
                    }
                        
                    break;
                case FailureType.PasswordAnswer:
                    if (DateTime.Compare(user.LastPasswordAnswerFailureDate, referenceTime) > 0)
                    {
                        user.LastActivityDate = DateTime.Now;
                        user.LastPasswordAnswerFailureDate = DateTime.Now;
                        user.PasswordAnswerFailureCount = user.PasswordAnswerFailureCount + 1;

                        if (user.PasswordAnswerFailureCount == maxFailureAttempts)
                        {
                            user.LastLockOutDate = DateTime.Now;
                            user.IsLockedOut = true;
        }

                        using (IUnitOfWork uow = this.GetUnitOfWork())
                        {
                            IRepository<User> repo = uow.GetRepository<User>();
                            repo.Put(user);
                            uow.Commit();
                        }
                    }
                    else
        {
                        user.LastActivityDate = DateTime.Now;
                        user.LastPasswordAnswerFailureDate = DateTime.Now;
                        user.PasswordAnswerFailureCount = 1;

                        using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<User> repo = uow.GetRepository<User>();
                            repo.Put(user);
                uow.Commit();
            }
                    }

                    break;
                default:
                    break;
            }
        }

        //
        private bool ValidateSecurityProperty(string value, string referenceValue, string salt)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(value));
            Contract.Requires(!String.IsNullOrWhiteSpace(referenceValue));
            Contract.Requires(!String.IsNullOrWhiteSpace(salt));

            // Computations
            if (referenceValue == EncodeSecurityProperty(value, salt))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //
        public bool ValidateUser(string userName, string password, int maxFailureAttempts, int failureAttemptsWindow, string machineKey)
        {
            // Requirements
            Contract.Requires(!String.IsNullOrWhiteSpace(userName));
            Contract.Requires(!String.IsNullOrWhiteSpace(password));
            Contract.Requires(maxFailureAttempts > 0);
            Contract.Requires(failureAttemptsWindow >= 0);
            Contract.Requires(!String.IsNullOrWhiteSpace(machineKey));

            // Variables
            bool validation = false;
            User user = GetUserByName(userName, true);

            if (user != null)
            {
                if (user.PasswordFailureCount <= maxFailureAttempts)
                {
                    if (user.IsApproved && !user.IsLockedOut)
                    {
                        if (ValidateSecurityProperty(password, user.Password, DecodeSalt(user.Salt, machineKey)))
                        {
                            validation = true;

                            user.LastLoginDate = DateTime.Now;

                            using (IUnitOfWork uow = this.GetUnitOfWork())
                            {
                                IRepository<User> repo = uow.GetRepository<User>();
                                repo.Put(user);
                                uow.Commit();
                            }
                        }
                        else
                        {
                            UpdateFailureCount(user, FailureType.Password, maxFailureAttempts, failureAttemptsWindow);
                        }
                    }
                }
            }

            return validation;
        }

        #endregion
    }
}
