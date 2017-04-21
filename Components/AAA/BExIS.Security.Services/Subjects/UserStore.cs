using BExIS.Security.Entities.Authentication;
using BExIS.Security.Entities.Authorization;
using BExIS.Security.Entities.Subjects;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Subjects
{
    public class UserStore : IUserEmailStore<User, long>, IUserPasswordStore<User, long>, IUserLoginStore<User, long>, IUserSecurityStampStore<User, long>, IUserLockoutStore<User, long>, IUserTwoFactorStore<User, long>, IUserRoleStore<User, long>, IQueryableUserStore<User, long>
    {
        public IReadOnlyRepository<Group> GroupRepository { get; }
        public IReadOnlyRepository<Login> LoginRepository { get; }
        public IReadOnlyRepository<Role> RoleRepository { get; }
        public IReadOnlyRepository<User> UserRepository { get; }

        public UserStore()
        {
            var uow = this.GetUnitOfWork();

            GroupRepository = uow.GetReadOnlyRepository<Group>();
            LoginRepository = uow.GetReadOnlyRepository<Login>();
            RoleRepository = uow.GetReadOnlyRepository<Role>();
            UserRepository = uow.GetReadOnlyRepository<User>();
        }

        public void Dispose()
        {
            // DO NOTHING!
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

        public Task<User> FindByIdAsync(long userId)
        {
            return Task.FromResult(UserRepository.Get(userId));
        }

        public Task<User> FindByNameAsync(string userName)
        {
            return Task.FromResult(UserRepository.Query().FirstOrDefault(u => u.Name.ToUpperInvariant() == userName.ToUpperInvariant()));
        }

        public Task SetEmailAsync(User user, string email)
        {
            user.Email = email;
            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(User user)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(User user)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(User user, bool confirmed)
        {
            user.EmailConfirmed = confirmed;
            return Task.FromResult(0);
        }

        public Task<User> FindByEmailAsync(string email)
        {
            return Task.FromResult(UserRepository.Query().FirstOrDefault(u => u.Email.ToUpperInvariant() == email.ToUpperInvariant()));
        }

        public Task SetPasswordHashAsync(User user, string passwordHash)
        {
            user.Password = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(User user)
        {
            return Task.FromResult(user.Password);
        }

        public Task<bool> HasPasswordAsync(User user)
        {
            return Task.FromResult(user.Password != null);
        }

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

        public Task<IList<UserLoginInfo>> GetLoginsAsync(User user)
        {
            return Task.FromResult<IList<UserLoginInfo>>((user.Logins).Select(login => new UserLoginInfo(login.LoginProvider, login.ProviderKey)).ToList());
        }

        public Task<User> FindAsync(UserLoginInfo login)
        {
            return Task.FromResult(LoginRepository.Query(m => m.LoginProvider == login.LoginProvider && m.ProviderKey == login.ProviderKey).Select(m => m.User).FirstOrDefault());
        }

        public Task AddToRoleAsync(User user, string roleName)
        {
            user.Roles.Add(RoleRepository.Query(m => m.Name.ToLowerInvariant() == roleName.ToLowerInvariant()).FirstOrDefault());
            UpdateAsync(user);

            return Task.FromResult(0);
        }

        public Task AddToGroupAsync(User user, string groupName)
        {
            user.Groups.Add(GroupRepository.Query(m => m.Name.ToLowerInvariant() == groupName.ToLowerInvariant()).FirstOrDefault());
            UpdateAsync(user);

            return Task.FromResult(0);
        }

        public Task RemoveFromRoleAsync(User user, string roleName)
        {
            user.Roles.Remove(RoleRepository.Query(m => m.Name.ToLowerInvariant() == roleName.ToLowerInvariant()).FirstOrDefault());
            UpdateAsync(user);

            return Task.FromResult(0);
        }

        public Task RemoveFromGroupAsync(User user, string groupName)
        {
            user.Groups.Remove(GroupRepository.Query(m => m.Name.ToLowerInvariant() == groupName.ToLowerInvariant()).FirstOrDefault());
            UpdateAsync(user);

            return Task.FromResult(0);
        }

        public Task<IList<string>> GetRolesAsync(User user)
        {
            return Task.FromResult((IList<string>)user.Roles.Select(m => m.Name).ToList());
        }

        public Task<bool> IsInRoleAsync(User user, string roleName)
        {
            return Task.FromResult(user.Roles.Any(m => m.Name.ToLowerInvariant() == roleName.ToLowerInvariant()));
        }

        public Task SetSecurityStampAsync(User user, string stamp)
        {
            user.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public Task<string> GetSecurityStampAsync(User user)
        {
            return Task.FromResult(user.SecurityStamp);
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

        public Task<int> IncrementAccessFailedCountAsync(User user)
        {
            user.AccessFailedCount = user.AccessFailedCount + 1;
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(User user)
        {
            user.AccessFailedCount = 0;
            return Task.FromResult(0);
        }

        public Task<int> GetAccessFailedCountAsync(User user)
        {
            return Task.FromResult(user.AccessFailedCount);
        }

        public Task<bool> GetLockoutEnabledAsync(User user)
        {
            return Task.FromResult(user.LockoutEnabled);
        }

        public Task SetLockoutEnabledAsync(User user, bool enabled)
        {
            user.LockoutEnabled = enabled;
            return Task.FromResult(0);
        }

        public Task SetTwoFactorEnabledAsync(User user, bool enabled)
        {
            user.TwoFactorEnabled = enabled;
            return Task.FromResult(0);
        }

        public Task<bool> GetTwoFactorEnabledAsync(User user)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public IQueryable<User> Users => UserRepository.Query();
    }
}
