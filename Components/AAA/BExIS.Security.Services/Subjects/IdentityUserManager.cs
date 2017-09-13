using BExIS.Dlm.Entities.Party;
using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Threading.Tasks;

namespace BExIS.Security.Services.Subjects
{
    public class IdentityUserManager : UserManager<User, long>
    {
        public IdentityUserManager() : base(new UserStore())
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

        public async Task<Party> GetPartyAsync(User user)
        {
            var store = Store as UserStore;
            if (store == null) throw new ArgumentNullException($"store");

            return await store.GetPartyAsync(user);
        }

        public async Task SetPartyAsync(User user, long partyId)
        {
            var store = Store as UserStore;
            if (store == null) throw new ArgumentNullException($"store");

            await store.SetPartyAsync(user, partyId);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Store.Dispose();
        }
    }
}