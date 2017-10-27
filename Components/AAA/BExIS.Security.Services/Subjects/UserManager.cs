using BExIS.Security.Entities.Authentication;
using BExIS.Security.Entities.Subjects;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Subjects
{
    public class UserManager : IUserEmailStore<User, long>, IUserLoginStore<User, long>, IUserPasswordStore<User, long>, IUserLockoutStore<User, long>, IUserRoleStore<User, long>, IUserTwoFactorStore<User, long>, IUserSecurityStampStore<User, long>, IQueryableUserStore<User, long>
    {
        private readonly IUnitOfWork _guow;
        private bool _isDisposed;

        public UserManager()
        {
            _guow = this.GetIsolatedUnitOfWork();
            UserRepository = _guow.GetReadOnlyRepository<User>();
        }

        ~UserManager()
        {
            Dispose(true);
        }

        public IQueryable<User> Users => UserRepository.Query();

        private IReadOnlyRepository<User> UserRepository { get; }

        #region IUserEmailStore

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task CreateAsync(User user)
        {
            if (string.IsNullOrEmpty(user.Name))
                return Task.FromResult(0);

            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetRepository<User>();
                userRepository.Put(user);
                uow.Commit();
            }

            return Task.FromResult(0);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task DeleteAsync(User user)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetRepository<User>();
                userRepository.Delete(user);
                uow.Commit();
            }

            return Task.FromResult(0);
        }

        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task<User> FindByEmailAsync(string email)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetRepository<User>();
                return Task.FromResult(userRepository.Query().FirstOrDefault(u => u.Email.ToUpperInvariant() == email.ToUpperInvariant()));
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public Task<User> FindByIdAsync(long userId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetRepository<User>();
                return Task.FromResult(userRepository.Get(userId));
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public Task<User> FindByNameAsync(string userName)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetRepository<User>();
                return Task.FromResult(userRepository.Query().FirstOrDefault(u => u.Name.ToUpperInvariant() == userName.ToUpperInvariant()));
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<string> GetEmailAsync(User user)
        {
            return Task.FromResult(user.Email);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<bool> GetEmailConfirmedAsync(User user)
        {
            return Task.FromResult(user.IsEmailConfirmed);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public Task SetEmailAsync(User user, string email)
        {
            user.Email = email;
            return Task.FromResult(0);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <param name="confirmed"></param>
        /// <returns></returns>
        public Task SetEmailConfirmedAsync(User user, bool confirmed)
        {
            user.IsEmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task UpdateAsync(User entity)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<User>();
                repo.Merge(entity);
                var merged = repo.Get(entity.Id);
                repo.Put(merged);
                uow.Commit();
            }

            return Task.FromResult(0);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    if (_guow != null)
                        _guow.Dispose();
                    _isDisposed = true;
                }
            }
        }

        #endregion IUserEmailStore

        #region IUserLoginStore

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <param name="login"></param>
        /// <returns></returns>
        public Task AddLoginAsync(User user, UserLoginInfo login)
        {
            user.Logins.Add(new Login()
            {
                ProviderKey = login.ProviderKey,
                LoginProvider = login.LoginProvider
            });

            UpdateAsync(user);
            return Task.FromResult<int>(0);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="login"></param>
        /// <returns></returns>
        public Task<User> FindAsync(UserLoginInfo login)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var loginRepository = uow.GetRepository<Login>();
                var user = loginRepository.Query(m => m.LoginProvider == login.LoginProvider && m.ProviderKey == login.ProviderKey).Select(m => m.User).FirstOrDefault();

                return Task.FromResult(user);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<IList<UserLoginInfo>> GetLoginsAsync(User user)
        {
            return Task.FromResult<IList<UserLoginInfo>>((user.Logins).Select(login => new UserLoginInfo(login.LoginProvider, login.ProviderKey)).ToList());
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <param name="login"></param>
        /// <returns></returns>
        public Task RemoveLoginAsync(User user, UserLoginInfo login)
        {
            var info = user.Logins.SingleOrDefault(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey);
            if (info != null)
            {
                user.Logins.Remove(info);
                UpdateAsync(user);
            }

            return Task.FromResult(0);
        }

        #endregion IUserLoginStore

        #region IUserPasswordStore

        public Task<string> GetPasswordHashAsync(User user)
        {
            return Task.FromResult(user.Password);
        }

        public Task<bool> HasPasswordAsync(User user)
        {
            return Task.FromResult(user.Password != null);
        }

        public Task SetPasswordHashAsync(User user, string passwordHash)
        {
            user.Password = passwordHash;
            return Task.FromResult(0);
        }

        #endregion IUserPasswordStore

        #region IUserLockoutStore

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<int> GetAccessFailedCountAsync(User user)
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<bool> GetLockoutEnabledAsync(User user)
        {
            return Task.FromResult(user.LockoutEnabled);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<DateTimeOffset> GetLockoutEndDateAsync(User user)
        {
            DateTimeOffset dateTimeOffset;

            if (user.LockoutEndDate.HasValue)
            {
                var lockoutEndDate = user.LockoutEndDate;
                dateTimeOffset = new DateTimeOffset(DateTime.SpecifyKind(lockoutEndDate.Value, DateTimeKind.Utc));
            }
            else
            {
                dateTimeOffset = new DateTimeOffset();
            }
            return Task.FromResult(dateTimeOffset); throw new NotImplementedException();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<int> IncrementAccessFailedCountAsync(User user)
        {
            user.AccessFailedCount = user.AccessFailedCount + 1;
            return Task.FromResult(user.AccessFailedCount);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task ResetAccessFailedCountAsync(User user)
        {
            user.AccessFailedCount = 0;
            return Task.FromResult(0);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public Task SetLockoutEnabledAsync(User user, bool enabled)
        {
            user.LockoutEnabled = enabled;
            return Task.FromResult(0);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <param name="lockoutEnd"></param>
        /// <returns></returns>
        public Task SetLockoutEndDateAsync(User user, DateTimeOffset lockoutEnd)
        {
            DateTime? nullable;

            if (lockoutEnd == DateTimeOffset.MinValue)
            {
                nullable = null;
            }
            else
            {
                nullable = lockoutEnd.UtcDateTime;
            }
            user.LockoutEndDate = nullable;
            return Task.FromResult(0);
        }

        #endregion IUserLockoutStore

        #region IUserRoleStore

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public Task AddToRoleAsync(User user, string roleName)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetReadOnlyRepository<Group>();
                var userRepository = uow.GetRepository<User>();
                var group = groupRepository.Query(g => g.Name.ToUpperInvariant() == roleName.ToUpperInvariant()).FirstOrDefault();

                // [Sven][Workaround][2017/10/09]
                // It is necessary to "re-get" the object. Other than that, "Load", "Reload" or other functions are not working here.
                // In general, there is an issue with the sessions. The primary error message was
                //  "reassociated object has dirty collection: BExIS.Security.Entities.Subjects.User.Groups"
                // but with "Load" or "Reload" it changed to
                //  "a different object with the same identifier value was already associated with the session: 32768, of entity: BExIS.Security.Entities.Subjects.User".
                // At https://forum.hibernate.org/viewtopic.php?t=934551 is claimed to change "session.Lock()" to "session.Update" which should be done at Vaiona.
                user = userRepository.Get(user.Id);

                if (group == null) return Task.FromResult(0);

                user.Groups.Add(group);

                userRepository.Put(user);
                uow.Commit();

                return Task.FromResult(0);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<IList<string>> GetRolesAsync(User user)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetRepository<Group>();
                return Task.FromResult((IList<string>)groupRepository.Query(g => g.Users.Any(u => u.Id == user.Id)).Select(m => m.Name).ToList());
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public Task<bool> IsInRoleAsync(User user, string roleName)
        {
            return Task.FromResult(user.Groups.Any(m => m.Name.ToLowerInvariant() == roleName.ToLowerInvariant()));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public Task RemoveFromRoleAsync(User user, string roleName)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetRepository<Group>();
                var userRepository = uow.GetRepository<User>();
                var group = groupRepository.Query(g => g.Name.ToUpperInvariant() == roleName.ToUpperInvariant()).FirstOrDefault();

                if (group == null) return Task.FromResult(0);

                user.Groups.Remove(group);

                userRepository.Put(user);
                uow.Commit();

                return Task.FromResult(0);
            }
        }

        #endregion IUserRoleStore

        #region IUserTwoFactorStore

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<bool> GetTwoFactorEnabledAsync(User user)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="user"></param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public Task SetTwoFactorEnabledAsync(User user, bool enabled)
        {
            throw new NotImplementedException();
        }

        #endregion IUserTwoFactorStore

        #region IUserSecurityStampStore

        public Task<string> GetSecurityStampAsync(User user)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        public Task SetSecurityStampAsync(User user, string stamp)
        {
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        #endregion IUserSecurityStampStore

        //public Task AddLoginAsync(User user, UserLoginInfo login)
        //{
        //    user.Logins.Add(new Login()
        //    {
        //        ProviderKey = login.ProviderKey,
        //        LoginProvider = login.LoginProvider
        //    });

        //    UpdateAsync(user);
        //    return Task.FromResult<int>(0);
        //}

        //public void AddToGroupAsync(User user, long groupId)
        //{
        //    user.Groups.Add(GroupRepository.Get(groupId));
        //    UpdateAsync(user);
        //}

        //public void Create(User user)
        //{
        //    if (string.IsNullOrEmpty(user.Name))
        //        return;

        //    using (var uow = this.GetUnitOfWork())
        //    {
        //        var userRepository = uow.GetRepository<User>();
        //        userRepository.Put(user);
        //        uow.Commit();
        //    }
        //}

        //public Task CreateAsync(User user)
        //{
        //    if (string.IsNullOrEmpty(user.Name))
        //        return Task.FromResult(0);

        //    using (var uow = this.GetUnitOfWork())
        //    {
        //        var userRepository = uow.GetRepository<User>();
        //        userRepository.Put(user);
        //        uow.Commit();
        //    }

        //    return Task.FromResult(0);
        //}

        //public void Delete(long userId)
        //{
        //    var user = UserRepository.Get(userId);

        //    using (var uow = this.GetUnitOfWork())
        //    {
        //        var userRepository = uow.GetRepository<User>();
        //        userRepository.Delete(user);
        //        uow.Commit();
        //    }
        //}

        //public Task DeleteAsync(User user)
        //{
        //    using (var uow = this.GetUnitOfWork())
        //    {
        //        var userRepository = uow.GetRepository<User>();
        //        userRepository.Delete(user);
        //        uow.Commit();
        //    }

        //    return Task.FromResult(0);
        //}

        //public void Dispose()
        //{
        //    _guow?.Dispose();
        //}

        //public Task<User> FindAsync(UserLoginInfo login)
        //{
        //    return Task.FromResult(LoginRepository.Query(m => m.LoginProvider == login.LoginProvider && m.ProviderKey == login.ProviderKey).Select(m => m.User).FirstOrDefault());
        //}

        //public Task<User> FindByEmailAsync(string email)
        //{
        //    return Task.FromResult(UserRepository.Query().FirstOrDefault(u => u.Email.ToUpperInvariant() == email.ToUpperInvariant()));
        //}

        //public User FindById(long userId)
        //{
        //    return UserRepository.Get(userId);
        //}

        //public Task<User> FindByIdAsync(long userId)
        //{
        //    return Task.FromResult(UserRepository.Get(userId));
        //}

        //public Task<User> FindByNameAsync(string userName)
        //{
        //    return Task.FromResult(UserRepository.Query().FirstOrDefault(u => u.Name.ToUpperInvariant() == userName.ToUpperInvariant()));
        //}

        //public Task<int> GetAccessFailedCountAsync(User user)
        //{
        //    return Task.FromResult(user.AccessFailedCount);
        //}

        //public Task<string> GetEmailAsync(User user)
        //{
        //    return Task.FromResult(user.Email);
        //}

        //public Task<bool> GetEmailConfirmedAsync(User user)
        //{
        //    return Task.FromResult(user.IsEmailConfirmed);
        //}

        //public Task<IList<string>> GetGroupsAsync(User user)
        //{
        //    return Task.FromResult((IList<string>)user.Groups.Select(m => m.Name).ToList());
        //}

        //public Task<bool> GetLockoutEnabledAsync(User user)
        //{
        //    return Task.FromResult(user.LockoutEnabled);
        //}

        //public Task<DateTimeOffset> GetLockoutEndDateAsync(User user)
        //{
        //    DateTimeOffset dateTimeOffset;

        //    if (user.LockoutEndDate.HasValue)
        //    {
        //        var lockoutEndDate = user.LockoutEndDate;
        //        dateTimeOffset = new DateTimeOffset(DateTime.SpecifyKind(lockoutEndDate.Value, DateTimeKind.Utc));
        //    }
        //    else
        //    {
        //        dateTimeOffset = new DateTimeOffset();
        //    }
        //    return Task.FromResult(dateTimeOffset);
        //}

        //public Task<IList<UserLoginInfo>> GetLoginsAsync(User user)
        //{
        //    return Task.FromResult<IList<UserLoginInfo>>((user.Logins).Select(login => new UserLoginInfo(login.LoginProvider, login.ProviderKey)).ToList());
        //}

        //public Task<string> GetPasswordHashAsync(User user)
        //{
        //    return Task.FromResult(user.Password);
        //}

        //public Task<string> GetSecurityStampAsync(User user)
        //{
        //    return Task.FromResult(user.SecurityStamp);
        //}

        //public Task<bool> GetTwoFactorEnabledAsync(User user)
        //{
        //    return Task.FromResult(false);
        //}

        //public Task<bool> HasPasswordAsync(User user)
        //{
        //    return Task.FromResult(user.Password != null);
        //}

        //public Task<int> IncrementAccessFailedCountAsync(User user)
        //{
        //    user.AccessFailedCount = user.AccessFailedCount + 1;
        //    return Task.FromResult(user.AccessFailedCount);
        //}

        //public Task<bool> IsInGroupAsync(User user, string groupName)
        //{
        //    return Task.FromResult(user.Groups.Any(m => m.Name.ToLowerInvariant() == groupName.ToLowerInvariant()));
        //}

        //public void RemoveFromGroupAsync(User user, long groupId)
        //{
        //    user.Groups.Remove(GroupRepository.Get(groupId));
        //    UpdateAsync(user);
        //}

        //public Task RemoveLoginAsync(User user, UserLoginInfo login)
        //{
        //    var info = user.Logins.SingleOrDefault(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey);
        //    if (info != null)
        //    {
        //        user.Logins.Remove(info);
        //        UpdateAsync(user);
        //    }

        //    return Task.FromResult(0);
        //}

        //public Task ResetAccessFailedCountAsync(User user)
        //{
        //    user.AccessFailedCount = 0;
        //    return Task.FromResult(0);
        //}

        //public Task SetEmailAsync(User user, string email)
        //{
        //    user.Email = email;
        //    return Task.FromResult(0);
        //}

        //public Task SetEmailConfirmedAsync(User user, bool confirmed)
        //{
        //    user.IsEmailConfirmed = confirmed;
        //    return Task.FromResult(0);
        //}

        //public Task SetLockoutEnabledAsync(User user, bool enabled)
        //{
        //    user.LockoutEnabled = enabled;
        //    return Task.FromResult(0);
        //}

        //public Task SetLockoutEndDateAsync(User user, DateTimeOffset lockoutEnd)
        //{
        //    DateTime? nullable;

        //    if (lockoutEnd == DateTimeOffset.MinValue)
        //    {
        //        nullable = null;
        //    }
        //    else
        //    {
        //        nullable = lockoutEnd.UtcDateTime;
        //    }
        //    user.LockoutEndDate = nullable;
        //    return Task.FromResult(0);
        //}

        //public Task SetPasswordHashAsync(User user, string passwordHash)
        //{
        //    user.Password = passwordHash;
        //    return Task.FromResult(0);
        //}

        //public Task SetSecurityStampAsync(User user, string stamp)
        //{
        //    user.SecurityStamp = stamp;
        //    return Task.FromResult(0);
        //}

        //public Task SetTwoFactorEnabledAsync(User user, bool enabled)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Update(User user)
        //{
        //    using (var uow = this.GetUnitOfWork())
        //    {
        //        var userRepository = uow.GetRepository<User>();
        //        userRepository.Put(user);
        //        uow.Commit();
        //    }
        //}

        //public Task UpdateAsync(User user)
        //{
        //    using (var uow = this.GetUnitOfWork())
        //    {
        //        var userRepository = uow.GetRepository<User>();
        //        userRepository.Put(user);
        //        uow.Commit();
        //    }

        //    return Task.FromResult(0);
        //}
    }
}