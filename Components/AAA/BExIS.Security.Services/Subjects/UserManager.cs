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
        private readonly IdentityUserManager _identityUserManager;

        private bool _isDisposed;

        public UserManager()
        {
            _identityUserManager = new IdentityUserManager();
        }

        ~UserManager()
        {
            Dispose(true);
        }

        public IQueryable<User> Users => _identityUserManager.Users;

        public Task<IdentityResult> AddLoginAsync(long userId, UserLoginInfo login)
        {
            return _identityUserManager.AddLoginAsync(userId, login);
        }

        public Task<IdentityResult> AddToGroupAsync(long userId, string groupName)
        {
            return _identityUserManager.AddToRoleAsync(userId, groupName);
        }

        public Task<IdentityResult> AddToGroupAsync(User user, Group group)
        {
            return _identityUserManager.AddToRoleAsync(user.Id, group.Name);
        }

        public Task<IdentityResult> AddToGroupsAsync(long userId, string[] groupNames)
        {
            return _identityUserManager.AddToRolesAsync(userId, groupNames);
        }

        public Task<IdentityResult> ChangePasswordAsync(long userId, string currentPassword, string newPassword)
        {
            return _identityUserManager.ChangePasswordAsync(userId, currentPassword, newPassword);
        }

        public Task<IdentityResult> ConfirmEmailAsync(long userId, string token)
        {
            return _identityUserManager.ConfirmEmailAsync(userId, token);
        }

        public Task<IdentityResult> CreateAsync(User user)
        {
            return _identityUserManager.CreateAsync(user);
        }

        public Task<IdentityResult> CreateAsync(User user, string password)
        {
            return _identityUserManager.CreateAsync(user, password);
        }

        public Task<IdentityResult> DeleteAsync(User user)
        {
            return _identityUserManager.DeleteAsync(user);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        public Task<User> FindByEmailAsync(string email)
        {
            return _identityUserManager.FindByEmailAsync(email);
        }

        public Task<User> FindByIdAsync(long userId)
        {
            return _identityUserManager.FindByIdAsync(userId);
        }

        public Task<User> FindByNameAsync(string userName)
        {
            return _identityUserManager.FindByNameAsync(userName);
        }

        public Task<string> GenerateEmailConfirmationTokenAsync(long userId)
        {
            return _identityUserManager.GenerateEmailConfirmationTokenAsync(userId);
        }

        public Task<string> GeneratePasswordResetTokenAsync(long userId)
        {
            return _identityUserManager.GeneratePasswordResetTokenAsync(userId);
        }

        public Task<IList<string>> GetGroupsAsync(long userId)
        {
            return _identityUserManager.GetRolesAsync(userId);
        }

        public Task<bool> IsEmailConfirmedAsync(long userId)
        {
            return _identityUserManager.IsEmailConfirmedAsync(userId);
        }

        public Task<IdentityResult> RemoveFromGroupAsync(long userId, string groupName)
        {
            return _identityUserManager.RemoveFromRoleAsync(userId, groupName);
        }

        public Task<IdentityResult> RemoveFromGroupAsync(long userId, string[] groupNames)
        {
            return _identityUserManager.RemoveFromRolesAsync(userId, groupNames);
        }

        public Task<IdentityResult> RemoveLoginAsync(long userId, UserLoginInfo login)
        {
            return _identityUserManager.RemoveLoginAsync(userId, login);
        }

        public Task<IdentityResult> ResetPasswordAsync(long userId, string token, string newPassword)
        {
            return _identityUserManager.ResetPasswordAsync(userId, token, newPassword);
        }

        public Task SendEmailAsync(long userId, string subject, string body)
        {
            return _identityUserManager.SendEmailAsync(userId, subject, body);
        }

        public Task<IdentityResult> SetEmailAsync(long userId, string email)
        {
            return _identityUserManager.SetEmailAsync(userId, email);
        }

        public Task<IdentityResult> UpdateAsync(User user)
        {
            return _identityUserManager.UpdateAsync(user);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;
            if (!disposing) return;
            _identityUserManager?.Dispose();
            _isDisposed = true;
        }
    }
}