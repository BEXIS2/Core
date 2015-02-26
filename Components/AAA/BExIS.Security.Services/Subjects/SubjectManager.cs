using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using BExIS.Security.Entities.Authentication;
using BExIS.Security.Entities.Objects;
using BExIS.Security.Entities.Subjects;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Subjects
{      
    public sealed class SubjectManager : ISubjectManager
    {      
        public SubjectManager()
        {
            IUnitOfWork uow = this.GetUnitOfWork();

            this.AuthenticatorsRepo = uow.GetReadOnlyRepository<Authenticator>();
            this.GroupsRepo = uow.GetReadOnlyRepository<Group>();
            this.SecurityQuestionsRepo = uow.GetReadOnlyRepository<SecurityQuestion>();
            this.SubjectsRepo = uow.GetReadOnlyRepository<Subject>();
            this.UsersRepo = uow.GetReadOnlyRepository<User>();
        }

        #region Data Readers

        public IReadOnlyRepository<Authenticator> AuthenticatorsRepo { get; private set; }
        public IReadOnlyRepository<Group> GroupsRepo { get; private set; }
        public IReadOnlyRepository<SecurityQuestion> SecurityQuestionsRepo { get; private set; }
        public IReadOnlyRepository<Subject> SubjectsRepo { get; private set; }
        public IReadOnlyRepository<User> UsersRepo { get; private set; }      

        #endregion

        #region Methods

        public IQueryable<Subject> GetAllSubjects()
        {
            return SubjectsRepo.Query(s => s.IsSystemSubject == false);
        }

        public bool AddUserToGroup(string userName, string groupName)
        {
            User user = GetUserByName(userName);
            Group group = GetGroupByName(groupName);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Group> repo = uow.GetRepository<Group>();

                repo.LoadIfNot(group.Users);
                if (!group.Users.Contains(user))
                {
                    group.Users.Add(user);
                    user.Groups.Add(group);
                    uow.Commit();

                    return (true);
                }

                return (false);
            }
        }

        public bool AddUserToGroup(long userId, long groupId)
        {
            User user = GetUserById(userId);
            Group group = GetGroupById(groupId);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Group> repo = uow.GetRepository<Group>();

                repo.LoadIfNot(group.Users);
                if (!group.Users.Contains(user))
                {
                    group.Users.Add(user);
                    user.Groups.Add(group);
                    uow.Commit();

                    return (true);
                }

                return (false);
            }
        }

        public Group CreateGroup(string groupName, string description, bool isSystemSubject = false)
        {
            Group group = new Group()
            {
                Name = groupName,
                Description = description,
                IsSystemSubject = isSystemSubject
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Group> featuresRepo = uow.GetRepository<Group>();
                featuresRepo.Put(group);

                uow.Commit();
            }

            return (group);
        }

        public bool DeleteGroupByName(string groupName)
        {
            Group group = GetGroupByName(groupName);

            if (group != null)
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Group> featurePermissionsRepo = uow.GetRepository<Group>();

                    featurePermissionsRepo.Delete(group);
                    uow.Commit();
                }

                return (true);
            }

            return (false);
        }

        public bool DeleteGroupById(long id)
        {
            Group group = GetGroupById(id);

            if (group != null)
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Group> featurePermissionsRepo = uow.GetRepository<Group>();

                    featurePermissionsRepo.Delete(group);
                    uow.Commit();
                }

                return (true);
            }

            return (false);
        }

        public bool ExistsGroupId(long id)
        {
            return GroupsRepo.Get(id) != null ? true : false;
        }

        public bool ExistsGroupName(string groupName)
        {
            return GroupsRepo.Get(g => g.Name.ToLower() == groupName.ToLower()).Count == 1 ? true : false;
        }

        public IQueryable<Group> GetAllGroups()
        {
            return GroupsRepo.Query(g => g.IsSystemSubject == false);
        }

        public Group GetGroupByName(string groupName)
        {
            return GroupsRepo.Get(g => g.Name.ToLower() == groupName.ToLower()).FirstOrDefault();
        }

        public Group GetGroupById(long id)
        {
            return GroupsRepo.Get(id);
        }

        public string GetGroupNameById(long id)
        {
            return GroupsRepo.Get(id).Name;
        }

        public IQueryable<Group> GetGroupsFromUserName(string userName)
        {
            return GroupsRepo.Query(g => g.Users.Any(u => u.Name.ToLower() == userName.ToLower()));
        }

        public IQueryable<User> GetUsersFromGroupName(string groupName)
        {
            return UsersRepo.Query(u => u.Groups.Any(g => g.Name.ToLower() == groupName.ToLower()));
        }

        public IQueryable<User> GetUsersFromGroupId(long groupId)
        {
            return UsersRepo.Query(u => u.Groups.Any(g => g.Id == groupId));
        }

        public bool IsGroupInUse(string groupName)
        {
            if (GetUsersFromGroupName(groupName).Count() == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsUserInGroup(long userId, long groupId)
        {
            if (GetUsersFromGroupId(groupId).Where(u => u.Id == userId).Count() == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool IsUserInGroup(string userName, string groupName)
        {
            if (GetUsersFromGroupName(groupName).Where(u => u.Name.ToLower() == userName.ToLower()).Count() == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool RemoveUserFromGroup(string userName, string groupName)
        {
            User user = GetUserByName(userName);
            Group group = GetGroupByName(groupName);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Group> repo = uow.GetRepository<Group>();

                repo.LoadIfNot(group.Users);
                if (group.Users.Contains(user))
                {
                    group.Users.Remove(user);
                    user.Groups.Remove(group);
                    uow.Commit();

                    return (true);
                }

                return (false);
            }
        }

        public bool RemoveUserFromGroup(long userId, long groupId)
        {
            User user = GetUserById(userId);
            Group group = GetGroupById(groupId);

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Group> repo = uow.GetRepository<Group>();

                repo.LoadIfNot(group.Users);
                if (group.Users.Contains(user))
                {
                    group.Users.Remove(user);
                    user.Groups.Remove(group);
                    uow.Commit();

                    return (true);
                }

                return (false);
            }
        }

        public Group UpdateGroup(Group group)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<Group> groupsRepo = uow.GetRepository<Group>();
                groupsRepo.Put(group);
                uow.Commit();
            }

            return (group);
        }

        public bool AutoActivation
        {
            get { return true; }
        }

        public bool AutoApproval
        {
            get { return true; }
        }

        public int MaxPasswordFailureAttempts
        {
            get { return 10; }
        }

        public int MaxSecurityAnswerFailureAttempts
        {
            get { return 10; }
        }

        public int OnlineWindow
        {
            get { return 15; }
        }

        public int PasswordFailureAttemptsWindow
        {
            get { return 30; }
        }

        public int SaltLength
        {
            get { return 64; }
        }

        public int SecurityAnswerFailureAttemptsWindow
        {
            get { return 30; }
        }

        public bool ActivateUser(string userName)
        {
            throw new NotImplementedException();
        }

        public bool ApproveUser(string userName)
        {
            throw new NotImplementedException();
        }

        public bool ChangePassword(long id, string password)
        {
            User user = GetUserById(id);

            if (user != null)
            {
                user.Password = hashSecurityProperty(password, user.PasswordSalt);
                user.LastPasswordChangeDate = DateTime.Now;
                user.PasswordFailureCount = 0;
                user.LastPasswordFailureDate = DateTime.Now;

                UpdateUser(user);
                return true;
            }

            return false;
        }

        public bool ChangeSecurityQuestionAndSecurityAnswer(string userName, string password, long securityQuestionId, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public User CreateUser(string userName, long authenticatorId)
        {
            User user = new User()
            {
                Name = userName,
                Authenticator = AuthenticatorsRepo.Get(authenticatorId),

                LastActivityDate = DateTime.Now,
                LastLockOutDate = DateTime.Now,
                LastLoginDate = DateTime.Now,
                LastPasswordChangeDate = DateTime.Now,
                RegistrationDate = DateTime.Now,

                IsApproved = AutoApproval,
                IsBlocked = false,
                IsLockedOut = false,

                PasswordFailureCount = 0,
                SecurityAnswerFailureCount = 0,
                LastSecurityAnswerFailureDate = DateTime.Now,
                LastPasswordFailureDate = DateTime.Now
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<User> usersRepo = uow.GetRepository<User>();
                usersRepo.Put(user);

                uow.Commit();
            }

            return (user);
        }

        public User CreateUser(string userName, string password, string fullName, string email, long securityQuestionId, string securityAnswer, long authenticatorId)
        {
            string passwordSalt = generateSalt(SaltLength);
            string securityAnswerSalt = generateSalt(SaltLength);

            User user = new User()
            {
                Name = userName,
                FullName = fullName,
                Email = email,
                Password = hashSecurityProperty(password, passwordSalt),
                PasswordSalt = passwordSalt,
                SecurityQuestion = SecurityQuestionsRepo.Get(securityQuestionId),
                SecurityAnswer = hashSecurityProperty(securityAnswer, securityAnswerSalt),
                SecurityAnswerSalt = securityAnswerSalt,

                LastActivityDate = DateTime.Now,
                LastLockOutDate = DateTime.Now,
                LastLoginDate = DateTime.Now,
                LastPasswordChangeDate = DateTime.Now,
                RegistrationDate = DateTime.Now,

                IsApproved = AutoApproval,
                IsBlocked = false,
                IsLockedOut = false,

                PasswordFailureCount = 0,
                SecurityAnswerFailureCount = 0,
                LastSecurityAnswerFailureDate = DateTime.Now,
                LastPasswordFailureDate = DateTime.Now,

                Authenticator = AuthenticatorsRepo.Get(authenticatorId)
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<User> usersRepo = uow.GetRepository<User>();
                usersRepo.Put(user);

                uow.Commit();
            }

            Group group = GetGroupByName("everyone");

            AddUserToGroup(user.Id, group.Id); 

            return (user);
        }

        public bool DeleteUserById(long id)
        {
            User user = UsersRepo.Get(id);

            if (user != null)
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Group> groupsRepo = uow.GetRepository<Group>();
                    IRepository<User> usersRepo = uow.GetRepository<User>();

                    foreach (Group group in user.Groups)
                    {
                        groupsRepo.LoadIfNot(group.Users);
                        group.Users.Remove(user);
                    }

                    usersRepo.Delete(user);

                    uow.Commit();
                }

                return (true);
            }

            return (false);
        }

        public bool DeleteUserByName(string userName)
        {
            User user = GetUserByName(userName);

            if (user != null)
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    IRepository<Group> groupsRepo = uow.GetRepository<Group>();
                    IRepository<User> usersRepo = uow.GetRepository<User>();

                    foreach (Group group in user.Groups)
                    {
                        groupsRepo.LoadIfNot(group.Users);
                        group.Users.Remove(user);
                    }

                    usersRepo.Delete(user);

                    uow.Commit();
                }

                return (true);
            }

            return (false);
        }

        public bool ExistsUserId(long id)
        {
            return UsersRepo.Get(id) != null ? true : false;
        }

        public bool ExistsUserName(string userName)
        {
            return UsersRepo.Get(u => u.Name.ToLower() == userName.ToLower()).Count == 1 ? true : false;
        }

        public bool ExistsUserNameWithAuthenticatorId(string userName, long authenticatorId)
        {
            return UsersRepo.Get(u => u.Name.ToLower() == userName.ToLower() && u.Authenticator.Id == authenticatorId).Count == 1 ? true : false;
        }

        private string generateSalt(int size)
        {
            RNGCryptoServiceProvider encryptionProvider = new RNGCryptoServiceProvider();

            byte[] salt = new byte[size];
            encryptionProvider.GetBytes(salt);

            return Convert.ToBase64String(salt);
        }

        public IQueryable<User> GetAllUsers()
        {
            return UsersRepo.Query(u => u.IsSystemSubject == false);
        }

        public User GetUserByEmail(string email)
        {
            return UsersRepo.Get(u => u.Email.ToLower() == email.ToLower()).FirstOrDefault();
        }

        public User GetUserById(long id, bool isOnline = false)
        {
            User user = UsersRepo.Get(id);

            if (user != null)
            {
                if(isOnline)
                {
                    user.LastActivityDate = DateTime.Now;

                    using (IUnitOfWork uow = this.GetUnitOfWork())
                    {
                        IRepository<User> usersRepo = uow.GetRepository<User>();
                        usersRepo.Put(user);
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

        public User GetUserByName(string userName, bool isOnline = false)
        {
            ICollection<User> users = UsersRepo.Query(u => u.Name.ToLower() == userName.ToLower()).ToArray();

            if (users.Count() == 1)
            {
                User user = users.FirstOrDefault();

                if (user != null && isOnline)
                {
                    user.LastActivityDate = DateTime.Now;

                    using (IUnitOfWork uow = this.GetUnitOfWork())
                    {
                        IRepository<User> usersRepo = uow.GetRepository<User>();
                        usersRepo.Put(user);
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

        public string GetUserNameByEmail(string email)
        {
            return UsersRepo.Get(u => u.Email.ToLower() == email.ToLower()).FirstOrDefault().Name;
        }

        public string GetUserNameById(long id)
        {
            return UsersRepo.Get(id).Name;
        }

        public int GetUsersOnline()
        {
            TimeSpan timeFrame = new TimeSpan(0, OnlineWindow, 0);
            DateTime referenceTime = DateTime.Now.Subtract(timeFrame);

            return UsersRepo.Query(u => (DateTime.Compare(u.LastActivityDate, referenceTime) > 0)).Count();
        }

        private string hashSecurityProperty(string securityProperty, string salt)
        {
            HashAlgorithm hashAlgorithm = new SHA512CryptoServiceProvider();

            byte[] value = System.Text.Encoding.UTF8.GetBytes(securityProperty + salt);

            byte[] hash = hashAlgorithm.ComputeHash(value);

            return Convert.ToBase64String(hash);
        }

        public User RegisterUser(string userName, string password, string fullName, string email, long securityQuestionId, string securityAnswer, long authenticatorId)
        {
            string passwordSalt = generateSalt(SaltLength);
            string securityAnswerSalt = generateSalt(SaltLength);

            User user = new User()
            {
                Name = userName,
                FullName = fullName,
                Email = email,
                Password = hashSecurityProperty(password, passwordSalt),
                PasswordSalt = passwordSalt,
                SecurityQuestion = SecurityQuestionsRepo.Get(securityQuestionId),
                SecurityAnswer = hashSecurityProperty(securityAnswer, securityAnswerSalt),
                SecurityAnswerSalt = securityAnswerSalt,

                LastActivityDate = DateTime.Now,
                LastLockOutDate = DateTime.Now,
                LastLoginDate = DateTime.Now,
                LastPasswordChangeDate = DateTime.Now,
                RegistrationDate = DateTime.Now,

                IsApproved = AutoApproval,
                IsBlocked = false,
                IsLockedOut = false,

                PasswordFailureCount = 0,
                SecurityAnswerFailureCount = 0,
                LastSecurityAnswerFailureDate = DateTime.Now,
                LastPasswordFailureDate = DateTime.Now,

                Authenticator = AuthenticatorsRepo.Get(authenticatorId)
            };

            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<User> usersRepo = uow.GetRepository<User>();
                usersRepo.Put(user);

                uow.Commit();
            }

            return (user);
        }

        public bool ResetPassword(string userName, string securityAnswer, string password)
        {
            User user = GetUserByName(userName);

            if (user != null)
            {
                if (hashSecurityProperty(securityAnswer, user.SecurityAnswerSalt) == user.SecurityAnswer)
                {
                    user.PasswordSalt = generateSalt(SaltLength);
                    user.Password = hashSecurityProperty(password, user.PasswordSalt);

                    using (IUnitOfWork uow = this.GetUnitOfWork())
                    {
                        IRepository<User> usersRepo = uow.GetRepository<User>();

                        usersRepo.Put(user);

                        uow.Commit();
                    }

                    return true;
                }
            }

            return false;
        }

        public bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        private void updateFailureCount(User user, FailureType failureType)
        {
            user = UsersRepo.Refresh(user.Id);

            if (user != null)
            {
                TimeSpan timeFrame;
                DateTime referenceTime;

                switch (failureType)
                {
                    case FailureType.Password:
                        timeFrame = new TimeSpan(0, PasswordFailureAttemptsWindow, 0);
                        referenceTime = DateTime.Now.Subtract(timeFrame);

                        if (DateTime.Compare(user.LastPasswordFailureDate, referenceTime) > 0)
                        {
                            user.LastPasswordFailureDate = DateTime.Now;
                            user.PasswordFailureCount = user.PasswordFailureCount + 1;

                            if (user.PasswordFailureCount == MaxPasswordFailureAttempts)
                            {
                                user.LastLockOutDate = DateTime.Now;
                                user.IsLockedOut = true;
                            }

                            using (IUnitOfWork uow = this.GetUnitOfWork())
                            {
                                IRepository<User> usersRepo = uow.GetRepository<User>();

                                usersRepo.Put(user);
                                uow.Commit();
                            }
                        }
                        else
                        {
                            user.LastPasswordFailureDate = DateTime.Now;
                            user.PasswordFailureCount = 1;

                            using (IUnitOfWork uow = this.GetUnitOfWork())
                            {
                                IRepository<User> usersRepo = uow.GetRepository<User>();

                                usersRepo.Put(user);
                                uow.Commit();
                            }
                        }

                        break;
                    case FailureType.SecurityAnswer:
                        timeFrame = new TimeSpan(0, SecurityAnswerFailureAttemptsWindow, 0);
                        referenceTime = DateTime.Now.Subtract(timeFrame);

                        if (DateTime.Compare(user.LastSecurityAnswerFailureDate, referenceTime) > 0)
                        {
                            user.LastActivityDate = DateTime.Now;
                            user.LastSecurityAnswerFailureDate = DateTime.Now;
                            user.SecurityAnswerFailureCount = user.SecurityAnswerFailureCount + 1;

                            if (user.SecurityAnswerFailureCount == MaxSecurityAnswerFailureAttempts)
                            {
                                user.LastLockOutDate = DateTime.Now;
                                user.IsLockedOut = true;
                            }

                            using (IUnitOfWork uow = this.GetUnitOfWork())
                            {
                                IRepository<User> usersRepo = uow.GetRepository<User>();

                                usersRepo.Put(user);
                                uow.Commit();
                            }
                        }
                        else
                        {
                            user.LastActivityDate = DateTime.Now;
                            user.LastSecurityAnswerFailureDate = DateTime.Now;
                            user.SecurityAnswerFailureCount = 1;

                            using (IUnitOfWork uow = this.GetUnitOfWork())
                            {
                                IRepository<User> usersRepo = uow.GetRepository<User>();

                                usersRepo.Put(user);
                                uow.Commit();
                            }
                        }

                        break;
                    default:
                        break;
                }
            }
        }

        public User UpdateUser(User user)
        {
            using (IUnitOfWork uow = this.GetUnitOfWork())
            {
                IRepository<User> usersRepo = uow.GetRepository<User>();
                usersRepo.Put(user);
                uow.Commit();
            }

            return (user);
        }

        public bool ValidateUser(string userName, string password)
        {
            bool valid = false;

            User user = GetUserByName(userName, true);

            if (user != null)
            {
                if (user.IsApproved && !user.IsLockedOut && !user.IsBlocked)
                {
                    if (hashSecurityProperty(password, user.PasswordSalt) == user.Password)
                    {
                        user.LastLoginDate = DateTime.Now;

                        using (IUnitOfWork uow = this.GetUnitOfWork())
                        {
                            IRepository<User> usersRepo = uow.GetRepository<User>();

                            usersRepo.Put(user);

                            uow.Commit();
                        }

                        valid = true;
                    }
                    else
                    {
                        updateFailureCount(user, FailureType.Password);
                    }
                }
            }

            return (valid);
        }

        #endregion
    }
}
