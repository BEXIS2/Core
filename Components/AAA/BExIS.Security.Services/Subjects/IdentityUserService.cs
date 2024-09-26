using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Threading.Tasks;

namespace BExIS.Security.Services.Subjects
{
    public class IdentityUserService : UserManager<User, long>
    {
        public IdentityUserService() : base(new UserManager())
        {
            // Configure validation logic for usernames
            UserValidator = new UserValidator<User, long>(this)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            PasswordValidator = new PasswordValidator
            {
                RequiredLength = 6,
                RequireNonLetterOrDigit = false,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false,
            };

            // Configure user lockout defaults
            UserLockoutEnabledByDefault = true;
            DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5);
            MaxFailedAccessAttemptsBeforeLockout = 5;

            // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            // You can write your own provider and plug it in here.
            EmailService = new EmailService();

            var dataProtectionProvider = Auth.DataProtectionProvider;

            if (dataProtectionProvider == null) return;

            var dataProtector = dataProtectionProvider.Create("ASP.NET Identity");
            UserTokenProvider = new DataProtectorTokenProvider<User, long>(dataProtector);
        }

        [Obsolete("Dot not use it, and there is no other way of changing phone numbers!", true)] // this is an example of making a base class method obsolete and causing compilation error
        public new Task<IdentityResult> ChangePhoneNumberAsync(long userId, string phoneNumber, string token)
        {
            return base.ChangePhoneNumberAsync(userId, phoneNumber, token);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Store.Dispose();
        }
    }
}