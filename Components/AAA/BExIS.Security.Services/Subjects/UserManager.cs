using BExIS.Security.Entities.Subjects;
using BExIS.Security.Services.Authentication;
using BExIS.Security.Services.Utilities;
using BExIS.Utils.NH.Querying;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Vaiona.Persistence.Api;

namespace BExIS.Security.Services.Subjects
{
    public class UserManager : UserManager<User, long>
    {
        public UserManager(IUserStore<User, long> store): base(store)
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
                RequiredLength = 12,
                RequireNonLetterOrDigit = true,
                RequireDigit = true,
                RequireLowercase = true,
                RequireUppercase = true
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

        public User Create(User user)
        {
            using (var uow = this.GetUnitOfWork())
            {
                var userRepository = uow.GetRepository<User>();
                userRepository.Put(user);
                uow.Commit();
                return user;
            }
        }

        public IList<User> Find(FilterExpression filter, OrderByExpression orderBy, int pageNumber, int pageSize, out int count)
        {
            var orderbyClause = orderBy?.ToLINQ();
            var whereClause = filter?.ToLINQ();
            count = 0;
            try
            {
                using (IUnitOfWork uow = this.GetUnitOfWork())
                {
                    var userRepository = uow.GetReadOnlyRepository<User>();
                    IQueryable<User> users = userRepository.Query();
                    if (whereClause != null && orderBy != null)
                    {
                        var l = users.Where(whereClause);
                        var x = l.OrderBy(orderbyClause);
                        var y = x.Skip((pageNumber - 1) * pageSize);
                        var z = y.Take(pageSize);

                        count = l.Count();

                        return z.ToList();
                    }
                    else if (whereClause != null)
                    {
                        var filtered = users.Where(whereClause);
                        count = filtered.Count();

                        return filtered.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                    }

                    if (orderBy != null)
                    {
                        count = users.Count();
                        return users.OrderBy(orderbyClause).Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                    }

                    count = count = users.Count();

                    // without filter and order
                    return users.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Could not retrieve filtered groups."), ex);
            }
        }
    }
}