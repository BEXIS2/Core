using BExIS.Security.Entities.Subjects;
using Microsoft.AspNet.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace BExIS.Security.Services.Subjects
{
    public sealed class UserManager : IUserManager
    {
        private IdentityManager IdentityManager { get; }
        public UserManager()
        {
            IdentityManager = new IdentityManager();
        }

        public IQueryable<User> Users => IdentityManager.Users;

        public Task<IdentityResult> CreateAsync(User user)
        {
            return IdentityManager.CreateAsync(user);
        }

        public Task<IdentityResult> CreateAsync(User user, string password)
        {
            return IdentityManager.CreateAsync(user, password);
        }

        public Task<IdentityResult> SetEmailAsync(long userId, string email)
        {
            return IdentityManager.SetEmailAsync(userId, email);
        }

        public Task<IdentityResult> AddToGroupAsync(long userId, string groupName)
        {
            return IdentityManager.AddToRoleAsync(userId, groupName);
        }

        public Task<IdentityResult> AddToGroupsAsync(long userId, string[] groupNames)
        {
            return IdentityManager.AddToRolesAsync(userId, groupNames);
        }

        public Task<IdentityResult> RemoveFromGroupAsync(long userId, string groupName)
        {
            return IdentityManager.RemoveFromRoleAsync(userId, groupName);
        }

        public Task<IdentityResult> RemoveFromGroupAsync(long userId, string[] groupNames)
        {
            return IdentityManager.RemoveFromRolesAsync(userId, groupNames);
        }

        public Task<IdentityResult> DeleteAsync(User user)
        {
            return IdentityManager.DeleteAsync(user);
        }

        public Task<IdentityResult> AddLoginAsync(long userId, UserLoginInfo login)
        {
            return IdentityManager.AddLoginAsync(userId, login);
        }

        public Task<IdentityResult> RemoveLoginAsync(long userId, UserLoginInfo login)
        {
            return IdentityManager.RemoveLoginAsync(userId, login);
        }

        public Task<User> FindByNameAsync(string userName)
        {
            return IdentityManager.FindByNameAsync(userName);
        }

        public Task<User> FindByIdAsync(long userId)
        {
            return IdentityManager.FindByIdAsync(userId);
        }

        public Task<IdentityResult> ConfirmEmailAsync(long userId, string token)
        {
            return IdentityManager.ConfirmEmailAsync(userId, token);
        }

        public Task<bool> IsEmailConfirmedAsync(long userId)
        {
            return IdentityManager.IsEmailConfirmedAsync(userId);
        }

        public Task<User> FindByEmailAsync(string email)
        {
            return IdentityManager.FindByEmailAsync(email);
        }

        public Task<string> GeneratePasswordResetTokenAsync(long userId)
        {
            return IdentityManager.GeneratePasswordResetTokenAsync(userId);
        }

        public Task SendEmailAsync(long userId, string subject, string body)
        {
            return IdentityManager.SendEmailAsync(userId, subject, body);
        }

        public Task<IdentityResult> ResetPasswordAsync(long userId, string token, string newPassword)
        {
            return IdentityManager.ResetPasswordAsync(userId, token, newPassword);
        }

        public Task<string> GenerateEmailConfirmationTokenAsync(long userId)
        {
            return IdentityManager.GenerateEmailConfirmationTokenAsync(userId);
        }

        public Task<IdentityResult> UpdateAsync(User user)
        {
            return IdentityManager.UpdateAsync(user);
        }

        public Task<IdentityResult> ChangePasswordAsync(long userId, string currentPassword, string newPassword)
        {
            return IdentityManager.ChangePasswordAsync(userId, currentPassword, newPassword);
        }
    }
}