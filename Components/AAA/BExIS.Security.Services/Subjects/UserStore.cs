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
    public class UserStore : IUserEmailStore<User, long>, IUserPasswordStore<User, long>, IUserLoginStore<User, long>, IUserSecurityStampStore<User, long>, IUserLockoutStore<User, long>, IUserTwoFactorStore<User, long>, IQueryableUserStore<User, long>
    {
        public UserStore()
        {
            var uow = this.GetUnitOfWork();

            GroupRepository = uow.GetReadOnlyRepository<Group>();
            LoginRepository = uow.GetReadOnlyRepository<Login>();
            UserRepository = uow.GetReadOnlyRepository<User>();
        }

        public IReadOnlyRepository<Group> GroupRepository { get; }
        public IReadOnlyRepository<Login> LoginRepository { get; }
        public IReadOnlyRepository<User> UserRepository { get; }
        public IQueryable<User> Users => UserRepository.Query();

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

        public void AddToGroupAsync(User user, long groupId)
        {
            user.Groups.Add(GroupRepository.Get(groupId));
            UpdateAsync(user);
        }

        public Task CreateAsync(User user)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetRepository<User>();
                userRepository.Put(user);
                uow.Commit();
            }

            return Task.FromResult(0);
        }

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

        public void Dispose()
        {
            // DO NOTHING!
        }

        public Task<User> FindAsync(UserLoginInfo login)
        {
            return Task.FromResult(LoginRepository.Query(m => m.LoginProvider == login.LoginProvider && m.ProviderKey == login.ProviderKey).Select(m => m.User).FirstOrDefault());
        }

        public Task<User> FindByEmailAsync(string email)
        {
            return Task.FromResult(UserRepository.Query().FirstOrDefault(u => u.Email.ToUpperInvariant() == email.ToUpperInvariant()));
        }

        public Task<User> FindByIdAsync(long userId)
        {
            return Task.FromResult(UserRepository.Get(userId));
        }

        public User FindById(long userId)
        {
            return UserRepository.Get(userId);
        }

        public Task<User> FindByNameAsync(string userName)
        {
            return Task.FromResult(UserRepository.Query().FirstOrDefault(u => u.Name.ToUpperInvariant() == userName.ToUpperInvariant()));
        }

        public Task<int> GetAccessFailedCountAsync(User user)
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<string> GetEmailAsync(User user)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(User user)
        {
            return Task.FromResult(user.IsEmailConfirmed);
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
                DateTime? lockoutEndDate = user.LockoutEndDate;
                dateTimeOffset = new DateTimeOffset(DateTime.SpecifyKind(lockoutEndDate.Value, DateTimeKind.Utc));
            }
            else
            {
                dateTimeOffset = new DateTimeOffset();
            }
            return Task.FromResult(dateTimeOffset);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(User user)
        {
            return Task.FromResult<IList<UserLoginInfo>>((user.Logins).Select(login => new UserLoginInfo(login.LoginProvider, login.ProviderKey)).ToList());
        }

        public Task<string> GetPasswordHashAsync(User user)
        {
            return Task.FromResult(user.Password);
        }

        public Task<IList<string>> GetGroupsAsync(User user)
        {
            return Task.FromResult((IList<string>)user.Groups.Select(m => m.Name).ToList());
        }

        public Task<string> GetSecurityStampAsync(User user)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        public Task<bool> GetTwoFactorEnabledAsync(User user)
        {
            return Task.FromResult(user.IsTwoFactorEnabled);
        }

        public Task<bool> HasPasswordAsync(User user)
        {
            return Task.FromResult(user.Password != null);
        }

        public Task<int> IncrementAccessFailedCountAsync(User user)
        {
            user.AccessFailedCount = user.AccessFailedCount + 1;
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> IsInGroupAsync(User user, string groupName)
        {
            return Task.FromResult(user.Groups.Any(m => m.Name.ToLowerInvariant() == groupName.ToLowerInvariant()));
        }

        public void RemoveFromGroupAsync(User user, long groupId)
        {
            user.Groups.Remove(GroupRepository.Get(groupId));
            UpdateAsync(user);
        }

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

        public Task ResetAccessFailedCountAsync(User user)
        {
            user.AccessFailedCount = 0;
            return Task.FromResult(0);
        }

        public Task SetEmailAsync(User user, string email)
        {
            user.Email = email;
            return Task.FromResult(0);
        }

        public Task SetEmailConfirmedAsync(User user, bool confirmed)
        {
            user.IsEmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task SetLockoutEnabledAsync(User user, bool enabled)
        {
            user.LockoutEnabled = enabled;
            return Task.FromResult(0);
        }

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

        public Task SetPasswordHashAsync(User user, string passwordHash)
        {
            user.Password = passwordHash;
            return Task.FromResult(0);
        }

        public Task SetSecurityStampAsync(User user, string stamp)
        {
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public Task SetTwoFactorEnabledAsync(User user, bool enabled)
        {
            user.IsTwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }

        public Task UpdateAsync(User user)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetRepository<User>();
                userRepository.Put(user);
                uow.Commit();
            }

            return Task.FromResult(0);
        }
    }
}