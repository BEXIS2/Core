using BExIS.Security.Entities.Authentication;
using BExIS.Security.Entities.Subjects;
using BExIS.Utils.NH.Querying;
using Microsoft.AspNet.Identity;
using Owin.Security.Providers.Orcid.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
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

        public List<User> GetUsers(FilterExpression filter, OrderByExpression orderBy, int pageNumber, int pageSize, out int count)
        {
            var orderbyClause = orderBy?.ToLINQ();
            var whereClause = filter?.ToLINQ();

            count = 0;

            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    if (whereClause != null && orderBy != null)
                    {
                        var l = Users.Where(whereClause);
                        var x = l.OrderBy(orderbyClause);
                        var y = x.Skip((pageNumber - 1) * pageSize);
                        var z = y.Take(pageSize);

                        count = l.Count();

                        return z.ToList();
                    }
                    else if (whereClause != null)
                    {
                        var filtered = Users.Where(whereClause);
                        count = filtered.Count();

                        return filtered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                    }

                    if (orderBy != null)
                    {
                        count = Users.Count();
                        return Users.OrderBy(orderbyClause).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                    }

                    count = Users.Count();

                    return Users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Could not retrieve filtered users."), ex);
            }
        }

        #region IUserEmailStore

        public Task CreateAsync(User user)
        {
            if (user == null)
                return Task.FromException(new Exception());

            if (string.IsNullOrEmpty(user.UserName))
                return Task.FromException(new Exception());

            if (FindByNameAsync(user.UserName)?.Result != null)
                return Task.FromException(new Exception());

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
                var userRepository = uow.GetRepository<User>();
                userRepository.Delete(user);
                uow.Commit();
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public Task<User> FindByEmailAsync(string email)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetRepository<User>();

                var users = userRepository.Query(u => u.Email.Equals(email, StringComparison.InvariantCultureIgnoreCase)).ToList();

                if (!users.Any())
                    return Task.FromException<User>(new Exception());

                if (users.Count > 1)
                    return Task.FromException<User>(new Exception());

                return Task.FromResult(users.Single());
            }
        }

        public Task<User> FindByIdAsync(long userId)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetRepository<User>();
                return Task.FromResult(userRepository.Get(userId));
            }
        }

        public Task<User> FindByNameAsync(string userName)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetRepository<User>();

                var users = userRepository.Query(u => u.Name.Equals(userName, StringComparison.InvariantCultureIgnoreCase)).ToList();

                if (!users.Any())
                    return Task.FromException<User>(new Exception());

                if (users.Count > 1)
                    return Task.FromException<User>(new Exception());

                return Task.FromResult(users.Single());
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
                return Task.FromException(new Exception());

            if (string.IsNullOrEmpty(user.UserName))
                return Task.FromException(new Exception());

            if (FindByIdAsync(user.Id)?.Result == null)
                return Task.FromException(new Exception());

            using (var uow = this.GetUnitOfWork())
            {
                var repo = uow.GetRepository<User>();
                repo.Merge(user);
                var merged = repo.Get(user.Id);
                repo.Put(merged);
                uow.Commit();
            }

            return Task.CompletedTask;
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

        public Task AddLoginAsync(User user, UserLoginInfo login)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetReadOnlyRepository<User>();
                var loginRepository = uow.GetRepository<Login>();

                user = userRepository.Get(user.Id);
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
                var loginRepository = uow.GetRepository<Login>();

                var users = loginRepository.Query(m => m.LoginProvider.Equals(login.LoginProvider, StringComparison.InvariantCultureIgnoreCase) && m.ProviderKey.Equals(login.ProviderKey, StringComparison.InvariantCultureIgnoreCase)).Select(m => m.User).ToList();

                if (!users.Any())
                    return Task.FromException<User>(new Exception());

                if (users.Count > 1)
                    return Task.FromException<User>(new Exception());

                return Task.FromResult(users.Single());
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
            return Task.FromResult(user.LockoutEndDate);
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

        #region IUserRoleStore

        public Task AddToRoleAsync(User user, string roleName)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetReadOnlyRepository<Group>();
                var groups = groupRepository.Query(g => g.Name.Equals(roleName, StringComparison.InvariantCultureIgnoreCase)).ToList();

                if (!groups.Any())
                    return Task.FromException<Group>(new Exception());

                if (groups.Count > 1)
                    return Task.FromException<Group>(new Exception());

                var group = groups.Single();

                //// [Sven][Workaround][2017/10/09]
                //// It is necessary to "re-get" the object. Other than that, "Load", "Reload" or other functions are not working here.
                //// In general, there is an issue with the sessions. The primary error message was
                ////  "reassociated object has dirty collection: BExIS.Security.Entities.Subjects.User.Groups"
                //// but with "Load" or "Reload" it changed to
                ////  "a different object with the same identifier value was already associated with the session: 32768, of entity: BExIS.Security.Entities.Subjects.User".
                //// At https://forum.hibernate.org/viewtopic.php?t=934551 is claimed to change "session.Lock()" to "session.Update" which should be done at Vaiona.
                ///
                var userRepository = uow.GetRepository<User>();
                user = userRepository.Get(user.Id);

                if (group == null) return Task.FromException<Group>(new Exception());

                user.Groups.Add(group);
                userRepository.Put(user);
                uow.Commit();

                return Task.CompletedTask;
            }
        }

        public Task<IList<string>> GetRolesAsync(User user)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetRepository<User>();

                user = userRepository.Get(user.Id);
                var groups = user.Groups.Select(m  => m.Name).ToList();

                return Task.FromResult((IList<string>)groups);
            }
        }

        public Task<bool> IsInRoleAsync(User user, string roleName)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetRepository<User>();
                user = userRepository.Get(user.Id);

                return Task.FromResult(user.Groups.Any(m => m.Name.Equals(roleName, StringComparison.InvariantCultureIgnoreCase)));
            }
        }

        public Task RemoveFromRoleAsync(User user, string roleName)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var groupRepository = uow.GetRepository<Group>();
                var userRepository = uow.GetRepository<User>();
                var group = groupRepository.Query(g => g.Name.ToUpperInvariant() == roleName.ToUpperInvariant()).FirstOrDefault();

                if (group == null) return Task.FromException(new Exception());

                user = userRepository.Get(user.Id);
                user.Groups.Remove(group);
                userRepository.Put(user);
                uow.Commit();

                return Task.CompletedTask;
            }
        }

        #endregion IUserRoleStore

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
    }
}