using BExIS.Security.Entities.Authentication;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using BExIS.Utils.NH.Querying;
using Microsoft.AspNet.Identity;
using NHibernate;
using Owin.Security.Providers.Orcid.Message;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Subjects
{
    public class UserStore : IUserEmailStore<User, long>, IUserLoginStore<User, long>, IUserPasswordStore<User, long>, IUserLockoutStore<User, long>, IUserRoleStore<User, long>, IUserSecurityStampStore<User, long>, IUserTwoFactorStore<User, long>, IQueryableUserStore<User, long>, IDisposable
    {
        public IQueryable<User> Users
        {
            get
            {
                var uow = this.GetUnitOfWork();
                return uow.GetReadOnlyRepository<User>().Query();
            }
        }

        #region IUserEmailStore

        public Task CreateAsync(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrEmpty(user.UserName))
                throw new ArgumentException("UserName required");

            if (FindByNameAsync(user.UserName)?.Result != null)
                throw new InvalidOperationException("UserName already exists");

            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetRepository<User>();
                userRepository.Put(user);
                uow.Commit();
            }

            return Task.CompletedTask;
        }

        public Task DeleteAsync(User user)
        {
            using (var uow = this.GetUnitOfWork())
            {
                uow.GetRepository<User>().Delete(user);
                uow.Commit();

                return Task.CompletedTask;
            }
        }

        public Task<bool> DeleteByIdAsync(long userId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetRepository<User>();

                var user = userRepository.Get(userId);

                if (user == null)
                    return Task.FromResult(false);

                // Logins
                var loginsRepository = uow.GetRepository<Login>();
                foreach (var login in loginsRepository.Get(l => l.User.Id == userId))
                {
                    loginsRepository.Delete(login);
                }

                // EntityPermissions
                var entityPermissionRepository = uow.GetRepository<EntityPermission>();
                foreach (var entityPermission in entityPermissionRepository.Get(e => e.Subject.Id == userId))
                {
                    entityPermissionRepository.Delete(entityPermission);
                }

                // FeaturePermissions
                var featurePermissionRepository = uow.GetRepository<FeaturePermission>();
                foreach (var featurePermission in featurePermissionRepository.Get(e => e.Subject.Id == userId))
                {
                    featurePermissionRepository.Delete(featurePermission);
                }

                var result = userRepository.Delete(user);
                uow.Commit();

                return Task.FromResult(result);
            }
        }

        public void Dispose(){}

        public Task<User> FindByEmailAsync(string email)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetReadOnlyRepository<User>();

                return Task.FromResult(userRepository.Query(u => u.Email == email).SingleOrDefault());
            }
        }

        public Task<User> FindByIdAsync(long userId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetReadOnlyRepository<User>();

                return Task.FromResult(userRepository.Get(userId));
            }
                
        }

        public Task<User> FindByNameAsync(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                return Task.FromResult<User>(null);

            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetReadOnlyRepository<User>();

                return Task.FromResult(userRepository.Query(u => u.Name.ToLower() == userName.ToLower()).SingleOrDefault());
            }
        }

        public Task<string> GetEmailAsync(User user)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(User user)
        {
            return Task.FromResult(user.IsEmailConfirmed);
        }

        public Task SetEmailAsync(User user, string email)
        {
            user.Email = email;
            return Task.CompletedTask;
        }

        public Task SetEmailConfirmedAsync(User user, bool confirmed)
        {
            user.IsEmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(User user)
        {
            if (user == null)
                return Task.FromException(new ArgumentNullException(nameof(user)));

            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<User>();
                repo.Merge(user);
                uow.Commit();
            }
            return Task.CompletedTask;
        }

        #endregion IUserEmailStore

        #region IUserLoginStore

        public Task AddLoginAsync(User user, UserLoginInfo login)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var loginRepository = uow.GetRepository<Login>();
                var userRepository = uow.GetRepository<User>();

                var userLogin = new Login()
                {
                    ProviderKey = login.ProviderKey,
                    LoginProvider = login.LoginProvider,
                    User = user
                };

                loginRepository.Put(userLogin);
                uow.Commit();

                return Task.CompletedTask;

            }  
        }

        public Task<User> FindAsync(UserLoginInfo login)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var loginRepository = uow.GetReadOnlyRepository<Login>();

                return Task.FromResult(loginRepository.Query(l => l.LoginProvider == login.LoginProvider && l.ProviderKey == login.ProviderKey).Select(u => u.User).SingleOrDefault());
            }
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(User user)
        {
            return Task.FromResult<IList<UserLoginInfo>>((user.Logins).Select(login => new UserLoginInfo(login.LoginProvider, login.ProviderKey)).ToList());
        }

        public Task RemoveLoginAsync(User user, UserLoginInfo login)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetRepository<User>();
                user = userRepository.Get(user.Id);
                var info = user.Logins.SingleOrDefault(x => x.LoginProvider == login.LoginProvider && x.ProviderKey == login.ProviderKey);
                if (info == null) return Task.FromResult(0);

                user.Logins.Remove(info);
                userRepository.Put(user);
                uow.Commit();

                return Task.CompletedTask;
            }
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
            return Task.CompletedTask;
        }

        #endregion IUserPasswordStore

        #region IUserRoleStore

        public Task AddToRoleAsync(User user, string roleName)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetReadOnlyRepository<Group>();
                var group = groupRepository.Query(g => g.Name == roleName).SingleOrDefault();

                if (group == null)
                    return Task.FromException(new InvalidOperationException($"Group '{roleName}' not found."));

                var userRepository = uow.GetRepository<User>();
                user = userRepository.Get(user.Id);
                user.Groups.Add(group);

                userRepository.Merge(user);
                uow.Commit();

                return Task.CompletedTask;
            }
        }

        public Task<IList<string>> GetRolesAsync(User user)
        {
            return Task.FromResult((IList<string>)user.Groups.Select(m => m.Name).ToList());
        }

        public Task<bool> IsInRoleAsync(User user, string roleName)
        {
            return Task.FromResult(user.Groups.Any(m => m.Name == roleName));
        }

        public Task RemoveFromRoleAsync(User user, string roleName)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetReadOnlyRepository<Group>();
                var userRepository = uow.GetRepository<User>();

                var group = groupRepository.Query(g => g.Name == roleName).FirstOrDefault();

                if (group == null) return Task.FromResult(false);

                user.Groups.Remove(group);
                userRepository.Put(user);
                uow.Commit();

                return Task.FromResult(true);
            }            
        }

        #endregion IUserRoleStore

        #region IUserSecurityStampStore

        public Task<string> GetSecurityStampAsync(User user)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        public Task SetSecurityStampAsync(User user, string stamp)
        {
            user.SecurityStamp = stamp;
            return Task.CompletedTask;
        }

        #endregion IUserSecurityStampStore

        #region IUserLockoutStore

        public Task<int> GetAccessFailedCountAsync(User user)
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(User user)
        {
            return Task.FromResult(user.LockoutEnabled);
        }

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

        public Task<int> IncrementAccessFailedCountAsync(User user)
        {
            user.AccessFailedCount = user.AccessFailedCount + 1;
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(User user)
        {
            user.AccessFailedCount = 0;
            return Task.CompletedTask;
        }

        public Task SetLockoutEnabledAsync(User user, bool enabled)
        {
            user.LockoutEnabled = enabled;
            return Task.CompletedTask;
        }

        public Task SetLockoutEndDateAsync(User user, DateTimeOffset lockoutEnd)
        {
            user.LockoutEndDate = lockoutEnd.UtcDateTime; ;
            return Task.CompletedTask;
        }

        #endregion IUserLockoutStore

        #region IUserTwoFactorStore

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
            user.IsTwoFactorEnabled = false;
            return Task.CompletedTask;
        }

        #endregion IUserTwoFactorStore
    }
}