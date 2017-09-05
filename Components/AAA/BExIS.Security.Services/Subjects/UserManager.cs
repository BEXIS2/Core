using BExIS.Security.Entities.Subjects;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BExIS.Security.Services.Subjects
{
    public class UserManager : IUserManager, IDisposable
    {
        private readonly IdentityManager _identityManager;
        public UserManager()
        {
            _identityManager = new IdentityManager();
        }

        private bool _isDisposed;
        ~UserManager()
        {
            Dispose(true);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (!disposing) return;
            _identityManager?.Dispose();
            _isDisposed = true;
        }

        public IQueryable<User> Users => _identityManager.Users;

        public Task<IdentityResult> CreateAsync(User user)
        {
            return _identityManager.CreateAsync(user);
        }

        public Task<IdentityResult> CreateAsync(User user, string password)
        {
            return _identityManager.CreateAsync(user, password);
        }

        public Task<IdentityResult> SetEmailAsync(long userId, string email)
        {
            return _identityManager.SetEmailAsync(userId, email);
        }

        public Task<IdentityResult> AddToGroupAsync(long userId, string groupName)
        {
            return _identityManager.AddToRoleAsync(userId, groupName);
        }

        public Task<IdentityResult> AddToGroupsAsync(long userId, string[] groupNames)
        {
            return _identityManager.AddToRolesAsync(userId, groupNames);
        }

        public Task<IdentityResult> RemoveFromGroupAsync(long userId, string groupName)
        {
            return _identityManager.RemoveFromRoleAsync(userId, groupName);
        }

        public Task<IdentityResult> RemoveFromGroupAsync(long userId, string[] groupNames)
        {
            return _identityManager.RemoveFromRolesAsync(userId, groupNames);
        }

        public Task<IdentityResult> DeleteAsync(User user)
        {
            return _identityManager.DeleteAsync(user);
        }

        public Task<IdentityResult> AddLoginAsync(long userId, UserLoginInfo login)
        {
            return _identityManager.AddLoginAsync(userId, login);
        }

        public Task<IdentityResult> RemoveLoginAsync(long userId, UserLoginInfo login)
        {
            return _identityManager.RemoveLoginAsync(userId, login);
        }

        public Task<User> FindByNameAsync(string userName)
        {
            return _identityManager.FindByNameAsync(userName);
        }

        public Task<User> FindByIdAsync(long userId)
        {
            return _identityManager.FindByIdAsync(userId);
        }

        public Task<IdentityResult> ConfirmEmailAsync(long userId, string token)
        {
            return _identityManager.ConfirmEmailAsync(userId, token);
        }

        public Task<bool> IsEmailConfirmedAsync(long userId)
        {
            return _identityManager.IsEmailConfirmedAsync(userId);
        }

        public Task<User> FindByEmailAsync(string email)
        {
            return _identityManager.FindByEmailAsync(email);
        }

        public Task<string> GeneratePasswordResetTokenAsync(long userId)
        {
            return _identityManager.GeneratePasswordResetTokenAsync(userId);
        }

        public Task SendEmailAsync(long userId, string subject, string body)
        {
            return _identityManager.SendEmailAsync(userId, subject, body);
        }

        public Task<IdentityResult> ResetPasswordAsync(long userId, string token, string newPassword)
        {
            return _identityManager.ResetPasswordAsync(userId, token, newPassword);
        }

        public Task<string> GenerateEmailConfirmationTokenAsync(long userId)
        {
            return _identityManager.GenerateEmailConfirmationTokenAsync(userId);
        }

        public Task<IdentityResult> UpdateAsync(User user)
        {
            return _identityManager.UpdateAsync(user);
        }

        public Task<IdentityResult> ChangePasswordAsync(long userId, string currentPassword, string newPassword)
        {
            return _identityManager.ChangePasswordAsync(userId, currentPassword, newPassword);
        }

        public Task<IList<string>> GetGroupsAsync(long userId)
        {
            return _identityManager.GetRolesAsync(userId);
        }
    }
}